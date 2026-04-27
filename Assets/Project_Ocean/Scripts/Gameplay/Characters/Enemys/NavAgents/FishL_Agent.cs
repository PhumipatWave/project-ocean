using Game.Testing;
using UnityEngine;
using UnityEngine.AI;

public class FishL_Agent : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Enemy enemy;

    [Header("Alert UI")]
    [SerializeField] private FishAlertUI fishAlertUI;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float boostSpeed = 7f;

    [Header("Rotation")]
    [SerializeField] private float turnSmoothTime = 0.25f;
    [SerializeField] private float maxTiltAngle = 35f;
    [SerializeField] private float flipDeadZone = 0.1f;
    [SerializeField] private float flipDelay = 0.4f;
    [SerializeField] private float flipCooldown = 0.7f;
    [SerializeField] private float spriteAngleOffset = 0f;
    [SerializeField] private float attackAngleThreshold = 15f;

    private float turnVelocity;
    private bool facingRight = true;
    private float flipTimer = 0f;
    private float flipCooldownTimer = 0f;

    [Header("Vision Settings")]
    [SerializeField] private float viewAngle = 90f;

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

    [Header("Hit Reaction")]
    [SerializeField] private float hitChaseDuration = 5f;
    [SerializeField] private bool chasePlayerWhenHit = true;

    private float hitChaseTimer = 0f;

    [Header("Hit Receiver")]
    [SerializeField] private string playerBulletTag = "PlayerBullet";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool destroyBulletOnHit = true;

    [Header("Near Wall Attack Fix")]
    [SerializeField] private float nearPlayerSearchRadius = 5f;
    [SerializeField] private int nearPlayerSearchSamples = 24;
    [SerializeField] private float nearPlayerSearchStep = 0.5f;

    private NavMeshAgent agent;
    private NavMeshPath path;

    private bool isPreparing = false;
    private bool isAttacking = false;
    private bool isFollowing = false;

    private float prepareTimer = 0f;
    private float attackTimer = 0f;
    private float followTimer = 0f;

    private Vector3 wanderTarget;
    private Vector3 lastKnownPlayerPosition;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        path = new NavMeshPath();

        facingRight = transform.localScale.x >= 0f;

        if (enemy != null && enemy.Player != null)
            lastKnownPlayerPosition = enemy.Player.transform.position;

        if (fishAlertUI != null)
            fishAlertUI.HideAll();

        wanderTimer = wanderChangeInterval;
        PickNewWanderPoint();
    }

    private void Update()
    {
        if (enemy == null || enemy.Player == null || enemy.IsDeath)
        {
            if (fishAlertUI != null)
                fishAlertUI.HideAll();

            return;
        }

        if (agent == null || !agent.isOnNavMesh) return;

        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);
        bool canSeePlayer = CanSeePlayer(dist);
        bool alertedByHit = hitChaseTimer > 0f;

        if (fishAlertUI != null)
            fishAlertUI.SetSeeingPlayer(canSeePlayer);

        if (hitChaseTimer > 0f)
            hitChaseTimer -= Time.deltaTime;

        if (canSeePlayer || alertedByHit)
        {
            lastKnownPlayerPosition = enemy.Player.transform.position;
            followTimer = followAfterLost;
        }

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (followTimer > 0f)
            followTimer -= Time.deltaTime;

        if (!isPreparing && !isAttacking && (canSeePlayer || alertedByHit) && dist > attackRange && attackTimer <= 0f)
        {
            StartPrepare();
        }

        if (!isPreparing && !isAttacking && !canSeePlayer && !alertedByHit && isFollowing && followTimer <= 0f)
        {
            isFollowing = false;

            if (fishAlertUI != null)
                fishAlertUI.HideAttackWarning();
        }

        if (!isPreparing && !isAttacking && (canSeePlayer || alertedByHit) && attackTimer > 0f)
        {
            isFollowing = true;
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
        isFollowing = false;
        prepareTimer = prepareTime;
        agent.isStopped = true;

        if (fishAlertUI != null)
            fishAlertUI.ShowAttackWarning();
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
        bool canSeePlayer = CanSeePlayer(dist);
        bool alertedByHit = hitChaseTimer > 0f;

        if (!canSeePlayer && !alertedByHit)
        {
            isAttacking = false;
            isFollowing = true;
            followTimer = followAfterLost;

            if (fishAlertUI != null)
                fishAlertUI.HideAttackWarning();

            return;
        }

        lastKnownPlayerPosition = enemy.Player.transform.position;

        if (dist <= attackRange)
        {
            Debug.Log("Player hit by fish!");
            StopAttack();
            return;
        }

        Vector3 target = enemy.Player.transform.position;

        bool success = SetSafeDestination(target);

        if (!success)
            success = SetSafeDestinationNearPlayer(enemy.Player.transform.position);

        if (!success)
        {
            agent.ResetPath();

            Vector2 dir = enemy.Player.transform.position - transform.position;
            if (dir.sqrMagnitude > 0.01f)
                RotateTowards(dir.normalized);

            return;
        }

        if (dist > detectDistance && !alertedByHit)
        {
            isAttacking = false;
            isFollowing = true;
            followTimer = followAfterLost;

            if (fishAlertUI != null)
                fishAlertUI.HideAttackWarning();
        }
    }

    private void StopAttack()
    {
        isAttacking = false;
        attackTimer = attackCooldown;
        agent.speed = speed;

        if (fishAlertUI != null)
            fishAlertUI.HideAttackWarning();
    }

    private void Follow()
    {
        agent.speed = speed;

        bool success = SetSafeDestination(lastKnownPlayerPosition);

        if (!success)
            success = SetSafeDestinationNearPlayer(lastKnownPlayerPosition);

        if (!success)
        {
            agent.ResetPath();
            return;
        }

        float distToLastKnown = Vector2.Distance(transform.position, lastKnownPlayerPosition);

        if (distToLastKnown < 0.5f && followTimer <= 0f && hitChaseTimer <= 0f)
        {
            isFollowing = false;

            if (fishAlertUI != null)
                fishAlertUI.HideAttackWarning();
        }
    }

    private void Wander()
    {
        agent.speed = speed;

        if (fishAlertUI != null)
            fishAlertUI.HideAttackWarning();

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

    private bool SetSafeDestinationNearPlayer(Vector3 playerPosition)
    {
        if (agent == null || !agent.isOnNavMesh)
            return false;

        Vector2 fromPlayerToFish = transform.position - playerPosition;

        if (fromPlayerToFish.sqrMagnitude < 0.01f)
            fromPlayerToFish = Random.insideUnitCircle;

        fromPlayerToFish.Normalize();

        for (float radius = nearPlayerSearchStep; radius <= nearPlayerSearchRadius; radius += nearPlayerSearchStep)
        {
            Vector3 candidate = playerPosition + (Vector3)(fromPlayerToFish * radius);

            if (CanFishFitAt(candidate) && IsPathComplete(candidate))
            {
                agent.SetDestination(candidate);
                return true;
            }
        }

        for (int i = 0; i < nearPlayerSearchSamples; i++)
        {
            float angle = (360f / nearPlayerSearchSamples) * i;

            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            for (float radius = nearPlayerSearchStep; radius <= nearPlayerSearchRadius; radius += nearPlayerSearchStep)
            {
                Vector3 candidate = playerPosition + (Vector3)(dir * radius);

                if (!CanFishFitAt(candidate))
                    continue;

                if (!IsPathComplete(candidate))
                    continue;

                agent.SetDestination(candidate);
                return true;
            }
        }

        return false;
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
        Collider2D hit = Physics2D.OverlapCircle(point, fishClearanceRadius, wallLayer);
        return hit == null;
    }

    private bool CanSeePlayer(float dist)
    {
        if (enemy == null || enemy.Player == null)
            return false;

        if (dist > detectDistance)
            return false;

        Vector2 toPlayer = enemy.Player.transform.position - transform.position;

        if (toPlayer.sqrMagnitude < 0.01f)
            return true;

        toPlayer.Normalize();

        Vector2 facingDir = GetFacingDirection();

        float angleToPlayer = Vector2.Angle(facingDir, toPlayer);

        if (angleToPlayer > viewAngle * 0.5f)
            return false;

        float distanceToPlayer = Vector2.Distance(transform.position, enemy.Player.transform.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer, distanceToPlayer, wallLayer);

        if (hit.collider != null)
            return false;

        return true;
    }

    private Vector2 GetFacingDirection()
    {
        Vector2 dir = facingRight ? transform.right : -transform.right;
        return dir.normalized;
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

            if (isAttacking || isFollowing)
            {
                bool success = SetSafeDestinationNearPlayer(lastKnownPlayerPosition);

                if (!success)
                {
                    Vector2 dir = lastKnownPlayerPosition - transform.position;
                    if (dir.sqrMagnitude > 0.01f)
                        RotateTowards(dir.normalized);
                }

                return;
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
            RotateTowards(agent.velocity.normalized);
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
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                Mathf.Abs(transform.localScale.y),
                transform.localScale.z
            );

            targetAngle = Mathf.Atan2(dir.y, Mathf.Abs(dir.x)) * Mathf.Rad2Deg;
        }
        else
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                Mathf.Abs(transform.localScale.y),
                transform.localScale.z
            );

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

        float angleDifference = Mathf.Abs(
            Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle)
        );

        return angleDifference <= attackAngleThreshold;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryReceiveBulletHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryReceiveBulletHit(collision.collider.gameObject);
    }

    private void TryReceiveBulletHit(GameObject hitObject)
    {
        if (hitObject == null)
            return;

        if (!hitObject.CompareTag(playerBulletTag))
            return;

        Debug.Log("Fish was hit by bullet object!");

        Transform playerTransform = FindPlayerTransform();

        OnHitByPlayer(playerTransform);

        if (destroyBulletOnHit)
            Destroy(hitObject);
    }

    private Transform FindPlayerTransform()
    {
        if (enemy != null && enemy.Player != null)
            return enemy.Player.transform;

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
            return playerObject.transform;

        return null;
    }

    public void OnHitByPlayer(Transform attacker)
    {
        Debug.Log("FISH HIT REACTION CALLED");

        if (!chasePlayerWhenHit)
            return;

        if (attacker != null)
            lastKnownPlayerPosition = attacker.position;
        else if (enemy != null && enemy.Player != null)
            lastKnownPlayerPosition = enemy.Player.transform.position;
        else
        {
            Debug.LogWarning("Fish was hit, but no player transform found.");
            return;
        }

        hitChaseTimer = hitChaseDuration;
        followTimer = followAfterLost;
        attackTimer = 0f;

        isPreparing = false;
        isAttacking = false;
        isFollowing = true;

        if (fishAlertUI != null)
            fishAlertUI.ShowAttackWarning();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = boostSpeed;

            bool success = SetSafeDestination(lastKnownPlayerPosition);

            if (!success)
                SetSafeDestinationNearPlayer(lastKnownPlayerPosition);
        }

        Vector2 dir = lastKnownPlayerPosition - transform.position;
        if (dir.sqrMagnitude > 0.01f)
            RotateTowards(dir.normalized);

        Debug.Log("Fish got hit! Chase player!");
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

        Vector2 facingDir = Application.isPlaying ? GetFacingDirection() : transform.right;

        Vector3 leftDir = Quaternion.Euler(0f, 0f, viewAngle * 0.5f) * facingDir;
        Vector3 rightDir = Quaternion.Euler(0f, 0f, -viewAngle * 0.5f) * facingDir;

        Gizmos.DrawLine(transform.position, transform.position + leftDir * detectDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * detectDistance);
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
}