using UnityEngine;

namespace Game.Testing
{
    public class QuestBoardTrigger : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private QuestBoardUI questBoardUI;       // ลาก Canvas UI กระดานเควสมาใส่ช่องนี้
        [SerializeField] private KeyCode interactKey = KeyCode.E; // ปุ่มกดเปิด (เช่น ปุ่ม E)

        private bool isPlayerNearby = false;

        private void Update()
        {
            // ถ้าผู้เล่นอยู่ใกล้ป้ายและกดปุ่ม E ให้เปิดกระดานเควสขึ้นมา
            if (isPlayerNearby && Input.GetKeyDown(interactKey))
            {
                if (questBoardUI != null)
                {
                    // สั่งเปิดบอร์ด UI (จะกระตุ้นฟังก์ชัน OnEnable ใน QuestBoardUI ให้รีโหลดเควสใหม่ทันที)
                    questBoardUI.gameObject.SetActive(true);
                }
            }
        }

        // ตรวจจับเมื่อผู้เล่นเดินเข้ามาในอาณาเขตของป้าย
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                isPlayerNearby = true;
                Debug.Log("Press E to view the QuestBoard.");
            }
        }

        // ตรวจจับเมื่อผู้เล่นเดินห่างออกไปจากป้าย
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                isPlayerNearby = false;
                Debug.Log("Exit to QuestBoard");

                // ถ้าผู้เล่นเดินหนีออกจากป้าย ให้ปิดหน้าต่างกระดานเควสทันทีแบบ Stardew Valley
                if (questBoardUI != null)
                {
                    questBoardUI.gameObject.SetActive(false);
                }
            }
        }
    }
}