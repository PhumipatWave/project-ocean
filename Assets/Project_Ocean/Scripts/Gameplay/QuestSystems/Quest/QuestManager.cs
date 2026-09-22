using System.Collections.Generic;
using System;
using UnityEngine;

namespace Game.Testing
{
    public class QuestManager : MonoBehaviour, ISaveParticipant
    {
        public static QuestManager Instance { get; private set; }
        public static event Action OnQuestUpdated;

        [Header("All Available Quests in Game")]
        [SerializeField] private List<QuestData> allQuestTemplates;

        private int trackedQuestId = -1;

        // The runtime list of quests, already converted from data and ready to use.
        private List<Quest> activeQuestList = new List<Quest>();

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
        }

        private void OnEnable()
        {
            EnemyEvents.OnEnemyKilled += HandleEnemyKilled;
            TutorialEvents.OnActionPerformed += HandleTutorialAction;
            NpcEvents.OnNpcTalkedTo += HandleNpcTalk;
            CutsceneEvents.OnCutsceneFinished += HandleCutsceneFinished;
            ExploreEvents.OnAreaReached += HandleAreaReached;
            InventoryManager.OnInventoryChanged += HandleInventoryChanged;
            PlayerMoney.OnMoneyChanged += HandleMoneyChanged;
            DayManager.OnDayAdvanced += HandleDayOrDiveChanged;
            DayManager.OnDivedToday += HandleDayOrDiveChanged;
        }

        private void OnDisable()
        {
            EnemyEvents.OnEnemyKilled -= HandleEnemyKilled;
            TutorialEvents.OnActionPerformed -= HandleTutorialAction;
            NpcEvents.OnNpcTalkedTo -= HandleNpcTalk;
            CutsceneEvents.OnCutsceneFinished -= HandleCutsceneFinished;
            ExploreEvents.OnAreaReached -= HandleAreaReached;
            InventoryManager.OnInventoryChanged -= HandleInventoryChanged;
            PlayerMoney.OnMoneyChanged -= HandleMoneyChanged;
            DayManager.OnDayAdvanced -= HandleDayOrDiveChanged;
            DayManager.OnDivedToday -= HandleDayOrDiveChanged;
        }

        // Converts each ScriptableObject (QuestData) into a runtime Quest.
        private void InitializeQuests()
        {
            activeQuestList.Clear();

            foreach (var data in allQuestTemplates)
            {
                if (data == null)
                    continue;

                activeQuestList.Add(new Quest(data));
            }

            // A quest authored to start already InProgress (e.g. an intro tutorial) never
            // goes through AcceptQuest — pick up the active Main quest (or any active
            // quest) so it's tracked from the very start.
            EnsureQuestTracked();

            Debug.Log($"[QuestManager] Initialized {activeQuestList.Count} quests.");
        }

        // =========================================================
        // ENEMY KILLED
        // =========================================================

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

