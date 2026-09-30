using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class EntityDeathHandler : MonoBehaviour, IPoolable
    {
        #region Fields & Properties
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer entitySprite;
        [SerializeField] private Color deathSpriteColor = Color.red;

        private Color originalSpriteColor;
        private string originalTag;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            if (entitySprite == null)
            {
                Transform visual = transform.Find("Visual");
                if (visual != null)
                    entitySprite = visual.GetComponentInChildren<SpriteRenderer>();
                if (entitySprite == null)
                    entitySprite = GetComponentInChildren<SpriteRenderer>();
            }

            if (entitySprite != null)
                originalSpriteColor = entitySprite.color;

            originalTag = gameObject.tag;
        }
        #endregion

        /// <summary>Stops the body, applies the death color, retags it as an item and shows the death popup.</summary>
        public void HandleDeath()
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            gameObject.tag = "Items";

            ApplyDeathVisual();
            ShowDeathDamagePoint();
        }

        private void ApplyDeathVisual()
        {
            if (entitySprite == null) return;

            Color color = deathSpriteColor;
            color.a          = entitySprite.color.a;
            entitySprite.color = color;
        }

        private void ShowDeathDamagePoint()
        {
            IDamagePopup damagePoint = GetComponentInChildren<IDamagePopup>();
            if (damagePoint != null)
                damagePoint.ShowDeath();
        }

        // ── IPoolable ──

        public void OnGetFromPool()
        {
            gameObject.tag = originalTag;

            if (entitySprite != null)
                entitySprite.color = originalSpriteColor;
        }

        public void OnReturnToPool() { }
    }
}
