using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public enum QuestType
    {
        Delivery,
        ExploreArea,
        Talk,
        Tutorial,
        // Added at the end — existing authored steps are serialized by enum index.
        Cutscene,
        // Completes once DayManager reports at least RequiredDaysToWait days have passed
        // since this step became current AND at least RequiredDivesThatDay dives happened
        // on the current day — see QuestManager.HandleDayOrDiveChanged. Deliberately NOT a
        // Tutorial step: Tutorial forces TutorialPromptUI's guided/input-locked mode, which
        // is wrong for an ordinary "come back after you sleep and dive" checkpoint.
        WaitingQuest
    }

    /// <summary>Delivery only — what drives step progress. InventoryHeld: live-rechecked
    /// against inventory (QuestManager.HandleInventoryChanged), same as always. EnemyKills:
    /// one-shot kill events matching TargetEnemyIds (replaces the old, now-removed KillCount
    /// QuestType) — merged in here since it shared everything else with Delivery already.</summary>
    public enum DeliverySource
    {
        InventoryHeld,
        EnemyKills,
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
        private QuestClassification classification;
        public QuestClassification Classification => classification;

        [SerializeField]
        private QuestStatus status;
        public QuestStatus Status
        {
            get => status;
            set => status = value;
        }

        // ── Checkpoints ──
        // A quest is a sequence of steps; only the current one accumulates progress.
        // Type/TargetEnemyId/TutorialAction/CompletionMode are deliberately NOT exposed
        // here — read them via CurrentStep so it's obvious at the call site that they're
        // the active checkpoint's, not some fixed quest-level fact. TargetCount/CurrentCount
        // stay as quest-level conveniences since they're purely for progress display.
        private List<QuestStep> steps = new();

        [SerializeField]
        private int currentStepIndex;
        public int CurrentStepIndex
        {
            get => currentStepIndex;
            set => currentStepIndex = Mathf.Clamp(value, 0, Mathf.Max(0, steps.Count - 1));
        }

        public int StepCount => steps.Count;

        public QuestStep CurrentStep =>
            steps.Count == 0 ? null : steps[currentStepIndex];

        public string StepDescription => CurrentStep?.StepDescription;

        public int TargetCount => CurrentStep.TargetCount;

        public int CurrentCount
        {
            get => CurrentStep.CurrentCount;
            set => CurrentStep.CurrentCount = value;
        }

        public string Progress => CurrentCount + " / " + TargetCount;

        public bool IsObjectiveComplete => CurrentStep != null && CurrentStep.IsComplete;

        // Sourced from the current checkpoint — no quest-level guided flag. Each step
        // can be guided independently (e.g. a Tutorial step guided, a later Delivery
        // step not).
        public bool IsGuidedSequence => CurrentStep?.IsGuidedSequence ?? false;

        /// <summary>What to do to NextQuestIds' quests when this one reaches Finish.</summary>
        public NextQuestMode NextQuestMode { get; private set; }
        public List<int> NextQuestIds { get; private set; }

        [SerializeField]
        private string questName;
        public string QuestName => questName;

        // Sourced from the current checkpoint — no quest-level NPC data. Only Talk steps
        // have one (see QuestStep.EffectiveNpcId); other step types resolve to null.
        public string NpcId => CurrentStep?.EffectiveNpcId;

        // Quest-level giver (authored on QuestData), falling back to the Talk-step NPC.
        // Display-only (name/avatar) — NpcId above stays the dialogue/navigation speaker.
        private string questGiverNpcId;
        public string DisplayNpcId => string.IsNullOrEmpty(questGiverNpcId) ? NpcId : questGiverNpcId;

        // Sourced from the current checkpoint — was quest-level (QuestData.questItemId),
        // now per-step so different checkpoints of the same quest can target different items.
        public string TargetQuestItemId => CurrentStep?.TargetQuestItemId;

        /// <summary>Delivery + CombinedTotal only — see QuestStep.TargetQuestItemIds.</summary>
        public List<string> TargetQuestItemIds => CurrentStep?.TargetQuestItemIds;

        /// <summary>Delivery + PerItem only — see QuestStep.TargetQuestItemRequirements.</summary>
        public List<QuestItemRequirement> TargetQuestItemRequirements => CurrentStep?.TargetQuestItemRequirements;

        public Quest(QuestData questData)
        {
            questId = questData.questId;
            category = questData.category;

            classification = questData.classification;
            status = questData.status;

            questName = questData.questName;
            questGiverNpcId = questData.questGiverNpcId;

            NextQuestMode = questData.nextQuestMode;
            NextQuestIds = questData.nextQuestIds ?? new List<int>();

            steps = BuildSteps(questData);
            currentStepIndex = 0;
        }

        /// <summary>Builds the runtime step list from QuestData's authored Steps.</summary>
        private static List<QuestStep> BuildSteps(QuestData questData)
        {
            var list = new List<QuestStep>();

            if (questData.steps == null)
                return list;

            foreach (var s in questData.steps)
            {
                // Legacy Delivery steps authored before these became lists only have the
                // singular id set — fall back to that as a one-entry list so old .asset data
                // keeps working without hand-editing every quest.
                var enemyIds = s.targetEnemyIds != null && s.targetEnemyIds.Count > 0
                    ? s.targetEnemyIds
                    : (string.IsNullOrEmpty(s.targetEnemyId) ? new List<string>() : new List<string> { s.targetEnemyId });
                var questItemIds = s.targetQuestItemIds != null && s.targetQuestItemIds.Count > 0
                    ? s.targetQuestItemIds
                    : (string.IsNullOrEmpty(s.targetQuestItemId) ? new List<string>() : new List<string> { s.targetQuestItemId });

                // Only consulted when DeliveryCountMode is PerItem (see ComputeDeliveryProgress)
                // — CombinedTotal uses questItemIds above directly instead. Falls back to that
                // same plain id list, one requirement per id at the step's old shared Target
                // Count, for a step authored before PerItem/requirements existed.
                var itemRequirements = s.targetQuestItemRequirements != null && s.targetQuestItemRequirements.Count > 0
                    ? s.targetQuestItemRequirements
                    : questItemIds.ConvertAll(id => new QuestItemRequirement { itemId = id, count = s.targetCount });

                // PerItem's "how many required" is the requirement count, not an authored
                // Target Count — deriving it here keeps the Inspector from showing a field
                // that would just be redundant/easy to leave mismatched. Talk's All mode is
                // the same idea: "how many NPCs" is just the list length. Never below 1: a
                // 0-target step is "complete" the moment it starts, so AddProgress (which bails
                // on an already-complete step) can never advance past it — the quest sticks on
                // it forever. Talk/Tutorial/Cutscene steps hide this field in the Inspector, so
                // a freshly added step is easily left at 0.
                bool isInventoryHeldPerItem = s.type == QuestType.Delivery &&
                    s.deliverySource == DeliverySource.InventoryHeld &&
                    s.deliveryCountMode == DeliveryCountMode.PerItem;
                bool isTalkAll = s.type == QuestType.Talk && !s.autoDisplayDialogue &&
                    s.talkCompletionMode == TalkCompletionMode.All;
                int targetCount = Mathf.Max(1,
                    isInventoryHeldPerItem ? itemRequirements.Count :
                    isTalkAll ? enemyIds.Count :
                    s.targetCount);

                var step = new QuestStep
                {
                    Type = s.type,
                    TargetEnemyId = s.targetEnemyId,
                    TargetEnemyIds = enemyIds,
                    TutorialAction = s.tutorialAction,
                    TargetQuestItemId = s.targetQuestItemId,
                    TargetQuestItemIds = questItemIds,
                    TargetQuestItemRequirements = itemRequirements,
                    DeliveryCountMode = s.deliveryCountMode,
                    DeliverySource = s.deliverySource,
                    TalkCompletionMode = s.talkCompletionMode,
                    ConsumeOnComplete = s.consumeOnComplete,
                    RequiredMoney = s.requiredMoney,
                    RequiredDaysToWait = s.requiredDaysToWait,
                    RequiredDivesThatDay = s.requiredDivesThatDay,
                    TargetCount = targetCount,
                    CompletionMode = s.completionMode,
                    StepDescription = s.stepDescription,
                    CurrentCount = 0,
                    IsGuidedSequence = s.isGuidedSequence,
                    AutoDisplayDialogue = (s.type == QuestType.Talk || s.type == QuestType.Cutscene) && s.autoDisplayDialogue,
                    NotificationImages = s.notificationImages,
                    RewardItemId = s.rewardItemId,
                    RewardMoney = s.rewardMoney,
                    UnlocksFeatures = s.unlocksFeatures,
                    LocksFeatures = s.locksFeatures,
                };
                step.SetDialogue(s.dialogue);
                list.Add(step);
            }

            if (list.Count == 0)
                Debug.LogWarning($"[Quest] '{questData.name}' has no Steps authored — it will have no objective.");

            return list;
        }

        /// <summary>
        /// Adds progress to the current checkpoint only. Finishing a checkpoint that
        /// isn't the last one just advances to the next checkpoint (returns false —
        /// the quest overall isn't done). Finishing the last checkpoint flips the
        /// quest's status per that checkpoint's completionMode and returns true.
        /// </summary>
        public bool AddProgress(int amount = 1)
        {
            if (status != QuestStatus.InProgress)
                return false;

            var step = CurrentStep;
            if (step == null || step.IsComplete)
                return false;

            step.CurrentCount = Mathf.Min(step.CurrentCount + amount, step.TargetCount);

            if (!step.IsComplete)
                return false;

            if (currentStepIndex < steps.Count - 1)
            {
                currentStepIndex++;
                return false;
            }

            status = step.CompletionMode == QuestCompletionMode.AutoComplete
                ? QuestStatus.Finish
                : QuestStatus.ReadyToTurnIn;

            return true;
        }

        /// <summary>
        /// Steps that have definitely already completed — for one-time-style migration
        /// purposes only (FeatureUnlockService re-deriving unlocks for a save made before
        /// that feature existed). Progression is sequential and one-way, so every step
        /// before CurrentStepIndex is guaranteed done; the step AT CurrentStepIndex counts
        /// too once the quest has reached Finish/ReadyToTurnIn (that index IS the last step,
        /// and it's what put the quest into that status).
        /// </summary>
        public IEnumerable<QuestStep> GetCompletedSteps()
        {
            int completedCount = (status == QuestStatus.Finish || status == QuestStatus.ReadyToTurnIn)
                ? currentStepIndex + 1
                : currentStepIndex;

            for (int i = 0; i < completedCount && i < steps.Count; i++)
                yield return steps[i];
        }

        public string TrackerText
        {
            get
            {
                if (status == QuestStatus.ReadyToTurnIn)
                    return "READY TO TURN IN";

                return steps.Count > 1
                    ? $"Step {currentStepIndex + 1}/{steps.Count} — {Progress}"
                    : Progress;
            }
        }
    }
}