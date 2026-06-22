using System;
using UnityEngine;

namespace Game.Testing
{
    public enum QuestType
    {
        Delivery,  // Delivery คือเควสประเภท เอาของที่ต้องการมาส่งให้ NPC
        KillCount, // KillCount คือฆ่ามอนสเตอร์ตามที่ NPC ต้องการ

        CollectTrash,
        ExploreArea,
        RepairObject

    }

    public enum QuestStatus
    {
        New,           // เควสใหม่ที่ยังไม่ได้รับ
        InProgress,    // เควสที่รับมาแล้ว
        Finish,        // เควสที่จบแล้ว
        ReadyToTurnIn, // เงื่อนไขครบแล้ว รอไปคุย NPC เพื่อส่งเควส (ReadyToHandIn mode)
        Reject         // เควสที่เราปฏิเสธไปแล้ว
    }

    [System.Serializable]
    public class Quest
    {
        [SerializeField]
        private int questId;
        public int QuestID { get { return questId; } }

        [SerializeField]
        private QuestType type;
        public QuestType Type { get { return type; } }

        [SerializeField]
        private QuestStatus status;
        public QuestStatus Status { get { return status; } set { status = value; } }

        [SerializeField]
        private string targetEnemyId;
        public string TargetEnemyId { get { return targetEnemyId; } }

        [SerializeField]
        private int targetCount;
        public int TargetCount { get { return targetCount; } }

        [SerializeField]
        private int currentCount;
        public int CurrentCount { get { return currentCount; } set { currentCount = value; } }

        [SerializeField]
        private QuestCompletionMode completionMode;
        public QuestCompletionMode CompletionMode { get { return completionMode; } }

        // เช่น "3 / 5"
        public string Progress { get { return currentCount + " / " + targetCount; } }
        public bool IsObjectiveComplete { get { return currentCount >= targetCount; } }

        [SerializeField]
        private string questName;
        public string QuestName { get { return questName; } }

        [SerializeField]
        private string questDetail;
        public string QuestDetail { get { return questDetail; } }

        [SerializeField]
        private string questItemId; // questItemId คือไอดีของ Item ที่ NPC ต้องการให้เราเอามาส่ง
        public string QuestItemId { get { return questItemId; } }

        [SerializeField]
        private string[] questDialogue; // questDialogue คือบทสนทนาที่ NPC พูดเป็น Step
        public string[] QuestDialogue { get { return questDialogue; } }

        [SerializeField]
        private string[] answerNext; // answerNext คือบทสนทนาที่ตัวละครเราพูดตอบเป็น Step
        public string[] AnswerNext { get { return answerNext; } }

        [SerializeField]
        private string answerAccept; // answerAccept คือคำตอบที่ตัวละครเรารับ Quest
        public string AnswerAccept { get { return answerAccept; } }

        [SerializeField]
        private string answerReject; // answerReject คือคำตอบที่ตัวละครเราปฏิเสธ Quest
        public string AnswerReject { get { return answerReject; } }

        [SerializeField]
        private string rewardItemId; // rewardItemId คือไอดี Item ของรางวัลจาก NPC
        public string RewardItemId { get { return rewardItemId; } }

        [SerializeField]
        private int rewardExp; // rewardExp คือ Exp รางวัลจากเควสนี้ (ยังไม่ใช้)
        public int RewardExp { get { return rewardExp; } }

        [SerializeField] private int rewardMoney;
        public int RewardMoney { get { return rewardMoney; } }

        [SerializeField]
        private string questionInProgress; // questionInProgress คือคำถามจาก NPC เวลาที่เรารับเควสไปแล้วกลับมาคุย
        public string QuestionInProgress { get { return questionInProgress; } }

        [SerializeField]
        private string answerFinish; // answerFinish คือคำตอบของเราที่พร้อมส่งเควส
        public string AnswerFinish { get { return answerFinish; } }

        [SerializeField]
        private string answerNotFinish; // answerNotFinish คือคำตอบของเราที่บอกว่ายังไม่พร้อมส่งเควส
        public string AnswerNotFinish { get { return answerNotFinish; } }

        [Header("NPCName")]
        private string npcName;
        public string NpcName { get { return npcName; } }

        private Sprite npcAvatar;
        public Sprite NpcAvatar { get { return npcAvatar; } }

        public Quest(QuestData questData)
        {
            questId = questData.questId;
            type = questData.type;
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

            targetEnemyId = questData.targetEnemyId;
            targetCount = questData.targetCount;
            completionMode = questData.completionMode;
            currentCount = 0;


        }

        /// <summary>
        /// เพิ่มความคืบหน้าให้เควส KillCount; คืนค่า true ถ้าเพิ่งถึงเป้าในครั้งนี้
        /// (ใช้ตัดสินใจว่าจะแสดงข้อความ "เควสเสร็จ/พร้อมส่ง" หรือไม่)
        /// </summary>
        public bool AddProgress(int amount = 1)
        {
            if (status != QuestStatus.InProgress) return false;
            if (currentCount >= targetCount) return false;

            currentCount += amount;
            if (currentCount < targetCount) return false;

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
