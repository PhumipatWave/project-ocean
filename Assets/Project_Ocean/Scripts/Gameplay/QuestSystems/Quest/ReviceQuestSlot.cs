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
            if (questNameText != null) questNameText.text = quest.StepDescription;
            if (progressText != null) progressText.text = quest.TrackerText;

            // NPC avatar — resolved live from NPCManager by id, not stored on the quest.
            QuestVisuals.Apply(npcAvatarImage, NPCManager.Instance?.GetNpcAvatar(quest.NpcId));
            QuestVisuals.Apply(targetImage, QuestVisuals.GetItemSprite(quest.TargetQuestItemId));
        }
    }
}
