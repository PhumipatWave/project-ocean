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

        // รายการเควสที่แปลงเป็น Object พร้อมใช้งานในเกมแล้ว
        private List<Quest> activeQuestList = new List<Quest>();

        private void Awake()
        {
            // Duplicate guard: Destroy() is deferred, so we MUST return here,
            // otherwise the doomed copy would still run InitializeQuests().
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Persistence is handled by the parent GameManager root's DontDestroyOnLoad.

            InitializeQuests();
        }

        private void OnEnable()  => EnemyEvents.OnEnemyKilled += HandleEnemyKilled;
        private void OnDisable() => EnemyEvents.OnEnemyKilled -= HandleEnemyKilled;

        // แปลง ScriptableObject (QuestData) ให้กลายเป็น Runtime Quest
        private void InitializeQuests()
        {
            foreach (var data in allQuestTemplates)
            {
                activeQuestList.Add(new Quest(data));
            }
        }

        /// <summary>
        /// Routes a kill to every active InProgress KillCount quest whose target
        /// matches the dead enemy's id. AddProgress handles the threshold + status flip
        /// (Finish for AutoComplete, ReadyToTurnIn for ReadyToHandIn).
        /// </summary>
        private void HandleEnemyKilled(string enemyId)
        {
            Debug.Log($"Quest Received Kill : {enemyId}");

            if (string.IsNullOrEmpty(enemyId))
                return;

            foreach (var q in activeQuestList)
            {
                Debug.Log($"Quest Target : {q.TargetEnemyId}");

                if (q.Type != QuestType.KillCount)
                    continue;

                if (q.Status != QuestStatus.InProgress)
                    continue;

                if (q.TargetEnemyId != enemyId)
                {
                    Debug.Log("Target Not Match");
                    continue;
                }

                Debug.Log("Target Match");

                bool justCompleted = q.AddProgress(1);

                Debug.Log($"Progress : {q.CurrentCount}/{q.TargetCount}");

                OnQuestUpdated?.Invoke();

                if (justCompleted && q.CompletionMode == QuestCompletionMode.AutoComplete)
                {
                    RemoveQuestItems(q);
                    if (q.RewardMoney > 0)
                    {
                        OnMoneyRewarded?.Invoke(q.RewardMoney);
                        Debug.Log($"[QuestManager] (AutoComplete) จ่ายเงินรางวัล: +{q.RewardMoney} บาท");
                    }
                }
            }
        }

        // ดึงรายการเควสทั้งหมดที่มีสถานะตามที่ต้องการ (เช่น เควสใหม่ที่ยังไม่ได้ทำ)
        public List<Quest> GetQuestsByStatus(QuestStatus status)
        {
            return activeQuestList.FindAll(q => q.Status == status);
        }

        public Quest GetQuestById(int id)
        {
            return activeQuestList.Find(q => q.QuestID == id);
        }

        // ── Save support ──

        /// <summary>Collects the mutable runtime state (id, status, kill progress) of each quest.</summary>
        public List<QuestSaveData> GetSaveState()
        {
            var list = new List<QuestSaveData>();
            foreach (var q in activeQuestList)
                list.Add(new QuestSaveData
                {
                    questId      = q.QuestID,
                    status       = (int)q.Status,
                    currentCount = q.CurrentCount,
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
                    quest.Status       = (QuestStatus)s.status;
                    quest.CurrentCount = s.currentCount;
                }
            }
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
            if (HasActiveQuest())
            {
                Debug.Log("Already have an active quest.");
                return false;
            }

            Quest quest = GetQuestById(questID);

            if (quest == null)
                return false;

            if (quest.Status != QuestStatus.New)
                return false;

            quest.Status = QuestStatus.InProgress;

            OnQuestUpdated?.Invoke();

            return true;
        } // AcceptQuest ===

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

            // รีเซ็ตสถานะ
            quest.Status = QuestStatus.New;

            // รีเซ็ต Progress
            quest.CurrentCount = 0;

            OnQuestUpdated?.Invoke();

            Debug.Log($"Quest {quest.QuestName} cancelled.");

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

            if (quest.RewardMoney > 0)
            {
                OnMoneyRewarded?.Invoke(quest.RewardMoney);
                Debug.Log($"[QuestManager] Completing quests: +{quest.RewardMoney} $");
            }

            OnQuestUpdated?.Invoke();
            return true;
        }



    }
}
