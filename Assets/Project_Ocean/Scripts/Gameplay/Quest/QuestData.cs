using UnityEngine;

namespace Game.Testing
{
    [CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
    public class QuestData : ScriptableObject
    {
        public string npcName; 
        public Sprite npcAvatar;

        public int questId;
        public QuestType type;
        public QuestStatus status;
        public string questName;
        public string questDetail;
        public string questItemId;
        public string[] questDialogue;
        public string[] answerNext;
        public string answerAccept;
        public string answerReject;
        public string rewardItemId;
        public int rewardExp;
        public string questionInProgress;
        public string answerFinish;
        public string answerNotFinish;

    }
}
