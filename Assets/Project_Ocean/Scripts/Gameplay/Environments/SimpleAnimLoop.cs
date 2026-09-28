using UnityEngine;
using System.Collections;
using Game.Core;

namespace Game.Gameplay
{
    [RequireComponent(typeof(Animator))]
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
            // --- ส่วนที่เพิ่ม: ปิดการทำงาน 1 วินาทีก่อนเริ่มครั้งแรก ---
            anim.enabled = false;
            yield return new WaitForSeconds(1f);
            // --------------------------------------------------

            while (true)
            {
                // 1. เปิด Animator และเริ่มว่ายจากเฟรม 0
                anim.enabled = true;
                anim.Play(0, 0, 0f);

                // 2. รอจนแอนิเมชันเล่นจบ
                yield return null;
                float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
                yield return new WaitForSeconds(animLength);

                // 3. บังคับไปเฟรมสุดท้ายก่อนปิด เพื่อไม่ให้เห็นเป็นภาพขาว/ปลาหาย
                anim.Play(0, 0, 0.99f);
                yield return new WaitForEndOfFrame(); // รอให้ GPU วาดเฟรมสุดท้ายเสร็จ

                // 4. ปิด Animator เพื่อค้างท่าจบไว้
                anim.enabled = false;

                // 5. สุ่มเวลาพัก 5-10 วินาที
                float waitTime = Random.Range(minWaitTime, maxWaitTime);
                yield return new WaitForSeconds(waitTime);
            }
        }
    }
}