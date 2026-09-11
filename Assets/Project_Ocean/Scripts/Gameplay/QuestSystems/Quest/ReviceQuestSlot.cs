using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class ReviceQuestSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image npcAvatarImage;
        [SerializeField] private Image targetImage;

        public void Setup(Quest quest)
        {
            if (titleText != null) titleText.text = quest.QuestName;
            if (questNameText != null) questNameText.text = quest.QuestDetail;
            if (progressText != null) progressText.text = quest.TrackerText;

            if (npcAvatarImage != null)
            {
                npcAvatarImage.sprite = quest.NpcAvatar;
                npcAvatarImage.enabled = quest.NpcAvatar != null;
            }

            if (targetImage != null)
            {
                Sprite sprite = GetQuestItemSprite(quest.QuestItemId);
                targetImage.sprite = sprite;
                targetImage.enabled = sprite != null;
            }
        }

        private static Sprite GetQuestItemSprite(string questItemId)
        {
            if (string.IsNullOrEmpty(questItemId)) return null;
            if (!int.TryParse(questItemId, out int id)) return null;
            return InventoryManager.Instance?.FindItemInDatabase(id)?.ItemVisual;
        }
    }
}
