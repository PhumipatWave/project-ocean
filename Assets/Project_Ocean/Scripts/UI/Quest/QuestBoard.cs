using UnityEngine;

public class QuestBoard : MonoBehaviour
{
    [Header("Quest Board Settings")]
    [SerializeField] private string playerTag = "Player";

    // ใส่รหัส ID ของเควสทั้งหมดที่อยากให้มาโชว์บนป้ายประกาศนี้ (เช่น 0, 1, 2)
    [SerializeField] private int[] boardQuestIds;

    private bool isPlayerInRange = false;

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
            // ถ้าเดินหนีออกจากป้าย ให้ปิดบอร์ดเควสทันที
            if (QuestManager.instance != null)
            {
                QuestManager.instance.CloseQuestWindow();
            }
        }
    }

    void Update()
    {
        // เมื่อผู้เล่นเดินมาใกล้ป้าย แล้วกดคลิกซ้ายหรือคลิกขวา
        if (isPlayerInRange && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            if (QuestManager.instance != null)
            {
                // สั่งให้ QuestManager เปิดหน้าต่างบอร์ดเควสรวมขึ้นมา
                QuestManager.instance.OpenQuestBoard(boardQuestIds);
            }
        }
    }
}