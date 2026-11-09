using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class EnemyAttackHitbox : MonoBehaviour
    {
        [SerializeField] private float hitCooldown = 0.5f;

        private Enemy enemy;
        private float damage;
        private float lastHitTime = float.MinValue;

        public void Initialize(Enemy enemy, float damage)
        {
            this.enemy = enemy;
            this.damage = damage;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            // A clip can keep the hitbox open after death when no death state interrupts it.
            if (enemy == null || enemy.IsDead) return;
            if (Time.time - lastHitTime < hitCooldown) return;

            lastHitTime = Time.time;
            enemy.Player?.TakeDamage(damage);
        }
    }
}
