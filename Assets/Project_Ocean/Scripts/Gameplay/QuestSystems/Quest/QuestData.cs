using UnityEngine;

namespace Game.Testing
{
    public enum QuestCompletionMode
    {
        AutoComplete,
        ReadyToHandIn,
    }

    // หมวดหลักของ Quest Board
    public enum QuestCategory
    {
        FishS,
        FishM,
        FishL,
        Trash,
        Explore
    }

    [CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("Quest Category")]
        public QuestCategory category;

        [Header("NPC")]
        public string npcName;
        public Sprite npcAvatar;

        [Header("Fish")]
        public Sprite fishSprite;

        public int questId;
        public QuestType type;
        public QuestStatus status;

        [Header("KillCount quest settings")]
        public string targetEnemyId;
        public int targetCount = 1;
        public QuestCompletionMode completionMode = QuestCompletionMode.AutoComplete;

        [Header("Quest Information")]
        public string questName;
        public string questDetail;
        public string questItemId;

        public string[] questDialogue;
        public string[] answerNext;
        public string answerAccept;
        public string answerReject;

        [Header("Reward")]
        public string rewardItemId;
        public int rewardExp;
        public int rewardMoney;

        [Header("Quest Conversation")]
        public string questionInProgress;
        public string answerFinish;
        public string answerNotFinish;
    }
}