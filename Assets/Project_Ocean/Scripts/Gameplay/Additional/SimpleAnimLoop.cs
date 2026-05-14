using UnityEngine;
using System.Collections;
namespace Game.Testing
{
    [RequireComponent(typeof(Animator))] // บังคับว่า Object นี้ต้องมี Animator ถึงจะแปะสคริปต์ได้
    public class SimpleAnimLoop : MonoBehaviour
    {
        [Header("Wait Time Settings")]
        [SerializeField] private float minWaitTime = 5f;
        [SerializeField] private float maxWaitTime = 10f;

        private Animator anim;

        void Awake()
        {
            anim = GetComponent<Animator>();
        }

        void Start()
        {
            if (anim != null)
            {
                StartCoroutine(ToggleAnimRoutine());
            }
        }

        IEnumerator ToggleAnimRoutine()
        {
            while (true)
            {
                // 1. เปิดการทำงาน Animator
                anim.enabled = true;

                // บรรทัดที่ต้องเพิ่ม: บังคับให้เล่นแอนิเมชันใหม่ตั้งแต่เฟรม 0
                // (0 คือชื่อ Layer, 0f คือเวลาเริ่มต้นที่วินาทีที่ 0)
                anim.Play(0, 0, 0f);

                // 2. รอจนจบ 1 ลูป
                yield return null; // สำคัญ: รอ 1 เฟรมเพื่อให้ Animator อัปเดตค่าความยาวใหม่
                float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
                yield return new WaitForSeconds(animLength);

                // 3. ปิด Animator ให้ค้างท่าสุดท้าย
                anim.enabled = false;

                // 4. สุ่มเวลาพัก
                float waitTime = Random.Range(minWaitTime, maxWaitTime);
                yield return new WaitForSeconds(waitTime);
            }
        }

    }
}