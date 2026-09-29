using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Shows a Marker — same handoff pattern as NPC.markerUI/DoorInteract.markerUI — when the
    /// tracked quest's current step requires the given TutorialActionId and the player is
    /// within QuestNavigation.CloseRangeMeters. For an objective that's neither an NPC nor a
    /// warp door (e.g. PlayerShip, which represents the Dive action). Self-registers like
    /// DoorInteract does, so QuestCompass can find "the object for action X" generically.
    /// </summary>
    public class TutorialActionMarker : MonoBehaviour
    {
        [SerializeField] private TutorialActionId action;

        [Tooltip("Other actions that happen at this same object (e.g. the upgrade shop counter is where both UpgradeRope and BuyItem happen).")]
        [SerializeField] private TutorialActionId[] additionalActions;
        [SerializeField] private GameObject markerUI;

        private static readonly List<TutorialActionMarker> all = new();

        public static Transform FindByTutorialAction(TutorialActionId action)
        {
            foreach (var marker in all)
                if (marker.Handles(action))
                    return marker.transform;
            return null;
        }

        private bool Handles(TutorialActionId candidate) =>
            action == candidate || (additionalActions != null && System.Array.IndexOf(additionalActions, candidate) >= 0);

        private void OnEnable()
        {
            all.Add(this);
            QuestManager.OnQuestUpdated += RefreshMarker;
        }

        private void OnDisable()
        {
            all.Remove(this);
            QuestManager.OnQuestUpdated -= RefreshMarker;
        }

        private bool ShouldShowMarker()
        {
            var step = QuestManager.Instance?.GetTrackedQuest()?.CurrentStep;
            if (step == null || step.Type != QuestType.Tutorial || !Handles(step.TutorialAction))
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
        private void Update() => RefreshMarker();
    }
}
