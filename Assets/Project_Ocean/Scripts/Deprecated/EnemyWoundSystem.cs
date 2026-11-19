using UnityEngine;
using Game.Core;
using Game.Gameplay;
using Game.UI;

namespace Game.Deprecated
{
    public class EnemyWoundSystem : MonoBehaviour
    {
        [Header("Wound Settings")]
        public int hitToOpen = 3;
        private int currentHit = 0;

        public bool isWoundOpen = false;

        [Header("Multiplier")]
        public float burstMultiplier = 3f;

        [Header("UI")]
        [SerializeField] private GameObject crossIcon;

        public float ProcessDamage(float baseDamage, Vector2 hitPosition)
        {
            // 💥 ถ้าแผลเปิด → burst
            if (isWoundOpen)
            {
                isWoundOpen = false;
                currentHit = 0;

                if (crossIcon != null)
                    crossIcon.SetActive(false);

                return baseDamage * burstMultiplier;
            }

            // สะสม hit
            currentHit++;

            if (currentHit >= hitToOpen)
            {
                isWoundOpen = true;

                if (crossIcon != null)
                {
                    crossIcon.SetActive(true);

                    // ย้าย ❌ ไปจุดที่แทง
                    crossIcon.transform.position = hitPosition;

                    // ให้ติดกับปลา
                    crossIcon.transform.SetParent(transform);
                }
            }

            return baseDamage;
        }
    }
}