using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
{
    /// <summary>
    /// Main enemy controller: health, death, visuals, loot, and brain coordination.
    /// </summary>
    [DisallowMultipleComponent]
    public class Enemy : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private EnemyConfig config;

        [Header("References")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer fishSprite;
        [SerializeField] private Animator anim;
        [SerializeField] private Item dropItem;

        [Header("Health Override (if no config asset)")]
        [SerializeField] private float maxHealthFallback = 100f;

        [Header("Animation")]
        [SerializeField] private bool isHasAnim;

        [Header("Death")]
        [SerializeField] private Color deathSpriteColor = Color.red;

        [Header("Flags")]
        [SerializeField] private bool isBullet;

        private float currentHealth;
        private bool isDeath;
        private Player player;

        private EnemyNavAgentBase navHost;
        private IEnemyBrain brain;

        public EnemyConfig Config => config;

        public Player Player => player;

        public float CurrentHealth => currentHealth;

        public float MaxHealth => config != null ? config.maxHealth : maxHealthFallback;

        public bool IsDeath => isDeath;

        public IEnemyBrain Brain => brain;

        public bool IsBullet
        {
            get => isBullet;
            set => isBullet = value;
        }

        public bool IsAttacking => !isDeath && brain != null && brain.IsAttacking;

        private void Awake()
        {
            CacheComponents();
            InitializeNavHost();
            InitializeState();
        }

        private void Update()
        {
            if (!isDeath && currentHealth <= 0f)
                Die();

            if (isDeath)
                return;

            SyncAttackAnimation();
        }

        private void FixedUpdate()
        {
            if (isBullet)
                return;
        }

        public void TakeDamage(float amount)
        {
            if (isDeath)
                return;

            currentHealth -= amount;

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Die();
                return;
            }

            PlayTakeDamageAnimation();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isDeath || !collision.gameObject.CompareTag("Player"))
                return;

            CollectByPlayer();
        }

        public void RegisterNavAgent(EnemyNavAgentBase agent)
        {
            if (agent == null)
                return;

            agent.Initialize(this);
            navHost = agent;
            brain = agent.ActiveBrain;
        }

        private void CacheComponents()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            if (fishSprite == null)
            {
                Transform visual = transform.Find("Visual");
                if (visual != null)
                    fishSprite = visual.GetComponent<SpriteRenderer>();
            }

            if (fishSprite == null)
                fishSprite = GetComponentInChildren<SpriteRenderer>();

            if (isHasAnim && anim == null)
                anim = GetComponentInChildren<Animator>();

            if (navHost == null)
                navHost = GetComponent<EnemyNavAgentBase>();
        }

        private void InitializeNavHost()
        {
            if (navHost == null)
                navHost = GetComponent<EnemyNavAgentBase>();

            if (navHost != null)
                navHost.Initialize(this);
        }

        private void InitializeState()
        {
            player = GameManager.CurPlayer != null
                ? GameManager.CurPlayer.GetComponent<Player>()
                : null;

            currentHealth = MaxHealth;
            isDeath = false;
        }

        private void Die()
        {
            if (isDeath)
                return;

            isDeath = true;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            gameObject.tag = "Items";

            brain?.OnDeath();
            StopAllNavigation();
            DisableNavHost();
            ApplyDeathVisual();

            DamagePoint damagePoint = GetComponentInChildren<DamagePoint>();
            if (damagePoint != null)
                damagePoint.ShowDeath();

            PlayDeathAnimation();
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

        private void DisableNavHost()
        {
            if (navHost != null)
                navHost.enabled = false;
        }

        private void ApplyDeathVisual()
        {
            if (fishSprite == null)
                return;

            Color color = deathSpriteColor;
            color.a = fishSprite.color.a;
            fishSprite.color = color;
        }

        private void CollectByPlayer()
        {
            if (GameManager.CurPlayer != null && GameManager.CurPlayer.Audio != null)
                GameManager.CurPlayer.Audio.PlayOneShot(GameManager.CurPlayer.playerCollectItemSound);

            if (dropItem != null)
                InventoryManager.Instance.AddItem(dropItem);

            Destroy(gameObject);
        }

        private void SyncAttackAnimation()
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
    }
}
