using UnityEngine;
using UnityEngine.AI;
using Game.Core;

namespace Game.Gameplay
{
    public abstract class EnemyNavAgentBase : MonoBehaviour, IPoolable
    {
        [Header("Alert UI")]
        [SerializeField] protected FishAlertUI fishAlertUI;

        [Header("Hit Receiver")]
        [SerializeField] protected string playerBulletTag = "Bullet";
        [SerializeField] protected string playerTag = "Player";

        [Header("Facing")]
        [SerializeField] protected Transform visualFlipRoot;
        [SerializeField] protected float turnSmoothTime = 0.25f;
        [SerializeField] protected float maxTiltAngle = 360f;
        [SerializeField] protected float flipDeadZone = 0.1f;
        [SerializeField] protected float flipDelay = 0.4f;
        [SerializeField] protected float flipCooldown = 0.5f;
        [SerializeField] protected float spriteAngleOffset = 0f;
        [SerializeField] protected float attackAngleThreshold = 15f;
        [SerializeField] protected EnemyFacingController.FacingTurnMode facingTurnMode = EnemyFacingController.FacingTurnMode.SmoothTime;
        [SerializeField] protected float turnSpeed = 5f;

        protected Enemy enemy { get; private set; }
        protected NavMeshAgent navAgent { get; private set; }
        protected EnemyFacingController facingCtr { get; private set; }

        private IEnemyBrain enemyBrain;
        private bool deathHandled;
        private bool isDormant;
        private string spawnId;

        public IEnemyBrain ActiveBrain => enemyBrain;

        /// Stable ID assigned by EnemySpawnManager — used to track kills across sessions.
        public void SetSpawnId(string id) => spawnId = id;

        /// Called synchronously from Enemy.OnDeathHandler so the kill is recorded
        /// before any pooling/collection can clear the spawnId.
        public void NotifyKill()
        {
            if (string.IsNullOrEmpty(spawnId)) return;
            if (EnemyKillTracker.Instance == null)
            {
                Debug.LogError($"[EnemyNavAgentBase] EnemyKillTracker.Instance is null — kill for '{spawnId}' was NOT registered. Add EnemyKillTracker to the persistent Bootstrap GameObject.");
                return;
            }
            EnemyKillTracker.Instance.RegisterKill(spawnId);
            GameLog.Info($"[EnemyNavAgentBase] Kill registered: '{spawnId}'");
        }

        /// The fish type ID from the brain profile (e.g. "fishM").
        public string FishId => GetFishId();

        /// Seconds after death before auto-returning to pool. Read by Enemy.
        public virtual float AutoReturnDelay => 60f;

        protected bool isEnemyDead => enemy != null && enemy.IsDead;
        protected Player player => enemy != null ? enemy.Player : null;
        protected Transform playerTransform => player != null ? player.transform : null;

        protected virtual bool requiresPlayer => true;
        protected virtual bool requiresNavMesh => true;
        protected virtual EnemyFacingController.FacingTurnMode defaultFacingTurnMode => facingTurnMode;

        protected virtual void Awake()
        {
            enemy = GetComponent<Enemy>();
        }

        protected virtual void Start()
        {
            if (enemy == null)
                enemy = GetComponent<Enemy>();

            navAgent = GetComponent<NavMeshAgent>();
            ConfigureNavAgent();

            facingCtr = new EnemyFacingController(
                transform,
                visualFlipRoot,
                turnSmoothTime,
                maxTiltAngle,
                flipDeadZone,
                flipDelay,
                flipCooldown,
                spriteAngleOffset,
                attackAngleThreshold,
                defaultFacingTurnMode,
                turnSpeed
            );
            facingCtr.Initialize();

            enemyBrain = CreateBrain();
            enemyBrain.Initialize(BuildBrainContext());
            OnBrainInitialized();

            enemy?.RegisterNavAgent(this);
        }

        protected virtual void Update()
        {
            if (!CanTick())
                return;

            OnTick();
            enemyBrain.Tick(Time.deltaTime);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (isEnemyDead)
                return;

            if (other.CompareTag(playerBulletTag))
                HandleBulletHit(other.gameObject);

            if (other.CompareTag(playerTag))
                HandlePlayerCollision();
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (isEnemyDead)
                return;

            if (collision.collider.CompareTag(playerBulletTag))
                HandleBulletHit(collision.collider.gameObject);
        }

