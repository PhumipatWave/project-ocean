using Game.Testing;
using UnityEngine;
using UnityEngine.AI;

public class FishL_Agent : MonoBehaviour
{
    private enum FishState
    {
        Wander,
        Prepare,
        Attack,
        DashRecover,
        FollowLastSeen,
        SearchLastSeen
    }

    [Header("Target")]
    [SerializeField] private Enemy enemy;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float boostSpeed = 7f;
    [SerializeField] private float normalAcceleration = 8f;
    [SerializeField] private float dashAcceleration = 60f;

    [Header("Rotation")]
    [SerializeField] private float turnSmoothTime = 0.25f;
    [SerializeField] private float maxTiltAngle = 35f;
    [SerializeField] private float flipDeadZone = 0.1f;
    [SerializeField] private float flipDelay = 0.4f;
    [SerializeField] private float flipCooldown = 0.7f;
    [SerializeField] private float spriteAngleOffset = 0f;
    [SerializeField] private float attackAngleThreshold = 15f;

    [Header("Vision")]
    [SerializeField] private float viewAngle = 90f;

    [Header("Alert Icon")]
    [SerializeField] private GameObject alertIcon;
    [SerializeField] private Transform alertIconPoint;
    [SerializeField] private Vector3 alertIconOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float alertIconDuration = 1f;
    [SerializeField] private float alertResetDelay = 1.2f;
    [SerializeField] private bool keepAlertIconUpright = true;

