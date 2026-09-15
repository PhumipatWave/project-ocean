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

        // รายการเควสที่แปลงเป็น Object พร้อมใช้งานในเกมแล้ว
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
        }

        private void OnDisable()
        {
            EnemyEvents.OnEnemyKilled -= HandleEnemyKilled;
            TutorialEvents.OnActionPerformed -= HandleTutorialAction;
            NpcEvents.OnNpcTalkedTo -= HandleNpcTalk;
        }

        // แปลง ScriptableObject (QuestData) ให้กลายเป็น Runtime Quest
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

        private void HandleEnemyKilled(string enemyId)
        {
            if (!string.IsNullOrEmpty(enemyId))
                TryProgress(QuestType.KillCount, step => step.TargetEnemyId == enemyId);
        }

        private void HandleTutorialAction(TutorialActionId actionId) =>
            TryProgress(QuestType.Tutorial, step => step.TutorialAction == actionId);

        /// <summary>Talk steps reuse TargetEnemyId as the npcId being matched.</summary>
        private void HandleNpcTalk(string npcId)
        {
            if (!string.IsNullOrEmpty(npcId))
                TryProgress(QuestType.Talk, step => step.TargetEnemyId == npcId);
        }

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
        /// waits for TurnInQuest, same as before, so it isn't paid twice.
        /// </summary>
        private void CompleteProgress(Quest q)
        {
            var completedStep = q.CurrentStep;

            bool justCompleted = q.AddProgress(1);

            OnQuestUpdated?.Invoke();

            // Moved on to a new checkpoint (as opposed to just ticking progress on the
            // same one, or finishing the quest outright) — announce its description.
            if (!justCompleted && q.CurrentStep != completedStep)
                NotifyStepDescription(q);

            if (justCompleted)
            {
                if (completedStep.CompletionMode == QuestCompletionMode.AutoComplete)
                {
                    RemoveQuestItems(q);
                    NotificationManager.Notify($"Quest complete: {q.QuestName}");

                    if (completedStep.RewardMoney > 0)
                        OnMoneyRewarded?.Invoke(completedStep.RewardMoney);

                    HandleNextQuest(q);
                }
                // else ReadyToHandIn: reward + next-quest handoff happen in TurnInQuest.
            }
            else if (completedStep.RewardMoney > 0)
            {
                OnMoneyRewarded?.Invoke(completedStep.RewardMoney);
                NotificationManager.Notify($"+{completedStep.RewardMoney}");
            }

            EnsureQuestTracked();
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
            if (!string.IsNullOrEmpty(q.StepDescription))
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

        // ดึงรายการเควสทั้งหมดที่มีสถานะตามที่ต้องการ (เช่น เควสใหม่ที่ยังไม่ได้ทำ)
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


        public List<Quest> GetActiveQuests()
        {
            return activeQuestList.FindAll(
                q =>
                    q.Status == QuestStatus.InProgress ||
                    q.Status == QuestStatus.ReadyToTurnIn
            );
        }

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
                q.CurrentStep.Type == QuestType.Talk &&
                q.CurrentStep.TargetEnemyId == npcId);
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
                });
            return list;
        }

        public List<Quest> GetCompletedQuests()
        {
            return activeQuestList.FindAll(
                q =>
                q.Status == QuestStatus.Finish
            );
        }

        /// <summary>
        /// Restores quest status + kill progress from saved data. Quests are matched by id
        /// and re-hydrated from templates (in Awake), so only mutable state is applied.
        /// Unknown ids are ignored; quests missing from the save keep their template default.
        /// </summary>
        public void LoadSaveState(List<QuestSaveData> saved)
        {
            if (saved == null) return;
            foreach (var s in saved)
            {
                var quest = GetQuestById(s.questId);
                if (quest != null)
                {
                    quest.Status           = (QuestStatus)s.status;
                    quest.CurrentStepIndex = s.currentStepIndex; // set before CurrentCount — CurrentCount targets the step at this index
                    quest.CurrentCount     = s.currentCount;
                }
            }

            EnsureQuestTracked();

            OnQuestUpdated?.Invoke();
        }

        // ── ISaveParticipant ──

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
                Debug.LogWarning($"Quest ID {questID} not found.");
                return false;
            }

            // รับได้เฉพาะ Quest ที่ยังไม่ได้รับ
            if (quest.Status != QuestStatus.New)
            {
                Debug.LogWarning(
                    $"Quest {quest.QuestName} cannot be accepted. Current status: {quest.Status}"
                );
                return false;
            }

            if (trackedQuestId == -1)
            {
                trackedQuestId = questID;
            }

            // รับ Quest
            quest.Status = QuestStatus.InProgress;

            NotifyStepDescription(quest);

            OnQuestUpdated?.Invoke();

            Debug.Log($"Accepted Quest: {quest.QuestName}");

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
        } // RejectQuest ===

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

            // รีเซ็ต Quest ที่ถูก Cancel
            quest.Status = QuestStatus.New;
            quest.CurrentCount = 0;

            // ถ้า Quest ที่ Cancel คือ Quest ที่กำลัง Track ให้หา Quest อื่นมา Track แทน
            if (trackedQuestId == questID)
                trackedQuestId = -1;

            EnsureQuestTracked();

            OnQuestUpdated?.Invoke();

            return true;
        }

        public bool HasActiveQuest()
        {
            return activeQuestList.Exists(
                q => q.Status == QuestStatus.InProgress ||
                     q.Status == QuestStatus.ReadyToTurnIn
            );
        }

        public Quest GetCurrentActiveQuest()
        {
            return activeQuestList.Find(
                q =>
                q.Status == QuestStatus.InProgress ||
                q.Status == QuestStatus.ReadyToTurnIn
            );
        }

        /// <summary>
        /// Called by the NPC when the player clicks Turn In.
        /// Quest must be ReadyToTurnIn — any other state returns false with a warning.
        /// </summary>
        
        public static event Action<int> OnMoneyRewarded;
        private void RemoveQuestItems(Quest q)
        {
            if (string.IsNullOrEmpty(q.QuestItemId)) return;
            if (!int.TryParse(q.QuestItemId, out int itemId)) return;
            if (InventoryManager.Instance == null) return;
            InventoryManager.Instance.RemoveQuestItem(itemId, q.TargetCount);
        }

        public bool TurnInQuest(int questID)
        {
            Quest quest = GetQuestById(questID);

            if (quest == null) return false;
            if (quest.Status != QuestStatus.ReadyToTurnIn) return false;

            quest.Status = QuestStatus.Finish;
            RemoveQuestItems(quest);

            NotificationManager.Notify($"Quest complete: {quest.QuestName}");

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

        public Quest GetTrackedQuest()
        {
            if (trackedQuestId == -1)
                return null;

            Quest quest = GetQuestById(trackedQuestId);

            if (quest == null)
                return null;

            if (quest.Status != QuestStatus.InProgress &&
                quest.Status != QuestStatus.ReadyToTurnIn)
                return null;

            return quest;
        }

        public bool SetTrackedQuest(int questID)
        {
            Quest quest = GetQuestById(questID);

            if (quest == null)
                return false;

            if (quest.Status != QuestStatus.InProgress &&
                quest.Status != QuestStatus.ReadyToTurnIn)
                return false;

            trackedQuestId = questID;

            OnQuestUpdated?.Invoke();

            Debug.Log($"Tracking Quest: {quest.QuestName}");

            return true;
        }



    }
}
