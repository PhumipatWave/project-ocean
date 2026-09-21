using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Completes a QuestType.ExploreArea step whose Area Id matches this one — either by the
    /// player interacting with this object (Interact) or by walking into its trigger collider
    /// (TriggerZone). Raises ExploreEvents.OnAreaReached; QuestManager only progresses a quest
    /// whose CURRENT step is that area, so reaching it before the step is active does nothing.
    /// </summary>
    public class ExploreAreaTrigger : MonoBehaviour
    {
        public enum Mode
        {
            Interact,     // Player presses E on this object (needs an InteractUI on the same GameObject).
            TriggerZone,  // Player walks into this object's trigger Collider2D (tag Player).
        }

        [Tooltip("Matches an ExploreArea quest step's Area Id.")]
        [SerializeField] private string areaId;

        [Tooltip("Interact: player presses E on this object (needs an InteractUI on the same GameObject). TriggerZone: player walks into this object's trigger Collider2D.")]
        [SerializeField] private Mode mode;

        [Tooltip("Optional arrow shown while this is the tracked quest's current target and the player is close — same handoff QuestCompass makes to NPC/door markers.")]
        [SerializeField] private GameObject markerUI;

        private static readonly List<ExploreAreaTrigger> all = new();

        /// <summary>The first registered trigger with this Area Id, or null if none is loaded.</summary>
        public static Transform FindByAreaId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            foreach (var trigger in all)
                if (trigger.areaId == id)
                    return trigger.transform;
            return null;
        }

        private InteractUI interactUI;
        private bool playerInside;
        private bool recheck;

        private void Awake()
        {
            if (mode == Mode.Interact)
            {
                interactUI = GetComponent<InteractUI>();
                if (interactUI != null)
                    interactUI.onInteract.AddListener(HandleInteract);
                else
                    Debug.LogWarning($"[ExploreAreaTrigger] '{name}' is Interact mode but has no InteractUI on it.", this);
            }
            else
            {
                var zone = GetComponent<Collider2D>();
                if (zone == null || !zone.isTrigger)
                    Debug.LogWarning($"[ExploreAreaTrigger] '{name}' is TriggerZone mode but has no trigger Collider2D on it.", this);
            }
        }

        private void OnDestroy()
        {
            if (interactUI != null)
                interactUI.onInteract.RemoveListener(HandleInteract);
        }

        private void OnEnable()
        {
            all.Add(this);
            QuestManager.OnQuestUpdated += HandleQuestUpdated;
        }

        private void OnDisable()
        {
            all.Remove(this);
            QuestManager.OnQuestUpdated -= HandleQuestUpdated;
            playerInside = false;
        }

        // InteractUI has already taken the player lock for this press — nothing opens here,
        // so give it straight back (before raising, so a cutscene the completion starts isn't
        // affected), then report the area.
        private void HandleInteract()
        {
            interactUI.ReleasePlayerLock();
            Raise();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (mode != Mode.TriggerZone || !other.CompareTag("Player")) return;

            playerInside = true;
            recheck = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (mode != Mode.TriggerZone || !other.CompareTag("Player")) return;

            playerInside = false;
        }

        // A step can become current while the player is already standing inside (Enter won't
        // fire again). Only flag it here — raising straight from OnQuestUpdated would re-enter
        // QuestManager while it is still iterating its quest list.
        private void HandleQuestUpdated()
        {
            if (playerInside) recheck = true;
        }

        private void Update()
        {
            // Held back while a cutscene plays (it may walk the player through the zone), then
            // fires once it is over if they are still inside.
            if (recheck && playerInside && !CutsceneGate.IsPlaying)
            {
                recheck = false;
                Raise();
            }

            if (markerUI != null)
                markerUI.SetActive(ShouldShowMarker());
        }

        private void Raise()
        {
            if (!string.IsNullOrEmpty(areaId))
                ExploreEvents.RaiseAreaReached(areaId);
        }

        private bool ShouldShowMarker()
        {
            var step = QuestManager.Instance?.GetTrackedQuest()?.CurrentStep;
            if (step == null || step.Type != QuestType.ExploreArea || step.EffectiveAreaId != areaId)
                return false;

            var player = GameManager.CurPlayer;
            return player != null &&
                Vector3.Distance(player.transform.position, QuestNavigation.InteractPosition(transform)) <= QuestNavigation.CloseRangeMeters;
        }
    }
}
