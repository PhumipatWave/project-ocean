using UnityEngine;

namespace Game.Testing
{
    public class Enemy : Entity
    {
        [Header("Enemy Components")]
        [SerializeField] private EnemyLoot loot;
        [SerializeField] private EnemyNavAgentBase navAgentHost;

        [SerializeField, HideInInspector] private float maxHealth = 100f;
        [SerializeField, HideInInspector] private Item dropItem;
        [SerializeField, HideInInspector] private SpriteRenderer fishSprite;
        [SerializeField, HideInInspector] private Animator anim;
        [SerializeField, HideInInspector] private bool hasAnimator;
        [SerializeField, HideInInspector] private Color deathSpriteColor = Color.red;

        private Player player;
        public Player Player => player;

        private IEnemyBrain brain;
        public IEnemyBrain Brain => brain;

        public bool IsAttacking => !IsDead && brain != null && brain.IsAttacking;

        protected override void Awake()
        {
            base.Awake();
            CacheEnemyComponents();
            ApplyLegacySerializedValues();
            InitializeNavHost();
            InitializeState();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void Update()
        {
            entityAnimation?.PlayAnim(AnimHash.IsAttack, IsAttacking);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;
            if (IsDead) { loot?.CollectByPlayer(); return; }
            (brain as IBrainHitReaction)?.OnPlayerCollision();
        }

        protected override void OnDamagedHandler()
        {
            (brain as IBrainHitReaction)?.OnHit();
            entityAnimation?.PlayAnim(AnimHash.TakeDamage);
        }

        protected override void OnDeathHandler()
        {
            entityDeathHandler?.HandleDeath(brain);
        }

        public void RegisterNavAgent(EnemyNavAgentBase agent)
        {
            if (agent == null) return;
            agent.Initialize(this);
            navAgentHost = agent;
            brain = agent.ActiveBrain;
        }

        private void CacheEnemyComponents()
        {
            if (loot == null) loot = GetOrAddComponent<EnemyLoot>();
            if (navAgentHost == null) navAgentHost = GetComponent<EnemyNavAgentBase>();
        }

        private void InitializeNavHost()
        {
            if (navAgentHost == null) navAgentHost = GetComponent<EnemyNavAgentBase>();
            navAgentHost?.Initialize(this);
        }

        private void InitializeState()
        {
            player = GameManager.CurPlayer != null
                ? GameManager.CurPlayer.GetComponent<Player>() : null;
            healthSource?.Initialize();
        }

        private void ApplyLegacySerializedValues()
        {
            (healthSource as EntityHealth)?.Configure(maxHealth);
            loot?.Configure(dropItem);
            entityAnimation?.Configure(anim);
            entityDeathHandler?.Configure(rb, fishSprite, deathSpriteColor);
        }
    }
}
