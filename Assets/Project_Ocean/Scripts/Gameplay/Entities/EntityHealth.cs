using System;
using System.Collections;

using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class EntityHealth : MonoBehaviour, IEntityHealth, IPoolable
    {
        #region Fields & Properties
        private float maxHealth = 100f;

        [Header("Hit Flash")]
        [SerializeField] private Color hitColor = new Color(0f, 0.1f, 0.5f);

        [SerializeField] private float flashDuration = 0.15f;
        [Tooltip("Sprite that flashes on hit. Empty = first child SpriteRenderer.")]
        [SerializeField] private SpriteRenderer flashTarget;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Coroutine flashCoroutine;

        public event Action OnDamaged;
        protected void RaiseOnDamaged() => OnDamaged?.Invoke();
        protected void RaiseOnDied() => OnDied?.Invoke();

        public event Action OnDied;

        private float currentHealth;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private bool isDead;
        public bool IsDead => isDead;
        #endregion

        public void Configure(float healthValue)
        {
            maxHealth = Mathf.Max(1f, healthValue);
        }

        /// Sets health without TakeDamage/Heal side effects, for subclasses that handle 0 health differently.
        protected void SetCurrentHealth(float value)
        {
            currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        }

        public virtual void Initialize()
        {
            currentHealth = maxHealth;
            isDead = false;
        }

        public virtual void TakeDamage(float amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);

            if (currentHealth <= 0f)
            {
                isDead = true;
                OnDied?.Invoke();
                return;
            }

            FlashHitColor();
            OnDamaged?.Invoke();
        }

        public virtual void Heal(float amount)
        {
            if (isDead) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        protected void FlashHitColor()
        {
            if (spriteRenderer == null) return;
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            flashCoroutine = null;
        }

        private void Awake()
        {
            spriteRenderer = flashTarget != null ? flashTarget : GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        // ── IPoolable ──
        public void OnGetFromPool()  => Initialize();
        public void OnReturnToPool() { }
    }
}
