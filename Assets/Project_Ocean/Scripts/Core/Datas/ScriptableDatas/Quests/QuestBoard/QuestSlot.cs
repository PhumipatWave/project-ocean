using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Game.Testing
{
    [RequireComponent(typeof(Button))]
    public class QuestSlot : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private Image fishImage;
        [SerializeField] private TMP_Text npcNameText;
        [SerializeField] private Image npcAvatarImage;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private TextMeshProUGUI questDescriptionText;

        private Button slotButton;
        private Quest currentQuest;
        private Action<Quest> onSlotClickedCallback;

        private void Awake()
        {
            slotButton = GetComponent<Button>();
            slotButton.onClick.AddListener(HandleSlotClick);
        }

        public void SetupSlot(
            Quest quest,
            Action<Quest> onClickedAction)
        {
            currentQuest = quest;
            onSlotClickedCallback = onClickedAction;

            gameObject.SetActive(true);

            // ชื่อ Quest
            if (questNameText != null)
            {
                questNameText.text = quest.QuestName;
            }

            // รายละเอียด Quest
            if (questDescriptionText != null)
            {
                questDescriptionText.text = quest.StepDescription;
            }

            // ชื่อ NPC — resolved live from NPCManager by id, not stored on the quest.
            if (npcNameText != null)
            {
                npcNameText.text = NPCManager.Instance?.GetNpcName(quest.NpcId);
            }

            // =========================
            // รูปปลา
            // =========================
            if (fishImage != null)
            {
                Sprite sprite = GetQuestItemSprite(quest.QuestItemId);

                fishImage.sprite = sprite;
                fishImage.enabled = sprite != null;
            }

            // รูป NPC — resolved live from NPCManager by id, not stored on the quest.
            if (npcAvatarImage != null)
            {
                Sprite avatar = NPCManager.Instance?.GetNpcAvatar(quest.NpcId);
                npcAvatarImage.sprite = avatar;
                npcAvatarImage.enabled = avatar != null;
            }

            // รางวัล — preview shows the first step's reward (board quests are single-step today)
            if (rewardText != null)
            {
                rewardText.text =
                    "Reward: " + quest.CurrentStep.RewardMoney;
            }
        }

        // =========================
        // ดึงรูปจาก ItemVisual
        // =========================
        private static Sprite GetQuestItemSprite(string questItemId)
        {
            if (string.IsNullOrEmpty(questItemId))
                return null;

            if (!int.TryParse(questItemId, out int id))
                return null;

            return InventoryManager.Instance?
                .FindItemInDatabase(id)?
                .ItemVisual;
        }

        public void ClearSlot()
        {
            currentQuest = null;
            onSlotClickedCallback = null;

            if (fishImage != null)
            {
                fishImage.sprite = null;
                fishImage.enabled = false;
            }

            if (npcAvatarImage != null)
            {
                npcAvatarImage.sprite = null;
                npcAvatarImage.enabled = false;
            }

            gameObject.SetActive(false);
        }

        private void HandleSlotClick()
        {
            if (currentQuest != null)
            {
                onSlotClickedCallback?.Invoke(currentQuest);
            }
        }
    }
}