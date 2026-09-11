using UnityEngine;

namespace Game.Testing
{
    /// Populates the accepted-quests scroll view (GameHUD/PlayerHUDPanel/QuestUI/ReviceQuestScrollView)
    /// with one ReviceQuestSlot per quest the player currently has accepted (InProgress or ReadyToTurnIn).
    public class ReviceQuestListUI : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private ReviceQuestSlot slotPrefab;

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

            foreach (var quest in QuestManager.Instance.GetActiveQuests())
            {
                var slot = Instantiate(slotPrefab, content);
                slot.Setup(quest);
            }
        }
    }
}
