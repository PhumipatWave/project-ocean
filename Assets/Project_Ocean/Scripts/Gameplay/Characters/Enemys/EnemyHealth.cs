using System;
using UnityEngine;

namespace Game.Testing
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;

        public event Action OnDamaged;
        public event Action OnDied;

        private float currentHealth;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private bool isDead;
        public bool IsDead => isDead;

        public void Configure(float healthValue)
        {
            maxHealth = Mathf.Max(1f, healthValue);
        }

        public void Initialize()
        {
            currentHealth = maxHealth;
            isDead = false;
        }

        public void TakeDamage(float amount)
        {
            if (isDead)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);

            if (currentHealth <= 0f)
            {
                isDead = true;
                OnDied?.Invoke();
                return;
            }

            OnDamaged?.Invoke();
        }
    }
}
