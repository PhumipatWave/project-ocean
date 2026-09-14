using System.Collections.Generic;
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

    // Where this quest sits in the game's overall quest structure (main story vs side vs tutorial vs special/event).
    public enum QuestClassification
    {
        MainQuest,
        SubQuest,
        Special
    }

    /// <summary>
    /// One checkpoint inside a multi-step quest. Progress only accumulates on the
    /// quest's current step; the step's own completionMode decides whether finishing
    /// it just advances to the next step or (on the last step) finishes the quest.
    /// </summary>
    [System.Serializable]
    public class QuestStepData
    {
        public QuestType type;

        [Tooltip("Used when type is KillCount/Delivery/ExploreArea.")]
        public string targetEnemyId;

        [Tooltip("Used when type is Tutorial/Talk — pick the action instead of typing an id.")]
        public TutorialActionId tutorialAction;

        public int targetCount = 1;
        public QuestCompletionMode completionMode = QuestCompletionMode.AutoComplete;
        [TextArea(2, 4)] public string stepDescription;
    }

    [CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("Quest Category")]
        public QuestCategory category;
        public QuestClassification classification;

        [Header("NPC")]
        public string npcName;
        public Sprite npcAvatar;

        [Header("Fish")]
        public Sprite fishSprite;

        public int questId;
        public QuestStatus status;

        [Header("Quest Steps (checkpoints)")]
        [Tooltip("Leave empty for a single-step quest — the fields below (type/targetEnemyId/targetCount/completionMode) are used as that one step. Add entries here to make a multi-checkpoint quest; the fields below are then ignored.")]
        public List<QuestStepData> steps = new();

        [Header("Legacy single-step settings (used only if Steps above is empty)")]
        public QuestType type;
        public string targetEnemyId;
        public TutorialActionId tutorialAction;
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