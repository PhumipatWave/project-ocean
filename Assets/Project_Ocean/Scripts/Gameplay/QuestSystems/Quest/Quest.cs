using System;
using UnityEngine;

namespace Game.Testing
{
    public enum QuestType
    {
        Delivery,
        KillCount,
        ExploreArea
    }

    public enum QuestStatus
    {
        New,
        InProgress,
        Finish,
        ReadyToTurnIn,
        Reject
    }

    [System.Serializable]
    public class Quest
    {
        [SerializeField]
        private int questId;
        public int QuestID => questId;

        [SerializeField]
        private QuestCategory category;
        public QuestCategory Category => category;

        [SerializeField]
        private QuestType type;
        public QuestType Type => type;

        [SerializeField]
        private QuestClassification classification;
        public QuestClassification Classification => classification;

        [SerializeField]
        private QuestStatus status;
        public QuestStatus Status
        {
            get => status;
            set => status = value;
        }

        [SerializeField]
        private string targetEnemyId;
        public string TargetEnemyId => targetEnemyId;

        [SerializeField]
        private int targetCount;
        public int TargetCount => targetCount;

        [SerializeField]
        private int currentCount;
        public int CurrentCount
        {
            get => currentCount;
            set => currentCount = value;
        }

        [SerializeField]
        private QuestCompletionMode completionMode;
        public QuestCompletionMode CompletionMode => completionMode;

        public string Progress => currentCount + " / " + targetCount;

        public bool IsObjectiveComplete =>
            currentCount >= targetCount;

        [SerializeField]
        private string questName;
        public string QuestName => questName;

        [SerializeField]
        private string questDetail;
        public string QuestDetail => questDetail;

        [SerializeField]
        private string questItemId;
        public string QuestItemId => questItemId;

        [SerializeField]
        private string[] questDialogue;
        public string[] QuestDialogue => questDialogue;

        [SerializeField]
        private string[] answerNext;
        public string[] AnswerNext => answerNext;

        [SerializeField]
        private string answerAccept;
        public string AnswerAccept => answerAccept;

        [SerializeField]
        private string answerReject;
        public string AnswerReject => answerReject;

        [SerializeField]
        private string rewardItemId;
        public string RewardItemId => rewardItemId;

        [SerializeField]
        private int rewardExp;
        public int RewardExp => rewardExp;

        [SerializeField]
        private int rewardMoney;
        public int RewardMoney => rewardMoney;

        [SerializeField]
        private string questionInProgress;
        public string QuestionInProgress => questionInProgress;

        [SerializeField]
        private string answerFinish;
        public string AnswerFinish => answerFinish;

        [SerializeField]
        private string answerNotFinish;
        public string AnswerNotFinish => answerNotFinish;

        [Header("NPCName")]
        private string npcName;
        public string NpcName => npcName;

        private Sprite npcAvatar;
        public Sprite NpcAvatar => npcAvatar;
        public Sprite fishSprite;
        public Quest(QuestData questData)
        {
            questId = questData.questId;

            // สำคัญ
            category = questData.category;

            type = questData.type;
            classification = questData.classification;
            status = questData.status;

            questName = questData.questName;
            questDetail = questData.questDetail;
            questItemId = questData.questItemId;

            questDialogue = questData.questDialogue;
            answerNext = questData.answerNext;

            answerAccept = questData.answerAccept;
            answerReject = questData.answerReject;

            rewardItemId = questData.rewardItemId;
            rewardExp = questData.rewardExp;
            rewardMoney = questData.rewardMoney;

            questionInProgress = questData.questionInProgress;
            answerFinish = questData.answerFinish;
            answerNotFinish = questData.answerNotFinish;

            npcName = questData.npcName;
            npcAvatar = questData.npcAvatar;
            fishSprite = questData.fishSprite;

            targetEnemyId = questData.targetEnemyId;
            targetCount = questData.targetCount;
            completionMode = questData.completionMode;

            currentCount = 0;
        }

        public bool AddProgress(int amount = 1)
        {
            if (status != QuestStatus.InProgress)
                return false;

            if (currentCount >= targetCount)
                return false;

            currentCount += amount;

            if (currentCount < targetCount)
                return false;

            currentCount = targetCount;

            status = completionMode == QuestCompletionMode.AutoComplete
                ? QuestStatus.Finish
                : QuestStatus.ReadyToTurnIn;

            return true;
        }

        public string TrackerText
        {
            get
            {
                if (status == QuestStatus.ReadyToTurnIn)
                    return "READY TO TURN IN";

                return Progress;
            }
        }
    }
}