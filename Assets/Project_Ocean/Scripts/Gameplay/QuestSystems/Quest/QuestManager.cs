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
        }

        private void OnDisable()
        {
            EnemyEvents.OnEnemyKilled -= HandleEnemyKilled;
        }

        // =========================================================
        // INITIALIZE QUESTS
        // =========================================================

        // แปลง ScriptableObject (QuestData)
        // ให้กลายเป็น Runtime Quest
        private void InitializeQuests()
        {
            activeQuestList.Clear();

            foreach (var data in allQuestTemplates)
            {
                if (data == null)
                    continue;

                activeQuestList.Add(new Quest(data));
            }

            Debug.Log(
                $"[QuestManager] Initialized {activeQuestList.Count} quests."
            );
        }

        // =========================================================
        // ENEMY KILLED
        // =========================================================

        /// <summary>
        /// Routes a kill to every active InProgress KillCount quest
        /// whose target matches the dead enemy's id.
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

                Debug.Log(
                    $"Progress : {q.CurrentCount}/{q.TargetCount}"
                );

                OnQuestUpdated?.Invoke();

                // =====================================================
                // AUTO COMPLETE
                // =====================================================

                if (justCompleted &&
                    q.CompletionMode == QuestCompletionMode.AutoComplete)
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
                    if (q.RewardMoney > 0)
                    {
                        OnMoneyRewarded?.Invoke(q.RewardMoney);

                        Debug.Log(
                            $"[QuestManager] (AutoComplete) " +
                            $"จ่ายเงินรางวัล: +{q.RewardMoney} บาท"
                        );
                    }
                }
            }
        }

        // =========================================================
        // GET QUESTS
        // =========================================================

        // ดึงรายการเควสทั้งหมดที่มีสถานะตามที่ต้องการ
        public List<Quest> GetQuestsByStatus(QuestStatus status)
        {
            return activeQuestList.FindAll(
                q => q.Status == status
            );
        }

        public List<Quest> GetQuestsByCategory(
            QuestCategory category)
        {
            return activeQuestList.FindAll(
                q =>
                    q.Category == category &&
                    q.Status == QuestStatus.New
            );
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
            return activeQuestList.Find(
                q => q.QuestID == id
            );
        }

        public List<Quest> GetCompletedQuests()
        {
            return activeQuestList.FindAll(
                q => q.Status == QuestStatus.Finish
            );
        }

        // =========================================================
        // SAVE SUPPORT
        // =========================================================

        /// <summary>
        /// Collects the mutable runtime state
        /// (id, status, kill progress) of each quest.
        /// </summary>
        public List<QuestSaveData> GetSaveState()
        {
            var list = new List<QuestSaveData>();

            foreach (var q in activeQuestList)
            {
                list.Add(
                    new QuestSaveData
                    {
                        questId = q.QuestID,
                        status = (int)q.Status,
                        currentCount = q.CurrentCount,
                    }
                );
            }

            return list;
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
                    quest.Status =
                        (QuestStatus)s.status;

                    quest.CurrentCount =
                        s.currentCount;
                }
            }

            OnQuestUpdated?.Invoke();
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

            // รับได้เฉพาะ Quest ที่ยังไม่ได้รับ
            if (quest.Status != QuestStatus.New)
            {
                Debug.LogWarning(
                    $"Quest {quest.QuestName} cannot be accepted. " +
                    $"Current status: {quest.Status}"
                );

                return false;
            }

            if (trackedQuestId == -1)
            {
                trackedQuestId = questID;
            }

            // รับ Quest
            quest.Status = QuestStatus.InProgress;

            OnQuestUpdated?.Invoke();

            Debug.Log(
                $"Accepted Quest: {quest.QuestName}"
            );

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

        // =========================================================
        // CANCEL QUEST
        // =========================================================

        public bool CancelQuest(int questID)
        {
            Quest quest = GetQuestById(questID);

            if (quest == null)
                return false;

            if (quest.Status != QuestStatus.InProgress &&
                quest.Status != QuestStatus.ReadyToTurnIn)
                return false;

            // รีเซ็ต Quest ที่ถูก Cancel
            quest.Status = QuestStatus.New;
            quest.CurrentCount = 0;

            // ถ้า Quest ที่ Cancel คือ Quest ที่กำลัง Track
            if (trackedQuestId == questID)
            {
                trackedQuestId = -1;

                // หา Quest อื่นที่ยังทำอยู่มา Track แทน
                Quest nextQuest = activeQuestList.Find(
                    q =>
                        q.Status == QuestStatus.InProgress ||
                        q.Status == QuestStatus.ReadyToTurnIn
                );

                if (nextQuest != null)
                {
                    trackedQuestId = nextQuest.QuestID;

                    Debug.Log(
                        $"Quest {quest.QuestName} cancelled. " +
                        $"Now tracking Quest: {nextQuest.QuestID}"
                    );
                }
                else
                {
                    Debug.Log(
                        $"Quest {quest.QuestName} cancelled. " +
                        $"No active quest left."
                    );
                }
            }

            OnQuestUpdated?.Invoke();

            return true;
        }

        // =========================================================
        // ACTIVE QUEST
        // =========================================================

        public bool HasActiveQuest()
        {
            return activeQuestList.Exists(
                q =>
                    q.Status == QuestStatus.InProgress ||
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

        // =========================================================
        // MONEY REWARD
        // =========================================================

        public static event Action<int> OnMoneyRewarded;

        // =========================================================
        // REMOVE QUEST ITEMS
        // =========================================================

        private void RemoveQuestItems(Quest q)
        {
            if (string.IsNullOrEmpty(q.QuestItemId))
                return;

            if (!int.TryParse(
                    q.QuestItemId,
                    out int itemId))
                return;

            if (InventoryManager.Instance == null)
                return;

            InventoryManager.Instance.RemoveQuestItem(
                itemId,
                q.TargetCount
            );
        }

        // =========================================================
        // TURN IN QUEST
        // =========================================================

        /// <summary>
        /// Called by the NPC when the player clicks Turn In.
        /// Quest must be ReadyToTurnIn.
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

            // เงินรางวัล
            if (quest.RewardMoney > 0)
            {
                OnMoneyRewarded?.Invoke(
                    quest.RewardMoney
                );

                Debug.Log(
                    $"[QuestManager] Completing quests: " +
                    $"+{quest.RewardMoney} $"
                );
            }

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