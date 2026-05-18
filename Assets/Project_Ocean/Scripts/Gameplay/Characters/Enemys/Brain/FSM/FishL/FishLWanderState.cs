using UnityEngine;

/// <summary>
/// FishL Wander state: picks random nav points and roams.
/// Transitions -> Follow when player is seen or hit alert fires.
/// </summary>
public class FishLWanderState : EnemyStateBase
{
    public override EnemyStateTag StateTag => EnemyStateTag.Wander;

    private readonly FishLBrainSettings s;
    private readonly EnemyNavPathfinder pathfinder;
    private readonly FishLVisionHelper vision;


    private float wanderTimer;
    private Vector3 wanderTarget;

    public FishLWanderState(FishLBrainSettings settings, EnemyNavPathfinder pathfinder, FishLVisionHelper vision)
    {
        s = settings;
        this.pathfinder = pathfinder;
        this.vision = vision;
    }

    public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
    {
        base.Enter(ctx, fsm);
        Ctx.Agent.speed = s.speed;
        wanderTimer = s.wanderInterval;
        PickWanderPoint();
        if (Ctx.AlertUI != null) Ctx.AlertUI.HideAll();
    }

public override void Tick(float deltaTime)
    {
        wanderTimer -= deltaTime;
        if (wanderTimer <= 0f || Vector2.Distance(Ctx.Transform.position, wanderTarget) < 0.5f)
        {
            PickWanderPoint();
            wanderTimer = s.wanderInterval;
        }
        pathfinder.SetSafeDestination(wanderTarget);

        if (Ctx.Agent.velocity.sqrMagnitude > 0.01f)
            Ctx.Facing.RotateFromVelocity(Ctx.Agent.velocity);

        // Detect player -> Follow
        if (Ctx.PlayerTransform != null && vision != null && vision.CheckLineOfSight())
        {
            if (Fsm.GetState(EnemyStateTag.Follow) is FishLFollowState follow)
                follow.LastKnownPlayerPosition = Ctx.PlayerTransform.position;
            Fsm.TransitionTo(EnemyStateTag.Follow);
        }
    }

    private void PickWanderPoint()
    {
        pathfinder.TryPickWanderPoint(Ctx.Transform.position, s.wanderRadius, out wanderTarget);
    }
}
