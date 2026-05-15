using Game.Testing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small fish: flocking, panic flee, and wall bounce behaviour.
/// </summary>
public class FishS_Agent : EnemyNavAgentBase
{
    private const float NavDestinationDistance = 3f;

    [Header("Movement")]
    [SerializeField] private float speed = 2.5f;

    [Header("Flock")]
    [SerializeField] private float groupRadius = 3f;
    [SerializeField] private float separationDistance = 1f;
    [SerializeField] private List<FishS_Agent> neighbors = new List<FishS_Agent>();
    public FishS_Agent leader;
    public int groupID;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 6f;
    [SerializeField] private float viewAngle = 90f;

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderInterval = 3f;

    [Header("Flocking Weights")]
    [SerializeField] private float cohesionWeight = 1f;
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 0.5f;

    [Header("Panic Flee")]
    [SerializeField] private float panicSpeedMultiplier = 2.5f;
    [SerializeField] private float panicDuration = 3f;
    [SerializeField] private float panicRandomAngle = 60f;

    [Header("Wall Avoid")]
    [SerializeField] private float wallDetectDistance = 2f;
    [SerializeField] private float wallAvoidWeight = 3f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Hit")]
    [SerializeField] private float hitChaseDuration = 5f;

    [SerializeField] private Transform playerTransform;

    private Vector2 currentDirection;
    private Vector2 wanderTarget;
    private float wanderTimer;
    private float hitChaseTimer;
    private float neighborTimer;
    private float panicTimer;
    private Vector2 panicDirection;

    private bool isPanicking;
    private bool isBouncing;

    private float neighborUpdateInterval = 0.2f;

    protected override bool RequiresPlayer => false;

    protected override EnemyFacingController.FacingTurnMode DefaultFacingTurnMode =>
        EnemyFacingController.FacingTurnMode.TurnSpeed;

    protected override void OnAgentStarted()
    {
        Agent.speed = speed;
        SetNewWanderTarget();

        if (PlayerTransform != null)
            playerTransform = PlayerTransform;
        else if (GameManager.CurPlayer != null)
            playerTransform = GameManager.CurPlayer.transform;
    }

    protected override bool CanTick()
    {
        if (IsEnemyDead)
        {
            OnEnemyDead();
            return false;
        }

        if (isBouncing)
            return false;

        return Agent != null && Agent.isOnNavMesh;
    }

    protected override void TickAgent()
    {
        neighborTimer -= Time.deltaTime;
        if (neighborTimer <= 0f)
        {
            UpdateNeighbors();
            neighborTimer = neighborUpdateInterval;
        }

        if (isPanicking)
        {
            panicTimer -= Time.deltaTime;
            if (panicTimer <= 0f)
                StopPanic();
        }

        Vector2 moveDir = Vector2.zero;
        bool playerNearby = playerTransform != null && CanSeePlayer(playerTransform);

        if (playerNearby)
        {
            if (!isPanicking)
                StartPanic(playerTransform.position);

            moveDir += panicDirection * 3f;

            if (leader == null)
                BroadcastPanicToFollowers();
        }
        else if (isPanicking)
        {
            moveDir += panicDirection * 2f;
        }

        moveDir += TickHitReaction();
        moveDir += TickLeaderAndWander(playerNearby);
        moveDir += CalcFlocking();
        moveDir += WallAvoidance();

        if (moveDir.sqrMagnitude > 0.01f)
        {
            currentDirection = moveDir.normalized;
            Agent.SetDestination(
                (Vector3)transform.position + (Vector3)currentDirection * NavDestinationDistance
            );
        }

        if (currentDirection.sqrMagnitude > 0.01f)
            Facing.RotateTowards(currentDirection);
    }

    private Vector2 TickHitReaction()
    {
        if (hitChaseTimer <= 0f || playerTransform == null)
            return Vector2.zero;

        hitChaseTimer -= Time.deltaTime;
        Vector2 flee = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
        return flee * 3f;
    }

    private Vector2 TickLeaderAndWander(bool playerNearby)
    {
        Vector2 moveDir = Vector2.zero;

        if (playerNearby || isPanicking)
            return moveDir;

        if (leader != null)
        {
            if (neighbors.Contains(leader))
            {
                Vector2 toLeader = (Vector2)leader.transform.position - (Vector2)transform.position;
                moveDir += toLeader.normalized;
            }
            else
            {
                moveDir += TickWanderDirection();
            }
        }
        else if (hitChaseTimer <= 0f)
        {
            moveDir += TickWanderDirection();
        }

        return moveDir;
    }

