using UnityEngine;

/// <summary>
/// Medium fish: simple wander, vision-based chase, and bite attack.
/// </summary>
public class FishM_Agent : EnemyNavAgentBase
{
    [Header("Movement")]
    [SerializeField] private float speed = 2.5f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDamage = 7.5f;

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderChangeInterval = 2f;

    [Header("Wall Avoid")]
    [SerializeField] private float wallDetectDistance = 1f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 6f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Hit Reaction")]
    [SerializeField] private float hitChaseDuration = 5f;
    [SerializeField] private bool chasePlayerWhenHit = true;

    private float attackTimer;
    private float wanderTimer;
    private float hitChaseTimer;
    private Vector2 currentDirection;
    private bool isFollowing;

    protected override void OnAgentStarted()
    {
        Agent.speed = speed;
        wanderTimer = wanderChangeInterval;
    }

    protected override void TickAgent()
    {
        if (hitChaseTimer > 0f)
        {
            hitChaseTimer -= Time.deltaTime;
            isFollowing = true;
        }
        else
        {
            isFollowing = CanSeePlayer();
        }

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (isFollowing)
            TickFollowAndBite();
        else
            TickWander();

        TickWallAvoidance();
        TickFacingFromDirection(currentDirection);
    }

    private bool CanSeePlayer()
    {
        return EnemyVisionHelper.HasLineOfSightRay(
            transform.position,
            PlayerTransform.position,
            currentDirection,
            viewDistance,
            viewAngle,
            obstacleLayer
        );
    }

    private void TickFollowAndBite()
    {
        Agent.SetDestination(PlayerTransform.position);

        if (Agent.velocity.magnitude > 0.1f)
            currentDirection = Agent.velocity.normalized;

        float dist = Vector2.Distance(transform.position, PlayerTransform.position);

        if (dist <= attackRange && attackTimer <= 0f)
        {
            Player.TakeDamage(attackDamage);
            attackTimer = attackCooldown;
        }
    }

    private void TickWander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f || Agent.remainingDistance < 0.2f)
        {
            Vector2 randomPos = Random.insideUnitCircle * wanderRadius;
            Vector3 target = transform.position + new Vector3(randomPos.x, randomPos.y, 0f);
            Agent.SetDestination(target);
            wanderTimer = wanderChangeInterval;
        }

        if (Agent.velocity.magnitude > 0.1f)
            currentDirection = Agent.velocity.normalized;
    }

    private void TickWallAvoidance()
    {
        if (currentDirection == Vector2.zero)
            return;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            currentDirection,
            wallDetectDistance,
            wallLayer
        );

        if (hit.collider == null)
            return;

        Vector2 avoidDir = Vector2.Perpendicular(currentDirection) * (Random.value > 0.5f ? 1f : -1f);
        Vector3 newTarget = transform.position + new Vector3(avoidDir.x, avoidDir.y, 0f);
        Agent.SetDestination(newTarget);
    }

    private void TickFacingFromDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.01f)
            return;

        Facing.RotateTowards(dir.normalized);
    }

    protected override void HandleBulletHit(GameObject bullet)
    {
        base.HandleBulletHit(bullet);

        if (chasePlayerWhenHit)
            hitChaseTimer = hitChaseDuration;
    }

    protected override void HandlePlayerCollision()
    {
        if (chasePlayerWhenHit)
            hitChaseTimer = hitChaseDuration;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 forward = transform.right;
        int steps = 20;
        float stepAngle = viewAngle / steps;
        float halfView = viewAngle / 2f;

        Vector3 prevPoint = transform.position + Quaternion.Euler(0, 0, -halfView) * forward * viewDistance;
        for (int i = 1; i <= steps; i++)
        {
            float angle = -halfView + stepAngle * i;
            Vector3 nextPoint = transform.position + Quaternion.Euler(0, 0, angle) * forward * viewDistance;
            Gizmos.DrawLine(prevPoint, nextPoint);
            Gizmos.DrawLine(transform.position, nextPoint);
            prevPoint = nextPoint;
        }
    }
}
