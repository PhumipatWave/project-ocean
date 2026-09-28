using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
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
            Action<Quest> onClickedAction,
            Sprite fallbackAvatar = null)
        {
            currentQuest = quest;
            onSlotClickedCallback = onClickedAction;

            gameObject.SetActive(true);

            // Quest name
            if (questNameText != null)
            {
                questNameText.text = quest.QuestName;
            }

            // Quest description
            if (questDescriptionText != null)
            {
                questDescriptionText.text = quest.StepDescription;
            }

            // NPC name — resolved live from NPCManager by id, not stored on the quest.
            if (npcNameText != null)
            {
                npcNameText.text = NPCManager.Instance?.GetNpcName(quest.DisplayNpcId);
            }

            QuestVisuals.Apply(fishImage, QuestVisuals.GetItemSprite(quest.TargetQuestItemId));

            // NPC avatar — resolved live from NPCManager by id, not stored on the quest. Pure
            // board quests have no giver id, so DisplayNpcId is null and the image hides.
            // Falls back to the quest board's own image when there's no giver portrait.
            QuestVisuals.Apply(npcAvatarImage, NPCManager.Instance?.GetNpcAvatar(quest.DisplayNpcId) ?? fallbackAvatar);

            // Reward preview shows the first step's reward (board quests are single-step today)
            if (rewardText != null)
            {
                rewardText.text =
                    "Reward: " + quest.CurrentStep.RewardMoney;
            }
        }

        public void ClearSlot()
        {
            currentQuest = null;
            onSlotClickedCallback = null;

            QuestVisuals.Apply(fishImage, null);
            QuestVisuals.Apply(npcAvatarImage, null);

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