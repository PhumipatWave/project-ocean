using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

namespace Game.Testing
{
    public class DoorInteract : MonoBehaviour
    {
        [Header("Warp Logic")]
        [SerializeField] private Transform warpDestination;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip doorOpenSound;

        [Header("UI Reference")]
        [SerializeField] private CanvasGroup blackScreenGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Camera Control")]
        [SerializeField] private bool snapCamera = true;

        [Header("Tutorial")]
        [Tooltip("Raised once the warp completes, if set — lets a guided Tutorial quest step require walking through this specific door (e.g. MoveInHouse for a house's entry door, MoveOutHouse for its exit door). Leave unset for a door no tutorial step ever targets.")]
        [SerializeField] private bool raiseTutorialAction;
        [SerializeField] private TutorialActionId tutorialAction;

        [Tooltip("Shown when this door is the tracked quest's current target and the player is within QuestNavigation.CloseRangeMeters — same handoff QuestCompass makes to NPC's marker. Leave unset for a door no tutorial step ever targets.")]
        [SerializeField] private GameObject markerUI;

        private CinemachineCamera vcam;
        private static bool isWarping;

        // Self-registers so QuestCompass can find "the door for MoveInShop" etc. without a
        // scene search — mirrors NPCManager.Register/Unregister's pattern for NPCs.
        private static readonly List<DoorInteract> all = new();

        /// <summary>This door's tagged tutorial action, if any — see raiseTutorialAction above.</summary>
        public TutorialActionId? TutorialAction => raiseTutorialAction ? tutorialAction : (TutorialActionId?)null;

        /// <summary>The first registered door tagged with this action, or null if none is.</summary>
        public static Transform FindByTutorialAction(TutorialActionId action)
        {
            foreach (var door in all)
                if (door.TutorialAction == action)
                    return door.transform;
            return null;
        }

        void Awake()
        {
            if (snapCamera)
                vcam = Object.FindFirstObjectByType<CinemachineCamera>();
        }

        void OnEnable()
        {
            all.Add(this);
            QuestManager.OnQuestUpdated += RefreshMarker;
        }

        void OnDisable()
        {
            all.Remove(this);
            QuestManager.OnQuestUpdated -= RefreshMarker;
        }

        /// <summary>True while this door is the tracked quest's current target and the player
        /// is close enough that QuestCompass has handed off to this marker — mirrors
        /// NPC.ShouldShowMarker's condition, but matched against TutorialAction instead of
        /// EffectiveNpcId (see QuestCompass.ResolveTarget, which targets the same door).
        /// Also shows with no quest involved at all if this is the exit door for whichever
        /// interior the player is currently in (see PlayerInteriorState) — same override
        /// QuestCompass gives that case priority over.</summary>
        private bool ShouldShowMarker()
        {
            if (!raiseTutorialAction)
                return false;

            var step = QuestManager.Instance?.GetTrackedQuest()?.CurrentStep;
            bool questWantsThis = step != null && step.Type == QuestType.Tutorial && step.TutorialAction == tutorialAction;
            bool isCurrentExit = PlayerInteriorState.CurrentExitAction == tutorialAction;
            if (!questWantsThis && !isCurrentExit)
                return false;

            var player = GameManager.CurPlayer;
            return player != null &&
                Vector3.Distance(player.transform.position, QuestNavigation.InteractPosition(transform)) <= QuestNavigation.CloseRangeMeters;
        }

        private void RefreshMarker()
        {
            if (markerUI != null)
                markerUI.SetActive(ShouldShowMarker());
        }

        // Proximity changes every frame as the player walks — skip entirely for the doors
        // no tutorial step ever targets (most of them).
        void Update()
        {
            if (markerUI != null)
                RefreshMarker();
        }

        public void Interact()
        {
            if (!isWarping)
                StartCoroutine(WarpSequence());
        }

        IEnumerator WarpSequence()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) yield break;

            isWarping = true;

            if (audioSource != null && doorOpenSound != null)
                audioSource.PlayOneShot(doorOpenSound);

            if (blackScreenGroup != null)
            {
                blackScreenGroup.gameObject.SetActive(true);
                blackScreenGroup.alpha = 0;
            }

            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                if (blackScreenGroup != null) blackScreenGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            if (blackScreenGroup != null) blackScreenGroup.alpha = 1;

            Vector3 oldPosition = player.transform.position;
            player.transform.position = warpDestination.position;

            if (snapCamera && vcam != null)
                vcam.OnTargetObjectWarped(player.transform, warpDestination.position - oldPosition);

            if (raiseTutorialAction)
                TutorialEvents.RaiseAction(tutorialAction);

            yield return new WaitForSeconds(0.1f);

            timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                if (blackScreenGroup != null) blackScreenGroup.alpha = 1 - (timer / fadeDuration);
                yield return null;
            }

            if (blackScreenGroup != null)
            {
                blackScreenGroup.alpha = 0;
                blackScreenGroup.gameObject.SetActive(false);
            }

            isWarping = false;
        }
    }
}
