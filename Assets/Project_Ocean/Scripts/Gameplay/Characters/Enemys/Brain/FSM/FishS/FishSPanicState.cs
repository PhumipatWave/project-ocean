using UnityEngine;

/// <summary>
/// FishS Panic state: flees from player / threat at boosted speed.
/// Broadcasts flee signal to group followers.
/// Transitions -> Flock when panic timer expires.
/// </summary>
public class FishSPanicState : EnemyStateBase
{
    public override EnemyStateTag StateTag => EnemyStateTag.Panic;

    private readonly FishSBrainSettings s;
    private readonly FishS_Agent host;

    private Vector2 panicDirection;
    private float panicTimer;
    private float hitChaseTimer;

    public FishSPanicState(FishSBrainSettings settings, FishS_Agent host)
    {
        s = settings;
        this.host = host;
    }

    public void SetFleeSource(Vector2 sourcePos)
    {
        Vector2 away = ((Vector2)Ctx.Transform.position - sourcePos).normalized;
        float randomAngle = Random.Range(-s.panicRandomAngle, s.panicRandomAngle);
        panicDirection = RotateVector(away, randomAngle);
    }

    public void SetHitChaseTimer(float duration) => hitChaseTimer = duration;

    public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
    {
        base.Enter(ctx, fsm);
        Ctx.Agent.speed = s.speed * s.panicSpeedMultiplier;
        panicTimer = s.panicDuration;
        BroadcastPanicToFollowers();
    }

    public override void Tick(float deltaTime)
    {
        panicTimer -= deltaTime;
        if (hitChaseTimer > 0f) hitChaseTimer -= deltaTime;

        if (panicTimer <= 0f && hitChaseTimer <= 0f)
        {
            Fsm.TransitionTo(EnemyStateTag.Flock);
            return;
        }

        Vector2 moveDir = panicDirection * 3f;

        // Flee from player if still in sight
        if (Ctx.PlayerTransform != null)
        {
            Vector2 flee = ((Vector2)Ctx.Transform.position - (Vector2)Ctx.PlayerTransform.position).normalized;
            moveDir += flee * 2f;
        }

        Vector2 finalDir = moveDir.normalized;
        Ctx.Agent.SetDestination((Vector3)Ctx.Transform.position + (Vector3)finalDir * s.navDestinationDistance);
        Ctx.Facing.RotateTowards(finalDir);
    }

    public override void Exit()
    {
        Ctx.Agent.speed = s.speed;
    }

    private void BroadcastPanicToFollowers()
    {
        if (host == null || Ctx.PlayerTransform == null) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(Ctx.Transform.position, s.groupRadius);
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            FishS_Agent fish = hit.GetComponent<FishS_Agent>();
            if (fish != null && fish != host && fish.leader == host && fish.groupID == host.groupID)
                fish.FleeFrom(Ctx.PlayerTransform.position);
        }
    }

    private static Vector2 RotateVector(Vector2 v, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        return new Vector2(v.x * Mathf.Cos(r) - v.y * Mathf.Sin(r), v.x * Mathf.Sin(r) + v.y * Mathf.Cos(r));
    }
}
