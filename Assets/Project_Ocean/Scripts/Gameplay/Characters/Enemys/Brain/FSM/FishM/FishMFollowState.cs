using UnityEngine;

/// <summary>
/// FishM Follow state: chases player and bites on contact.
/// Transitions -> Wander when sight is lost and hit timer expires.
/// </summary>
public class FishMFollowState : EnemyStateBase
{
    public override EnemyStateTag StateTag => EnemyStateTag.Follow;

    private readonly FishMBrainSettings s;
    private Vector2 currentDirection;
    private float attackTimer;
    private float hitChaseTimer;

    public void SetHitChaseTimer(float duration) { hitChaseTimer = duration; }

    public FishMFollowState(FishMBrainSettings settings) { s = settings; }

    public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
    {
        base.Enter(ctx, fsm);
        Ctx.Agent.speed = s.speed;
    }

    public override void Tick(float deltaTime)
    {
        if (hitChaseTimer > 0f) hitChaseTimer -= deltaTime;
        if (attackTimer > 0f) attackTimer -= deltaTime;

        if (Ctx.PlayerTransform == null) { Fsm.TransitionTo(EnemyStateTag.Wander); return; }

        bool canSee = CheckLineOfSight();
        bool alerted = hitChaseTimer > 0f;

        if (!canSee && !alerted)
        {
            Fsm.TransitionTo(EnemyStateTag.Wander);
            return;
        }

        Ctx.Agent.SetDestination(Ctx.PlayerTransform.position);

        if (Ctx.Agent.velocity.magnitude > 0.1f)
            currentDirection = Ctx.Agent.velocity.normalized;

        float dist = Vector2.Distance(Ctx.Transform.position, Ctx.PlayerTransform.position);
        if (dist <= s.attackRange && attackTimer <= 0f)
        {
            Ctx.Player.TakeDamage(s.attackDamage);
            attackTimer = s.attackCooldown;
        }

        TickWallAvoidance();
        if (currentDirection.sqrMagnitude > 0.01f)
            Ctx.Facing.RotateTowards(currentDirection.normalized);
    }

    private bool CheckLineOfSight()
    {
        return EnemyVisionHelper.HasLineOfSightRay(
            Ctx.Transform.position,
            Ctx.PlayerTransform.position,
            currentDirection,
            s.viewDistance,
            s.viewAngle,
            s.obstacleLayer
        );
    }

    private void TickWallAvoidance()
    {
        if (currentDirection == Vector2.zero) return;
        RaycastHit2D hit = Physics2D.Raycast(Ctx.Transform.position, currentDirection, s.wallDetectDistance, s.wallLayer);
        if (hit.collider == null) return;
        Vector2 avoid = Vector2.Perpendicular(currentDirection) * (Random.value > 0.5f ? 1f : -1f);
        Ctx.Agent.SetDestination(Ctx.Transform.position + new Vector3(avoid.x, avoid.y, 0f));
    }
}
