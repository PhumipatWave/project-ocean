using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
{
    public abstract class EnemyNavAgentBase : MonoBehaviour
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

        public IEnemyBrain ActiveBrain => enemyBrain;

        protected bool isEnemyDead => enemy != null && enemy.IsDeath;
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

        protected void StopAgentMovement()
        {
            if (navAgent == null)
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

            navAgent.updateRotation = false;
            navAgent.updateUpAxis = false;
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
                Host = this
            };
        }

        private bool CanTick()
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

            return !requiresNavMesh || (navAgent != null && navAgent.isOnNavMesh);
        }

        private Transform FindPlayerTransform()
        {
            if (playerTransform != null)
                return playerTransform;

            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            return playerObject != null ? playerObject.transform : null;
        }
    }
}
