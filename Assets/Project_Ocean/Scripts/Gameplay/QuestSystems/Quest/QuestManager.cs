using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Header("All Available Quests in Game")]
        [SerializeField] private List<QuestData> allQuestTemplates;

        // รายการเควสที่แปลงเป็น Object พร้อมใช้งานในเกมแล้ว
        private List<Quest> activeQuestList = new List<Quest>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializeQuests();
        }

        // แปลง ScriptableObject (QuestData) ให้กลายเป็น Runtime Quest
        private void InitializeQuests()
        {
            foreach (var data in allQuestTemplates)
            {
                activeQuestList.Add(new Quest(data));
            }
        }

        // ดึงรายการเควสทั้งหมดที่มีสถานะตามที่ต้องการ (เช่น เควสใหม่ที่ยังไม่ได้ทำ)
        public List<Quest> GetQuestsByStatus(QuestStatus status)
        {
            return activeQuestList.FindAll(q => q.Status == status);
        }

        public Quest GetQuestById(int id)
        {
            // ค้นหาในรายชื่อคลังเควสทั้งหมดที่มี QuestID ตรงกับที่ระบุ
            // (หมายเหตุ: ให้เปลี่ยนคำว่า activeQuestList เป็นชื่อตัวแปรคลัง List<Quest> จริงในสคริปต์ของคุณ)
            return activeQuestList.Find(q => q.QuestID == id);
        }

    }
}