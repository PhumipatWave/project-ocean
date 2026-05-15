using UnityEngine;

/// <summary>
/// Large fish: wander, search, prepare, attack, and follow states with alert UI.
/// </summary>
public class FishL_Agent : EnemyNavAgentBase, IEnemyAttackState
{
    private enum BehaviourState
    {
        Wander,
        Prepare,
        Attack,
        Follow,
        Search
    }

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float boostSpeed = 7f;

    [Header("Vision")]
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask visionObstacleMask;

    [Header("Wall Avoidance")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float fishClearanceRadius = 0.8f;
    [SerializeField] private float wallCheckDistance = 0.8f;
    [SerializeField] private int maxWanderSampleTries = 20;

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderChangeInterval = 2f;

    [Header("Search")]
    [SerializeField] private float searchDuration = 4f;
    [SerializeField] private float searchRadius = 2.5f;

    [Header("Attack")]
    [SerializeField] private float detectDistance = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float prepareTime = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float followAfterLost = 6f;
    [SerializeField] private float attackDamage = 10.25f;

    [Header("Hit Reaction")]
    [SerializeField] private float hitChaseDuration = 5f;
    [SerializeField] private bool chasePlayerWhenHit = true;

    [Header("Near Wall Attack")]
    [SerializeField] private float nearPlayerSearchRadius = 5f;
    [SerializeField] private int nearPlayerSearchSamples = 24;
    [SerializeField] private float nearPlayerSearchStep = 0.5f;

    private EnemyNavPathfinder pathfinder;

    private BehaviourState state = BehaviourState.Wander;
    private bool isAttacking;
    public bool IsAttacking => isAttacking;

    private float prepareTimer;
    private float attackTimer;
    private float followTimer;
    private float searchTimer;
    private float wanderTimer;
    private float hitChaseTimer;

    private Vector3 wanderTarget;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 lastSeenPosition;

    protected override void OnAgentStarted()
    {
        pathfinder = new EnemyNavPathfinder(
            Agent,
            wallLayer,
            fishClearanceRadius,
            maxWanderSampleTries,
            nearPlayerSearchRadius,
            nearPlayerSearchSamples,
            nearPlayerSearchStep
        );

        if (PlayerTransform != null)
            lastKnownPlayerPosition = PlayerTransform.position;

        if (fishAlertUI != null)
            fishAlertUI.HideAll();

        wanderTimer = wanderChangeInterval;
        PickNewWanderPoint();
    }

    protected override void TickAgent()
    {
        float dist = Vector2.Distance(transform.position, PlayerTransform.position);
        bool canSeePlayer = CanSeePlayer(dist);
        bool alertedByHit = hitChaseTimer > 0f;

        SetSeeingPlayer(canSeePlayer);

        if (hitChaseTimer > 0f)
            hitChaseTimer -= Time.deltaTime;

        if (canSeePlayer || alertedByHit)
        {
            lastKnownPlayerPosition = PlayerTransform.position;
            followTimer = followAfterLost;
        }

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (followTimer > 0f)
            followTimer -= Time.deltaTime;

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
                TickSearch();
                break;
            default:
                TickWander();
                break;
        }

        AvoidWallWhileMoving();
        TickFacing();
    }

    private void UpdateStateTransitions(bool canSeePlayer, bool alertedByHit, float dist)
    {
        if (state != BehaviourState.Prepare && state != BehaviourState.Attack
            && (canSeePlayer || alertedByHit) && dist > attackRange && attackTimer <= 0f)
        {
            state = BehaviourState.Prepare;
            BeginPrepare();
        }

        if (state != BehaviourState.Prepare && state != BehaviourState.Attack
            && !canSeePlayer && !alertedByHit && state == BehaviourState.Follow && followTimer <= 0f)
        {
            state = BehaviourState.Search;
            searchTimer = searchDuration;
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
        prepareTimer = prepareTime;
        Agent.isStopped = true;
        ShowAttackWarning();
    }

    private void TickPrepare()
    {
        prepareTimer -= Time.deltaTime;

        Vector2 dir = PlayerTransform.position - transform.position;
        if (dir.sqrMagnitude <= 0.01f)
            return;

        Facing.SetSuppressRotation(true);
        bool facingPlayer = Facing.RotateTowards(dir.normalized);
        Facing.SetSuppressRotation(false);

        if (prepareTimer <= 0f && facingPlayer)
        {
            state = BehaviourState.Attack;
            isAttacking = true;
            Agent.isStopped = false;
            Agent.speed = boostSpeed;
        }
    }

    private void TickAttack()
    {
        float dist = Vector2.Distance(transform.position, PlayerTransform.position);
        bool canSeePlayer = CanSeePlayer(dist);
        bool alertedByHit = hitChaseTimer > 0f;

        if (!canSeePlayer && !alertedByHit)
        {
            EndAttack(applyCooldown: false);
            state = BehaviourState.Follow;
            followTimer = followAfterLost;
            return;
        }

        lastKnownPlayerPosition = PlayerTransform.position;

        if (dist <= attackRange)
        {
            Player.TakeDamage(attackDamage);
            EndAttack(applyCooldown: true);
            return;
        }

        Vector3 target = PlayerTransform.position;
        bool success = pathfinder.SetSafeDestination(target);

        if (!success)
            success = pathfinder.SetSafeDestinationNear(target);

        if (!success)
        {
            Agent.ResetPath();
            Vector2 dir = PlayerTransform.position - transform.position;
            if (dir.sqrMagnitude > 0.01f)
                Facing.RotateTowards(dir.normalized);
            return;
        }

        if (dist > detectDistance && !alertedByHit)
        {
            EndAttack(applyCooldown: false);
            state = BehaviourState.Follow;
            followTimer = followAfterLost;
        }
    }

    private void EndAttack(bool applyCooldown)
    {
        isAttacking = false;

        if (applyCooldown)
            attackTimer = attackCooldown;

        Agent.speed = speed;
        HideAttackWarning();
    }

    private void TickFollow()
    {
        Agent.speed = speed;

        bool success = pathfinder.SetSafeDestination(lastKnownPlayerPosition);
        if (!success)
            success = pathfinder.SetSafeDestinationNear(lastKnownPlayerPosition);

        if (!success)
        {
            Agent.ResetPath();
            return;
        }

        float distToLastKnown = Vector2.Distance(transform.position, lastKnownPlayerPosition);

        if (distToLastKnown < 0.5f && hitChaseTimer <= 0f)
        {
            state = BehaviourState.Search;
            searchTimer = searchDuration;
            lastSeenPosition = lastKnownPlayerPosition;
            PickSearchWanderPoint();
            HideAttackWarning();
        }
    }

    private void TickSearch()
    {
        Agent.speed = speed;
        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            state = BehaviourState.Wander;
            wanderTimer = wanderChangeInterval;
            PickNewWanderPoint();
            return;
        }

        if (Vector2.Distance(transform.position, wanderTarget) < 0.5f)
            PickSearchWanderPoint();

        pathfinder.SetSafeDestination(wanderTarget);
    }

