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

        [Header("Interactive Buttons")]
        [SerializeField] private Button acceptButton;
        [SerializeField] private TextMeshProUGUI acceptButtonText;
        [SerializeField] private Button rejectButton;
        [SerializeField] private TextMeshProUGUI rejectButtonText;
        [SerializeField] private Button closeButton;

        private Quest currentQuest;
        private Action onQuestStatusChangedCallback;

        private void Awake()
        {
            closeButton.onClick.AddListener(HidePanel);

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
            if (currentQuest == null) return;

            // เปลี่ยนสถานะเป็นรับไปทำแล้วตาม Enum เดิมของคุณ
            currentQuest.Status = QuestStatus.InProgress;
            Debug.Log($"AcceptQuest: {currentQuest.QuestName}");

            NotifyAndClose();
        }

        private void HandleRejectQuest()
        {
            if (currentQuest == null) return;

            // เปลี่ยนสถานะเป็นปฏิเสธเควสตาม Enum เดิมของคุณ
            currentQuest.Status = QuestStatus.Reject;
            Debug.Log($"RejectQuest: {currentQuest.QuestName}");

            NotifyAndClose();
        }

        private void NotifyAndClose()
        {
            onQuestStatusChangedCallback?.Invoke();
            HidePanel();
        }
    }
}