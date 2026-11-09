using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.Gameplay
{
    public class AcceptedQuestSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image npcAvatarImage;
        [SerializeField] private Image targetImage;

        [Tooltip("Dimmed (not hidden) for a completed quest — same alpha-fade convention used elsewhere in the HUD.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Alpha applied when the quest passed to Setup is already Finish.")]
        [SerializeField] private float completedAlpha = 0.5f;

        public void Setup(Quest quest)
        {
            if (titleText != null) titleText.text = quest.QuestName;

            if (questNameText != null)
            {
                // Delivery: append item name + held/needed + Required Money, same as QuestTrackerUI.
                var deliverySuffix = QuestVisuals.GetDeliveryStatusSuffix(quest);
                questNameText.text = deliverySuffix != null
                    ? $"{quest.StepDescription} {deliverySuffix}"
                    : quest.StepDescription;
            }

            if (progressText != null) progressText.text = quest.TrackerText;

            // NPC avatar — resolved live from NPCManager by id, not stored on the quest.
            QuestVisuals.Apply(npcAvatarImage, NPCManager.Instance?.GetNpcAvatar(quest.DisplayNpcId));
            QuestVisuals.Apply(targetImage, QuestVisuals.GetStepItemSprite(quest.CurrentStep));

            if (canvasGroup != null)
                canvasGroup.alpha = quest.Status == QuestStatus.Finish ? completedAlpha : 1f;
        }
    }
}
