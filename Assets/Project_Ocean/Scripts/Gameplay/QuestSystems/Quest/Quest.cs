using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public enum QuestType
    {
        Delivery,
        KillCount,
        ExploreArea
    }

    public enum QuestStatus
    {
        New,
        InProgress,
        Finish,
        ReadyToTurnIn,
        Reject
    }

    /// <summary>Runtime copy of one QuestStepData checkpoint, with its own live counter.</summary>
    public class QuestStep
    {
        public QuestType Type;
        public string TargetEnemyId;
        public int TargetCount;
        public QuestCompletionMode CompletionMode;
        public string StepDescription;
        public int CurrentCount;

        public bool IsComplete => CurrentCount >= TargetCount;
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
        // Type/TargetEnemyId/TargetCount/CompletionMode/CurrentCount below all proxy
        // to CurrentStep, so every existing caller (QuestManager, UI) that reads a
        // "quest's" type/target/progress automatically reads its active checkpoint
        // without needing to know steps exist.
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

        public QuestType Type => CurrentStep.Type;
        public string TargetEnemyId => CurrentStep.TargetEnemyId;
        public int TargetCount => CurrentStep.TargetCount;
        public QuestCompletionMode CompletionMode => CurrentStep.CompletionMode;

        public int CurrentCount
        {
            get => CurrentStep.CurrentCount;
            set => CurrentStep.CurrentCount = value;
        }

        public string Progress => CurrentCount + " / " + TargetCount;

        public bool IsObjectiveComplete => CurrentStep != null && CurrentStep.IsComplete;

        [SerializeField]
        private string questName;
        public string QuestName => questName;

        [SerializeField]
        private string questDetail;
        public string QuestDetail => questDetail;

        [SerializeField]
        private string questItemId;
        public string QuestItemId => questItemId;

        [SerializeField]
        private string[] questDialogue;
        public string[] QuestDialogue => questDialogue;

        [SerializeField]
        private string[] answerNext;
        public string[] AnswerNext => answerNext;

        [SerializeField]
        private string answerAccept;
        public string AnswerAccept => answerAccept;

        [SerializeField]
        private string answerReject;
        public string AnswerReject => answerReject;

        [SerializeField]
        private string rewardItemId;
        public string RewardItemId => rewardItemId;

        [SerializeField]
        private int rewardExp;
        public int RewardExp => rewardExp;

        [SerializeField]
        private int rewardMoney;
        public int RewardMoney => rewardMoney;

        [SerializeField]
        private string questionInProgress;
        public string QuestionInProgress => questionInProgress;

        [SerializeField]
        private string answerFinish;
        public string AnswerFinish => answerFinish;

        [SerializeField]
        private string answerNotFinish;
        public string AnswerNotFinish => answerNotFinish;

        [Header("NPCName")]
        private string npcName;
        public string NpcName => npcName;

        private Sprite npcAvatar;
        public Sprite NpcAvatar => npcAvatar;
        public Sprite fishSprite;
        public Quest(QuestData questData)
        {
            questId = questData.questId;

            // สำคัญ
            category = questData.category;

            classification = questData.classification;
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
            fishSprite = questData.fishSprite;

            steps = BuildSteps(questData);
            currentStepIndex = 0;
        }

        /// <summary>
        /// Uses the authored Steps list if present; otherwise synthesizes a single
        /// step from the legacy flat type/targetEnemyId/targetCount/completionMode
        /// fields, so existing single-objective QuestData assets keep working unchanged.
        /// </summary>
        private static List<QuestStep> BuildSteps(QuestData questData)
        {
            var list = new List<QuestStep>();

            if (questData.steps != null && questData.steps.Count > 0)
            {
                foreach (var s in questData.steps)
                {
                    list.Add(new QuestStep
                    {
                        Type = s.type,
                        TargetEnemyId = s.targetEnemyId,
                        TargetCount = s.targetCount,
                        CompletionMode = s.completionMode,
                        StepDescription = s.stepDescription,
                        CurrentCount = 0,
                    });
                }
            }
            else
            {
                list.Add(new QuestStep
                {
                    Type = questData.type,
                    TargetEnemyId = questData.targetEnemyId,
                    TargetCount = questData.targetCount,
                    CompletionMode = questData.completionMode,
                    StepDescription = questData.questDetail,
                    CurrentCount = 0,
                });
            }

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