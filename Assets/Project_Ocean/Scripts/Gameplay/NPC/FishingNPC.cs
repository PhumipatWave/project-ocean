using Game.Testing;
using UnityEngine;

public class FishingNPC : MonoBehaviour
{
    [SerializeField] private string npcName;
    [SerializeField] private Sprite npcAvatar;
    [SerializeField] private QuestData assignedQuestData; // ลากไฟล์เควสที่อยากให้ NPC ตัวนี้แจกมาใส่

    private Quest npcQuest;

    public string NpcName => npcName;
    public Sprite NpcAvatar => npcAvatar;

    void Start()
    {
        if (assignedQuestData != null)
        {
            // แปลง ScriptableObject ออกมาเป็น Runtime Object เควสจริงตอนเริ่มเกม
            npcQuest = new Quest(assignedQuestData);
        }
    }

    public Quest GetNpcQuest()
    {
        return npcQuest;
    }

    // ฟังก์ชันเมื่อผู้เล่นเดินมาคลิก หรือกดปุ่ม E เพื่อคุยกับ NPC ตัวนี้
    public void InteractWithNPC()
    {
        QuestManager.instance.OpenQuestSystem(this);
    }
}