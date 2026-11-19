using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Optional arrow and button pulse on a UI panel, shown while the tracked quest's Tutorial step wants <see cref="action"/>.</summary>
    public class TutorialUIMarker : QuestMarkerBase
    {
        [Tooltip("The Tutorial step action this marker points the player toward (e.g. SellItem for the shop's Sell All button).")]
        [SerializeField] private TutorialActionId action;

        [Tooltip("The button this marker points at; it scales up and down while the marker shows.")]
        [SerializeField] private RectTransform pulseTarget;
        [SerializeField] private float pulseScale = 1.1f;
        [SerializeField] private float pulseSpeed = 6f;

        private Graphic graphic;
        private Vector3 baseScale;
        private bool pulsing;

        // The player is already inside the panel, so there's no proximity to poll.
        protected override bool PollEveryFrame => false;

        private void Awake()
        {
            // No Graphic = pulse-only marker. Don't put one on the button's own Image: it would hide the button.
            graphic = GetComponent<Graphic>();
            if (graphic != null) graphic.raycastTarget = false; // a guide arrow must never block the button it points at
            if (pulseTarget != null) baseScale = pulseTarget.localScale;
        }

        protected override bool ShouldShow() => QuestNavigation.CurrentTutorialAction() == action;

        // Toggles the Graphic, not the GameObject, so the marker keeps listening while hidden.
        protected override void SetVisible(bool visible)
        {
            if (graphic != null) graphic.enabled = visible;
            pulsing = visible && pulseTarget != null;
            if (!pulsing && pulseTarget != null) pulseTarget.localScale = baseScale;
        }

        // Unscaled time: shop panels may pause the game while open.
        private void LateUpdate()
        {
            if (!pulsing) return;
            float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
            pulseTarget.localScale = baseScale * Mathf.Lerp(1f, pulseScale, t);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (pulseTarget != null) pulseTarget.localScale = baseScale;
        }
    }
}
