using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class QuestTrackerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private GameObject cancelButton;

        [Header("Images")]
        [SerializeField] private Image npcAvatarImage;
        [SerializeField] private Image targetImage;

        private void OnEnable()
        {
            QuestManager.OnQuestUpdated += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            QuestManager.OnQuestUpdated -= Refresh;
        }

        private void Refresh()
        {
            if (QuestManager.Instance == null)
                return;

            Quest activeQuest =
                QuestManager.Instance.GetTrackedQuest();

            // ไม่มี Quest ที่กำลัง Track
            if (activeQuest == null)
            {
                if (titleText != null)
                    titleText.text = "";

                if (questNameText != null)
                    questNameText.text = "";

                if (progressText != null)
                    progressText.text = "";

                if (cancelButton != null)
                    cancelButton.SetActive(false);

                if (npcAvatarImage != null)
                    npcAvatarImage.enabled = false;

                if (targetImage != null)
                    targetImage.enabled = false;

                return;
            }

            // =========================
            // Step ปัจจุบันของ Quest ที่กำลัง Track (persistent, unlike the step notification)
            // =========================

            if (questNameText != null)
            {
                string text = string.IsNullOrEmpty(activeQuest.StepDescription)
                    ? activeQuest.QuestName
                    : activeQuest.StepDescription;

                // Delivery: show item name + held/needed, e.g. "Material 0/5", plus Required
                // Money if the step has any — see QuestVisuals.GetDeliveryStatusSuffix.
                var deliverySuffix = QuestVisuals.GetDeliveryStatusSuffix(activeQuest);
                if (deliverySuffix != null)
                    text += $" {deliverySuffix}";

                questNameText.text = text;
            }

            // =========================
            // ส่วนอื่น ๆ
            // =========================

            if (titleText != null)
                titleText.text = activeQuest.QuestName;

            if (progressText != null)
                progressText.text = activeQuest.TrackerText;

            if (cancelButton != null)
                cancelButton.SetActive(true);

            // NPC Avatar
            QuestVisuals.Apply(npcAvatarImage, NPCManager.Instance?.GetNpcAvatar(activeQuest.DisplayNpcId));

            // Quest Target Image
            QuestVisuals.Apply(targetImage, QuestVisuals.GetItemSprite(activeQuest.TargetQuestItemId));
        }

        public void OnClickCancelQuest()
        {
            if (QuestManager.Instance == null)
                return;

            Quest activeQuest =
                QuestManager.Instance.GetTrackedQuest();

            if (activeQuest == null)
                return;

            QuestManager.Instance.CancelQuest(activeQuest.QuestID);
        }
    }
}