    [Header("Wall Avoidance")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float fishClearanceRadius = 0.8f;
    [SerializeField] private float wallCheckDistance = 0.8f;
    [SerializeField] private int maxWanderSampleTries = 20;

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderChangeInterval = 2f;

    [Header("Attack")]
    [SerializeField] private float detectDistance = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float prepareTime = 0.5f;
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private float followAfterLost = 6f;

    [Header("Dash Attack")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.45f;
    [SerializeField] private float dashRecoveryTime = 0.5f;
    [SerializeField] private float dashStopDistance = 0.3f;

    [Header("Dash Swim Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Animator fishAnimator;
    [SerializeField] private string swimBoolName = "IsSwimming";
    [SerializeField] private string dashBoolName = "IsDashing";
    [SerializeField] private float dashWiggleAmount = 8f;
    [SerializeField] private float dashWiggleSpeed = 25f;

    [Header("Search Last Seen")]
    [SerializeField] private float searchDuration = 3f;
    [SerializeField] private float searchRadius = 3f;
    [SerializeField] private float searchChangeInterval = 1f;

    [Header("Hit Reaction")]
    [SerializeField] private float hitChaseDuration = 5f;
    [SerializeField] private bool chasePlayerWhenHit = true;

    [Header("Hit Receiver")]
    [SerializeField] private string playerBulletTag = "Bullet";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool destroyBulletOnHit = true;

    [Header("Near Wall Attack Fix")]
    [SerializeField] private float nearPlayerSearchRadius = 5f;
    [SerializeField] private int nearPlayerSearchSamples = 24;
    [SerializeField] private float nearPlayerSearchStep = 0.5f;

    private NavMeshAgent agent;
    private NavMeshPath path;

    private FishState currentState = FishState.Wander;

    private Vector3 wanderTarget;
    private Vector3 searchTarget;
    private Vector3 dashTarget;
    private Vector3 lastKnownPlayerPosition;

    private float wanderTimer;
    private float prepareTimer;
    private float attackTimer;
    private float followTimer;
    private float searchTimer;
    private float searchMoveTimer;
    private float hitChaseTimer;
    private float dashTimer;
    private float dashRecoveryTimer;

    private float turnVelocity;
    private float flipTimer;
    private float flipCooldownTimer;

    private bool facingRight = true;

    private bool hasAlertedThisEncounter = false;
    private float alertLostTimer = 0f;
    private float alertIconTimer = 0f;

    private Quaternion visualDefaultLocalRotation;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.acceleration = normalAcceleration;

        path = new NavMeshPath();

        facingRight = transform.localScale.x >= 0f;

        if (visualRoot != null)
            visualDefaultLocalRotation = visualRoot.localRotation;

        if (enemy != null && enemy.Player != null)
            lastKnownPlayerPosition = enemy.Player.transform.position;

        if (alertIcon != null)
            alertIcon.SetActive(false);

        wanderTimer = wanderChangeInterval;
        PickNewWanderPoint();
    }

    private void Update()
    {
        if (!CanRunAI())
            return;

        UpdateTimers();

        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);
        bool canSeePlayer = CanSeePlayer(dist);
        bool alertedByHit = hitChaseTimer > 0f;

        HandleAlertIcon(canSeePlayer);

        if (canSeePlayer || alertedByHit)
        {
            RememberPlayerPosition();

            if (currentState == FishState.SearchLastSeen)
                currentState = FishState.Wander;
        }

        HandleStateTransition(canSeePlayer, alertedByHit, dist);
        RunCurrentState();

        AvoidWallWhileMoving();
        RotateFish();
    }

    private bool CanRunAI()
    {
        if (enemy == null || enemy.Player == null || enemy.IsDeath)
            return false;

        if (agent == null || !agent.isOnNavMesh)
            return false;

        return true;
    }

    private void UpdateTimers()
    {
        if (hitChaseTimer > 0f)
            hitChaseTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (followTimer > 0f)
            followTimer -= Time.deltaTime;

        if (flipCooldownTimer > 0f)
            flipCooldownTimer -= Time.deltaTime;
    }

    private void RememberPlayerPosition()
    {
        lastKnownPlayerPosition = enemy.Player.transform.position;
        followTimer = followAfterLost;
    }

    private void HandleStateTransition(bool canSeePlayer, bool alertedByHit, float dist)
    {
        bool hasTarget = canSeePlayer || alertedByHit;

        bool isBusy =
            currentState == FishState.Prepare ||
            currentState == FishState.Attack ||
            currentState == FishState.DashRecover;

        if (!isBusy && hasTarget && dist > attackRange && attackTimer <= 0f)
        {
            StartPrepare();
            return;
        }

        if (!isBusy && hasTarget && attackTimer > 0f)
        {
            StartFollowLastSeen();
            return;
        }

        if (!isBusy && !canSeePlayer && !alertedByHit && currentState == FishState.FollowLastSeen && followTimer <= 0f)
        {
            StartSearchLastSeen();
        }
    }

    private void RunCurrentState()
    {
        switch (currentState)
        {
            case FishState.Wander:
                Wander();
                break;

            case FishState.Prepare:
                Prepare();
                break;

            case FishState.Attack:
                Attack();
                break;

            case FishState.DashRecover:
                DashRecover();
                break;

            case FishState.FollowLastSeen:
                FollowLastSeen();
                break;

            case FishState.SearchLastSeen:
                SearchLastSeenArea();
                break;
        }
    }

    private void HandleAlertIcon(bool canSeePlayer)
    {
        if (canSeePlayer)
        {
            alertLostTimer = 0f;

            if (!hasAlertedThisEncounter)
            {
                ShowAlertIcon();
                hasAlertedThisEncounter = true;
            }
        }
        else
        {
            alertLostTimer += Time.deltaTime;

            if (alertLostTimer >= alertResetDelay)
                hasAlertedThisEncounter = false;
        }

        if (alertIcon == null || !alertIcon.activeSelf)
            return;

        alertIconTimer -= Time.deltaTime;
        UpdateAlertIconPosition();

        if (alertIconTimer <= 0f)
            alertIcon.SetActive(false);
    }

    private void ShowAlertIcon()
    {
        if (alertIcon == null)
            return;

        alertIconTimer = alertIconDuration;
        alertIcon.SetActive(true);
        UpdateAlertIconPosition();
    }

    private void UpdateAlertIconPosition()
    {
        if (alertIcon == null)
            return;

        Vector3 basePosition = alertIconPoint != null ? alertIconPoint.position : transform.position;

        alertIcon.transform.position = basePosition + alertIconOffset;

        if (keepAlertIconUpright)
            alertIcon.transform.rotation = Quaternion.identity;
    }

    private void StartPrepare()
    {
        currentState = FishState.Prepare;

        prepareTimer = prepareTime;

        ResetSwimVisual();
        SetSwimVisual(false, false);

        agent.ResetPath();
        agent.isStopped = true;
        agent.speed = speed;
        agent.acceleration = normalAcceleration;
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
            StartDashAttack();
        }
    }

    private void StartDashAttack()
    {
        dashTarget = enemy.Player.transform.position;
        dashTimer = dashDuration;

        currentState = FishState.Attack;

        agent.isStopped = false;
        agent.speed = dashSpeed;
        agent.acceleration = dashAcceleration;

        bool success = SetSafeDestination(dashTarget);

        if (!success)
            success = SetSafeDestinationNearPlayer(dashTarget);

        if (!success)
        {
            StopDashAttack();
            return;
        }

        SetSwimVisual(true, true);
    }

    private void Attack()
    {
        dashTimer -= Time.deltaTime;

        UpdateDashSwimVisual();

        float distToPlayer = Vector2.Distance(transform.position, enemy.Player.transform.position);
        float distToDashTarget = Vector2.Distance(transform.position, dashTarget);

        if (distToPlayer <= attackRange)
        {
            Debug.Log("Player hit by fish!");
            StopDashAttack();
            return;
        }

        agent.isStopped = false;
        agent.speed = dashSpeed;
        agent.acceleration = dashAcceleration;

        // พุ่งไปจุดที่ล็อกไว้ ไม่เปลี่ยนตามผู้เล่นระหว่างพุ่ง
        agent.SetDestination(dashTarget);

        if (dashTimer <= 0f || distToDashTarget <= dashStopDistance)
        {
            StopDashAttack();
        }
    }

    private void StopDashAttack()
    {
        agent.ResetPath();
        agent.isStopped = true;
        agent.speed = speed;
        agent.acceleration = normalAcceleration;

        dashRecoveryTimer = dashRecoveryTime;
        attackTimer = attackCooldown;

        SetSwimVisual(false, false);
        ResetSwimVisual();

        currentState = FishState.DashRecover;
    }

    private void DashRecover()
    {
        dashRecoveryTimer -= Time.deltaTime;

        Vector2 dir = enemy.Player.transform.position - transform.position;

        if (dir.sqrMagnitude > 0.01f)
            RotateTowards(dir.normalized);

        if (dashRecoveryTimer > 0f)
            return;

        if (attackTimer > 0f)
            return;

        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);
        bool canSeePlayer = CanSeePlayer(dist);
        bool alertedByHit = hitChaseTimer > 0f;

        if (canSeePlayer || alertedByHit)
        {
            StartPrepare();
        }
        else
        {
            StartFollowLastSeen();
        }
    }