        // Not routed through TryProgress — a kill that would complete the step still has to
        // hold back one short of completion when RequiredMoney isn't paid yet (see
        // MoneyRequirementMet/HandleMoneyChanged), which a plain type+match predicate can't express.
        private void HandleEnemyKilled(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return;

            foreach (var q in activeQuestList)
            {
                if (q.Status != QuestStatus.InProgress) continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != QuestType.Delivery ||
                    step.DeliverySource != DeliverySource.EnemyKills ||
                    step.TargetEnemyIds == null || !step.TargetEnemyIds.Contains(enemyId))
                    continue;

                if (step.CurrentCount + 1 >= step.TargetCount && !MoneyRequirementMet(step))
                {
                    step.MoneyPending = true;
                    if (step.CurrentCount != step.TargetCount - 1)
                    {
                        step.CurrentCount = step.TargetCount - 1;
                        OnQuestUpdated?.Invoke();
                    }
                    continue;
                }

                CompleteProgress(q);
            }
        }

        /// <summary>Delivery only — true if the step has no RequiredMoney, or the player can
        /// currently afford it. See QuestStep.RequiredMoney.</summary>
        private static bool MoneyRequirementMet(QuestStep step)
        {
            if (step.RequiredMoney <= 0) return true;

            var money = GameManager.CurPlayer != null ? GameManager.CurPlayer.GetComponent<PlayerMoney>() : null;
            return money != null && money.CurrentMoney >= step.RequiredMoney;
        }

        /// <summary>Finalizes any Delivery step that was held back solely on RequiredMoney
        /// (see QuestStep.MoneyPending) now that the balance can cover it. Also pokes
        /// OnQuestUpdated unconditionally — QuestVisuals.GetDeliveryStatusSuffix shows the
        /// player's live balance against RequiredMoney, so the tracker needs to redraw on
        /// every money change even when no step actually completes.</summary>
        private void HandleMoneyChanged(int _)
        {
            OnQuestUpdated?.Invoke();

            foreach (var q in activeQuestList)
            {
                if (q.Status != QuestStatus.InProgress) continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != QuestType.Delivery || !step.MoneyPending) continue;
                if (!MoneyRequirementMet(step)) continue;

                step.MoneyPending = false;
                CompleteProgress(q, step.TargetCount - step.CurrentCount);
            }
        }

        /// <summary>WaitingQuest only — records the day this step became current, once. Safe to
        /// call repeatedly (AcceptQuest/CompleteProgress/LoadSaveState all call it) — a no-op once
        /// DayStepBecameCurrent is already set, since a step only becomes current once.</summary>
        private static void StampWaitingQuestStart(QuestStep step)
        {
            if (step != null && step.Type == QuestType.WaitingQuest &&
                step.DayStepBecameCurrent < 0 && DayManager.Instance != null)
                step.DayStepBecameCurrent = DayManager.Instance.CurrentDay;
        }

        /// <summary>
        /// Not routed through TryProgress — WaitingQuest is a live state condition (day count +
        /// today's dive count), not a one-shot event match, same reasoning as Delivery's
        /// HandleInventoryChanged. Re-checks every InProgress quest on every day-advance or dive.
        /// </summary>
        private void HandleDayOrDiveChanged()
        {
            if (DayManager.Instance == null) return;

            foreach (var q in activeQuestList)
            {
                if (q.Status != QuestStatus.InProgress) continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != QuestType.WaitingQuest) continue;

                StampWaitingQuestStart(step);

                bool dayReady = DayManager.Instance.CurrentDay >= step.DayStepBecameCurrent + step.RequiredDaysToWait;
                bool diveReady = DayManager.Instance.DivesToday >= step.RequiredDivesThatDay;
                if (dayReady && diveReady)
                    CompleteProgress(q);
            }
        }

        private void HandleTutorialAction(TutorialActionId actionId) =>
            TryProgress(QuestType.Tutorial, step => step.TutorialAction == actionId);

        // Not routed through the generic TryProgress — All mode needs per-NPC dedup (talking
        // to the same one twice must not double-count) that a plain type+match predicate can't
        // express, so this owns its own loop over activeQuestList instead.
        private void HandleNpcTalk(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return;

            foreach (var q in activeQuestList)
            {
                if (q.Status != QuestStatus.InProgress) continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != QuestType.Talk || !step.EffectiveNpcIds.Contains(npcId))
                    continue;

                // Already talked to this one — no further progress, no re-triggered rewards.
                if (step.TalkCompletionMode == TalkCompletionMode.All && !step.MarkNpcTalkedTo(npcId))
                    continue;

                CompleteProgress(q);
            }
        }

        /// <summary>Completes a Talk step authored with Auto Display Dialogue — it has no NPC id
        /// for NpcEvents.RaiseNpcTalkedTo to match on, so PlayerSelfDialogueUI calls this directly.</summary>
        public void CompleteAutoDialogue(Quest q)
        {
            var step = q?.CurrentStep;
            if (q != null && q.Status == QuestStatus.InProgress &&
                step != null && step.Type == QuestType.Talk && step.AutoDisplayDialogue)
                CompleteProgress(q);
        }

        private void HandleAreaReached(string areaId)
        {
            if (!string.IsNullOrEmpty(areaId))
                TryProgress(QuestType.ExploreArea, step => step.EffectiveAreaId == areaId);
        }

        private void HandleCutsceneFinished(string cutsceneId)
        {
            if (!string.IsNullOrEmpty(cutsceneId))
                TryProgress(QuestType.Cutscene, step => step.EffectiveCutsceneId == cutsceneId);
        }

        /// <summary>
        /// Delivery has no single discrete "did the thing" event like every other step type —
        /// it's satisfied by whatever the player is currently holding, which can go up or down
        /// as they pick up/drop/sell items. So instead of TryProgress's one-shot event match,
        /// this re-checks live inventory against the current step's requirements on every
        /// change. Below the requirement, it just updates CurrentCount for display (no reward
        /// side effects — see CompleteProgress's doc). At/above it, routes the completing delta
        /// through the normal CompleteProgress pipeline so Delivery pays out/advances exactly
        /// like every other type.
        /// </summary>
        private void HandleInventoryChanged()
        {
            if (InventoryManager.Instance == null)
                return;

            foreach (var q in activeQuestList)
            {
                if (q.Status != QuestStatus.InProgress)
                    continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != QuestType.Delivery ||
                    step.DeliverySource != DeliverySource.InventoryHeld || step.IsComplete)
                    continue;

                int progress = step.ComputeDeliveryProgress(HeldCountOf);

                // Items are ready but RequiredMoney hasn't been paid yet — hold one short of
                // completion (AddProgress refuses to act on an already-complete step) and flag
                // it; HandleMoneyChanged finalizes once the player can afford it.
                if (progress >= step.TargetCount && !MoneyRequirementMet(step))
                {
                    step.MoneyPending = true;
                    int capped = step.TargetCount - 1;
                    if (step.CurrentCount != capped)
                    {
                        step.CurrentCount = capped;
                        OnQuestUpdated?.Invoke();
                    }
                    continue;
                }
                step.MoneyPending = false;

                if (progress == step.CurrentCount)
                    continue;

                // Player dropped/sold some — keep the displayed count live.
                if (progress < step.CurrentCount)
                {
                    step.CurrentCount = progress;
                    OnQuestUpdated?.Invoke();
                    continue;
                }

                bool justCompleted = q.AddProgress(1);

                Debug.Log(
                    $"Progress : {q.CurrentCount}/{q.TargetCount}"
                );

                OnQuestUpdated?.Invoke();

                // =====================================================
                // AUTO COMPLETE
                // =====================================================

                if (justCompleted && step.CompletionMode == QuestCompletionMode.AutoComplete)
                {
                    RemoveQuestItems(q);

                    // Notification เดิม
                    //NotificationManager.Notify($"Quest complete: {q.QuestName}");
                    
                    // แสดง Quest Complete UI + Reward
                    if (QuestCompleteUI.Instance != null)
                    {
                        QuestCompleteUI.Instance.Show(q);
                    }

                    // เงินรางวัล
                    if (step.RewardMoney > 0)
                    {
                        OnMoneyRewarded?.Invoke(step.RewardMoney);

                        Debug.Log(
                            $"[QuestManager] (AutoComplete) " +
                            $"จ่ายเงินรางวัล: +{step.RewardMoney} บาท"
                        );
                    }
                }

                if (progress >= step.TargetCount)
                {
                    CompleteProgress(q, step.TargetCount - step.CurrentCount);
                }
                else
                {
                    step.CurrentCount = progress;
                    OnQuestUpdated?.Invoke();
                }
            }
        }

        private static int HeldCountOf(string itemIdStr) =>
            int.TryParse(itemIdStr, out int id) ? InventoryManager.Instance.GetQuestItemCount(id) : 0;

        /// <summary>
        /// Routes an event to every InProgress quest whose current checkpoint is of the
        /// given type and whose target matches. Shared by HandleEnemyKilled/HandleTutorialAction/
        /// HandleNpcTalk — they differ only in which step type and which field they match on.
        /// </summary>
        private void TryProgress(QuestType type, Func<QuestStep, bool> targetMatches)
        {
            foreach (var q in activeQuestList)
            {
                if (q.Status != QuestStatus.InProgress)
                    continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != type)
                    continue;

                if (!targetMatches(step))
                    continue;

                CompleteProgress(q);
            }
        }

        /// <summary>
        /// Each checkpoint pays its own reward the moment IT completes — not just the
        /// quest's final step — so a quest can pay out incrementally per checkpoint.
        /// Exception: a final step set to ReadyToHandIn pays nothing here — its reward
        /// waits for TurnInQuest, same as before, so it isn't paid twice. amount is 1 for
        /// every discrete-event step type (one kill, one talk, ...); Delivery's live
        /// inventory check passes whatever delta finishes it — see HandleInventoryChanged,
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
                ApplyStepFeatureChanges(completedStep);

            // Moved on to a new checkpoint (as opposed to just ticking progress on the
            // same one, or finishing the quest outright) — announce its description.
            if (!justCompleted && q.CurrentStep != completedStep)
                NotifyStepDescription(q);

            if (justCompleted)
            {
                if (completedStep.CompletionMode == QuestCompletionMode.AutoComplete)
                {
                    RemoveQuestItems(q);
                    if (QuestCompleteUI.Instance != null)
                        QuestCompleteUI.Instance.Show(q);

                    if (completedStep.RewardMoney > 0)
                        OnMoneyRewarded?.Invoke(completedStep.RewardMoney);

                    HandleNextQuest(q);
                }
                // else ReadyToHandIn: reward + next-quest handoff happen in TurnInQuest.
            }
            else if (completedStep.IsComplete && completedStep.RewardMoney > 0)
            {
                OnMoneyRewarded?.Invoke(completedStep.RewardMoney);
                NotificationManager.Notify($"+{completedStep.RewardMoney} Money");
            }

            EnsureQuestTracked();

            // A step just became current (this one, or the next one it advanced into) — if
            // it's an InventoryHeld Delivery step, the player may already be holding enough
            // (e.g. it was an EnemyKills bounty's own loot, collected before this checkpoint
            // was even reached). Inventory hasn't changed, so nothing would otherwise trigger
            // HandleInventoryChanged.
            if (q.CurrentStep?.Type == QuestType.Delivery && q.CurrentStep?.DeliverySource == DeliverySource.InventoryHeld)
                HandleInventoryChanged();

            // Same idea for WaitingQuest — stamp its start day now, and check right away in
            // case DayManager already satisfies it (e.g. RequiredDaysToWait is 0).
            if (q.CurrentStep?.Type == QuestType.WaitingQuest)
            {
                StampWaitingQuestStart(q.CurrentStep);
                HandleDayOrDiveChanged();
            }
        }

        // Locks always win over unlocks (see FeatureUnlockService.IsUnlocked) — order here
        // doesn't matter, but unlocking first reads naturally as "grant, then restrict".
        private static void ApplyStepFeatureChanges(QuestStep step)
        {
            if (step.UnlocksFeatures != null)
                foreach (var feature in step.UnlocksFeatures)
                    FeatureUnlockService.Instance.Unlock(feature);

            if (step.LocksFeatures != null)
                foreach (var feature in step.LocksFeatures)
                    FeatureUnlockService.Instance.Lock(feature);
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

            // Re-read now, not at schedule time — the quest may have advanced again since.
            // Skip guided steps — TutorialPromptUI already shows this same description on
            // the dedicated persistent panel; toasting it too would show it twice at once.
            if (!string.IsNullOrEmpty(q.StepDescription) && !q.IsGuidedSequence)
                NotificationManager.Notify(q.StepDescription);
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
            var list = new List<QuestSaveData>();
            foreach (var q in activeQuestList)
                list.Add(new QuestSaveData
                {
                    questId         = q.QuestID,
                    status          = (int)q.Status,
                    currentCount    = q.CurrentCount,
                    currentStepIndex = q.CurrentStepIndex,
                    dayStepBecameCurrent = q.CurrentStep?.DayStepBecameCurrent ?? -1,
                });
            return list;
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

            foreach (var s in saved)
            {
                var quest = GetQuestById(s.questId);

                if (quest != null)
                {
                    quest.Status           = (QuestStatus)s.status;
                    quest.CurrentStepIndex = s.currentStepIndex; // set before CurrentCount — CurrentCount targets the step at this index
                    quest.CurrentCount     = s.currentCount;

                    if (quest.CurrentStep != null && s.dayStepBecameCurrent >= 0)
                        quest.CurrentStep.DayStepBecameCurrent = s.dayStepBecameCurrent;
                }
            }

            EnsureQuestTracked();

            OnQuestUpdated?.Invoke();

            // A resumed save could land mid-Delivery-step already satisfied (or the player
            // adjusted their inventory between sessions) — HandleInventoryChanged re-checks
            // every InProgress quest itself, so one call covers all of them.
            HandleInventoryChanged();

            // Same idea for WaitingQuest — old saves (or a step reached for the first time
            // this session) have no dayStepBecameCurrent yet; StampWaitingQuestStart inside
            // it fills that in from the live day as a reasonable fallback.
            HandleDayOrDiveChanged();
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
                HandleInventoryChanged();

            if (quest.CurrentStep?.Type == QuestType.WaitingQuest)
            {
                StampWaitingQuestStart(quest.CurrentStep);
                HandleDayOrDiveChanged();
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
        /// Removes the quest item(s) consumed by turn-in. Delivery steps skip entirely when
        /// ConsumeOnComplete is off. InventoryHeld removes according to its count mode: PerItem
        /// takes each requirement's own count from TargetQuestItemRequirements; CombinedTotal has
        /// no per-item counts to go by (its ids are the plain TargetQuestItemIds list), so it
        /// takes greedily in list order until the shared Target Count is consumed. EnemyKills
        /// (and every non-Delivery type) keeps the single-item field (e.g. a kill bounty's
        /// hand-in loot item) — always consumed, no ConsumeOnComplete toggle for those.
        /// </summary>
        private void RemoveQuestItems(Quest q)
        {
            if (InventoryManager.Instance == null) return;

            if (q.CurrentStep.Type == QuestType.Delivery)
            {
                if (!q.CurrentStep.ConsumeOnComplete) return;

                if (q.CurrentStep.RequiredMoney > 0)
                {
                    var money = GameManager.CurPlayer != null ? GameManager.CurPlayer.GetComponent<PlayerMoney>() : null;
                    money?.Use(q.CurrentStep.RequiredMoney);
                }

                if (q.CurrentStep.DeliverySource == DeliverySource.EnemyKills)
                {
                    if (string.IsNullOrEmpty(q.TargetQuestItemId)) return;
                    if (!int.TryParse(q.TargetQuestItemId, out int lootItemId)) return;
                    InventoryManager.Instance.RemoveQuestItem(lootItemId, q.TargetCount);
                    return;
                }

                if (q.CurrentStep.DeliveryCountMode == DeliveryCountMode.PerItem)
                {
                    var requirements = q.TargetQuestItemRequirements;
                    if (requirements == null) return;

                    foreach (var req in requirements)
                        if (!string.IsNullOrEmpty(req.itemId) && int.TryParse(req.itemId, out int reqItemId))
                            InventoryManager.Instance.RemoveQuestItem(reqItemId, req.count);
                }
                else
                {
                    var itemIds = q.TargetQuestItemIds;
                    if (itemIds == null) return;

                    int remaining = q.TargetCount;
                    foreach (var idStr in itemIds)
                    {
                        if (remaining <= 0) break;
                        if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out int listItemId)) continue;

                        int take = Mathf.Min(InventoryManager.Instance.GetQuestItemCount(listItemId), remaining);
                        if (take <= 0) continue;

                        InventoryManager.Instance.RemoveQuestItem(listItemId, take);
                        remaining -= take;
                    }
                }
                return;
            }

            if (string.IsNullOrEmpty(q.TargetQuestItemId)) return;
            if (!int.TryParse(q.TargetQuestItemId, out int itemId)) return;
            InventoryManager.Instance.RemoveQuestItem(itemId, q.TargetCount);
        }

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

            RemoveQuestItems(quest);

            // Notification เดิม
            //NotificationManager.Notify($"Quest complete: {quest.QuestName}");
            
            // แสดง Quest Complete UI + Reward
            if (QuestCompleteUI.Instance != null)
            {
                QuestCompleteUI.Instance.Show(quest);
            }

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