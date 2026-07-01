using System.Collections;
using UnityEngine;

namespace Game.Testing
{
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

        [Header("Proximity")]
        [Tooltip("NavAgent and brain pause when player is farther than this. 0 = never sleep.")]
        [SerializeField] private float sleepDistance = 40f;
        [Tooltip("Return to pool when player is farther than this. 0 = never self-despawn.")]
        [SerializeField] private float despawnDistance = 60f;
        [SerializeField] private float despawnCheckInterval = 1f;

        public float SleepDistanceSqr  { get; private set; }
        private float despawnDistanceSqr;

        [Header("Quest Tracking")]
        [Tooltip("Id matched against QuestData.targetEnemyId for kill quests (e.g. \"fishL\", \"jellyfish\").")]
        [SerializeField] private string enemyId;
        public string EnemyId => enemyId;

        [SerializeField] private float maxHealth = 100f;
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
            SleepDistanceSqr   = sleepDistance   > 0f ? sleepDistance   * sleepDistance   : float.MaxValue;
            despawnDistanceSqr = despawnDistance > 0f ? despawnDistance * despawnDistance : float.MaxValue;
        }

        private void OnEnable()
        {
            if (despawnDistance > 0f)
                StartCoroutine(DespawnCheckRoutine());
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
            if (IsDead) return;
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
            Debug.Log($"Enemy Killed : {enemyId}");

            entityAnimation?.PlayAnim(AnimHash.IsDeath, true);

            if (!string.IsNullOrEmpty(enemyId))
                EnemyEvents.RaiseEnemyKilled(enemyId);

            // Register kill immediately so it's tracked even if the enemy is
            // collected (pooled) before the next Update cycle runs CanTick.
            navAgentHost?.NotifyKill();

            entityDeathHandler?.HandleDeath(brain);
            StartCoroutine(AutoReturnRoutine());
        }

        private IEnumerator AutoReturnRoutine()
        {
            float delay = navAgentHost != null ? navAgentHost.AutoReturnDelay : 60f;
            yield return new WaitForSeconds(delay);
            ObjectPoolManager.Instance?.Return(gameObject);
        }

        private IEnumerator DespawnCheckRoutine()
        {
            var wait = new WaitForSeconds(despawnCheckInterval);
            while (true)
            {
                yield return wait;
                if (IsDead) yield break;

                var nearest = PlayerRegistry.GetNearest(transform.position);
                if (nearest == null) continue;

                float sqrDist = ((Vector2)(nearest.transform.position - transform.position)).sqrMagnitude;
                if (sqrDist > despawnDistanceSqr)
                {
                    ObjectPoolManager.Instance?.Return(gameObject);
                    yield break;
                }
            }
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
            player = PlayerRegistry.Local;
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