    private void SetSwimVisual(bool swimming, bool dashing)
    {
        if (fishAnimator == null)
            return;

        if (!string.IsNullOrEmpty(swimBoolName))
            fishAnimator.SetBool(swimBoolName, swimming);

        if (!string.IsNullOrEmpty(dashBoolName))
            fishAnimator.SetBool(dashBoolName, dashing);
    }

    private void UpdateDashSwimVisual()
    {
        if (visualRoot == null)
            return;

        float wiggle = Mathf.Sin(Time.time * dashWiggleSpeed) * dashWiggleAmount;

        visualRoot.localRotation = visualDefaultLocalRotation * Quaternion.Euler(0f, 0f, wiggle);
    }

    private void ResetSwimVisual()
    {
        if (visualRoot == null)
            return;

        visualRoot.localRotation = visualDefaultLocalRotation;
    }

    private void StartFollowLastSeen()
    {
        currentState = FishState.FollowLastSeen;

        SetSwimVisual(false, false);
        ResetSwimVisual();

        agent.isStopped = false;
        agent.speed = speed;
        agent.acceleration = normalAcceleration;

        if (followTimer <= 0f)
            followTimer = followAfterLost;
    }

    private void FollowLastSeen()
    {
        agent.speed = speed;
        agent.acceleration = normalAcceleration;

        bool success = SetSafeDestination(lastKnownPlayerPosition);

        if (!success)
            success = SetSafeDestinationNearPlayer(lastKnownPlayerPosition);

        if (!success)
        {
            agent.ResetPath();
            StartSearchLastSeen();
            return;
        }

        float distToLastKnown = Vector2.Distance(transform.position, lastKnownPlayerPosition);

        if (distToLastKnown < 0.5f)
        {
            StartSearchLastSeen();
            return;
        }

        if (followTimer <= 0f && hitChaseTimer <= 0f)
        {
            StartSearchLastSeen();
        }
    }

