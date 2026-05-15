using Game.Testing;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMesh host: facing, hits, and <see cref="IEnemyBrain"/> execution.
/// </summary>
public abstract class EnemyNavAgentBase : MonoBehaviour
{
    [Header("Data (optional)")]
    [SerializeField] protected EnemyConfig config;

    [Header("Alert UI (optional)")]
    [SerializeField] protected FishAlertUI fishAlertUI;

    [Header("Hit Receiver")]
    [SerializeField] protected string playerBulletTag = "Bullet";
    [SerializeField] protected string playerTag = "Player";
    [SerializeField] protected bool destroyBulletOnHit = true;

    [Header("Facing")]
    [SerializeField] protected Transform visualFlipRoot;
    [SerializeField] protected float turnSmoothTime = 0.25f;
    [SerializeField] protected float maxTiltAngle = 360f;
    [SerializeField] protected float flipDeadZone = 0.1f;
    [SerializeField] protected float flipDelay = 0.4f;
    [SerializeField] protected float flipCooldown = 0.5f;
    [SerializeField] protected float spriteAngleOffset = 0f;
    [SerializeField] protected float attackAngleThreshold = 15f;
    [SerializeField] protected EnemyFacingController.FacingTurnMode facingTurnMode =
        EnemyFacingController.FacingTurnMode.SmoothTime;
    [SerializeField] protected float turnSpeed = 5f;

    protected Enemy Owner { get; private set; }
    protected NavMeshAgent Agent { get; private set; }
    protected EnemyFacingController Facing { get; private set; }
    protected IEnemyBrain Brain { get; private set; }

    private bool deathHandled;

    public IEnemyBrain ActiveBrain => Brain;

    protected bool IsEnemyDead => Owner != null && Owner.IsDeath;

    protected Player Player => Owner != null ? Owner.Player : null;

    protected Transform PlayerTransform =>
        Player != null ? Player.transform : null;

    protected virtual bool RequiresPlayer => true;
    protected virtual bool RequiresNavMesh => true;

    protected virtual EnemyFacingController.FacingTurnMode DefaultFacingTurnMode =>
        facingTurnMode;

    public virtual void Initialize(Enemy owner)
    {
        Owner = owner;

        if (config == null && owner != null)
            config = owner.Config;
    }

    protected abstract IEnemyBrain CreateBrain();

    protected virtual void Awake()
    {
        if (Owner == null)
            Owner = GetComponent<Enemy>();
    }

    protected virtual void Start()
    {
        if (Owner == null)
            Owner = GetComponent<Enemy>();

        if (config == null && Owner != null)
            config = Owner.Config;

        Agent = GetComponent<NavMeshAgent>();

        if (Agent != null)
        {
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
        }

        Facing = new EnemyFacingController(
            transform,
            visualFlipRoot,
            turnSmoothTime,
            maxTiltAngle,
            flipDeadZone,
            flipDelay,
            flipCooldown,
            spriteAngleOffset,
            attackAngleThreshold,
            DefaultFacingTurnMode,
            turnSpeed
        );
        Facing.Initialize();

        Brain = CreateBrain();
        Brain.Initialize(BuildBrainContext());
        OnBrainInitialized();

        if (Owner != null)
            Owner.RegisterNavAgent(this);
    }

    protected virtual void OnBrainInitialized() { }

    protected EnemyBrainContext BuildBrainContext()
    {
        return new EnemyBrainContext
        {
            Owner = Owner,
            Config = config,
            Transform = transform,
            Agent = Agent,
            Facing = Facing,
            AlertUI = fishAlertUI,
            Host = this
        };
    }

    protected virtual void Update()
    {
        if (!CanTick())
            return;

        Brain.Tick(Time.deltaTime);
    }

    protected virtual bool CanTick()
    {
        if (Brain == null)
            return false;

        if (IsEnemyDead)
        {
            OnEnemyDead();
            return false;
        }

        if (RequiresPlayer && PlayerTransform == null)
        {
            OnEnemyUnavailable();
            return false;
        }

        if (RequiresNavMesh && (Agent == null || !Agent.isOnNavMesh))
            return false;

        return true;
    }

    protected virtual void OnEnemyDead()
    {
        if (deathHandled)
            return;

        deathHandled = true;
        Brain?.OnDeath();
        OnEnemyUnavailable();
        StopAgentMovement();
    }

    protected virtual void OnEnemyUnavailable()
    {
        if (fishAlertUI != null)
            fishAlertUI.HideAll();
    }

    protected void StopAgentMovement()
    {
        if (Agent == null)
            return;

        Agent.isStopped = true;
        Agent.ResetPath();
        Agent.velocity = Vector3.zero;
    }

    protected void TickFacingFromAgentVelocity(float minSpeed = 0.01f)
    {
        if (Agent == null)
            return;

        Facing.RotateFromVelocity(Agent.velocity, minSpeed);
    }

    protected Transform FindPlayerTransform()
    {
        if (PlayerTransform != null)
            return PlayerTransform;

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        return playerObject != null ? playerObject.transform : null;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (IsEnemyDead)
            return;

        if (other.CompareTag(playerBulletTag))
            HandleBulletHit(other.gameObject);

        if (other.CompareTag(playerTag))
            HandlePlayerCollision();
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsEnemyDead)
            return;

        if (collision.collider.CompareTag(playerBulletTag))
            HandleBulletHit(collision.collider.gameObject);
    }

    protected virtual void HandleBulletHit(GameObject bullet)
    {
        if (IsEnemyDead)
            return;

        if (bullet == null || !bullet.CompareTag(playerBulletTag))
            return;

        OnHitByPlayer(FindPlayerTransform());

        if (destroyBulletOnHit)
            Destroy(bullet);
    }

    protected virtual void HandlePlayerCollision() { }

    protected virtual void OnHitByPlayer(Transform attacker)
    {
        Brain?.OnHit(attacker);
    }
}
