using UnityEngine;

namespace Game.Testing
{
    public class InteractionTrigger : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private GameObject interactionUI; // ลากปุ่ม UI ที่ต้องการให้ขึ้นมาใส่ในช่องนี้

        [Header("Tag Settings")]
        [SerializeField] private string targetTag = "Player"; // ชื่อ Tag ของวัตถุที่เราจะไปชน

        private void Start()
        {
            // เริ่มเกมมาให้แน่ใจว่า UI ปิดอยู่
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }

        // เมื่อเดินเข้าชน Collider (ที่เป็น Is Trigger)
        private void OnTriggerEnter2D(Collider2D other)
        {
            // เช็กว่าวัตถุที่ชนมี Tag ตรงกับที่ตั้งไว้ไหม
            if (other.CompareTag(targetTag))
            {
                if (interactionUI != null)
                {
                    interactionUI.SetActive(true); // เปิด UI
                }

                Debug.Log("Trigger Enter!!!");
            }
        }

        // เมื่อเดินออกจาก Collider
        private void OnTriggerExit2D(Collider2D other)
        {
            // เช็ก Tag เหมือนเดิมตอนเดินออก
            if (other.CompareTag(targetTag))
            {
                if (interactionUI != null)
                {
                    interactionUI.SetActive(false); // ปิด UI
                }
                Debug.Log("Trigger Exit!!!");
            }
        }
    }
}
