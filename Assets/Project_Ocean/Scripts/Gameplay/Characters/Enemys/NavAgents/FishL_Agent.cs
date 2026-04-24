using Game.Testing;
using UnityEngine;
using UnityEngine.AI;

public class FishL_Agent : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Enemy enemy;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float boostSpeed = 7f;
    [SerializeField] private float swayAmount = 0.1f;

    [Header("Rotation")]
    [SerializeField] private float turnSmoothTime = 0.25f;
    [SerializeField] private float maxTiltAngle = 360f;
    [SerializeField] private float flipDeadZone = 0.1f;
    [SerializeField] private float spriteAngleOffset = 0f;
    [SerializeField] private float attackAngleThreshold = 15f;

    private float turnVelocity;
    private bool facingRight = true;

    [Header("Wall Avoidance")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float fishClearanceRadius = 0.8f;
    [SerializeField] private float wallCheckDistance = 0.8f;
    [SerializeField] private int maxWanderSampleTries = 20;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderChangeInterval = 2f;

    private float wanderTimer;

    [Header("Attack Settings")]
    [SerializeField] private float detectDistance = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float prepareTime = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float followAfterLost = 6f;

    private NavMeshAgent agent;
    private NavMeshPath path;

    private bool isPreparing = false;
    private bool isAttacking = false;
    private bool isFollowing = false;

    private float prepareTimer = 0f;
    private float attackTimer = 0f;
    private float followTimer = 0f;

    private Vector3 wanderTarget;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        path = new NavMeshPath();

        facingRight = transform.localScale.x >= 0f;

        wanderTimer = wanderChangeInterval;
        PickNewWanderPoint();
    }

    private void Update()
    {
        if (enemy == null || enemy.Player == null || enemy.IsDeath) return;
        if (agent == null || !agent.isOnNavMesh) return;

        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);

        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        if (followTimer > 0f) followTimer -= Time.deltaTime;

        if (!isPreparing && !isAttacking && dist < detectDistance && dist > attackRange && attackTimer <= 0f)
        {
            StartPrepare();
        }

        if (!isPreparing && !isAttacking && dist > detectDistance && followTimer <= 0f)
        {
            isFollowing = true;
            followTimer = followAfterLost;
        }

        if (isPreparing)
            Prepare();
        else if (isAttacking)
            Attack();
        else if (isFollowing)
            Follow();
        else
            Wander();

        AvoidWallWhileMoving();
        RotateFish();
    }

    private void StartPrepare()
    {
        isPreparing = true;
        prepareTimer = prepareTime;
        agent.isStopped = true;
    }

    private void Prepare()
    {
        prepareTimer -= Time.deltaTime;

        Vector2 dir = enemy.Player.transform.position - transform.position;

        if (dir.sqrMagnitude <= 0.01f)
            return;

        bool facingPlayer = RotateTowards(dir.normalized);

        if (prepareTimer <= 0f && facingPlayer)
        {
            isPreparing = false;
            isAttacking = true;

            agent.isStopped = false;
            agent.speed = boostSpeed;
        }
    }

    private void Attack()
    {
        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);

        if (dist <= attackRange)
        {
            Debug.Log("Player hit by fish!");
            StopAttack();
            return;
        }

        Vector2 sway = new Vector2(
            Mathf.Sin(Time.time * 2f),
            Mathf.Cos(Time.time * 2f)
        ) * swayAmount;

        Vector3 target = enemy.Player.transform.position + (Vector3)sway;

        bool success = SetSafeDestination(target);

        if (!success)
        {
            StopAttack();
            PickNewWanderPoint();
            return;
        }

        if (dist > detectDistance)
        {
            isAttacking = false;
            isFollowing = true;
            followTimer = followAfterLost;
        }
    }

    private void StopAttack()
    {
        isAttacking = false;
        attackTimer = attackCooldown;
        agent.speed = speed;
    }

    private void Follow()
    {
        agent.speed = speed;

        bool success = SetSafeDestination(enemy.Player.transform.position);

        if (!success)
        {
            isFollowing = false;
            PickNewWanderPoint();
            return;
        }

        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);

        if (followTimer <= 0f || dist > detectDistance + 10f)
        {
            isFollowing = false;
        }
    }

    private void Wander()
    {
        agent.speed = speed;

        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f || Vector2.Distance(transform.position, wanderTarget) < 0.5f)
        {
            PickNewWanderPoint();
            wanderTimer = wanderChangeInterval;
        }

        SetSafeDestination(wanderTarget);
    }

    private void PickNewWanderPoint()
    {
        for (int i = 0; i < maxWanderSampleTries; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
            Vector3 randomPoint = transform.position + new Vector3(randomDir.x, randomDir.y, 0f);

            if (!CanFishFitAt(randomPoint))
                continue;

            if (IsPathComplete(randomPoint))
            {
                wanderTarget = randomPoint;
                return;
            }
        }

        wanderTarget = transform.position;
    }

    private bool SetSafeDestination(Vector3 target)
    {
        if (agent == null || !agent.isOnNavMesh)
            return false;

        if (!CanFishFitAt(target))
            return false;

        if (!IsPathComplete(target))
            return false;

        return agent.SetDestination(target);
    }

    private bool IsPathComplete(Vector3 target)
    {
        if (agent == null || !agent.isOnNavMesh)
            return false;

        if (path == null)
            path = new NavMeshPath();

        bool hasPath = agent.CalculatePath(target, path);

        return hasPath && path.status == NavMeshPathStatus.PathComplete;
    }

    private bool CanFishFitAt(Vector3 point)
    {
        Collider2D hit = Physics2D.OverlapCircle(
            point,
            fishClearanceRadius,
            wallLayer
        );

        return hit == null;
    }

    private void AvoidWallWhileMoving()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        if (agent.velocity.sqrMagnitude < 0.01f)
            return;

        Vector2 direction = agent.velocity.normalized;
        Vector2 origin = transform.position;

        RaycastHit2D hit = Physics2D.CircleCast(
            origin,
            fishClearanceRadius,
            direction,
            wallCheckDistance,
            wallLayer
        );

        if (hit.collider != null)
        {
            agent.ResetPath();

            if (isAttacking)
            {
                isAttacking = false;
                attackTimer = attackCooldown;
            }

            if (isFollowing)
            {
                isFollowing = false;
            }

            PickNewWanderPoint();
            SetSafeDestination(wanderTarget);
        }
    }

    private void RotateFish()
    {
        if (isPreparing)
            return;

        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            RotateTowards(agent.velocity.normalized);
        }
    }

    private bool RotateTowards(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.01f)
            return true;

        bool oldFacingRight = facingRight;

        // ถ้าทิศทางไปขวาชัด ๆ ให้หันขวา
        if (dir.x > flipDeadZone)
        {
            facingRight = true;
        }
        // ถ้าทิศทางไปซ้ายชัด ๆ ให้ flip ไปซ้าย
        else if (dir.x < -flipDeadZone)
        {
            facingRight = false;
        }

        // ถ้ามีการเปลี่ยนด้าน ให้ reset ความเร็วการหมุน
        // เพื่อไม่ให้ SmoothDampAngle ลากมุมเก่าจนดูหมุน 180
        if (oldFacingRight != facingRight)
        {
            turnVelocity = 0f;
        }

        float targetAngle;

        if (facingRight)
        {
            // หันขวา = ไม่ flip
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                Mathf.Abs(transform.localScale.y),
                transform.localScale.z
            );

            targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        else
        {
            // หันซ้าย = flip แทนการหมุน 180
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                Mathf.Abs(transform.localScale.y),
                transform.localScale.z
            );

            // กลับมุมให้เหลือแค่มุมเอียงขึ้น/ลง ไม่หมุนหงายท้อง
            targetAngle = Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg;
        }

        // จำกัดมุมเอียง ไม่ให้ปลาตั้ง/หงายเกินไป
        targetAngle = Mathf.Clamp(targetAngle, -maxTiltAngle, maxTiltAngle);

        // เผื่อ sprite หัวไม่ตรง
        targetAngle += spriteAngleOffset;

        float smoothAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.z,
            targetAngle,
            ref turnVelocity,
            turnSmoothTime
        );

        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);

        float angleDifference = Mathf.Abs(
            Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle)
        );

        return angleDifference <= attackAngleThreshold;
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
    }
}