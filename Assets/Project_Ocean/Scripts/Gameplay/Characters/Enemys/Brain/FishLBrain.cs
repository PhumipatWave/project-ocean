using UnityEngine;

/// <summary>
/// Large fish AI: wander, search, prepare, attack, and follow.
/// </summary>
public class FishLBrain : EnemyBrainBase, IBrainVision, IBrainAttack, IBrainHitReaction
{
    private enum BehaviourState
    {
        Wander,
        Prepare,
        Attack,
        Follow,
        Search
    }

    private readonly FishLBrainSettings settings;
    private EnemyNavPathfinder pathfinder;

    private BehaviourState state = BehaviourState.Wander;
    private bool isAttacking;

    private float prepareTimer;
    private float attackTimer;
    private float followTimer;
    private float searchTimer;
    private float wanderTimer;
    private float hitChaseTimer;

    private Vector3 wanderTarget;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 lastSeenPosition;
    private bool wasPlayerInDetectRadius;
    private bool couldSeePlayerLastTick;

    public override bool IsAttacking => isAttacking;

    #region IBrainVision
    public float ViewDistance => settings.viewDistance;
    public float ViewAngle => settings.viewAngle;
    public bool CanSeePlayer()
    {
        if (Ctx?.PlayerTransform == null) return false;
        float dist = Vector2.Distance(Ctx.Transform.position, Ctx.PlayerTransform.position);
        return CheckLineOfSight(dist);
    }
    #endregion

    #region IBrainHitReaction
    public void OnBulletHit() => OnHit(Ctx?.PlayerTransform);
    public void OnPlayerCollision() => OnHit(Ctx?.PlayerTransform);
    #endregion

    #region IBrainAttack
    public float AttackRange => settings.attackRange;
    public void TryAttack()
    {
        if (Ctx?.PlayerTransform == null || attackTimer > 0f) return;
        float dist = Vector2.Distance(Ctx.Transform.position, Ctx.PlayerTransform.position);
        if (dist > settings.attackRange) return;
        Ctx.Player.TakeDamage(settings.attackDamage);
        EndAttack(applyCooldown: true);
    }
    #endregion

    public FishLBrain(FishLBrainSettings settings)
    {
        this.settings = settings;
    }

    public override void Initialize(EnemyBrainContext context)
    {
        base.Initialize(context);

        pathfinder = new EnemyNavPathfinder(
            Ctx.Agent,
            settings.wallLayer,
            settings.fishClearanceRadius,
            settings.maxWanderSampleTries,
            settings.nearPlayerSearchRadius,
            settings.nearPlayerSearchSamples,
            settings.nearPlayerSearchStep
        );

        if (Ctx.PlayerTransform != null)
            lastKnownPlayerPosition = Ctx.PlayerTransform.position;

        HideAllAlerts();
        wanderTimer = settings.wanderInterval;
        PickNewWanderPoint();
    }

    public override void Tick(float deltaTime)
    {
        if (Ctx.PlayerTransform == null || Ctx.Agent == null)
            return;

        float dist = Vector2.Distance(Ctx.Transform.position, Ctx.PlayerTransform.position);
        bool canSeePlayer = CheckLineOfSight(dist);
        bool alertedByHit = hitChaseTimer > 0f;

        SetSeeingPlayer(canSeePlayer);

        if (hitChaseTimer > 0f)
            hitChaseTimer -= deltaTime;

        if (canSeePlayer || alertedByHit)
        {
            lastKnownPlayerPosition = Ctx.PlayerTransform.position;
            followTimer = settings.followAfterLost;
        }

        if (attackTimer > 0f)
            attackTimer -= deltaTime;

        if (followTimer > 0f)
            followTimer -= deltaTime;

        UpdateStateTransitions(canSeePlayer, alertedByHit, dist);

        switch (state)
        {
            case BehaviourState.Prepare:
                TickPrepare();
                break;
            case BehaviourState.Attack:
                TickAttack();
                break;
            case BehaviourState.Follow:
                TickFollow();
                break;
            case BehaviourState.Search:
                TickSearch(deltaTime);
                break;
            default:
                TickWander(deltaTime);
                break;
        }

        AvoidWallWhileMoving();
        TickFacing();
    }

