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
        Tutorial,
        // Added at the end — existing authored steps are serialized by enum index.
        Cutscene
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

        /// <summary>KillCount only — kills of any of these ids count toward TargetCount.</summary>
        public List<string> TargetEnemyIds;
        public TutorialActionId TutorialAction;
        public string TargetQuestItemId;

        /// <summary>Delivery only — every id here is removed (TargetCount each) on turn-in.</summary>
        public List<string> TargetQuestItemIds;
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

        /// <summary>Cutscene steps reuse TargetEnemyId as the Cutscene Id to play — this is
        /// that value, named for what it actually is at this step type.</summary>
        public string EffectiveCutsceneId => Type == QuestType.Cutscene ? TargetEnemyId : null;

        /// <summary>Authored on QuestStepData — marks THIS checkpoint as guided, so TutorialPromptUI shows/gates it.</summary>
        public bool IsGuidedSequence;

        /// <summary>Optional images for this checkpoint's guide notification.</summary>
        public List<Sprite> NotificationImages;

        // ponytail: the notification banner only shows one image — the first in the list.
        // If a step ever needs to cycle through several, this is the place to extend.
        public Sprite NotificationImage =>
            NotificationImages != null && NotificationImages.Count > 0 ? NotificationImages[0] : null;

        private Dictionary<(string characterId, DialogueType type), string[]> dialogue;
        private Dictionary<(string characterId, DialogueType type), string> cutsceneIds;

        public string RewardItemId;
        public int RewardMoney;

        /// <summary>Feature(s) unlocked when this checkpoint completes — see FeatureUnlockService.</summary>
        public List<GameFeature> UnlocksFeatures;

        public bool IsComplete => CurrentCount >= TargetCount;

        public void SetDialogue(List<QuestDialogueLine> lines)
        {
            dialogue = new Dictionary<(string, DialogueType), string[]>();
            cutsceneIds = new Dictionary<(string, DialogueType), string>();
            if (lines == null)
                return;

            foreach (var line in lines)
            {
                var key = (line.characterId ?? "", line.type);
                dialogue[key] = line.lines ?? System.Array.Empty<string>();
                if (!string.IsNullOrEmpty(line.cutsceneId))
                    cutsceneIds[key] = line.cutsceneId;
            }
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

        /// <summary>Cutscene id authored on this (characterId, type) dialogue entry, if any — see QuestDialogueLine.cutsceneId.</summary>
        public string GetCutsceneId(string characterId, DialogueType type) =>
            cutsceneIds != null && cutsceneIds.TryGetValue((characterId ?? "", type), out var id) ? id : null;

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

        // Sourced from the current checkpoint — no quest-level NPC data. Only Talk steps
        // have one (see QuestStep.EffectiveNpcId); other step types resolve to null.
        public string NpcId => CurrentStep?.EffectiveNpcId;

        // Sourced from the current checkpoint — was quest-level (QuestData.questItemId),
        // now per-step so different checkpoints of the same quest can target different items.
        public string TargetQuestItemId => CurrentStep?.TargetQuestItemId;

        /// <summary>Delivery only — see QuestStep.TargetQuestItemIds.</summary>
        public List<string> TargetQuestItemIds => CurrentStep?.TargetQuestItemIds;

        public Quest(QuestData questData)
        {
            questId = questData.questId;
            category = questData.category;

            classification = questData.classification;
            status = questData.status;

            questName = questData.questName;

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
                // Legacy KillCount/Delivery steps authored before these became lists only have
                // the singular id set — fall back to that as a one-entry list so old .asset
                // data keeps working without hand-editing every quest.
                var enemyIds = s.targetEnemyIds != null && s.targetEnemyIds.Count > 0
                    ? s.targetEnemyIds
                    : (string.IsNullOrEmpty(s.targetEnemyId) ? new List<string>() : new List<string> { s.targetEnemyId });
                var questItemIds = s.targetQuestItemIds != null && s.targetQuestItemIds.Count > 0
                    ? s.targetQuestItemIds
                    : (string.IsNullOrEmpty(s.targetQuestItemId) ? new List<string>() : new List<string> { s.targetQuestItemId });

                var step = new QuestStep
                {
                    Type = s.type,
                    TargetEnemyId = s.targetEnemyId,
                    TargetEnemyIds = enemyIds,
                    TutorialAction = s.tutorialAction,
                    TargetQuestItemId = s.targetQuestItemId,
                    TargetQuestItemIds = questItemIds,
                    TargetCount = s.targetCount,
                    CompletionMode = s.completionMode,
                    StepDescription = s.stepDescription,
                    CurrentCount = 0,
                    IsGuidedSequence = s.isGuidedSequence,
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