using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public enum QuestType
    {
        Delivery,
        KillCount,
        ExploreArea,
        Talk,
        Tutorial
    }

    public enum QuestStatus
    {
        New,
        InProgress,
        Finish,
        ReadyToTurnIn,
        Reject
    }

    /// <summary>
    /// Runtime copy of one QuestStepData checkpoint, with its own live counter. Each step
    /// is self-contained: its own in-progress/turn-in dialogue and its own reward, paid
    /// when THIS step completes.
    /// </summary>
    public class QuestStep
    {
        public QuestType Type;
        public string TargetEnemyId;
        public TutorialActionId TutorialAction;
        public int TargetCount;
        public QuestCompletionMode CompletionMode;
        public string StepDescription;
        public int CurrentCount;

        /// <summary>
        /// Default speaker for context-free dialogue lookups (board/tracker avatar,
        /// QuestDetailPanel's Accept/Reject). Only Talk steps have one — it's just
        /// TargetEnemyId (who you're talking to), not a separately-authored id. Other
        /// step types (KillCount, etc.) have no default speaker.
        /// </summary>
        public string EffectiveNpcId => Type == QuestType.Talk ? TargetEnemyId : null;

        /// <summary>Authored on QuestStepData — marks THIS checkpoint as guided, so TutorialPromptUI shows/gates it.</summary>
        public bool IsGuidedSequence;

        private Dictionary<(string characterId, DialogueType type), string[]> dialogue;

        public string RewardItemId;
        public int RewardMoney;

        public bool IsComplete => CurrentCount >= TargetCount;

        public void SetDialogue(List<QuestDialogueLine> lines)
        {
            dialogue = new Dictionary<(string, DialogueType), string[]>();
            if (lines == null)
                return;

            foreach (var line in lines)
                dialogue[(line.characterId ?? "", line.type)] = line.lines ?? System.Array.Empty<string>();
        }

        /// <summary>All lines for one character + dialogue type. Empty array if none authored.</summary>
        public string[] GetLines(string characterId, DialogueType type) =>
            dialogue != null && dialogue.TryGetValue((characterId ?? "", type), out var lines)
                ? lines
                : System.Array.Empty<string>();

        public string GetLine(string characterId, DialogueType type, int index = 0)
        {
            var lines = GetLines(characterId, type);
            return index >= 0 && index < lines.Length ? lines[index] : string.Empty;
        }

        // Convenience for consumers that don't track their own character id (e.g. QuestDetailPanel) —
        // resolves against EffectiveNpcId (Talk steps only).
        public string[] QuestDialogue => GetLines(EffectiveNpcId, DialogueType.Offer);
        public string QuestionInProgress => GetLine(EffectiveNpcId, DialogueType.InProgress);
        public string AnswerFinish => GetLine(EffectiveNpcId, DialogueType.Finish);
        public string AnswerNotFinish => GetLine(EffectiveNpcId, DialogueType.NotFinish);
        public string AnswerAccept => GetLine(EffectiveNpcId, DialogueType.Accept);
        public string AnswerReject => GetLine(EffectiveNpcId, DialogueType.Reject);
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
        // can be guided independently (e.g. a Tutorial step guided, a later KillCount
        // step not).
        public bool IsGuidedSequence => CurrentStep?.IsGuidedSequence ?? false;

        /// <summary>What to do to NextQuestIds' quests when this one reaches Finish.</summary>
        public NextQuestMode NextQuestMode { get; private set; }
        public List<int> NextQuestIds { get; private set; }

        [SerializeField]
        private string questName;
        public string QuestName => questName;

        [SerializeField]
        private string questItemId;
        public string QuestItemId => questItemId;

        // Sourced from the current checkpoint — no quest-level NPC data. Only Talk steps
        // have one (see QuestStep.EffectiveNpcId); other step types resolve to null.
        public string NpcId => CurrentStep?.EffectiveNpcId;

        public Quest(QuestData questData)
        {
            questId = questData.questId;

            // สำคัญ
            category = questData.category;

            classification = questData.classification;
            status = questData.status;

            questName = questData.questName;
            questItemId = questData.questItemId;

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
                var step = new QuestStep
                {
                    Type = s.type,
                    TargetEnemyId = s.targetEnemyId,
                    TutorialAction = s.tutorialAction,
                    TargetCount = s.targetCount,
                    CompletionMode = s.completionMode,
                    StepDescription = s.stepDescription,
                    CurrentCount = 0,
                    IsGuidedSequence = s.isGuidedSequence,
                    RewardItemId = s.rewardItemId,
                    RewardMoney = s.rewardMoney,
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