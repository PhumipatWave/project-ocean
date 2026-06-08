using System;
using UnityEngine;

namespace Game.Testing
{
    public abstract class Entity : MonoBehaviour
    {
        [Header("Entity Components")]
        [SerializeField] protected EntityAnimation entityAnimation;
        [SerializeField] protected EntityDeathHandler entityDeathHandler;
        [SerializeField] protected Rigidbody2D rb;

        [SerializeField] protected EntityHealth entityHealth;

        protected IEntityHealth healthSource;

        public float CurrentHealth => healthSource != null ? healthSource.CurrentHealth : 0f;
        public float MaxHealth => healthSource != null ? healthSource.MaxHealth : 1f;
        public bool IsDead => healthSource != null && healthSource.IsDead;

        protected abstract void OnDamagedHandler();
        protected abstract void OnDeathHandler();
        protected virtual void OnDamagedVisual() { }

        protected virtual IEntityHealth ProvideHealthSource()
        {
            return entityHealth != null ? entityHealth : GetComponent<EntityHealth>();
        }

        protected virtual void Awake()
        {
            WireEntityComponents();
            healthSource = ProvideHealthSource();
            SubscribeEntityEvent();
            healthSource?.Initialize();
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeEntityEvent();
        }

        private void WireEntityComponents()
        {
            if (entityAnimation == null)
                entityAnimation = GetComponent<EntityAnimation>();

            if (entityDeathHandler == null)
                entityDeathHandler = GetComponent<EntityDeathHandler>();

            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
        }

        private void SubscribeEntityEvent()
        {
            if (healthSource == null)
                return;

            healthSource.OnDamaged += HandleDamaged;
            healthSource.OnDied += HandleDied;
        }

        private void UnsubscribeEntityEvent()
        {
            if (healthSource == null)
                return;

            healthSource.OnDamaged -= HandleDamaged;
            healthSource.OnDied -= HandleDied;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) 
                return;

            healthSource?.TakeDamage(amount);
        }

        public void Heal(float amount)
        {
            if (IsDead) 
                return;

            healthSource?.Heal(amount);
        }

        protected void StopMovement()
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        private void HandleDamaged()
        {
            OnDamagedVisual();
            OnDamagedHandler();
        }

        private void HandleDied()
        {
            OnDeathHandler();
        }
    }
}
