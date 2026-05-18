using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FishS Flock/Wander state: boids-style movement + leader following.
/// Transitions -> Panic when player is spotted or FleeFrom() is called.
/// </summary>
public class FishSFlockState : EnemyStateBase
{
    public override EnemyStateTag StateTag => EnemyStateTag.Flock;

    private readonly FishSBrainSettings s;
    private readonly FishS_Agent host;
    private readonly List<FishS_Agent> neighbors;

    private Vector2 currentDirection;
    private Vector2 wanderTarget;
    private float wanderTimer;
    private float neighborTimer;

    public Vector2 CurrentDirection => currentDirection;

    public FishSFlockState(FishSBrainSettings settings, FishS_Agent host, List<FishS_Agent> neighbors)
    {
        s = settings;
        this.host = host;
        this.neighbors = neighbors;
    }

    public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
    {
        base.Enter(ctx, fsm);
        Ctx.Agent.speed = s.speed;
        SetNewWanderTarget();
    }

    public override void Tick(float deltaTime)
    {
        neighborTimer -= deltaTime;
        if (neighborTimer <= 0f)
        {
            UpdateNeighbors();
            neighborTimer = s.neighborUpdateInterval;
        }

        // Spot player -> Panic
        if (Ctx.PlayerTransform != null && CheckLineOfSight(Ctx.PlayerTransform))
        {
            if (Fsm.CurrentState is FishSPanicState panic)
                panic.SetFleeSource(Ctx.PlayerTransform.position);
            Fsm.TransitionTo(EnemyStateTag.Panic);
            return;
        }

        Vector2 moveDir = TickLeaderAndWander(deltaTime);
        moveDir += CalcFlocking();
        moveDir += WallAvoidance();

        if (moveDir.sqrMagnitude > 0.01f)
        {
            currentDirection = moveDir.normalized;
            Ctx.Agent.SetDestination(
                (Vector3)Ctx.Transform.position + (Vector3)currentDirection * s.navDestinationDistance
            );
        }

        if (currentDirection.sqrMagnitude > 0.01f)
            Ctx.Facing.RotateTowards(currentDirection);
    }

    public void FleeFrom(Vector2 source)
    {
        if (Fsm.HasState(EnemyStateTag.Panic))
        {
            if (Fsm.CurrentState is FishSPanicState panic)
                panic.SetFleeSource(source);
            Fsm.TransitionTo(EnemyStateTag.Panic);
        }
    }

    private Vector2 TickLeaderAndWander(float deltaTime)
    {
        if (host == null) return TickWanderDirection(deltaTime);
        if (host.leader != null && neighbors.Contains(host.leader))
            return ((Vector2)host.leader.transform.position - (Vector2)Ctx.Transform.position).normalized;
        return TickWanderDirection(deltaTime);
    }

    private Vector2 TickWanderDirection(float deltaTime)
    {
        wanderTimer -= deltaTime;
        if (wanderTimer <= 0f || Vector2.Distance(Ctx.Transform.position, wanderTarget) < 0.5f)
            SetNewWanderTarget();
        return wanderTarget - (Vector2)Ctx.Transform.position;
    }

    private void SetNewWanderTarget()
    {
        wanderTarget = (Vector2)Ctx.Transform.position + Random.insideUnitCircle * s.wanderRadius;
        wanderTimer = s.wanderInterval;
    }

    private Vector2 CalcFlocking()
    {
        if (neighbors.Count == 0) return Vector2.zero;
        Vector2 cohesion = Vector2.zero, separation = Vector2.zero, alignment = Vector2.zero;
        int sepCount = 0;
        foreach (var n in neighbors)
        {
            Vector2 toN = (Vector2)n.transform.position - (Vector2)Ctx.Transform.position;
            float dist = toN.magnitude;
            cohesion += (Vector2)n.transform.position;
            if (dist < s.separationDistance && dist > 0f) { separation += -toN.normalized / dist; sepCount++; }
            alignment += n.MoveDirection;
        }
        Vector2 result = Vector2.zero;
        Vector2 toCentre = cohesion / neighbors.Count - (Vector2)Ctx.Transform.position;
        if (toCentre.sqrMagnitude > 0.01f) result += toCentre.normalized * s.cohesionWeight;
        if (sepCount > 0) result += (separation / sepCount).normalized * s.separationWeight;
        Vector2 avgDir = alignment / neighbors.Count;
        if (avgDir.sqrMagnitude > 0.01f) result += avgDir.normalized * s.alignmentWeight;
        return result;
    }

    private void UpdateNeighbors()
    {
        neighbors.Clear();
        Collider2D selfCol = Ctx.Host != null ? Ctx.Host.GetComponent<Collider2D>() : null;
        Collider2D[] hits = Physics2D.OverlapCircleAll(Ctx.Transform.position, s.groupRadius);
        foreach (var hit in hits)
        {
            if (hit == null || hit == selfCol) continue;
            FishS_Agent fish = hit.GetComponent<FishS_Agent>();
            if (fish != null && host != null && fish.groupID == host.groupID)
                neighbors.Add(fish);
        }
    }

    private bool CheckLineOfSight(Transform target) =>
        EnemyVisionHelper.HasLineOfSightRay(
            Ctx.Transform.position, target.position, currentDirection,
            s.viewDistance, s.viewAngle, s.wallLayer, target
        );

    private Vector2 WallAvoidance()
    {
        if (currentDirection.sqrMagnitude < 0.01f) return Vector2.zero;
        Vector2 avoid = Vector2.zero;
        float detectDist = s.wallDetectDistance * (Ctx.Agent.speed / s.speed);
        float[] angles = { 0f, 45f, 90f, 135f, 180f, -45f, -90f, -135f };
        foreach (float a in angles)
        {
            Vector2 dir = RotateVector(currentDirection, a);
            RaycastHit2D hit = Physics2D.Raycast(Ctx.Transform.position, dir, detectDist, s.wallLayer);
            if (hit.collider == null) continue;
            avoid += hit.normal * (1f - hit.distance / detectDist);
        }
        return avoid.sqrMagnitude > 0.01f ? avoid.normalized * s.wallAvoidWeight : Vector2.zero;
    }

    private static Vector2 RotateVector(Vector2 v, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        return new Vector2(v.x * Mathf.Cos(r) - v.y * Mathf.Sin(r), v.x * Mathf.Sin(r) + v.y * Mathf.Cos(r));
    }
}
