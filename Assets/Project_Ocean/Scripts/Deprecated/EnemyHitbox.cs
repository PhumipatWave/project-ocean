using UnityEngine;
using Game.Core;
using Game.Gameplay;
using Game.UI;

namespace Game.Deprecated
{
    public class EnemyHitbox : MonoBehaviour
    {
        public enum HitPart
        {
            Head,
            Body,
            Tail
        }

        [Header("Setup")]
        public HitPart hitPart;
        public Enemy enemy;
        public EnemyWoundSystem woundSystem;

        [Header("Damage")]
        public float headDamage = 25f;
        public float bodyDamage = 10f;
        public float tailDamage = 5f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("PlayerAttack"))
                return;

            float damage = GetDamage();

            // ✅ เอาตำแหน่งที่โดนจริง
            Vector2 hitPoint = other.ClosestPoint(transform.position);

            damage = woundSystem.ProcessDamage(damage, hitPoint);

            enemy.TakeDamage(damage);
        }

        float GetDamage()
        {
            switch (hitPart)
            {
                case HitPart.Head: return headDamage;
                case HitPart.Body: return bodyDamage;
                case HitPart.Tail: return tailDamage;
            }
            return 0;
        }
    }
}