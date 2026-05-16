using UnityEngine;

/// <summary>
/// FishL Search state: wanders around the last seen position for a limited time.
/// Transitions -> Follow if player spotted again.
/// Transitions -> Wander when timer expires.
/// </summary>
public class FishLSearchState : EnemyStateBase
{
    public override EnemyStateTag StateTag => EnemyStateTag.Search;

    private readonly FishLBrainSettings s;
    private readonly EnemyNavPathfinder pathfinder;
    private readonly FishLVisionHelper vision;

    private float searchTimer;
    private Vector3 wanderTarget;

    public Vector3 SearchOrigin { get; set; }

    public FishLSearchState(FishLBrainSettings settings, EnemyNavPathfinder pathfinder, FishLVisionHelper vision)
    {
        s = settings;
        this.pathfinder = pathfinder;
        this.vision = vision;
    }

    public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
    {
        base.Enter(ctx, fsm);
        Ctx.Agent.speed = s.speed;
        searchTimer = s.searchDuration;
        PickSearchPoint();
        if (Ctx.AlertUI != null) Ctx.AlertUI.HideAttackWarning();
    }

public override void Tick(float deltaTime)
    {
        searchTimer -= deltaTime;

        // Re-spot player -> Follow
        if (vision.CheckLineOfSight())
        {
            if (Fsm.GetState(EnemyStateTag.Follow) is FishLFollowState follow)
                follow.LastKnownPlayerPosition = Ctx.PlayerTransform.position;
            Fsm.TransitionTo(EnemyStateTag.Follow);
            return;
        }

        if (searchTimer <= 0f)
        {
            Fsm.TransitionTo(EnemyStateTag.Wander);
            return;
        }

        if (Vector2.Distance(Ctx.Transform.position, wanderTarget) < 0.5f)
            PickSearchPoint();

        pathfinder.SetSafeDestination(wanderTarget);

        if (Ctx.Agent.velocity.sqrMagnitude > 0.01f)
            Ctx.Facing.RotateFromVelocity(Ctx.Agent.velocity);
    }

    private void PickSearchPoint()
    {
        pathfinder.TryPickWanderPoint(SearchOrigin, s.searchRadius, out wanderTarget);
    }
}