    public override void OnHit(Transform attacker)
    {
        if (!settings.chasePlayerWhenHit)
            return;

        if (attacker != null)
            lastKnownPlayerPosition = attacker.position;
        else if (Ctx.PlayerTransform != null)
            lastKnownPlayerPosition = Ctx.PlayerTransform.position;
        else
            return;

        if (IsCommittedToAttack)
        {
            hitChaseTimer = settings.hitChaseDuration;
            followTimer = settings.followAfterLost;
            return;
        }

        hitChaseTimer = settings.hitChaseDuration;
        followTimer = settings.followAfterLost;
        attackTimer = 0f;
        isAttacking = false;
        state = BehaviourState.Follow;

        ShowAttackWarning();

        if (Ctx.Agent.isOnNavMesh)
        {
            Ctx.Agent.isStopped = false;
            Ctx.Agent.speed = settings.boostSpeed;

            if (!pathfinder.SetSafeDestination(lastKnownPlayerPosition))
                pathfinder.SetSafeDestinationNear(lastKnownPlayerPosition);
        }

        Vector2 dir = lastKnownPlayerPosition - Ctx.Transform.position;
        if (dir.sqrMagnitude > 0.01f)
            Ctx.Facing.RotateTowards(dir.normalized);
    }

    private bool IsCommittedToAttack =>
        state == BehaviourState.Prepare || state == BehaviourState.Attack;

    private void UpdateStateTransitions(bool canSeePlayer, bool alertedByHit, float dist)
    {
        if (state != BehaviourState.Prepare && state != BehaviourState.Attack
            && (canSeePlayer || alertedByHit) && attackTimer <= 0f)
        {
            state = BehaviourState.Prepare;
            BeginPrepare();
        }

        if (state != BehaviourState.Prepare && state != BehaviourState.Attack
            && !canSeePlayer && !alertedByHit && state == BehaviourState.Follow && followTimer <= 0f)
        {
            state = BehaviourState.Search;
            searchTimer = settings.searchDuration;
            lastSeenPosition = lastKnownPlayerPosition;
            PickSearchWanderPoint();
            HideAttackWarning();
        }

        if (state != BehaviourState.Prepare && state != BehaviourState.Attack
            && (canSeePlayer || alertedByHit) && attackTimer > 0f)
        {
            state = BehaviourState.Follow;
        }
    }

    private void BeginPrepare()
    {
        prepareTimer = settings.prepareTime;
        StopAgentForPrepare();
        ShowAttackWarning();
    }

    private void TickPrepare()
    {
        StopAgentForPrepare();
        prepareTimer -= Time.deltaTime;

        Vector2 dir = Ctx.PlayerTransform.position - Ctx.Transform.position;
        if (dir.sqrMagnitude <= 0.01f)
            return;

        Ctx.Facing.SetSuppressRotation(true);
        bool facingPlayer = Ctx.Facing.RotateTowards(dir.normalized);
        Ctx.Facing.SetSuppressRotation(false);

        if (prepareTimer <= 0f && facingPlayer)
        {
            state = BehaviourState.Attack;
            isAttacking = true;
            Ctx.Agent.isStopped = false;
            Ctx.Agent.speed = settings.boostSpeed;
            LogVision("Fish L attack started.");
        }
    }

    private void TickAttack()
    {
        float dist = Vector2.Distance(Ctx.Transform.position, Ctx.PlayerTransform.position);
        bool canSeePlayer = CheckLineOfSight(dist);
        bool alertedByHit = hitChaseTimer > 0f;

        if (!canSeePlayer && !alertedByHit)
        {
            EndAttack(applyCooldown: false);
            state = BehaviourState.Follow;
            followTimer = settings.followAfterLost;
            return;
        }

        lastKnownPlayerPosition = Ctx.PlayerTransform.position;

        if (dist <= settings.attackRange)
        {
            Ctx.Player.TakeDamage(settings.attackDamage);
            EndAttack(applyCooldown: true);
            return;
        }

        Vector3 target = Ctx.PlayerTransform.position;
        bool success = pathfinder.SetSafeDestination(target);

        if (!success)
            success = pathfinder.SetSafeDestinationNear(target);

        if (!success)
        {
            Ctx.Agent.ResetPath();
            Vector2 dir = Ctx.PlayerTransform.position - Ctx.Transform.position;
            if (dir.sqrMagnitude > 0.01f)
                Ctx.Facing.RotateTowards(dir.normalized);
            return;
        }

        if (dist > settings.detectDistance && !alertedByHit)
        {
            EndAttack(applyCooldown: false);
            state = BehaviourState.Follow;
            followTimer = settings.followAfterLost;
        }
    }

    private void EndAttack(bool applyCooldown)
    {
        isAttacking = false;

        if (applyCooldown)
            attackTimer = settings.attackCooldown;

        Ctx.Agent.speed = settings.speed;
        HideAttackWarning();
    }

    private void StopAgentForPrepare()
    {
        if (Ctx.Agent == null || !Ctx.Agent.isOnNavMesh)
            return;

        Ctx.Agent.isStopped = true;
        Ctx.Agent.ResetPath();
        Ctx.Agent.velocity = Vector3.zero;
    }

