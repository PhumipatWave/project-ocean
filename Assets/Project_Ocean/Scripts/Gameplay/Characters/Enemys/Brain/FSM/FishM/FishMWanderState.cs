using UnityEngine;

/// <summary>
/// FishM Wander state: random wandering with wall avoidance.
/// Transitions -> Follow when line-of-sight or hit alert fires.
/// </summary>
public class FishMWanderState : EnemyStateBase
{
    public override EnemyStateTag StateTag => EnemyStateTag.Wander;

    private readonly FishMBrainSettings s;
    private Vector2 currentDirection;
    private float wanderTimer;

    public Vector2 CurrentDirection => currentDirection;

    public FishMWanderState(FishMBrainSettings settings) { s = settings; }

    public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
    {
        base.Enter(ctx, fsm);
        Ctx.Agent.speed = s.speed;
        wanderTimer = s.wanderInterval;
    }

    public override void Tick(float deltaTime)
    {
        wanderTimer -= deltaTime;

        // Spot player -> Follow
        if (Ctx.PlayerTransform != null)
        {
            Vector2 dir = currentDirection.sqrMagnitude > 0.01f ? currentDirection : Vector2.right;
            bool canSee = EnemyVisionHelper.HasLineOfSightRay(
                Ctx.Transform.position, Ctx.PlayerTransform.position,
                dir, s.viewDistance, s.viewAngle, s.obstacleLayer);
            if (canSee) { Fsm.TransitionTo(EnemyStateTag.Follow); return; }
        }

        if (wanderTimer <= 0f || Ctx.Agent.remainingDistance < 0.2f)
        {
            Vector2 rnd = UnityEngine.Random.insideUnitCircle * s.wanderRadius;
            Ctx.Agent.SetDestination(Ctx.Transform.position + new Vector3(rnd.x, rnd.y, 0f));
            wanderTimer = s.wanderInterval;
        }

        if (Ctx.Agent.velocity.magnitude > 0.1f)
            currentDirection = Ctx.Agent.velocity.normalized;

        TickWallAvoidance();
        if (currentDirection.sqrMagnitude > 0.01f)
            Ctx.Facing.RotateTowards(currentDirection.normalized);
    }

    private void TickWallAvoidance()
    {
        if (currentDirection == Vector2.zero) return;
        RaycastHit2D hit = Physics2D.Raycast(Ctx.Transform.position, currentDirection, s.wallDetectDistance, s.wallLayer);
        if (hit.collider == null) return;
        Vector2 avoid = Vector2.Perpendicular(currentDirection) * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
        Ctx.Agent.SetDestination(Ctx.Transform.position + new Vector3(avoid.x, avoid.y, 0f));
    }
}
