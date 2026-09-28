using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// A marker arrow drawn on a Land UI panel (dive, shop, board, home, upgrade) — shown only while
    /// the tracked quest's current step is a Tutorial step for <see cref="action"/>. Same condition
    /// as TutorialActionMarker, minus the proximity check (the player is already inside the panel).
    /// Toggles the Graphic instead of the GameObject so it keeps listening while hidden, and hides
    /// itself in the panel's closed state simply because its panel is inactive.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class TutorialUIMarker : MonoBehaviour
    {
        [Tooltip("The Tutorial step action this marker points the player toward (e.g. SellItem for the shop's Sell All button).")]
        [SerializeField] private TutorialActionId action;

        private Graphic graphic;

        private void Awake()
        {
            graphic = GetComponent<Graphic>();
            graphic.raycastTarget = false; // a guide arrow must never block the button it points at
        }

        private void OnEnable()
        {
            QuestManager.OnQuestUpdated += Refresh;
            Refresh();
        }

        private void OnDisable() => QuestManager.OnQuestUpdated -= Refresh;

        private void Refresh()
        {
            var step = QuestManager.Instance?.GetTrackedQuest()?.CurrentStep;
            graphic.enabled = step != null && step.Type == QuestType.Tutorial && step.TutorialAction == action;
        }
    }
}
