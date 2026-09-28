using UnityEngine;
using TMPro;
using System.Collections;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class QuestCompleteUI : MonoBehaviour
    {
        public static QuestCompleteUI Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private GameObject panel;

        [SerializeField] private TextMeshProUGUI questCompleteText;
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private TextMeshProUGUI questDetailText;
        [SerializeField] private TextMeshProUGUI rewardText;

        [Header("Auto Close")]
        [SerializeField] private float displayTime = 3f;

        private Coroutine hideCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (panel != null)
                panel.SetActive(false);

            QuestManager.OnQuestCompleted += Show;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                QuestManager.OnQuestCompleted -= Show;
        }

        public void Show(Quest quest)
        {
            if (quest == null)
                return;

            StartCoroutine(ShowWhenReady(quest));
        }

        // Quests can finish as the day advances; wait so the next-day animation doesn't cover the panel.
        private IEnumerator ShowWhenReady(Quest quest)
        {
            yield return null; // let NextDayAnimationUI start first if it's reacting to the same event
            while (NextDayAnimationUI.IsPlaying)
                yield return null;

            Display(quest);
        }

        private void Display(Quest quest)
        {
            // ==============================
            // OPEN PANEL
            // ==============================

            if (panel != null)
                panel.SetActive(true);

            // ==============================
            // QUEST COMPLETE
            // ==============================

            if (questCompleteText != null)
            {
                questCompleteText.text =
                    "QUEST COMPLETE!";
            }

            // ==============================
            // QUEST NAME
            // ==============================

            if (questNameText != null)
            {
                questNameText.text =
                    quest.QuestName;
            }

            // ==============================
            // QUEST DETAIL
            // ==============================

            if (questDetailText != null)
            {
                questDetailText.text =
                    quest.StepDescription;
            }

            // ==============================
            // REWARDS
            // ==============================

            if (rewardText != null)
            {
                string rewards = "";

                if (quest.CurrentStep.RewardMoney > 0)
                {
                    rewards +=
                        "Money  +" +
                        quest.CurrentStep.RewardMoney +
                        "\n";
                }

                if (!string.IsNullOrEmpty(
                    quest.CurrentStep.RewardItemId))
                {
                    rewards +=
                        "Item  " +
                        quest.CurrentStep.RewardItemId +
                        "\n";
                }

                rewardText.text = rewards; // empty when there is no reward
            }

            // ==============================
            // AUTO CLOSE
            // ==============================

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }

            hideCoroutine =
                StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(
                displayTime
            );

            Close();
        }

        private void Close()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            hideCoroutine = null;
        }
    }
}