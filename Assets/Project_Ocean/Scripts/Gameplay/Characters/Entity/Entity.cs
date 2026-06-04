using System;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Abstract base class for all living entities (Player, Enemy).
    ///
    /// Health is abstracted behind IEntityHealth so different entities
    /// can use different health sources:
    ///   - Enemy  → EntityHealth component (standard HP)
    ///   - Player → PlayerOxygenTube (rope length = health)
    ///
    /// Subclasses override ProvideHealthSource() to return their source.
    /// If not overridden, Entity auto-finds/creates an EntityHealth component.
    /// </summary>
    public abstract class Entity : MonoBehaviour
    {
        #region Shared Component References
        [Header("Entity Components")]
        [SerializeField] protected EntityAnimation entityAnimation;
        [SerializeField] protected EntityDeathHandler entityDeathHandler;
        [SerializeField] protected Rigidbody2D rb;

        [SerializeField] private EntityHealth entityHealth;

        protected IEntityHealth healthSource;
        #endregion

        #region Public State
        public float CurrentHealth => healthSource != null ? healthSource.CurrentHealth : 0f;
        public float MaxHealth => healthSource != null ? healthSource.MaxHealth : 1f;
        public bool IsDead => healthSource != null && healthSource.IsDead;
        #endregion

        #region Public Events
        public event Action OnDamagedEvent;
        public event Action OnDiedEvent;
        #endregion

        #region Abstract
        protected abstract void OnDamagedHandler();
        protected abstract void OnDeathHandler();
        #endregion

        #region Health Source — override in subclass to swap health provider
        /// <summary>
        /// Return the IEntityHealth this entity should use.
        /// Default: EntityHealth component (auto-created if missing).
        /// Player overrides this to return PlayerOxygenTube.
        /// </summary>
        protected virtual IEntityHealth ProvideHealthSource()
        {
            if (entityHealth == null)
                entityHealth = GetOrAddComponent<EntityHealth>();
            return entityHealth;
        }
        #endregion

        #region Shared Concrete Methods
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

        protected T GetOrAddComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null) component = gameObject.AddComponent<T>();
            return component;
        }
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            CacheEntityComponents();
            healthSource = ProvideHealthSource();
            SubscribeHealthSource();
            healthSource?.Initialize();
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeHealthSource();
        }
        #endregion

        #region Private Health Wiring
        private void CacheEntityComponents()
        {
            if (entityAnimation == null)
                entityAnimation = GetOrAddComponent<EntityAnimation>();

            if (entityDeathHandler == null)
                entityDeathHandler = GetOrAddComponent<EntityDeathHandler>();

            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
        }

        private void SubscribeHealthSource()
        {
            if (healthSource == null) return;
            healthSource.OnDamaged += HandleDamaged;
            healthSource.OnDied    += HandleDied;
        }

        private void UnsubscribeHealthSource()
        {
            if (healthSource == null) return;
            healthSource.OnDamaged -= HandleDamaged;
            healthSource.OnDied -= HandleDied;
        }

        private void HandleDamaged()
        {
            OnDamagedEvent?.Invoke();
            OnDamagedHandler();
        }

        private void HandleDied()
        {
            OnDiedEvent?.Invoke();
            OnDeathHandler();
        }
        #endregion
    }
}
