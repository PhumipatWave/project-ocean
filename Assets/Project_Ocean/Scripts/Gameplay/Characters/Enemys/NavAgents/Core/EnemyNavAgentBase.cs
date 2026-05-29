using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
{
    public abstract class EnemyNavAgentBase : MonoBehaviour
    {
        #region FieldProperties
        // Merge to scriptable brain later***** (health, damage, speed, drop item).
        [Header("Data (optional)")]
        [SerializeField] protected EnemyConfig config;
        //*****

        [Header("Alert UI (optional)")]
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

        protected IEnemyBrain enemyBrain { get; private set; }
        public IEnemyBrain ActiveBrain => enemyBrain;

        private bool deathHandled;

        protected bool isEnemyDead => enemy != null && enemy.IsDeath;

        protected Player player => enemy != null ? enemy.Player : null;
        protected Transform playerTransform => player != null ? player.transform : null;

        protected virtual bool requiresPlayer => true;
        protected virtual bool requiresNavMesh => true;

        protected virtual EnemyFacingController.FacingTurnMode defaultFacingTurnMode => facingTurnMode;
        #endregion

        #region UnityCallbacks
        protected virtual void Awake()
        {
            if (enemy == null)
                enemy = GetComponent<Enemy>();
        }

        protected virtual void Start()
        {
            if (enemy == null)
                enemy = GetComponent<Enemy>();

            if (config == null && enemy != null)
                config = enemy.Config;

            navAgent = GetComponent<NavMeshAgent>();

            if (navAgent != null)
            {
                navAgent.updateRotation = false;
                navAgent.updateUpAxis = false;
            }

            facingCtr = new EnemyFacingController(transform, visualFlipRoot, turnSmoothTime, maxTiltAngle, flipDeadZone, flipDelay, flipCooldown, spriteAngleOffset, attackAngleThreshold, defaultFacingTurnMode, turnSpeed);
            facingCtr.Initialize();

            enemyBrain = CreateBrain();
            enemyBrain.Initialize(BuildBrainContext());
            OnBrainInitialized();

            if (enemy != null)
                enemy.RegisterNavAgent(this);
        }

        protected virtual void Update()
        {
            if (!CanTick())
                return;

            // ✅ เพิ่ม hook ให้ subclass ใส่ logic ก่อน brain tick ได้
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
        #endregion

        #region Initialization
        public virtual void Initialize(Enemy owner)
        {
            enemy = owner;

            if (config == null && owner != null)
                config = owner.Config;
        }

        protected abstract IEnemyBrain CreateBrain();

        protected virtual void OnBrainInitialized() { }

        /// <summary>
        /// เรียกทุก frame ก่อน brain.Tick() — override ใน subclass เพื่อเพิ่ม steering หรือ logic อื่น
        /// </summary>
        protected virtual void OnTick() { }

        protected EnemyBrainContext BuildBrainContext()
        {
            return new EnemyBrainContext
            {
                Owner = enemy,
                Config = config,
                Transform = transform,
                Agent = navAgent,
                Facing = facingCtr,
                AlertUI = fishAlertUI,
                Host = this
            };
        }

        protected virtual bool CanTick()
        {
            if (enemyBrain == null)
                return false;

            if (isEnemyDead)
            {
                OnEnemyDead();
                return false;
            }

            if (requiresPlayer && playerTransform == null)
            {
                OnEnemyUnavailable();
                return false;
            }

            if (requiresNavMesh && (navAgent == null || !navAgent.isOnNavMesh))
                return false;

            return true;
        }
        #endregion

        protected Transform FindPlayerTransform()
        {
            if (playerTransform != null)
                return playerTransform;

            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            return playerObject != null ? playerObject.transform : null;
        }

        protected virtual void OnEnemyUnavailable()
        {
            if (fishAlertUI != null)
                fishAlertUI.HideAll();
        }

        protected void StopAgentMovement()
        {
            if (navAgent == null)
                return;

            navAgent.isStopped = true;
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
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

        protected void TickFacingFromAgentVelocity(float minSpeed = 0.01f)
        {
            if (navAgent == null)
                return;

            facingCtr.RotateFromVelocity(navAgent.velocity, minSpeed);
        }

        protected virtual void HandleBulletHit(GameObject bullet)
        {
            if (isEnemyDead)
                return;

            if (bullet == null || !bullet.CompareTag(playerBulletTag))
                return;

            OnHitByPlayer(FindPlayerTransform());
        }

        protected virtual void HandlePlayerCollision() { }

        protected virtual void OnHitByPlayer(Transform attacker)
        {
            enemyBrain?.OnHit(attacker);
        }
    }
}
