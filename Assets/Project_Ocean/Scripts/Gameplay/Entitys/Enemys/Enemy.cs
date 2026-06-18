using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Decoupled channel between "an enemy died" and systems that care about kills
    /// (e.g. QuestManager). Enemy fires OnEnemyKilled; subscribers react.
    /// Mirrors the DayManager.OnDayAdvanced pattern — no enemy↔quest references.
    /// </summary>
    public static class EnemyEvents
    {
        /// <summary>Raised once when an enemy dies. Payload = the enemy's id (Enemy.EnemyId).</summary>
        public static event System.Action<string> OnEnemyKilled;

        public static void RaiseEnemyKilled(string enemyId)
        {
            OnEnemyKilled?.Invoke(enemyId);
        }
    }

    public class Enemy : Entity
    {
        [Header("Enemy Components")]
        [SerializeField] private EnemyLoot loot;
        [SerializeField] private EnemyNavAgentBase navAgentHost;

        [Header("Quest Tracking")]
        [Tooltip("Id matched against QuestData.targetEnemyId for kill quests (e.g. \"fishL\", \"jellyfish\").")]
        [SerializeField] private string enemyId;
        public string EnemyId => enemyId;

        private float maxHealth = 100f;
        private SpriteRenderer visual;
        private Animator anim;
        private Color deathSpriteColor = Color.red;

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

        [ContextMenu("Debug/Kill Enemy")]
        private void DebugKill() => TakeDamage(MaxHealth);

        protected override void OnDeathHandler()
        {
            // Fire the kill event — runs exactly once (EntityHealth.OnDied is isDead-guarded).
            if (!string.IsNullOrEmpty(enemyId))
                EnemyEvents.RaiseEnemyKilled(enemyId);

            // Register kill immediately so it's tracked even if the enemy is
            // collected (pooled) before the next Update cycle runs CanTick.
            navAgentHost?.NotifyKill();

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
            if (loot == null) loot = GetComponent<EnemyLoot>();
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
            entityAnimation?.Configure(anim);
            entityDeathHandler?.Configure(rb, visual, deathSpriteColor);
        }
    }
}
