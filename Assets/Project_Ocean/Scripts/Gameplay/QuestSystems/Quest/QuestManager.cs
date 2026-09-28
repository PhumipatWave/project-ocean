using System.Collections.Generic;
using System;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class QuestManager : MonoBehaviour, ISaveParticipant
    {
        public static QuestManager Instance { get; private set; }
        public static event Action OnQuestUpdated;
        /// <summary>Raised when a quest (or an auto-complete step) finishes and the complete popup should show.</summary>
        public static event Action<Quest> OnQuestCompleted;

        [Header("All Available Quests in Game")]
        [SerializeField] private List<QuestData> allQuestTemplates;

        private int trackedQuestId = -1;

        // The runtime list of quests, already converted from data and ready to use.
        private List<Quest> activeQuestList = new List<Quest>();

        private QuestProgressTracker progress;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeQuests();
            progress = new QuestProgressTracker(activeQuestList, CompleteProgress, RaiseQuestUpdated);
        }

        private void OnEnable() => progress?.Subscribe();   // null on a duplicate destroyed in Awake

        private void OnDisable() => progress?.Unsubscribe();

        private static void RaiseQuestUpdated() => OnQuestUpdated?.Invoke();

        /// <summary>Rebuilds every quest from scratch, as if the game had just started —
        /// called by SaveBridge.ResetForNewGame() so "New Game" doesn't inherit a previous
        /// playthrough's progress.</summary>
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

            // A quest authored to start already InProgress (e.g. an intro tutorial) never
            // goes through AcceptQuest — pick up the active Main quest (or any active
            // quest) so it's tracked from the very start.
            EnsureQuestTracked();

            Debug.Log($"[QuestManager] Initialized {activeQuestList.Count} quests.");
        }

        /// <summary>
        /// Re-syncs the tracked quest whenever nothing valid is currently tracked. The
        /// active Main quest wins if there is one — so it "stays all the time," auto-tracked
        /// at game start and automatically re-tracked across a quest chain's hand-offs —
        /// otherwise falls back to any other active quest. Never overrides an already-valid
        /// tracked quest, so a player's explicit choice to track a Sub quest is left alone.
        /// </summary>
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
                    Debug.Log($"[QuestManager] Now tracking Main Quest: {q.QuestName}");
                    return;
                }

                fallback ??= q;
            }

            trackedQuestId = fallback?.QuestID ?? -1;
            if (fallback != null)
                Debug.Log($"[QuestManager] Now tracking Quest: {fallback.QuestName}");
        }

        /// <summary>Completes a Talk or Cutscene step authored with Auto Display Dialogue — it
        /// has no NPC id for NpcEvents.RaiseNpcTalkedTo to match on, so PlayerSelfDialogueUI
        /// calls this directly.</summary>
        public void CompleteAutoDialogue(Quest q)
        {
            var step = q?.CurrentStep;
            if (q != null && q.Status == QuestStatus.InProgress && step != null &&
                (step.Type == QuestType.Talk || step.Type == QuestType.Cutscene) && step.AutoDisplayDialogue)
                CompleteProgress(q);
        }

        /// <summary>
        /// Each checkpoint pays its own reward the moment IT completes — not just the
        /// quest's final step — so a quest can pay out incrementally per checkpoint.
        /// Exception: a final step set to ReadyToHandIn pays nothing here — its reward
        /// waits for TurnInQuest, same as before, so it isn't paid twice. amount is 1 for
        /// every discrete-event step type (one kill, one talk, ...); Delivery's live
        /// inventory check passes whatever delta finishes it — see QuestProgressTracker.CheckInventory,
        /// which never calls this for a delta that DOESN'T finish the step, since the
        /// partial-progress branch below would otherwise pay reward money mid-collection.
        /// </summary>
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

                // Consume per step, not per quest — a mid-quest Delivery step (e.g. MQ_1003's
                // "Find materials") used to never take its items. A final ReadyToHandIn step
                // consumes in TurnInQuest instead.
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
                        OnMoneyRewarded?.Invoke(completedStep.RewardMoney);

                    HandleNextQuest(q);
                }
                // else ReadyToHandIn: reward + next-quest handoff happen in TurnInQuest.
            }
            else if (completedStep.IsComplete && completedStep.RewardMoney > 0)
            {
                OnMoneyRewarded?.Invoke(completedStep.RewardMoney);
            }

            EnsureQuestTracked();

            // A step just became current (this one, or the next one it advanced into) — if
            // it's an InventoryHeld Delivery step, the player may already be holding enough
            // (e.g. it was an EnemyKills bounty's own loot, collected before this checkpoint
            // was even reached). Inventory hasn't changed, so nothing would otherwise trigger
            // QuestProgressTracker.CheckInventory.
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

        /// <summary>
        /// Toasts the current checkpoint's Step Description after a short delay. Cancels any
        /// still-pending call first — advancing two steps within the delay window (e.g.
        /// dismissing one Talk step and immediately completing the next) would otherwise leave
        /// a stale toast queued for a step that isn't current anymore.
        /// </summary>
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

        /// <summary>
        /// If this quest's own NextQuestMode is AutoAccept, immediately accept every
        /// quest in its NextQuestIds. Unlock needs no action here — IsUnlocked already
        /// re-evaluates live off this quest's Status, so each successor just stops
        /// being locked.
        /// </summary>
        private void HandleNextQuest(Quest q)
        {
            if (q.NextQuestMode != NextQuestMode.AutoAccept)
                return;

            foreach (var id in q.NextQuestIds)
                AcceptQuest(id);
        }

        /// <summary>
        /// A quest is locked if some OTHER quest's NextQuestIds contains it (with mode
        /// Unlock or AutoAccept) and that other quest hasn't reached Finish yet. A quest
        /// nothing points to is always unlocked. Rejecting the prerequisite does NOT
        /// unlock the successor — only an actual Finish counts.
        /// </summary>
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

        /// <summary>
        /// The active quest (if any) whose CURRENT checkpoint is a Main-quest Talk step
        /// targeting this NPC. A Main quest takes priority over whatever quest the NPC is
        /// otherwise assigned to — main-story dialogue always wins over an NPC's own Sub
        /// quest — independent of IsGuidedSequence, which is purely about the tutorial
        /// prompt/input-lock (see TutorialPromptUI), not dialogue selection.
        /// </summary>
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

        // ── Save support ──

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

        /// <summary>
        /// Restores quest status + kill progress from saved data.
        /// </summary>
        public void LoadSaveState(
            List<QuestSaveData> saved)
        {
            if (saved == null)
                return;

            QuestSaveMapper.Apply(saved, GetQuestById);

            EnsureQuestTracked();

            OnQuestUpdated?.Invoke();

            // A resumed save could land mid-Delivery-step already satisfied (or the player
            // adjusted their inventory between sessions) — QuestProgressTracker.CheckInventory re-checks
            // every InProgress quest itself, so one call covers all of them.
            progress.CheckInventory();

            // Same idea for WaitingQuest — old saves (or a step reached for the first time
            // this session) have no dayStepBecameCurrent yet; QuestStepEffects.StampWaitingStart inside
            // it fills that in from the live day as a reasonable fallback.
            progress.CheckDayOrDive();
        }

        // =========================================================
        // ISaveParticipant
        // =========================================================

        public void Collect(SaveData data)
        {
            data.quests = GetSaveState();
        }

        public void Apply(SaveData data)
        {
            LoadSaveState(data.quests);
        }

        // =========================================================
        // ACCEPT QUEST
        // =========================================================

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

            // Quest ที่รับใหม่จะถูก Track ทันที — except a non-Main quest never takes tracking away
            // from the Main quest that's already tracked: the compass, door/area markers and the HUD
            // tracker all follow the tracked quest, so a Sub quest taken off the board mid-story
            // (e.g. the AcceptQuest tutorial step) would otherwise cut off the story's guidance.
            // The player can still switch to it explicitly via SetTrackedQuest.
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

            Debug.Log($"Accepted Quest: {quest.QuestName}");

            return true;
        }

        // =========================================================
        // REJECT QUEST
        // =========================================================

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

        public static event Action<int> OnMoneyRewarded;

        /// <summary>
        /// Called by the NPC when the player clicks Turn In. Quest must be ReadyToTurnIn —
        /// any other state returns false with a warning.
        /// </summary>
        public bool TurnInQuest(int questID)
        {
            Quest quest = GetQuestById(questID);

            if (quest == null)
                return false;

            if (quest.Status != QuestStatus.ReadyToTurnIn)
                return false;

            quest.Status = QuestStatus.Finish;

            QuestStepEffects.RemoveQuestItems(quest.CurrentStep);

            // Notification เดิม
            //NotificationManager.Notify($"Quest complete: {quest.QuestName}");
            
            // แสดง Quest Complete UI + Reward
            OnQuestCompleted?.Invoke(quest);

            var finalStepReward = quest.CurrentStep.RewardMoney;
            if (finalStepReward > 0)
            {
                OnMoneyRewarded?.Invoke(finalStepReward);
                Debug.Log($"[QuestManager] Completing quests: +{finalStepReward} $");
            }

            HandleNextQuest(quest);

            EnsureQuestTracked();

            OnQuestUpdated?.Invoke();

            return true;
        }

        // =========================================================
        // TRACKED QUEST
        // =========================================================

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

        /// <summary>Dev/cheat only — forces the tracked quest's current step to complete
        /// through the normal CompleteProgress pipeline (rewards, item/money consumption,
        /// feature unlocks, HandleNextQuest all still run), regardless of whether its real
        /// requirement is actually met. See DevPanel.</summary>
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

            Debug.Log(
                $"Tracking Quest: {quest.QuestName}"
            );

            return true;
        }
    }
}