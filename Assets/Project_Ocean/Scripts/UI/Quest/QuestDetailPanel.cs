using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class QuestDetailPanel : MonoBehaviour, IQuestOfferView
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

            if (titleText != null) titleText.text = quest.QuestName;
            if (descriptionText != null) descriptionText.text = quest.StepDescription;
            if (acceptButtonText != null) acceptButtonText.text = quest.CurrentStep.AnswerAccept;
            if (rejectButtonText != null) rejectButtonText.text = quest.CurrentStep.AnswerReject;

            QuestVisuals.Apply(itemImage, QuestVisuals.GetItemSprite(quest.TargetQuestItemId));

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
                GameLog.Info("Accept Quest");
                TutorialEvents.RaiseAction(TutorialActionId.AcceptQuest);
                NotifyAndClose();
            }
            else
            {
                GameLog.Info("You already have an active quest.");
            }
        }

        private void HandleRejectQuest()
        {
            if (currentQuest == null)
                return;

            GameLog.Info($"Close Quest Panel : {currentQuest.QuestName}");

            HidePanel();
        }

        private void NotifyAndClose()
        {
            onQuestStatusChangedCallback?.Invoke();
            HidePanel();
        }
    }
}