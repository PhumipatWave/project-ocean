using UnityEngine;
using Game.Testing;

public class FishingNPC : MonoBehaviour
{
    [Header("NPC Profiles")]
    [SerializeField] private string npcName = "Fisherman";
    [SerializeField] private Sprite npcAvatar;
    [SerializeField] private string playerTag = "Player";

    [Header("Quest Assignment")]
    // 💡 เปลี่ยนจากไฟล์ ScriptableObject เป็นรหัสเควส (ตัวเลข) แทน
    [SerializeField] private int assignQuestId = 0;

    private bool isPlayerInRange = false;

    // Property ให้ QuestManager ดึงค่า ID นี้ไปค้นหาใน All Quest Data
    public int AssignQuestId => assignQuestId;
    public string NpcName => npcName;
    public Sprite NpcAvatar => npcAvatar;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;

            if (QuestManager.instance != null)
            {
                QuestManager.instance.CloseQuestWindow();
            }
        }
    }

    void Update()
    {
        if (isPlayerInRange && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            if (QuestManager.instance != null)
            {
                // ส่งตัวเองเข้าไป เพื่อให้ QuestManager ไปดึง AssignQuestId ไปทำงานต่อ
                QuestManager.instance.OpenQuestSystem(this);
            }
        }
    }
}