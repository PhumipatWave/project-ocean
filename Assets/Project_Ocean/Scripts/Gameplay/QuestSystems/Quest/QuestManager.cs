using System.Collections.Generic;
using System;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class QuestManager : Singleton<QuestManager>, ISaveParticipant
    {
        public static event Action OnQuestUpdated;
        /// <summary>Raised when a quest (or an auto-complete step) finishes and the complete popup should show.</summary>
        public static event Action<Quest> OnQuestCompleted;

        [Header("All Available Quests in Game")]
        [SerializeField] private List<QuestData> allQuestTemplates;

        private int trackedQuestId = -1;

        // The runtime list of quests, already converted from data and ready to use.
        private List<Quest> activeQuestList = new List<Quest>();

        private QuestProgressTracker progress;

        protected override bool Persistent => true;

        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicate) return;

            InitializeQuests();
            progress = new QuestProgressTracker(activeQuestList, CompleteProgress, RaiseQuestUpdated);
        }

        private void OnEnable() => progress?.Subscribe();   // null on a duplicate destroyed in Awake

        private void OnDisable() => progress?.Unsubscribe();

        private static void RaiseQuestUpdated() => OnQuestUpdated?.Invoke();

        /// <summary>Rebuilds every quest from scratch for a New Game.</summary>
        public void ResetToDefault()
        {
            trackedQuestId = -1;
            InitializeQuests();
            OnQuestUpdated?.Invoke();
        }

        // Converts each ScriptableObject (QuestData) into a runtime Quest.
        private void InitializeQuests()
        {
            activeQuestList.Clear();
            var seenIds = new HashSet<int>();

            foreach (var data in allQuestTemplates)
            {
                if (data == null)
                    continue;

                var quest = new Quest(data);
                // Save/load matches quests by id — a duplicate would load another quest's progress.
                if (!seenIds.Add(quest.QuestID))
                {
                    Debug.LogError($"[QuestManager] Duplicate quest id {quest.QuestID} ('{data.name}') in All Quest Templates — skipped. Give each quest a unique id.", this);
                    continue;
                }

                activeQuestList.Add(quest);
            }

            // A quest authored to start InProgress never goes through AcceptQuest, so track it here.
            EnsureQuestTracked();

            GameLog.Info($"[QuestManager] Initialized {activeQuestList.Count} quests.");
        }

        /// <summary>Tracks the active Main quest (else any active quest) when nothing valid is tracked; never overrides a valid choice.</summary>
        private void EnsureQuestTracked()
        {
            if (GetTrackedQuest() != null)
                return;

            Quest fallback = null;
            foreach (var q in activeQuestList)
            {
                if (q.Status != QuestStatus.InProgress && q.Status != QuestStatus.ReadyToTurnIn)
                    continue;

                if (q.Classification == QuestClassification.MainQuest)
                {
                    trackedQuestId = q.QuestID;
                    GameLog.Info($"[QuestManager] Now tracking Main Quest: {q.QuestName}");
                    return;
                }

                fallback ??= q;
            }

            trackedQuestId = fallback?.QuestID ?? -1;
            if (fallback != null)
                GameLog.Info($"[QuestManager] Now tracking Quest: {fallback.QuestName}");
        }

        /// <summary>Completes an Auto Display Dialogue step, which has no NPC id for the talk event to match.</summary>
        public void CompleteAutoDialogue(Quest q)
        {
            var step = q?.CurrentStep;
            if (q != null && q.Status == QuestStatus.InProgress && step != null &&
                (step.Type == QuestType.Talk || step.Type == QuestType.Cutscene) && step.AutoDisplayDialogue)
                CompleteProgress(q);
        }

        /// <summary>Advances a quest; each step pays its own reward when it completes.</summary>
        // A final ReadyToHandIn step pays at TurnInQuest instead, so it isn't paid twice.
        private void CompleteProgress(Quest q, int amount = 1)
        {
            var completedStep = q.CurrentStep;

            bool justCompleted = q.AddProgress(amount);

            OnQuestUpdated?.Invoke();

            // Reliably fires exactly once — once complete, this step is no longer
            // q.CurrentStep, so TryProgress stops routing further matching events to it.
            if (completedStep.IsComplete)
            {
                QuestStepEffects.ApplyFeatureChanges(completedStep);

                // Consume per step; a final ReadyToHandIn step consumes in TurnInQuest instead.
                if (!(justCompleted && completedStep.CompletionMode != QuestCompletionMode.AutoComplete))
                    QuestStepEffects.RemoveQuestItems(completedStep);
            }

            // Moved on to a new checkpoint (as opposed to just ticking progress on the
            // same one, or finishing the quest outright) — announce its description.
            if (!justCompleted && q.CurrentStep != completedStep)
                NotifyStepDescription(q);

            if (justCompleted)
            {
                if (completedStep.CompletionMode == QuestCompletionMode.AutoComplete)
                {
                    OnQuestCompleted?.Invoke(q);

                    if (completedStep.RewardMoney > 0)
                        RewardEvents.RaiseMoneyRewarded(completedStep.RewardMoney);

                    HandleNextQuest(q);
                }
                // else ReadyToHandIn: reward + next-quest handoff happen in TurnInQuest.
            }
            else if (completedStep.IsComplete && completedStep.RewardMoney > 0)
            {
                RewardEvents.RaiseMoneyRewarded(completedStep.RewardMoney);
            }

            EnsureQuestTracked();

            // The player may already hold enough for a new InventoryHeld step, and no inventory change will trigger a check.
            if (q.CurrentStep?.Type == QuestType.Delivery && q.CurrentStep?.DeliverySource == DeliverySource.InventoryHeld)
                progress.CheckInventory();

            // Same idea for WaitingQuest — stamp its start day now, and check right away in
            // case DayManager already satisfies it (e.g. RequiredDaysToWait is 0).
            if (q.CurrentStep?.Type == QuestType.WaitingQuest)
            {
                QuestStepEffects.StampWaitingStart(q.CurrentStep);
                progress.CheckDayOrDive();
            }
        }

        private const float StepDescriptionNotifyDelay = 1.5f;
        private Coroutine stepDescriptionRoutine;

        /// <summary>Toasts the current step's description after a short delay, cancelling any stale pending toast.</summary>
        private void NotifyStepDescription(Quest q)
        {
            if (stepDescriptionRoutine != null)
                StopCoroutine(stepDescriptionRoutine);

            stepDescriptionRoutine = StartCoroutine(NotifyStepDescriptionDelayed(q));
        }

        private System.Collections.IEnumerator NotifyStepDescriptionDelayed(Quest q)
        {
            yield return new WaitForSeconds(StepDescriptionNotifyDelay);
            stepDescriptionRoutine = null;

            // Re-read now: the quest may have advanced during the delay.
            // Guided steps are shown by TutorialPromptUI; skip them to avoid a duplicate.
            if (!string.IsNullOrEmpty(q.StepDescription) && !q.IsGuidedSequence)
                NotificationManager.Notify(q.StepDescription, NotificationChannel.Center);
        }

        /// <summary>Auto-accepts the next quests when NextQuestMode is AutoAccept; unlocking needs nothing, IsUnlocked is live.</summary>
        private void HandleNextQuest(Quest q)
        {
            if (q.NextQuestMode != NextQuestMode.AutoAccept)
                return;

            foreach (var id in q.NextQuestIds)
                AcceptQuest(id);
        }

        /// <summary>Locked while any quest pointing to it via NextQuestIds hasn't finished; rejecting that quest doesn't unlock it.</summary>
        public bool IsUnlocked(Quest quest)
        {
            return !activeQuestList.Exists(other =>
                other.NextQuestMode != NextQuestMode.None &&
                other.NextQuestIds.Contains(quest.QuestID) &&
                other.Status != QuestStatus.Finish);
        }

        /// <summary>Every quest regardless of status — for FeatureUnlockService's one-time-style save migration.</summary>
        public List<Quest> GetAllQuests() => activeQuestList;

        // Returns every quest with the given status (e.g. New quests not yet accepted).
        public List<Quest> GetQuestsByStatus(QuestStatus status)
        {
            return activeQuestList.FindAll(q => q.Status == status);
        }

        public List<Quest> GetQuestsByCategory(QuestCategory category)
        {
            var quests = activeQuestList.FindAll(q =>
                q.Category == category &&
                q.Status == QuestStatus.New &&
                IsUnlocked(q)
            );

            // MainQuest before SubQuest before Special — matches enum declaration order.
            quests.Sort((a, b) => a.Classification.CompareTo(b.Classification));
            return quests;
        }
        
        /// <summary>Accepted and not yet finished — shared by every "is this quest active" query below.</summary>
        private static bool IsActive(Quest q) =>
            q.Status == QuestStatus.InProgress || q.Status == QuestStatus.ReadyToTurnIn;

        public List<Quest> GetActiveQuests() => activeQuestList.FindAll(IsActive);

        public Quest GetQuestById(int id)
        {
            return activeQuestList.Find(q => q.QuestID == id);
        }

        /// <summary>The active Main quest whose current Talk step targets this NPC; main-story dialogue beats the NPC's own quest.</summary>
        public Quest GetMainQuestTalkQuest(string npcId)
        {
            if (string.IsNullOrEmpty(npcId))
                return null;

            return activeQuestList.Find(q =>
                q.Status == QuestStatus.InProgress &&
                q.Classification == QuestClassification.MainQuest &&
                q.CurrentStep != null &&
                q.CurrentStep.EffectiveNpcIds.Contains(npcId));
        }

        /// <summary>Collects the mutable runtime state (id, status, kill progress) of each quest.</summary>
        public List<QuestSaveData> GetSaveState()
        {
            return QuestSaveMapper.ToSaveData(activeQuestList);
        }

        public List<Quest> GetCompletedQuests()
        {
            return activeQuestList.FindAll(
                q => q.Status == QuestStatus.Finish
            );
        }

        /// <summary>Restores quest status and progress from saved data.</summary>
        public void LoadSaveState(
            List<QuestSaveData> saved)
        {
            if (saved == null)
                return;

            QuestSaveMapper.Apply(saved, GetQuestById);

            EnsureQuestTracked();

            OnQuestUpdated?.Invoke();

            // A loaded save can already satisfy a Delivery step; one check covers every InProgress quest.
            progress.CheckInventory();

            // Older saves have no start day for WaitingQuest steps; this stamps the current day as a fallback.
            progress.CheckDayOrDive();
        }

        public void Collect(SaveData data)
        {
            data.quests = GetSaveState();
        }

        public void Apply(SaveData data)
        {
            LoadSaveState(data.quests);
        }

        public bool AcceptQuest(int questID)
        {
            Quest quest = GetQuestById(questID);

            if (quest == null)
            {
                Debug.LogWarning(
                    $"Quest ID {questID} not found."
                );

                return false;
            }

            // Only a quest that hasn't been accepted yet can be accepted.
            if (quest.Status != QuestStatus.New)
            {
                Debug.LogWarning(
                    $"Quest {quest.QuestName} cannot be accepted. " +
                    $"Current status: {quest.Status}"
                );

                return false;
            }

            // Track the new quest, unless that would take tracking away from the Main quest: the compass,
            // markers and HUD follow it, so a Sub quest taken mid-story would cut off the story's guidance.
            var tracked = GetTrackedQuest();
            bool keepMainTracked = tracked != null &&
                tracked.Classification == QuestClassification.MainQuest &&
                quest.Classification != QuestClassification.MainQuest;
            if (!keepMainTracked)
                trackedQuestId = questID;

            quest.Status = QuestStatus.InProgress;

            NotifyStepDescription(quest);

            OnQuestUpdated?.Invoke();

            // First step could already be a satisfied InventoryHeld Delivery (items collected
            // before ever accepting) — nothing else would trigger a check for that.
            if (quest.CurrentStep?.Type == QuestType.Delivery && quest.CurrentStep?.DeliverySource == DeliverySource.InventoryHeld)
                progress.CheckInventory();

            if (quest.CurrentStep?.Type == QuestType.WaitingQuest)
            {
                QuestStepEffects.StampWaitingStart(quest.CurrentStep);
                progress.CheckDayOrDive();
            }

            GameLog.Info($"Accepted Quest: {quest.QuestName}");

            return true;
        }

        public bool RejectQuest(int questID)
        {
            Quest quest = GetQuestById(questID);

            if (quest == null)
                return false;

            if (quest.Status != QuestStatus.New)
                return false;

            quest.Status = QuestStatus.Reject;

            OnQuestUpdated?.Invoke();

            return true;
        }

        public bool CancelQuest(int questID)
        {
            Quest quest = GetQuestById(questID);

            if (quest == null)
                return false;

            if (quest.Status != QuestStatus.InProgress &&
                quest.Status != QuestStatus.ReadyToTurnIn)
                return false;

            // Main quests are meant to always be running — cancelling isn't allowed.
            if (quest.Classification == QuestClassification.MainQuest)
            {
                Debug.LogWarning($"Quest {quest.QuestName} is a Main Quest and cannot be cancelled.");
                return false;
            }

            // Reset the cancelled quest back to New.
            quest.Status = QuestStatus.New;
            quest.CurrentCount = 0;

            // If the cancelled quest was the tracked one, find another quest to track instead.
            if (trackedQuestId == questID)
                trackedQuestId = -1;

            EnsureQuestTracked();

            OnQuestUpdated?.Invoke();

            return true;
        }

        public bool HasActiveQuest() => activeQuestList.Exists(IsActive);

        public Quest GetCurrentActiveQuest() => activeQuestList.Find(IsActive);

        /// <summary>Turns in a ReadyToTurnIn quest; any other status returns false.</summary>
        public bool TurnInQuest(int questID)
        {
            Quest quest = GetQuestById(questID);

            if (quest == null)
                return false;

            if (quest.Status != QuestStatus.ReadyToTurnIn)
                return false;

            // The loot or money may have been sold/spent since the step became ready.
            if (!QuestStepEffects.RequirementsMet(quest.CurrentStep))
                return false;

            quest.Status = QuestStatus.Finish;

            QuestStepEffects.RemoveQuestItems(quest.CurrentStep);

            
            OnQuestCompleted?.Invoke(quest);

            var finalStepReward = quest.CurrentStep.RewardMoney;
            if (finalStepReward > 0)
            {
                RewardEvents.RaiseMoneyRewarded(finalStepReward);
                GameLog.Info($"[QuestManager] Completing quests: +{finalStepReward} $");
            }

            HandleNextQuest(quest);

            EnsureQuestTracked();

            OnQuestUpdated?.Invoke();

            return true;
        }

        public Quest GetTrackedQuest()
        {
            if (trackedQuestId == -1)
                return null;

            Quest quest =
                GetQuestById(trackedQuestId);

            if (quest == null)
                return null;

            if (quest.Status != QuestStatus.InProgress &&
                quest.Status != QuestStatus.ReadyToTurnIn)
                return null;

            return quest;
        }

        /// <summary>Dev only: completes the tracked step through the normal pipeline, even if its requirement isn't met.</summary>
        public void DevSkipCurrentStep()
        {
            var q = GetTrackedQuest();
            var step = q?.CurrentStep;
            if (q == null || q.Status != QuestStatus.InProgress || step == null) return;

            CompleteProgress(q, step.TargetCount - step.CurrentCount);
        }

        public bool SetTrackedQuest(int questID)
        {
            Quest quest =
                GetQuestById(questID);

            if (quest == null)
                return false;

            if (quest.Status != QuestStatus.InProgress &&
                quest.Status != QuestStatus.ReadyToTurnIn)
                return false;

            trackedQuestId = questID;

            OnQuestUpdated?.Invoke();

            GameLog.Info(
                $"Tracking Quest: {quest.QuestName}"
            );

            return true;
        }
    }
}