        public virtual void Initialize(Enemy owner)
        {
            enemy = owner;
        }

        protected virtual string GetFishId() => string.Empty;

        protected abstract IEnemyBrain CreateBrain();

        protected virtual void OnBrainInitialized() { }
        protected virtual void OnTick() { }
        protected virtual void HandlePlayerCollision() { }

        protected virtual void HandleBulletHit(GameObject bullet)
        {
            if (isEnemyDead)
                return;

            if (bullet == null || !bullet.CompareTag(playerBulletTag))
                return;

            enemyBrain?.OnHit(FindPlayerTransform());
        }

        protected virtual void OnEnemyUnavailable()
        {
            fishAlertUI?.HideAll();
        }

        protected virtual void OnEnemyDead()
        {
            if (deathHandled)
                return;

            deathHandled = true;
            enemyBrain?.OnDeath();
            OnEnemyUnavailable();
            StopAgentMovement();
        }

        /// <summary>Stops AI, movement and schooling for good; OnGetFromPool undoes it.</summary>
        public void StopForDeath()
        {
            OnEnemyDead();

            var boid = GetComponent<Boid>();
            if (boid != null)
                boid.enabled = false;

            enabled = false;
        }

        protected void StopAgentMovement()
        {
            if (navAgent == null || !navAgent.isOnNavMesh)
                return;

            navAgent.isStopped = true;
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
        }

        protected void TickFacingFromAgentVelocity(float minSpeed = 0.01f)
        {
            if (navAgent == null)
                return;

            facingCtr.RotateFromVelocity(navAgent.velocity, minSpeed);
        }

        private void ConfigureNavAgent()
        {
            if (navAgent == null)
                return;

            if (!requiresNavMesh)
            {
                navAgent.enabled = false;
                navAgent = null;
                return;
            }

            navAgent.updateRotation = false;
            navAgent.updateUpAxis   = false;
        }

        private EnemyBrainContext BuildBrainContext()
        {
            return new EnemyBrainContext
            {
                Owner = enemy,
                Transform = transform,
                Agent = navAgent,
                Facing = facingCtr,
                AlertUI = fishAlertUI,
                Host = this,
                School = GetComponent<Boid>()
            };
        }

        private bool CanTick()
        {
            if (enemyBrain == null) return false;

            if (isEnemyDead) { OnEnemyDead(); return false; }

            if (requiresPlayer && playerTransform == null) { OnEnemyUnavailable(); return false; }

            if (requiresNavMesh && (navAgent == null || !navAgent.isOnNavMesh)) return false;

            // Dormancy — pause AI when player is far away (threshold set on Enemy).
            if (playerTransform != null)
            {
                float sqrDist = ((Vector2)(playerTransform.position - transform.position)).sqrMagnitude;
                float sleepSqr = enemy != null ? enemy.SleepDistanceSqr : float.MaxValue;
                if (sqrDist > sleepSqr)
                {
                    if (!isDormant) EnterDormancy();
                    return false;
                }
                if (isDormant) ExitDormancy();
            }

            return true;
        }

        private void EnterDormancy()
        {
            isDormant = true;
            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                navAgent.ResetPath();
            }
        }

        private void ExitDormancy()
        {
            isDormant = false;
            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = false;
                navAgent.ResetPath(); // clear stale path; FSM will pick a fresh destination
            }
        }

        // ── IPoolable ──

        public void OnGetFromPool()
        {
            deathHandled = false;
            isDormant = false;
            enabled = true;
            StopAllCoroutines();
            fishAlertUI?.HideAll();

            if (navAgent != null)
            {
                navAgent.enabled = true;
                if (navAgent.isOnNavMesh) navAgent.isStopped = false;
            }

            enemyBrain?.Initialize(BuildBrainContext());
        }

        public void OnReturnToPool()
        {
            spawnId         = null;
            deathHandled    = false;
            StopAllCoroutines();
            fishAlertUI?.HideAll();
        }

        private Transform FindPlayerTransform()
        {
            if (playerTransform != null)
                return playerTransform;

            var nearest = PlayerRegistry.GetNearest(transform.position);
            return nearest != null ? nearest.transform : null;
        }
    }
}
