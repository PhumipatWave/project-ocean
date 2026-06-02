using UnityEngine;

namespace Game.Testing
{
    public class Enemy : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private EnemyHealth health;
        [SerializeField] private EnemyLoot loot;
        [SerializeField] private EnemyAnimation enemyAnimation;
        [SerializeField] private EnemyDeathHandler deathHandler;
        [SerializeField] private EnemyNavAgentBase navAgentHost;

        [SerializeField, HideInInspector] private float maxHealth = 100f;
        [SerializeField, HideInInspector] private Item dropItem;
        [SerializeField, HideInInspector] private Rigidbody2D rb;
        [SerializeField, HideInInspector] private SpriteRenderer fishSprite;
        [SerializeField, HideInInspector] private Animator anim;
        [SerializeField, HideInInspector] private bool hasAnimator;
        [SerializeField, HideInInspector] private Color deathSpriteColor = Color.red;

        private Player player;
        public Player Player => player;

        private IEnemyBrain brain;
        public IEnemyBrain Brain => brain;

        public float CurrentHealth => health != null ? health.CurrentHealth : 0f;
        public float MaxHealth => health != null ? health.MaxHealth : 1f;
        public bool IsAttacking => !IsDeath && brain != null && brain.IsAttacking;
        public bool IsDeath => health != null && health.IsDead;

        private void Awake()
        {
            CacheComponents();
            SubscribeHealth();
            InitializeNavHost();
            InitializeState();
        }

        private void OnDestroy()
        {
            UnsubscribeHealth();
        }

        private void Update()
        {
            enemyAnimation?.SetAttacking(IsAttacking);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
                return;

            if (IsDeath)
            {
                loot?.CollectByPlayer();
                return;
            }

            (brain as IBrainHitReaction)?.OnPlayerCollision();
        }

        public void RegisterNavAgent(EnemyNavAgentBase agent)
        {
            if (agent == null)
                return;

            agent.Initialize(this);
            navAgentHost = agent;
            brain = agent.ActiveBrain;
        }

        public void TakeDamage(float amount)
        {
            health?.TakeDamage(amount);
        }

        private void CacheComponents()
        {
            if (health == null)
                health = GetOrAddComponent<EnemyHealth>();

            if (loot == null)
                loot = GetOrAddComponent<EnemyLoot>();

            if (enemyAnimation == null)
                enemyAnimation = GetOrAddComponent<EnemyAnimation>();

            if (deathHandler == null)
                deathHandler = GetOrAddComponent<EnemyDeathHandler>();

            if (navAgentHost == null)
                navAgentHost = GetComponent<EnemyNavAgentBase>();

            ApplyLegacySerializedValues();
        }

        private void SubscribeHealth()
        {
            if (health == null)
                return;

            health.OnDamaged += OnDamaged;
            health.OnDied += OnDied;
        }

        private void UnsubscribeHealth()
        {
            if (health == null)
                return;

            health.OnDamaged -= OnDamaged;
            health.OnDied -= OnDied;
        }

        private void InitializeNavHost()
        {
            if (navAgentHost == null)
                navAgentHost = GetComponent<EnemyNavAgentBase>();

            navAgentHost?.Initialize(this);
        }

        private void InitializeState()
        {
            player = GameManager.CurPlayer != null
                ? GameManager.CurPlayer.GetComponent<Player>()
                : null;

            health?.Initialize();
        }

        private void OnDamaged()
        {
            enemyAnimation?.PlayTakeDamage();
        }

        private void OnDied()
        {
            deathHandler?.HandleDeath(brain);
            enemyAnimation?.PlayDeath();
        }

        private void ApplyLegacySerializedValues()
        {
            health?.Configure(maxHealth);
            loot?.Configure(dropItem);
            enemyAnimation?.Configure(hasAnimator, anim);
            deathHandler?.Configure(rb, fishSprite, deathSpriteColor);
        }

        private T GetOrAddComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