    private void TickWander()
    {
        Agent.speed = speed;
        HideAttackWarning();

        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f || Vector2.Distance(transform.position, wanderTarget) < 0.5f)
        {
            PickNewWanderPoint();
            wanderTimer = wanderChangeInterval;
        }

        pathfinder.SetSafeDestination(wanderTarget);
    }

    private void PickNewWanderPoint()
    {
        pathfinder.TryPickWanderPoint(transform.position, wanderRadius, out wanderTarget);
    }

    private void PickSearchWanderPoint()
    {
        pathfinder.TryPickWanderPoint(lastSeenPosition, searchRadius, out wanderTarget);
    }

    private bool CanSeePlayer(float dist)
    {
        if (PlayerTransform == null || dist > detectDistance)
            return false;

        LayerMask obstacleMask = visionObstacleMask.value != 0 ? visionObstacleMask : wallLayer;

        return EnemyVisionHelper.HasLineOfSight(
            transform.position,
            PlayerTransform.position,
            Facing.GetFacingDirection(),
            detectDistance,
            viewAngle,
            obstacleMask
        );
    }

    private void AvoidWallWhileMoving()
    {
        if (Agent.velocity.sqrMagnitude < 0.01f)
            return;

        Vector2 direction = Agent.velocity.normalized;
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            fishClearanceRadius,
            direction,
            wallCheckDistance,
            wallLayer
        );

        if (hit.collider == null)
            return;

        Agent.ResetPath();

        if (state == BehaviourState.Attack || state == BehaviourState.Follow)
        {
            if (!pathfinder.SetSafeDestinationNear(lastKnownPlayerPosition))
            {
                Vector2 dir = lastKnownPlayerPosition - transform.position;
                if (dir.sqrMagnitude > 0.01f)
                    Facing.RotateTowards(dir.normalized);
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

        TickFacingFromAgentVelocity();
    }

    private bool IsCommittedToAttack =>
        state == BehaviourState.Prepare || state == BehaviourState.Attack;

    protected override void OnHitByPlayer(Transform attacker)
    {
        if (!chasePlayerWhenHit)
            return;

        if (attacker != null)
            lastKnownPlayerPosition = attacker.position;
        else if (PlayerTransform != null)
            lastKnownPlayerPosition = PlayerTransform.position;
        else
            return;

        // Keep prepare / attack going when shot; only refresh chase awareness.
        if (IsCommittedToAttack)
        {
            hitChaseTimer = hitChaseDuration;
            followTimer = followAfterLost;
            return;
        }

        hitChaseTimer = hitChaseDuration;
        followTimer = followAfterLost;
        attackTimer = 0f;
        isAttacking = false;
        state = BehaviourState.Follow;

        ShowAttackWarning();

        if (Agent.isOnNavMesh)
        {
            Agent.isStopped = false;
            Agent.speed = boostSpeed;

            if (!pathfinder.SetSafeDestination(lastKnownPlayerPosition))
                pathfinder.SetSafeDestinationNear(lastKnownPlayerPosition);
        }

        Vector2 dir = lastKnownPlayerPosition - transform.position;
        if (dir.sqrMagnitude > 0.01f)
            Facing.RotateTowards(dir.normalized);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fishClearanceRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.position,
            transform.position + transform.right * wallCheckDistance
        );

        Gizmos.color = Color.green;
        Vector2 facingDir = Application.isPlaying && Facing != null
            ? Facing.GetFacingDirection()
            : transform.right;

        Vector3 leftDir = Quaternion.Euler(0f, 0f, viewAngle * 0.5f) * facingDir;
        Vector3 rightDir = Quaternion.Euler(0f, 0f, -viewAngle * 0.5f) * facingDir;

        Gizmos.DrawLine(transform.position, transform.position + leftDir * detectDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * detectDistance);
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
}
