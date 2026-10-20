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
        // Completes after RequiredDaysToWait days and RequiredDivesThatDay dives. Not a Tutorial step, because
        // Tutorial locks input to one action, which is wrong for "come back after you sleep and dive".
        WaitingQuest
    }

    /// <summary>Delivery only: InventoryHeld re-checks held items live; EnemyKills counts kill events.</summary>
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

        // Step-level fields (type, targets) are read through CurrentStep, so call sites show they belong to the current step.
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

        // Guided is per step, so a quest can have both guided and unguided steps.
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

        // Per step, so different steps of one quest can target different items.
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
                // Old steps only set the single id; treat it as a one-item list.
                var enemyIds = s.targetEnemyIds != null && s.targetEnemyIds.Count > 0
                    ? s.targetEnemyIds
                    : (string.IsNullOrEmpty(s.targetEnemyId) ? new List<string>() : new List<string> { s.targetEnemyId });
                var questItemIds = s.targetQuestItemIds != null && s.targetQuestItemIds.Count > 0
                    ? s.targetQuestItemIds
                    : (string.IsNullOrEmpty(s.targetQuestItemId) ? new List<string>() : new List<string> { s.targetQuestItemId });

                // Only PerItem reads this; steps without requirements get one per id at the step's Target Count.
                var itemRequirements = s.targetQuestItemRequirements != null && s.targetQuestItemRequirements.Count > 0
                    ? s.targetQuestItemRequirements
                    : questItemIds.ConvertAll(id => new QuestItemRequirement { itemId = id, count = s.targetCount });

                // PerItem and Talk-All derive the target from their list length. Never below 1: a 0-target step counts as
                // complete at once, AddProgress skips it, and the quest sticks there forever.
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
                };
                step.SetDialogue(s.dialogue);
                list.Add(step);
            }

            if (list.Count == 0)
                Debug.LogWarning($"[Quest] '{questData.name}' has no Steps authored — it will have no objective.");

            return list;
        }

        /// <summary>Adds progress to the current step; true only when the last step completes the quest.</summary>
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

        /// <summary>Steps already completed; used to rebuild feature unlocks for older saves.</summary>
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