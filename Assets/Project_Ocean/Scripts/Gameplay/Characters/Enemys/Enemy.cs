using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
{
    /// <summary>
    /// Main enemy controller: health, death, visuals, loot, and brain coordination.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        #region FieldProperties
        // Merge to scriptable brain later***** (health, damage, speed, drop item).
        [SerializeField] private EnemyConfig config;
        public EnemyConfig Config => config;
        // Health Override (if no config asset)
        [SerializeField] private float maxHealthFallback = 100f;
        //*****

        [Header("References")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer fishSprite;
        [SerializeField] private Animator anim;

        // Merge to scriptable brain later***** (health, damage, speed, drop item).
        [SerializeField] private Item dropItem;
        //*****

        // For now.
        [SerializeField] private bool isHasAnim;

        [Header("Death")]
        [SerializeField] private Color deathSpriteColor = Color.red;

        private float currentHealth;
        private bool isDeath;

        private Player player;
        public Player Player => player;

        private EnemyNavAgentBase navBase;

        private IEnemyBrain brain;
        public IEnemyBrain Brain => brain;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => config != null ? config.maxHealth : maxHealthFallback;
        public bool IsAttacking => !isDeath && brain != null && brain.IsAttacking;
        public bool IsDeath => isDeath;
        #endregion

        #region UnityCallbacks
        private void Awake()
        {
            RecheckComponent();
            InitializeNavHost();
            InitializeState();
        }

        private void Update()
        {
            if (!isDeath && currentHealth <= 0f)
                Death();

            if (isDeath)
                return;

            PlayAttackAnimation();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
                return;

            if (isDeath)
            {
                CollectByPlayer();
                return;
            }

            // Live fish touched by player — notify brain but do NOT collect
            (brain as IBrainHitReaction)?.OnPlayerCollision();
        }
        #endregion

        #region Initialization
        private void RecheckComponent()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            Transform visual = transform.Find("Visual");
            fishSprite = visual != null
                ? visual.GetComponent<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>();

            if (isHasAnim && anim == null)
                anim = GetComponentInChildren<Animator>();

            if (navBase == null)
                navBase = GetComponent<EnemyNavAgentBase>();
        }

        private void InitializeNavHost()
        {
            if (navBase == null)
                navBase = GetComponent<EnemyNavAgentBase>();

            if (navBase != null)
                navBase.Initialize(this);
        }

        private void InitializeState()
        {
            player = GameManager.CurPlayer != null ? GameManager.CurPlayer.GetComponent<Player>() : null;

            currentHealth = MaxHealth;
            isDeath = false;
        }

        public void RegisterNavAgent(EnemyNavAgentBase agent)
        {
            if (agent == null)
                return;

            agent.Initialize(this);
            navBase = agent;
            brain = agent.ActiveBrain;
        }
        #endregion

        #region EnemyActions
        public void TakeDamage(float amount)
        {
            if (isDeath)
                return;

            currentHealth -= amount;

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Death();
                return;
            }

            PlayTakeDamageAnimation();
        }

        private void DisableNavAgent()
        {
            if (navBase != null)
                navBase.enabled = false;
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
        private void ApplyDeathVisual()
        {
            if (fishSprite == null)
                return;

            Color color = deathSpriteColor;
            color.a = fishSprite.color.a;
            fishSprite.color = color;
        }

        private void Death()
        {
            if (isDeath)
                return;

            isDeath = true;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            gameObject.tag = "Items";

            brain?.OnDeath();
            StopAllNavigation();
            DisableNavAgent();
            ApplyDeathVisual();

            DamagePoint damagePoint = GetComponentInChildren<DamagePoint>();
            if (damagePoint != null)
                damagePoint.ShowDeath();

            PlayDeathAnimation();
        }

        private void CollectByPlayer()
        {
            if (GameManager.CurPlayer != null && GameManager.CurPlayer.Audio != null)
                GameManager.CurPlayer.Audio.PlayOneShot(GameManager.CurPlayer.playerCollectItemSound);

            if (dropItem != null)
                InventoryManager.Instance.AddItem(dropItem);

            Destroy(gameObject);
        }
        #endregion

        #region Animations
        private void PlayAttackAnimation()
        {
            if (!isHasAnim || anim == null)
                return;

            anim.SetBool("isAttack_01", IsAttacking);
        }

        private void PlayTakeDamageAnimation()
        {
            if (!isHasAnim || anim == null)
                return;

            anim.SetTrigger("takeDamage");
        }

        private void PlayDeathAnimation()
        {
            if (!isHasAnim || anim == null)
                return;

            anim.SetBool("isDeath", true);
        }
        #endregion
    }
}
