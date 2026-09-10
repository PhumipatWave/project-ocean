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
                questDescriptionText.text = quest.QuestDetail;
            }

            // ชื่อ NPC
            if (npcNameText != null)
            {
                npcNameText.text = quest.NpcName;
            }

            // รูปปลา
            // ตอนนี้ Quest.cs ยังไม่มี Sprite ของปลา
            // จึงยังไม่สามารถเอารูปปลาจาก Quest มาใส่อัตโนมัติได้
            if (fishImage != null)
            {
                fishImage.enabled = false;
            }

            // รูป NPC
            if (npcAvatarImage != null)
            {
                npcAvatarImage.sprite = quest.NpcAvatar;
                npcAvatarImage.enabled =
                    quest.NpcAvatar != null;
            }

            // รางวัล
            if (rewardText != null)
            {
                rewardText.text =
                    "Reward: " + quest.RewardMoney;
            }
        }

        public void ClearSlot()
        {
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