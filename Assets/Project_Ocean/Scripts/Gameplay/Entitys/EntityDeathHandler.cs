using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
{
    public class EntityDeathHandler : MonoBehaviour
    {
        #region Fields & Properties
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer entitySprite;
        [SerializeField] private Color deathSpriteColor = Color.red;

        private EnemyNavAgentBase navAgent;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            if (entitySprite == null)
            {
                Transform visual = transform.Find("Visual");
                entitySprite = visual != null
                    ? visual.GetComponent<SpriteRenderer>()
                    : GetComponentInChildren<SpriteRenderer>();
            }

            // Optional — only present on enemies
            navAgent = GetComponent<EnemyNavAgentBase>();
        }
        #endregion

        public void Configure(Rigidbody2D rigidbody2D, SpriteRenderer spriteRenderer, Color deathColor)
        {
            if (rigidbody2D != null)   rb           = rigidbody2D;
            if (spriteRenderer != null) entitySprite = spriteRenderer;
            deathSpriteColor = deathColor;
        }

        /// <summary>
        /// Shared death path — used by Player and any entity without a brain.
        /// Stops movement, applies death color, shows damage point.
        /// </summary>
        public void HandleDeath()
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            gameObject.tag = "Items";

            ApplyDeathVisual();
            ShowDeathDamagePoint();
        }

        /// <summary>
        /// Enemy overload — handles brain shutdown and NavMesh cleanup
        /// before calling the shared HandleDeath().
        /// </summary>
        public void HandleDeath(IEnemyBrain brain)
        {
            brain?.OnDeath();
            StopAllNavigation();
            DisableNavAgent();
            HandleDeath();
        }

        // ── Private helpers ───────────────────────────────────────────────

        private void StopAllNavigation()
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent == null) return;

            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity  = Vector3.zero;
        }

        private void DisableNavAgent()
        {
            if (navAgent != null)
                navAgent.enabled = false;
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
            DamagePoint damagePoint = GetComponentInChildren<DamagePoint>();
            if (damagePoint != null)
                damagePoint.ShowDeath();
        }
    }
}
