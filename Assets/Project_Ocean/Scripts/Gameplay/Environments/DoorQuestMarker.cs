using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Tags a door with a MoveIn/MoveOut tutorial action: raises it on warp and shows the quest marker.</summary>
    [RequireComponent(typeof(DoorInteract))]
    public class DoorQuestMarker : MonoBehaviour
    {
        [Tooltip("Raised once the warp completes, if set — lets a guided Tutorial quest step require walking through this specific door (e.g. MoveInHouse for a house's entry door, MoveOutHouse for its exit door).")]
        [SerializeField] private bool raiseTutorialAction = true;
        [SerializeField] private TutorialActionId tutorialAction;

        [Tooltip("Shown when this door is the tracked quest's current target and the player is within QuestNavigation.CloseRangeMeters — same handoff QuestCompass makes to NPC's marker.")]
        [SerializeField] private GameObject markerUI;

        // Self-registers so QuestCompass can find "the door for MoveInShop" without a scene search.
        private static readonly List<DoorQuestMarker> all = new();

        private DoorInteract door;

        public TutorialActionId? TutorialAction => raiseTutorialAction ? tutorialAction : (TutorialActionId?)null;

        /// <summary>The first registered door tagged with this action, or null if none is.</summary>
        public static Transform FindByTutorialAction(TutorialActionId action)
        {
            foreach (var marker in all)
                if (marker.TutorialAction == action)
                    return marker.transform;
            return null;
        }

        /// <summary>The MoveOut* action of the exit door nearest to <paramref name="position"/> within <paramref name="maxDistance"/>, or null.</summary>
        public static TutorialActionId? FindNearestExitAction(Vector3 position, float maxDistance)
        {
            TutorialActionId? best = null;
            float bestDist = maxDistance;
            foreach (var marker in all)
            {
                var action = marker.TutorialAction;
                if (action == null || PlayerInteriorState.EntryActionFor(action.Value) == null)
                    continue;

                float dist = Vector3.Distance(position, QuestNavigation.InteractPosition(marker.transform));
                if (dist <= bestDist)
                {
                    bestDist = dist;
                    best = action;
                }
            }
            return best;
        }

        void Awake() => door = GetComponent<DoorInteract>();

        void OnEnable()
        {
            all.Add(this);
            door.Warped += RaiseAction;
            QuestManager.OnQuestUpdated += RefreshMarker;
        }

        void OnDisable()
        {
            all.Remove(this);
            door.Warped -= RaiseAction;
            QuestManager.OnQuestUpdated -= RefreshMarker;
        }

        private void RaiseAction()
        {
            if (raiseTutorialAction)
                TutorialEvents.RaiseAction(tutorialAction);
        }

        // Shows when the quest wants this door, or as a fallback for the exit of the interior the
        // player is in, unless the quest targets something else specific (e.g. a counter inside).
        private bool ShouldShowMarker()
        {
            if (!raiseTutorialAction)
                return false;

            var step = QuestManager.Instance?.GetTrackedQuest()?.CurrentStep;
            bool questWantsThis = (step != null && step.Type == QuestType.Tutorial && step.TutorialAction == tutorialAction) ||
                QuestNavigation.ResolveCompassTarget() == transform;

            var questTarget = QuestNavigation.ResolveQuestTarget();
            bool isCurrentExit = PlayerInteriorState.CurrentExitAction == tutorialAction &&
                (questTarget == null || questTarget == transform);

            if (!questWantsThis && !isCurrentExit)
                return false;

            var player = PlayerRegistry.Local;
            return player != null &&
                Vector3.Distance(player.transform.position, QuestNavigation.InteractPosition(transform)) <= QuestNavigation.CloseRangeMeters;
        }

        private void RefreshMarker()
        {
            if (markerUI != null)
                markerUI.SetActive(ShouldShowMarker());
        }

        // Proximity changes every frame as the player walks.
        void Update() => RefreshMarker();
    }
}
