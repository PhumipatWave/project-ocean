using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
        }

        private void OnDisable()
        {
            QuestManager.OnQuestUpdated -= Refresh;
        }

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (QuestManager.Instance == null)
                return;

            Quest activeQuest =
                QuestManager.Instance.GetCurrentActiveQuest();

            if (activeQuest == null)
            {
                titleText.text = "";
                questNameText.text = "";
                progressText.text = "";
                cancelButton.SetActive(false);

                if (npcAvatarImage != null) npcAvatarImage.enabled = false;
                if (targetImage != null)    targetImage.enabled    = false;

                return;
            }

            titleText.text     = activeQuest.QuestName;
            questNameText.text = activeQuest.QuestDetail;
            progressText.text  = activeQuest.TrackerText;
            cancelButton.SetActive(true);

            if (npcAvatarImage != null)
            {
                npcAvatarImage.sprite  = activeQuest.NpcAvatar;
                npcAvatarImage.enabled = activeQuest.NpcAvatar != null;
            }

            if (targetImage != null)
            {
                Sprite sprite = GetQuestItemSprite(activeQuest.QuestItemId);
                targetImage.sprite  = sprite;
                targetImage.enabled = sprite != null;
            }
        }

        private static Sprite GetQuestItemSprite(string questItemId)
        {
            if (string.IsNullOrEmpty(questItemId)) return null;
            if (!int.TryParse(questItemId, out int id)) return null;
            return InventoryManager.Instance?.FindItemInDatabase(id)?.ItemVisual;
        }

        public void OnClickCancelQuest()
        {
            Quest activeQuest =
                QuestManager.Instance.GetCurrentActiveQuest();

            if (activeQuest == null)
                return;

            QuestManager.Instance.CancelQuest(activeQuest.QuestID);
        }
    }
}