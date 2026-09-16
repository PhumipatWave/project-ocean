using UnityEngine;
using TMPro;
using System.Collections;

namespace Game.Testing
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
        }

        public void Show(Quest quest)
        {
            if (quest == null)
                return;

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
                    quest.QuestDetail;
            }

            // ==============================
            // REWARDS
            // ==============================

            if (rewardText != null)
            {
                string rewards = "";

                if (quest.RewardMoney > 0)
                {
                    rewards +=
                        "Money  +" +
                        quest.RewardMoney +
                        "\n";
                }

                if (quest.RewardExp > 0)
                {
                    rewards +=
                        "EXP  +" +
                        quest.RewardExp +
                        "\n";
                }

                if (!string.IsNullOrEmpty(
                    quest.RewardItemId))
                {
                    rewards +=
                        "Item  " +
                        quest.RewardItemId +
                        "\n";
                }

                if (string.IsNullOrEmpty(rewards))
                {
                    rewards = "No Rewards";
                }

                rewardText.text = rewards;
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