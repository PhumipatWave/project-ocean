using Game.Testing;
using UnityEngine;
using UnityEngine.AI;

public class FishM_Agent : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Enemy enemy;

    [Header("Movement")]
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float turnSpeed = 5f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackTimer = 0f;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderChangeInterval = 2f;

    [Header("Wall Avoid Settings")]
    [SerializeField] private float wallDetectDistance = 1f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Sensor Settings")]
    [SerializeField] private float viewDistance = 6f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Rotation/Flip Settings")]
    [SerializeField] private float turnSmoothTime = 0.25f;
    [SerializeField] private float maxTiltAngle = 35f;
    [SerializeField] private float flipDeadZone = 0.1f;
    [SerializeField] private float flipDelay = 0.4f;
    [SerializeField] private float flipCooldown = 0.5f;
    [SerializeField] private float spriteAngleOffset = 0f;
    [SerializeField] private float attackAngleThreshold = 15f;

    private float wanderTimer = 0f;
    private NavMeshAgent agent;
    private bool isFollowing = false;
    private Vector2 currentDirection;

    // Rotation / Flip
    private bool facingRight = true;
    private float turnVelocity;
    private float flipTimer = 0f;
    private float flipCooldownTimer = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;

        wanderTimer = wanderChangeInterval;
    }

    private void Update()
    {
        if (enemy.Player == null || enemy.IsDeath) return;

        // Sensor ตรวจจับผู้เล่น
        isFollowing = CanSeePlayer();

        // cooldown
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (isFollowing)
            FollowAndBite();
        else
            Wander();

        WallAvoidance();
        RotateFish(currentDirection);
    }

    private bool CanSeePlayer()
    {
        Vector2 directionToPlayer = (enemy.Player.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, enemy.Player.transform.position);

        if (distanceToPlayer > viewDistance)
            return false;

        float angleToPlayer = Vector2.Angle(currentDirection, directionToPlayer);
        if (angleToPlayer > viewAngle / 2f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        if (hit.collider != null)
            return false;

        return true;
    }

    private void FollowAndBite()
    {
        agent.SetDestination(enemy.Player.transform.position);

        if (agent.velocity.magnitude > 0.1f)
            currentDirection = agent.velocity.normalized;

        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);

        if (dist <= attackRange && attackTimer <= 0f)
        {
            Debug.Log("Player ถูกกัดโดย NavMesh Fish!");
            attackTimer = attackCooldown;
        }
    }

    private void Wander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f || agent.remainingDistance < 0.2f)
        {
            Vector2 randomPos = Random.insideUnitCircle * wanderRadius;
            Vector3 target = transform.position + new Vector3(randomPos.x, randomPos.y, 0);
            agent.SetDestination(target);
            wanderTimer = wanderChangeInterval;
        }

        if (agent.velocity.magnitude > 0.1f)
            currentDirection = agent.velocity.normalized;
    }

    private void WallAvoidance()
    {
        Vector2 origin = transform.position;
        Vector2 dir = currentDirection;

        if (dir == Vector2.zero) return;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, wallDetectDistance, wallLayer);
        if (hit.collider != null)
        {
            Vector2 avoidDir = Vector2.Perpendicular(dir) * (Random.value > 0.5f ? 1f : -1f);
            Vector3 newTarget = transform.position + new Vector3(avoidDir.x, avoidDir.y, 0);
            agent.SetDestination(newTarget);
        }
    }

    // ===== Rotation + Flip =====
    private void RotateFish(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.01f)
            return;

        RotateTowards(dir.normalized);
    }

    private bool RotateTowards(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.01f)
            return true;

        if (flipCooldownTimer > 0f)
            flipCooldownTimer -= Time.deltaTime;

        bool wantsFaceRight = facingRight;

        if (dir.x > flipDeadZone)
            wantsFaceRight = true;
        else if (dir.x < -flipDeadZone)
            wantsFaceRight = false;

        if (wantsFaceRight != facingRight)
        {
            flipTimer += Time.deltaTime;

            if (flipTimer >= flipDelay && flipCooldownTimer <= 0f)
            {
                facingRight = wantsFaceRight;
                flipTimer = 0f;
                flipCooldownTimer = flipCooldown;
                turnVelocity = 0f;
            }
        }
        else
        {
            flipTimer = 0f;
        }

        float targetAngle;

        if (facingRight)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x),
                                               Mathf.Abs(transform.localScale.y),
                                               transform.localScale.z);
            targetAngle = Mathf.Atan2(dir.y, Mathf.Abs(dir.x)) * Mathf.Rad2Deg;
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x),
                                               Mathf.Abs(transform.localScale.y),
                                               transform.localScale.z);
            targetAngle = Mathf.Atan2(-dir.y, Mathf.Abs(dir.x)) * Mathf.Rad2Deg;
        }

        targetAngle = Mathf.Clamp(targetAngle, -maxTiltAngle, maxTiltAngle);
        targetAngle += spriteAngleOffset;

        float smoothAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.z,
            targetAngle,
            ref turnVelocity,
            turnSmoothTime
        );

        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);

        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle));
        return angleDifference <= attackAngleThreshold;
    }

    // ===== Visual Debug ของ Sensor =====
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