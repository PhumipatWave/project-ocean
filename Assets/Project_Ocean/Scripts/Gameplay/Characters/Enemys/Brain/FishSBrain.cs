using System.Collections.Generic;
using UnityEngine;

public class FishSBrain : EnemyBrainBase, IBrainVision
{
    private readonly FishSBrainSettings settings;
    private FishS_Agent host;

    private readonly List<FishS_Agent> neighbors = new List<FishS_Agent>();

    private Transform playerTransform;
    private Vector2 currentDirection;
    private Vector2 wanderTarget;
    private float wanderTimer;
    private float hitChaseTimer;
    private float neighborTimer;
    private float panicTimer;
    private Vector2 panicDirection;

    private bool isPanicking;

    public Vector2 CurrentDirection => currentDirection;

    #region IBrainVision
    public float ViewDistance => settings.viewDistance;
    public float ViewAngle    => settings.viewAngle;
    public bool CanSeePlayer() => playerTransform != null && CheckLineOfSight(playerTransform);
    #endregion

    public FishSBrain(FishSBrainSettings settings)
    {
        this.settings = settings;
    }

    public override void Initialize(EnemyBrainContext context)
    {
        base.Initialize(context);
        host = context.Host as FishS_Agent;

        Ctx.Agent.speed = settings.speed;
        SetNewWanderTarget();

        if (Ctx.PlayerTransform != null)
            playerTransform = Ctx.PlayerTransform;
    }

    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    public override void Tick(float deltaTime)
    {
        if (Ctx.Agent == null)
            return;

        neighborTimer -= deltaTime;
        if (neighborTimer <= 0f)
        {
            UpdateNeighbors();
            neighborTimer = settings.neighborUpdateInterval;
        }

        if (isPanicking)
        {
            panicTimer -= deltaTime;
            if (panicTimer <= 0f)
                StopPanic();
        }

        Vector2 moveDir = Vector2.zero;
        bool playerNearby = playerTransform != null && CheckLineOfSight(playerTransform);

        if (playerNearby)
        {
            if (!isPanicking)
                StartPanic(playerTransform.position);

            moveDir += panicDirection * 3f;

            if (host != null && host.leader == null)
                BroadcastPanicToFollowers();
        }
        else if (isPanicking)
        {
            moveDir += panicDirection * 2f;
        }

        moveDir += TickHitReaction(deltaTime);
        moveDir += TickLeaderAndWander(playerNearby, deltaTime);
        moveDir += CalcFlocking();
        moveDir += WallAvoidance();

        if (moveDir.sqrMagnitude > 0.01f)
        {
            currentDirection = moveDir.normalized;
            Ctx.Agent.SetDestination(
                (Vector3)Ctx.Transform.position + (Vector3)currentDirection * settings.navDestinationDistance
            );
        }

        if (currentDirection.sqrMagnitude > 0.01f)
            Ctx.Facing.RotateTowards(currentDirection);
    }

    public void FleeFrom(Vector2 targetPos)
    {
        if (!isPanicking)
            StartPanic(targetPos);

        currentDirection = ((Vector2)Ctx.Transform.position - targetPos).normalized;
        Ctx.Agent.SetDestination(
            (Vector3)Ctx.Transform.position + (Vector3)currentDirection * settings.navDestinationDistance
        );
        hitChaseTimer = settings.hitChaseDuration;
    }

    public void ReflectDirection(Vector2 normal, bool randomizePanic)
    {
        currentDirection = Vector2.Reflect(currentDirection, normal).normalized;

        if (randomizePanic && isPanicking)
        {
            float randomAngle = Random.Range(-settings.panicRandomAngle, settings.panicRandomAngle);
            panicDirection = RotateVector(currentDirection, randomAngle).normalized;
        }
    }

    public void ApplyBounceDestination()
    {
        Ctx.Agent.SetDestination(
            (Vector3)Ctx.Transform.position + (Vector3)currentDirection * settings.navDestinationDistance
        );
    }

