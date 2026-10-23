using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
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

        [Header("Animation")]
        [SerializeField] private PunchScale questNamePunch;

        private int lastQuestId = -1;
        private int lastStepIndex = -1;
        // False until a quest state has been seen, so the first one shown doesn't pop.
        private bool hasBaseline;

        private void OnEnable()
        {
            QuestManager.OnQuestUpdated += Refresh;
            KeyBindManager.OnBindingsChanged += Refresh;
            hasBaseline = false;
            Refresh();
        }

        private void OnDisable()
        {
            QuestManager.OnQuestUpdated -= Refresh;
            KeyBindManager.OnBindingsChanged -= Refresh;
        }

        private void Refresh()
        {
            if (QuestManager.Instance == null)
                return;

            Quest activeQuest =
                QuestManager.Instance.GetTrackedQuest();

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

                lastQuestId = -1;
                lastStepIndex = -1;
                hasBaseline = true;
                return;
            }

            // The tracked step stays on screen, unlike the one-off step notification.

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

            // Pop on a new step or newly tracked quest only — progress-text updates (0/5 → 1/5) stay still.
            bool stepChanged = activeQuest.QuestID != lastQuestId || activeQuest.CurrentStepIndex != lastStepIndex;
            lastQuestId = activeQuest.QuestID;
            lastStepIndex = activeQuest.CurrentStepIndex;
            if (stepChanged && hasBaseline && !LoadingManager.IsLoading && questNamePunch != null)
                questNamePunch.Play();
            hasBaseline = true;


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