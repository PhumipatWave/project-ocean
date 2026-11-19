using System;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Lists accepted and completed quests, grouped by classification, in the HUD quest panel.</summary>
    public class AcceptedQuestListUI : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private AcceptedQuestSlot slotPrefab;
        [SerializeField] private TextMeshProUGUI headerPrefab;

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
            if (content == null || slotPrefab == null) return;

            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            if (QuestManager.Instance == null) return;

            // Completed quests stay listed, dimmed, after the active ones in each group.
            var quests = QuestManager.Instance.GetActiveQuests()
                .Concat(QuestManager.Instance.GetCompletedQuests())
                .ToList();

            foreach (QuestClassification classification in Enum.GetValues(typeof(QuestClassification)))
            {
                var questsInGroup = quests.Where(q => q.Classification == classification).ToList();
                if (questsInGroup.Count == 0) continue;

                if (headerPrefab != null)
                {
                    var header = Instantiate(headerPrefab, content);
                    header.text = FormatClassificationName(classification);
                }

                foreach (var quest in questsInGroup)
                {
                    var slot = Instantiate(slotPrefab, content);
                    slot.Setup(quest);
                }
            }
        }

        // "MainQuest" -> "Main Quest"
        private static string FormatClassificationName(QuestClassification classification)
        {
            string name = classification.ToString();
            var sb = new StringBuilder();

            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]))
                    sb.Append(' ');
                sb.Append(name[i]);
            }

            return sb.ToString();
        }
    }
}