    public bool IsDirectionBlocked()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            Ctx.Transform.position,
            currentDirection,
            settings.wallDetectDistance,
            settings.wallLayer
        );
        return hit.collider != null;
    }

    public void NudgeDirection(float degrees)
    {
        currentDirection = RotateVector(currentDirection, degrees).normalized;
    }

    private Vector2 TickHitReaction(float deltaTime)
    {
        if (hitChaseTimer <= 0f || playerTransform == null)
            return Vector2.zero;

        hitChaseTimer -= deltaTime;
        Vector2 flee = ((Vector2)Ctx.Transform.position - (Vector2)playerTransform.position).normalized;
        return flee * 3f;
    }

    private Vector2 TickLeaderAndWander(bool playerNearby, float deltaTime)
    {
        Vector2 moveDir = Vector2.zero;

        if (playerNearby || isPanicking || host == null)
            return moveDir;

        if (host.leader != null)
        {
            if (neighbors.Contains(host.leader))
            {
                Vector2 toLeader = (Vector2)host.leader.transform.position - (Vector2)Ctx.Transform.position;
                moveDir += toLeader.normalized;
            }
            else
            {
                moveDir += TickWanderDirection(deltaTime);
            }
        }
        else if (hitChaseTimer <= 0f)
        {
            moveDir += TickWanderDirection(deltaTime);
        }

        return moveDir;
    }

    private Vector2 TickWanderDirection(float deltaTime)
    {
        wanderTimer -= deltaTime;

        if (wanderTimer <= 0f || Vector2.Distance(Ctx.Transform.position, wanderTarget) < 0.5f)
            SetNewWanderTarget();

        return wanderTarget - (Vector2)Ctx.Transform.position;
    }

    private void BroadcastPanicToFollowers()
    {
        if (host == null || playerTransform == null)
            return;

        foreach (FishS_Agent fish in neighbors)
        {
            if (fish.leader == host && fish.groupID == host.groupID)
                fish.FleeFrom(playerTransform.position);
        }
    }

    private void StartPanic(Vector2 playerPos)
    {
        isPanicking = true;
        panicTimer = settings.panicDuration;

        Vector2 awayFromPlayer = ((Vector2)Ctx.Transform.position - playerPos).normalized;
        float randomAngle = Random.Range(-settings.panicRandomAngle, settings.panicRandomAngle);
        panicDirection = RotateVector(awayFromPlayer, randomAngle);

        Ctx.Agent.speed = settings.speed * settings.panicSpeedMultiplier;
    }

    private void StopPanic()
    {
        isPanicking = false;
        panicTimer = 0f;
        Ctx.Agent.speed = settings.speed;
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
            Vector2 toNeighbor = (Vector2)neighbor.transform.position - (Vector2)Ctx.Transform.position;
            float dist = toNeighbor.magnitude;

            cohesionSum += (Vector2)neighbor.transform.position;

            if (dist < settings.separationDistance && dist > 0f)
            {
                separationSum += -toNeighbor.normalized / dist;
                separationCount++;
            }

            alignmentSum += neighbor.MoveDirection;
        }

        Vector2 result = Vector2.zero;

        Vector2 centerOfMass = cohesionSum / neighbors.Count;
        Vector2 toCentre = centerOfMass - (Vector2)Ctx.Transform.position;
        if (toCentre.sqrMagnitude > 0.01f)
            result += toCentre.normalized * settings.cohesionWeight;

        if (separationCount > 0)
            result += (separationSum / separationCount).normalized * settings.separationWeight;

        Vector2 avgDir = alignmentSum / neighbors.Count;
        if (avgDir.sqrMagnitude > 0.01f)
            result += avgDir.normalized * settings.alignmentWeight;

        return result;
    }

    private void UpdateNeighbors()
    {
        neighbors.Clear();
        Collider2D[] hits = Physics2D.OverlapCircleAll(Ctx.Transform.position, settings.groupRadius);

        Collider2D selfCollider = Ctx.Host != null ? Ctx.Host.GetComponent<Collider2D>() : null;

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit == selfCollider)
                continue;

            FishS_Agent fish = hit.GetComponent<FishS_Agent>();
            if (fish != null && host != null && fish.groupID == host.groupID)
                neighbors.Add(fish);
        }
    }

    private void SetNewWanderTarget()
    {
        Vector2 randomDir = Random.insideUnitCircle * settings.wanderRadius;
        wanderTarget = (Vector2)Ctx.Transform.position + randomDir;
        wanderTimer = settings.wanderInterval;
    }

    private bool CheckLineOfSight(Transform target)
    {
        return EnemyVisionHelper.HasLineOfSightRay(
            Ctx.Transform.position,
            target.position,
            currentDirection,
            settings.viewDistance,
            settings.viewAngle,
            settings.wallLayer,
            target
        );
    }

    private Vector2 WallAvoidance()
    {
        if (currentDirection.sqrMagnitude < 0.01f)
            return Vector2.zero;

        Vector2 avoidDir = Vector2.zero;
        float detectDist = settings.wallDetectDistance * (Ctx.Agent.speed / settings.speed);
        float[] angles = { 0f, 45f, 90f, 135f, 180f, -45f, -90f, -135f };

        foreach (float angle in angles)
        {
            Vector2 dir = RotateVector(currentDirection, angle);
            RaycastHit2D hit = Physics2D.Raycast(Ctx.Transform.position, dir, detectDist, settings.wallLayer);

            if (hit.collider == null)
                continue;

            float proximity = 1f - hit.distance / detectDist;
            avoidDir += hit.normal * proximity;
        }

        return avoidDir.sqrMagnitude > 0.01f
            ? avoidDir.normalized * settings.wallAvoidWeight
            : Vector2.zero;
    }
}
