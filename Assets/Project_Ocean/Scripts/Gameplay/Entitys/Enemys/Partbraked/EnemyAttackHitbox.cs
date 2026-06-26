using UnityEngine;

namespace Game.Testing
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
            if (Time.time - lastHitTime < hitCooldown) return;

            lastHitTime = Time.time;
            enemy.Player?.TakeDamage(damage);
        }
    }
}
