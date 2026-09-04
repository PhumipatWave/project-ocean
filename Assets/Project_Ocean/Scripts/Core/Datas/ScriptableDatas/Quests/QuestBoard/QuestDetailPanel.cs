using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Game.Testing
{
    public class QuestDetailPanel : MonoBehaviour
    {
        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Images")]
        [SerializeField] private Image itemImage;

        [Header("Interactive Buttons")]
        [SerializeField] private Button acceptButton;
        [SerializeField] private TextMeshProUGUI acceptButtonText;
        [SerializeField] private Button rejectButton;
        [SerializeField] private TextMeshProUGUI rejectButtonText;

        private Quest currentQuest;
        private Action onQuestStatusChangedCallback;

        private void Awake()
        {
            if (acceptButton != null)
                acceptButton.onClick.AddListener(HandleAcceptQuest);

            if (rejectButton != null)
                rejectButton.onClick.AddListener(HandleRejectQuest);
        }

        public void ShowPanel(Quest quest, Action onStatusChanged)
        {
            currentQuest = quest;
            onQuestStatusChangedCallback = onStatusChanged;

            // ผูกข้อมูลตรงจากตัวแปรในคลาส Quest เดิมของคุณทั้งหมด
            if (titleText != null) titleText.text = quest.QuestName;
            if (descriptionText != null) descriptionText.text = quest.QuestDetail;
            if (acceptButtonText != null) acceptButtonText.text = quest.AnswerAccept;
            if (rejectButtonText != null) rejectButtonText.text = quest.AnswerReject;

            if (itemImage != null)
            {
                Sprite sprite = GetQuestItemSprite(quest.QuestItemId);
                itemImage.sprite  = sprite;
                itemImage.enabled = sprite != null;
            }

            gameObject.SetActive(true);
        }

        public void HidePanel()
        {
            gameObject.SetActive(false);
            currentQuest = null;
            onQuestStatusChangedCallback = null;
        }

        private void HandleAcceptQuest()
        {
            if (currentQuest == null)
                return;

            bool success =
                QuestManager.Instance.AcceptQuest(
                    currentQuest.QuestID
                );

            if (success)
            {
                Debug.Log("Accept Quest");
                NotifyAndClose();
            }
            else
            {
                Debug.Log("You already have an active quest.");
            }
        }

        private void HandleRejectQuest()
        {
            if (currentQuest == null)
                return;

            Debug.Log($"Close Quest Panel : {currentQuest.QuestName}");

            HidePanel();
        }

        private void NotifyAndClose()
        {
            onQuestStatusChangedCallback?.Invoke();
            HidePanel();
        }

        private static Sprite GetQuestItemSprite(string questItemId)
        {
            if (string.IsNullOrEmpty(questItemId)) return null;
            if (!int.TryParse(questItemId, out int id)) return null;
            return InventoryManager.Instance?.FindItemInDatabase(id)?.ItemVisual;
        }
    }
}