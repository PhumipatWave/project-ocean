using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Arrow on a Land UI panel, shown while the tracked quest's Tutorial step wants <see cref="action"/>.</summary>
    [RequireComponent(typeof(Graphic))]
    public class TutorialUIMarker : QuestMarkerBase
    {
        [Tooltip("The Tutorial step action this marker points the player toward (e.g. SellItem for the shop's Sell All button).")]
        [SerializeField] private TutorialActionId action;

        private Graphic graphic;

        // The player is already inside the panel, so there's no proximity to poll.
        protected override bool PollEveryFrame => false;

        private void Awake()
        {
            graphic = GetComponent<Graphic>();
            graphic.raycastTarget = false; // a guide arrow must never block the button it points at
        }

        protected override bool ShouldShow() => QuestNavigation.CurrentTutorialAction() == action;

        // Toggles the Graphic, not the GameObject, so the marker keeps listening while hidden.
        protected override void SetVisible(bool visible) => graphic.enabled = visible;
    }
}
