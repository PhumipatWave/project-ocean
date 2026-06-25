using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Testing
{
    public class QuestTrackerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private GameObject cancelButton;

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

                return;
            }

            titleText.text = activeQuest.QuestName;
            questNameText.text = activeQuest.QuestDetail;
            progressText.text = activeQuest.TrackerText;

            cancelButton.SetActive(true);
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