    private void TickFollow()
    {
        Ctx.Agent.speed = settings.speed;

        bool success = pathfinder.SetSafeDestination(lastKnownPlayerPosition);
        if (!success)
            success = pathfinder.SetSafeDestinationNear(lastKnownPlayerPosition);

        if (!success)
        {
            Ctx.Agent.ResetPath();
            return;
        }

        float distToLastKnown = Vector2.Distance(Ctx.Transform.position, lastKnownPlayerPosition);

        if (distToLastKnown < 0.5f && hitChaseTimer <= 0f)
        {
            state = BehaviourState.Search;
            searchTimer = settings.searchDuration;
            lastSeenPosition = lastKnownPlayerPosition;
            PickSearchWanderPoint();
            HideAttackWarning();
        }
    }

    private void TickSearch(float deltaTime)
    {
        Ctx.Agent.speed = settings.speed;
        searchTimer -= deltaTime;

        if (searchTimer <= 0f)
        {
            state = BehaviourState.Wander;
                        wanderTimer = settings.wanderInterval;
            PickNewWanderPoint();
            return;
        }

        if (Vector2.Distance(Ctx.Transform.position, wanderTarget) < 0.5f)
            PickSearchWanderPoint();

        pathfinder.SetSafeDestination(wanderTarget);
    }

    private void TickWander(float deltaTime)
    {
        Ctx.Agent.speed = settings.speed;
        HideAttackWarning();

        wanderTimer -= deltaTime;

        if (wanderTimer <= 0f || Vector2.Distance(Ctx.Transform.position, wanderTarget) < 0.5f)
        {
            PickNewWanderPoint();
                        wanderTimer = settings.wanderInterval;
        }

        pathfinder.SetSafeDestination(wanderTarget);
    }

    private void PickNewWanderPoint()
    {
        pathfinder.TryPickWanderPoint(Ctx.Transform.position, settings.wanderRadius, out wanderTarget);
    }

    private void PickSearchWanderPoint()
    {
        pathfinder.TryPickWanderPoint(lastSeenPosition, settings.searchRadius, out wanderTarget);
    }

    private bool CheckLineOfSight(float dist)
    {
        if (Ctx.PlayerTransform == null)
            return false;

        bool playerInDetectRadius = dist <= settings.detectDistance;
        if (playerInDetectRadius != wasPlayerInDetectRadius)
        {
            wasPlayerInDetectRadius = playerInDetectRadius;
            LogVision(playerInDetectRadius
                ? $"Player entered Fish L detect radius. Distance: {dist:0.00}"
                : $"Player left Fish L detect radius. Distance: {dist:0.00}");
        }

        if (!playerInDetectRadius)
        {
            LogSeeChange(false);
            return false;
        }

        LayerMask obstacleMask = settings.visionObstacleMask.value != 0
            ? settings.visionObstacleMask
            : settings.wallLayer;

        bool canSeePlayer = EnemyVisionHelper.HasLineOfSightRay(
            Ctx.Transform.position,
            Ctx.PlayerTransform.position,
            Vector2.zero,
            settings.detectDistance,
            settings.viewAngle,
            obstacleMask,
            Ctx.PlayerTransform
        );
        LogSeeChange(canSeePlayer);
        return canSeePlayer;
    }

    private void LogSeeChange(bool canSeePlayer)
    {
        if (canSeePlayer == couldSeePlayerLastTick)
            return;

        couldSeePlayerLastTick = canSeePlayer;
        LogVision(canSeePlayer ? "Fish L can see player." : "Fish L cannot see player.");
    }

    private void LogVision(string message)
    {
        if (!settings.debugVision)
            return;

        Debug.Log($"[FishLBrain] {message}", Ctx.Host);
    }

    private void AvoidWallWhileMoving()
    {
        if (state == BehaviourState.Prepare)
            return;

        if (Ctx.Agent.velocity.sqrMagnitude < 0.01f)
            return;

        Vector2 direction = Ctx.Agent.velocity.normalized;
        RaycastHit2D hit = Physics2D.CircleCast(
            Ctx.Transform.position,
            settings.fishClearanceRadius,
            direction,
            settings.wallCheckDistance,
            settings.wallLayer
        );

        if (hit.collider == null)
            return;

        Ctx.Agent.ResetPath();

        if (state == BehaviourState.Attack || state == BehaviourState.Follow)
        {
            if (!pathfinder.SetSafeDestinationNear(lastKnownPlayerPosition))
            {
                Vector2 dir = lastKnownPlayerPosition - Ctx.Transform.position;
                if (dir.sqrMagnitude > 0.01f)
                    Ctx.Facing.RotateTowards(dir.normalized);
            }

            return;
        }

        PickNewWanderPoint();
        pathfinder.SetSafeDestination(wanderTarget);
    }

    private void TickFacing()
    {
        if (state == BehaviourState.Prepare)
            return;

        if (Ctx.Agent.velocity.sqrMagnitude > 0.01f)
            Ctx.Facing.RotateFromVelocity(Ctx.Agent.velocity);
    }
}
