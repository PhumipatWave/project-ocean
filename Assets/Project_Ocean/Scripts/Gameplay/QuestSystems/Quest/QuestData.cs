using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public enum QuestCompletionMode
    {
        AutoComplete,
        ReadyToHandIn,
    }

    // Top-level category shown on the Quest Board.
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

    /// <summary>Delivery only. CombinedTotal: any mix of the listed items reaches Target Count. PerItem: each id has its own count.</summary>
    public enum DeliveryCountMode
    {
        CombinedTotal,
        PerItem,
    }

    /// <summary>Talk only. AnyOne: any listed NPC completes the step. All: every listed NPC, once each.</summary>
    public enum TalkCompletionMode
    {
        AnyOne,
        All,
    }

    /// <summary>An item id and required count for PerItem delivery; a list of these stands in for a Dictionary.</summary>
    [System.Serializable]
    public class QuestItemRequirement
    {
        [Tooltip("Parsed as an int item id, same as Target Quest Item Id elsewhere.")]
        public string itemId;
        [Tooltip("Exactly how many of this item are required.")]
        public int count = 1;
    }

    /// <summary>Dialogue lines for one character and dialogue type.</summary>
    [System.Serializable]
    public class QuestDialogueLine
    {
        [Tooltip("Who speaks: matches a CharacterDatabase id / an NPC's npcId (name and portrait follow it). Leave blank to default to this step's Target NPC Id (Talk steps); on an Auto Display Dialogue step, blank = the player.")]
        public string characterId;
        public DialogueType type;
        [Tooltip("Multiple entries only meaningful for Offer, which plays sequentially.")]
        public string[] lines;
        [Tooltip("Optional — matches a Cutscene component's Cutscene Id in the current scene. Plays once when this line first shows. Leave blank for plain dialogue.")]
        public string cutsceneId;
    }

    /// <summary>One step of a quest, with its own target, dialogue and reward.</summary>
    // completionMode only matters on the last step: finish the quest at once or wait for a turn-in.
    [System.Serializable]
    public class QuestStepData
    {
        public QuestType type;

        [Tooltip("Used when type is ExploreArea (Delivery + EnemyKills uses Target Enemy Ids below instead).")]
        public string targetEnemyId;

        [Tooltip("Delivery + EnemyKills only — kills of ANY enemy id in this list count toward the step's shared Target Count.")]
        public List<string> targetEnemyIds = new();

        [Tooltip("Used when type is Tutorial/Talk — pick the action instead of typing an id.")]
        public TutorialActionId tutorialAction;

        [Tooltip("Item removed from inventory when this checkpoint completes (parsed as an int item id) — was quest-level, now per-step so different checkpoints can target different items. Delivery + InventoryHeld uses Target Quest Item Ids below instead; Delivery + EnemyKills uses this as its optional single hand-in item.")]
        public string targetQuestItemId;

        [Tooltip("Delivery only — what drives this step's progress. InventoryHeld: live-rechecked against the player's current inventory (drag items in/out, sell, etc.). EnemyKills: one-shot kill events matching Target Enemy Ids below (the old KillCount type, merged in here).")]
        public DeliverySource deliverySource = DeliverySource.InventoryHeld;

        [Tooltip("Delivery + InventoryHeld + CombinedTotal only — which items count toward the shared Target Count (any mix of these, summed, must reach it). No per-item count — that's what PerItem's Target Quest Item Requirements below is for.")]
        public List<string> targetQuestItemIds = new();

        [Tooltip("Delivery + InventoryHeld + PerItem only — which items are required and how many of each, independently.")]
        public List<QuestItemRequirement> targetQuestItemRequirements = new();

        [Tooltip("Delivery + InventoryHeld only — CombinedTotal (any mix summing to Target Count) or PerItem (each requirement's own Count must be met).")]
        public DeliveryCountMode deliveryCountMode = DeliveryCountMode.CombinedTotal;

        [Tooltip("Delivery only — ON (default): completing this step removes the delivered/killed-for item(s) from inventory, same as always. OFF: the step still completes and pays out, but nothing is removed — e.g. \"show me 5 fish\" without losing them.")]
        public bool consumeOnComplete = true;

        [Tooltip("Delivery only — extra money required alongside the item/kill count above (0 = none). The step won't complete until BOTH are satisfied; whichever is met second finishes it. Consume On Complete also governs whether this money is spent on completion.")]
        public int requiredMoney = 0;

        [Tooltip("WaitingQuest only — how many day-advances (sleeps) must happen after this step becomes current before it can complete.")]
        public int requiredDaysToWait = 1;

        [Tooltip("WaitingQuest only — how many dives must happen on the day the wait finishes (capped by DayManager.MaxDivesPerDay — don't set this higher).")]
        public int requiredDivesThatDay = 1;

        public int targetCount = 1;
        public QuestCompletionMode completionMode = QuestCompletionMode.AutoComplete;
        [TextArea(2, 4)] public string stepDescription;

        [Tooltip("Collected by (character id, dialogue type). For a Talk step, leave characterId blank to default to Target NPC Id.")]
        public List<QuestDialogueLine> dialogue = new();

        [Tooltip("Marks this checkpoint as a guided-sequence step — TutorialPromptUI shows/gates it (even a Talk step), not just literal Tutorial-typed ones.")]
        public bool isGuidedSequence;

        [Tooltip("Talk only — ON: the line shows by itself the moment this step becomes current (self-talk, no NPC to walk up to; Target Id is just the key its dialogue lines are stored under) and one Interact press completes it. OFF: the player must walk up to the Target NPC and talk to them.")]
        public bool autoDisplayDialogue;

        [Tooltip("Talk (non-auto) only — AnyOne: talking to any single listed NPC completes the step. All: every listed NPC must be talked to before it completes.")]
        public TalkCompletionMode talkCompletionMode = TalkCompletionMode.AnyOne;

        [Tooltip("Optional image(s) for this checkpoint's guide notification. The banner shows the first entry; leave empty for a text-only notification.")]
        public List<Sprite> notificationImages = new();

        public string rewardItemId;
        public int rewardMoney;

        [Tooltip("Feature(s) unlocked when this checkpoint completes — see FeatureUnlockService.")]
        public List<GameFeature> unlocksFeatures = new();
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

        [Tooltip("Optional — npcId / CharacterDatabase id of the NPC who gave this quest; drives the tracker/accepted-quest avatar. Leave blank for quest-board quests (no avatar).")]
        public string questGiverNpcId;
    }
}