    private Vector2 TickWanderDirection()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f || Vector2.Distance(transform.position, wanderTarget) < 0.5f)
            SetNewWanderTarget();

        return wanderTarget - (Vector2)transform.position;
    }

    private void BroadcastPanicToFollowers()
    {
        foreach (FishS_Agent fish in neighbors)
        {
            if (fish.leader == this && fish.groupID == groupID)
                fish.FleeFrom(playerTransform.position);
        }
    }

    private void StartPanic(Vector2 playerPos)
    {
        isPanicking = true;
        panicTimer = panicDuration;

        Vector2 awayFromPlayer = ((Vector2)transform.position - playerPos).normalized;
        float randomAngle = Random.Range(-panicRandomAngle, panicRandomAngle);
        panicDirection = RotateVector(awayFromPlayer, randomAngle);

        Agent.speed = speed * panicSpeedMultiplier;
    }

    private void StopPanic()
    {
        isPanicking = false;
        panicTimer = 0f;
        Agent.speed = speed;
    }

    private static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    private Vector2 CalcFlocking()
    {
        if (neighbors.Count == 0)
            return Vector2.zero;

        Vector2 cohesionSum = Vector2.zero;
        Vector2 separationSum = Vector2.zero;
        Vector2 alignmentSum = Vector2.zero;
        int separationCount = 0;

        foreach (FishS_Agent neighbor in neighbors)
        {
            Vector2 toNeighbor = (Vector2)neighbor.transform.position - (Vector2)transform.position;
            float dist = toNeighbor.magnitude;

            cohesionSum += (Vector2)neighbor.transform.position;

            if (dist < separationDistance && dist > 0f)
            {
                separationSum += -toNeighbor.normalized / dist;
                separationCount++;
            }

            alignmentSum += neighbor.currentDirection;
        }

        Vector2 result = Vector2.zero;

        Vector2 centerOfMass = cohesionSum / neighbors.Count;
        Vector2 toCentre = centerOfMass - (Vector2)transform.position;
        if (toCentre.sqrMagnitude > 0.01f)
            result += toCentre.normalized * cohesionWeight;

        if (separationCount > 0)
            result += (separationSum / separationCount).normalized * separationWeight;

        Vector2 avgDir = alignmentSum / neighbors.Count;
        if (avgDir.sqrMagnitude > 0.01f)
            result += avgDir.normalized * alignmentWeight;

        return result;
    }

    private void UpdateNeighbors()
    {
        neighbors.Clear();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, groupRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit == GetComponent<Collider2D>())
                continue;

            FishS_Agent fish = hit.GetComponent<FishS_Agent>();
            if (fish != null && fish.groupID == groupID)
                neighbors.Add(fish);
        }
    }

    private void SetNewWanderTarget()
    {
        Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
        wanderTarget = (Vector2)transform.position + randomDir;
        wanderTimer = wanderInterval;
    }

    private bool CanSeePlayer(Transform target)
    {
        return EnemyVisionHelper.HasLineOfSightRay(
            transform.position,
            target.position,
            currentDirection,
            viewDistance,
            viewAngle,
            wallLayer,
            target
        );
    }

    private Vector2 WallAvoidance()
    {
        if (currentDirection.sqrMagnitude < 0.01f)
            return Vector2.zero;

        Vector2 avoidDir = Vector2.zero;
        float detectDist = wallDetectDistance * (Agent.speed / speed);
        float[] angles = { 0f, 45f, 90f, 135f, 180f, -45f, -90f, -135f };

        foreach (float angle in angles)
        {
            Vector2 dir = RotateVector(currentDirection, angle);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, detectDist, wallLayer);

            if (hit.collider == null)
                continue;

            float proximity = 1f - hit.distance / detectDist;
            avoidDir += hit.normal * proximity;
        }

        return avoidDir.sqrMagnitude > 0.01f
            ? avoidDir.normalized * wallAvoidWeight
            : Vector2.zero;
    }

    public void FleeFrom(Vector2 targetPos)
    {
        if (!isPanicking)
            StartPanic(targetPos);

        currentDirection = ((Vector2)transform.position - targetPos).normalized;
        Agent.SetDestination(
            (Vector3)transform.position + (Vector3)currentDirection * NavDestinationDistance
        );
        hitChaseTimer = hitChaseDuration;
    }

    protected override void HandleBulletHit(GameObject bullet)
    {
        if (IsEnemyDead)
            return;

        if (bullet == null || !bullet.CompareTag(playerBulletTag))
            return;

        FleeFrom(bullet.transform.position);

        if (destroyBulletOnHit)
            Destroy(bullet);
    }

    protected override void HandlePlayerCollision()
    {
        if (playerTransform != null)
            FleeFrom(playerTransform.position);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsEnemyDead)
            return;

        if (((1 << collision.gameObject.layer) & wallLayer) == 0)
            return;

        Vector2 normal = collision.contacts[0].normal;
        currentDirection = Vector2.Reflect(currentDirection, normal).normalized;

        if (isPanicking)
        {
            float randomAngle = Random.Range(-panicRandomAngle, panicRandomAngle);
            panicDirection = RotateVector(currentDirection, randomAngle).normalized;
        }

        StopAllCoroutines();
        StartCoroutine(WallBounceRoutine());
    }

    private IEnumerator WallBounceRoutine()
    {
        isBouncing = true;

        int attempts = 0;
        bool freeFromWall = false;

        while (attempts < 10 && !freeFromWall)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                currentDirection,
                wallDetectDistance,
                wallLayer
            );

            if (hit.collider == null)
            {
                freeFromWall = true;
                break;
            }

            float angle = Random.Range(45f, 135f);
            currentDirection = RotateVector(currentDirection, angle).normalized;
            attempts++;
            yield return null;
        }

        Agent.SetDestination(
            (Vector3)transform.position + (Vector3)currentDirection * NavDestinationDistance
        );

        isBouncing = false;
    }
}
