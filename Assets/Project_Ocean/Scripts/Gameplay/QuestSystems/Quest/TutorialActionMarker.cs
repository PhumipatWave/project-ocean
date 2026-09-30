using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Marks a non-NPC, non-door objective (e.g. PlayerShip for Dive) for a Tutorial action when the player is near.</summary>
    public class TutorialActionMarker : QuestMarkerBase
    {
        [SerializeField] private TutorialActionId action;

        [Tooltip("Other actions that happen at this same object (e.g. the upgrade shop counter is where both UpgradeRope and BuyItem happen).")]
        [SerializeField] private TutorialActionId[] additionalActions;

        // Self-registers so QuestNavigation can find "the object for action X" without a scene search.
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

        protected override void OnEnable()
        {
            all.Add(this);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            all.Remove(this);
            base.OnDisable();
        }

        protected override bool ShouldShow() =>
            QuestNavigation.CurrentTutorialAction() is TutorialActionId current && Handles(current) &&
            QuestNavigation.IsPlayerNear(transform);
    }
}