    private void StartSearchLastSeen()
    {
        currentState = FishState.SearchLastSeen;

        SetSwimVisual(false, false);
        ResetSwimVisual();

        searchTimer = searchDuration;
        searchMoveTimer = 0f;

        agent.isStopped = false;
        agent.speed = speed;
        agent.acceleration = normalAcceleration;

        PickSearchPoint();
    }

    private void SearchLastSeenArea()
    {
        searchTimer -= Time.deltaTime;
        searchMoveTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            currentState = FishState.Wander;
            PickNewWanderPoint();
            return;
        }

        if (searchMoveTimer <= 0f || Vector2.Distance(transform.position, searchTarget) < 0.5f)
        {
            PickSearchPoint();
            searchMoveTimer = searchChangeInterval;
        }

        SetSafeDestination(searchTarget);
    }

    private void Wander()
    {
        agent.speed = speed;
        agent.acceleration = normalAcceleration;

        SetSwimVisual(false, false);
        ResetSwimVisual();

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

    private void PickSearchPoint()
    {
        for (int i = 0; i < maxWanderSampleTries; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle * searchRadius;
            Vector3 randomPoint = lastKnownPlayerPosition + new Vector3(randomDir.x, randomDir.y, 0f);

            if (!CanFishFitAt(randomPoint))
                continue;

            if (IsPathComplete(randomPoint))
            {
                searchTarget = randomPoint;
                return;
            }
        }

        searchTarget = lastKnownPlayerPosition;
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

        if (hit.collider == null)
            return;

        if (currentState == FishState.Attack)
        {
            StopDashAttack();
            return;
        }

        agent.ResetPath();

        if (currentState == FishState.FollowLastSeen)
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

        if (currentState == FishState.SearchLastSeen)
        {
            PickSearchPoint();
            SetSafeDestination(searchTarget);
            return;
        }

        PickNewWanderPoint();
        SetSafeDestination(wanderTarget);
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

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            toPlayer,
            distanceToPlayer,
            wallLayer
        );

        if (hit.collider != null)
            return false;

        return true;
    }

    private Vector2 GetFacingDirection()
    {
        Vector2 dir = facingRight ? transform.right : -transform.right;
        return dir.normalized;
    }

    private void RotateFish()
    {
        if (currentState == FishState.Prepare || currentState == FishState.DashRecover)
            return;

        if (agent.velocity.sqrMagnitude > 0.01f)
            RotateTowards(agent.velocity.normalized);
    }

    private bool RotateTowards(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.01f)
            return true;

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

        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle));

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
        if (!chasePlayerWhenHit)
            return;

        if (attacker != null)
        {
            lastKnownPlayerPosition = attacker.position;
        }
        else if (enemy != null && enemy.Player != null)
        {
            lastKnownPlayerPosition = enemy.Player.transform.position;
        }
        else
        {
            return;
        }

        hitChaseTimer = hitChaseDuration;
        followTimer = followAfterLost;
        attackTimer = 0f;

        currentState = FishState.FollowLastSeen;

        SetSwimVisual(false, false);
        ResetSwimVisual();

        agent.isStopped = false;
        agent.speed = boostSpeed;
        agent.acceleration = normalAcceleration;

        bool success = SetSafeDestination(lastKnownPlayerPosition);

        if (!success)
            SetSafeDestinationNearPlayer(lastKnownPlayerPosition);

        Vector2 dir = lastKnownPlayerPosition - transform.position;

        if (dir.sqrMagnitude > 0.01f)
            RotateTowards(dir.normalized);
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(lastKnownPlayerPosition, searchRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(dashTarget, dashStopDistance);
    }
}