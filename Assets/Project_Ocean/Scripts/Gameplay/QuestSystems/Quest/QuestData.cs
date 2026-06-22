using UnityEngine;

namespace Game.Testing
{
    public enum QuestCompletionMode
    {
        AutoComplete,   // ถึงเป้า → จบเควสทันที (Finish)
        ReadyToHandIn,  // ถึงเป้า → รอไปส่งที่ NPC (ReadyToTurnIn)
    }

    [CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
    public class QuestData : ScriptableObject
    {
        public string npcName;
        public Sprite npcAvatar;

        public int questId;
        public QuestType type;
        public QuestStatus status;



        [Header("KillCount quest settings")]
        public string targetEnemyId;   // ไอดีของศัตรูที่ต้องฆ่า (ตรงกับ EnemyIdentity.EnemyId)
        public int targetCount = 1;    // จำนวนที่ต้องฆ่า
        public QuestCompletionMode completionMode = QuestCompletionMode.AutoComplete;

        public string questName;
        public string questDetail;
        public string questItemId;
        public string[] questDialogue;
        public string[] answerNext;
        public string answerAccept;
        public string answerReject;
        public string rewardItemId;
        public int rewardExp;
        public int rewardMoney;
        public string questionInProgress;
        public string answerFinish;
        public string answerNotFinish;
    }
}
