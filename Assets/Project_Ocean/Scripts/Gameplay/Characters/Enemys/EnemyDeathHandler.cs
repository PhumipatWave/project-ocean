using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
{
    public class EnemyDeathHandler : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer fishSprite;
        [SerializeField] private Color deathSpriteColor = Color.red;

        private EnemyNavAgentBase navAgent;

        private void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            if (fishSprite == null)
            {
                Transform visual = transform.Find("Visual");
                fishSprite = visual != null
                    ? visual.GetComponent<SpriteRenderer>()
                    : GetComponentInChildren<SpriteRenderer>();
            }

            navAgent = GetComponent<EnemyNavAgentBase>();
        }

        public void Configure(Rigidbody2D rigidbody2D, SpriteRenderer spriteRenderer, Color deathColor)
        {
            if (rigidbody2D != null)
                rb = rigidbody2D;

            if (spriteRenderer != null)
                fishSprite = spriteRenderer;

            deathSpriteColor = deathColor;
        }

        public void HandleDeath(IEnemyBrain brain)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            gameObject.tag = "Items";

            brain?.OnDeath();
            StopAllNavigation();
            DisableNavAgent();
            ApplyDeathVisual();
            ShowDeathDamagePoint();
        }

        private void StopAllNavigation()
        {
            NavMeshAgent navAgent = GetComponent<NavMeshAgent>();
            if (navAgent == null)
                return;

            navAgent.isStopped = true;
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
        }

        private void DisableNavAgent()
        {
            if (navAgent != null)
                navAgent.enabled = false;
        }

        private void ApplyDeathVisual()
        {
            if (fishSprite == null)
                return;

            Color color = deathSpriteColor;
            color.a = fishSprite.color.a;
            fishSprite.color = color;
        }

        private void ShowDeathDamagePoint()
        {
            DamagePoint damagePoint = GetComponentInChildren<DamagePoint>();
            if (damagePoint != null)
                damagePoint.ShowDeath();
        }
    }
}
