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

    /// <summary>What happens to nextQuestIds' quests when THIS quest reaches Finish.</summary>
    public enum NextQuestMode
    {
        None,
        Unlock,      // becomes available (New, no longer locked) — player still has to accept it
        AutoAccept,  // accepted immediately, skips straight to InProgress
    }

    /// <summary>Which line of a step's conversation this is — mirrors the quest's own flow (offer → in-progress → turn-in).</summary>
    public enum DialogueType
    {
        Offer,       // shown while the quest is New. Only meaningful on the first step.
        InProgress,
        Finish,
        NotFinish,
        Accept,
        Reject,
    }

    /// <summary>
    /// One line (or line sequence, for Offer) of dialogue, said by one character. A step's
    /// dialogue list is a flat collection of these — effectively a (characterId, type) → lines
    /// dictionary, since Unity can't serialize Dictionary directly.
    /// </summary>
    [System.Serializable]
    public class QuestDialogueLine
    {
        [Tooltip("Matches NPCInteract's npcId. Leave blank for the step's own assigned NPC.")]
        public string characterId;
        public DialogueType type;
        [Tooltip("Multiple entries only meaningful for Offer, which plays sequentially.")]
        public string[] lines;
    }

    /// <summary>
    /// One checkpoint inside a multi-step quest — fully self-contained: its own target,
    /// its own NPC, its own in-progress/turn-in dialogue, and its own reward, paid when
    /// THIS step completes (not just the quest's final step). The step's own completionMode
    /// only controls whether finishing it (as the LAST step) auto-finishes the quest or
    /// waits for a manual turn-in.
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

        [Tooltip("Matches NPCInteract's npcId — the default speaker for this checkpoint's dialogue. Name/avatar are synced from that id, not stored here.")]
        public string npcId;

        [Tooltip("Collected by (character id, dialogue type). Leave characterId blank to use this step's own npcId.")]
        public List<QuestDialogueLine> dialogue = new();

        [Tooltip("Marks this checkpoint as a guided-sequence step — TutorialPromptUI shows/gates it (even a Talk step), not just literal Tutorial-typed ones.")]
        public bool isGuidedSequence;

        public string rewardItemId;
        public int rewardMoney;
    }

    [CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("Quest Category")]
        public QuestCategory category;
        public QuestClassification classification;

        public int questId;
        public QuestStatus status;

        [Header("Next Quest")]
        public NextQuestMode nextQuestMode;
        [Tooltip("Ignored when Next Quest Mode is None. All ids share the same mode above.")]
        public List<int> nextQuestIds = new();

        [Header("Quest Steps (checkpoints)")]
        [Tooltip("A quest needs at least one step — even a single-objective quest is a Steps list with one entry.")]
        public List<QuestStepData> steps = new();

        [Header("Quest Information")]
        public string questName;
        public string questItemId;
    }
}