using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class QuestManager : MonoBehaviour, ISaveParticipant
    {
        public static QuestManager Instance { get; private set; }

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
            if (string.IsNullOrEmpty(enemyId)) return;

            foreach (var q in activeQuestList)
            {
                if (q.Type != QuestType.KillCount) continue;
                if (q.Status != QuestStatus.InProgress) continue;
                if (q.TargetEnemyId != enemyId) continue;

                bool justCompleted = q.AddProgress(1);
                if (justCompleted)
                    Debug.Log($"[QuestManager] Quest '{q.QuestName}' objective complete ({q.Progress}) -> {q.Status}");
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
    }
}
