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
                // 1. เปิดการทำงาน Animator ให้เล่นไป
                anim.enabled = true;

                // 2. รอจนจบ 1 ลูป (ดึงค่าจากคลิปที่เล่นอยู่)
                yield return null; // รอให้ Animator อัปเดต 1 เฟรม
                float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
                yield return new WaitForSeconds(animLength);

                // 3. ปิด Animator (ภาพจะหยุดนิ่งอยู่ที่เฟรมสุดท้าย)
                anim.enabled = false;

                // 4. สุ่มเวลาพัก 5-10 วินาที
                float waitTime = Random.Range(minWaitTime, maxWaitTime);
                yield return new WaitForSeconds(waitTime);
            }
        }

    }
}