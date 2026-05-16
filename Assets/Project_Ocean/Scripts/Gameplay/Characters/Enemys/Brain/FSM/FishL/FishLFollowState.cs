using UnityEngine;

/// <summary>
/// FishL Follow state: moves toward last known player position.
/// Transitions -> Prepare only when player is visible AND attack is off cooldown.
/// Transitions -> Search when follow timer and hit-chase timer both expire.
/// </summary>
public class FishLFollowState : EnemyStateBase
{
    public override EnemyStateTag StateTag => EnemyStateTag.Follow;

    private readonly FishLBrainSettings s;
    private readonly EnemyNavPathfinder pathfinder;
    private readonly FishLVisionHelper vision;

    private float followTimer;
    private float hitChaseTimer;
    private float attackCooldown;

    public Vector3 LastKnownPlayerPosition { get; set; }

    public void SetHitChaseTimer(float duration) { hitChaseTimer = duration; }
    public void SetAttackCooldown(float duration) { attackCooldown = duration; }

    public FishLFollowState(FishLBrainSettings settings, EnemyNavPathfinder pathfinder, FishLVisionHelper vision)
    {
        s = settings;
        this.pathfinder = pathfinder;
        this.vision = vision;
    }

    public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
    {
        base.Enter(ctx, fsm);
        Ctx.Agent.speed = s.speed;
        followTimer = s.followAfterLost;
        if (Ctx.AlertUI != null) Ctx.AlertUI.ShowAttackWarning();
    }

public override void Tick(float deltaTime)
    {
        if (hitChaseTimer > 0f) hitChaseTimer -= deltaTime;
        if (followTimer > 0f) followTimer -= deltaTime;
        if (attackCooldown > 0f) attackCooldown -= deltaTime;

        bool canSee = vision.CheckLineOfSight();

        if (canSee || hitChaseTimer > 0f)
        {
            LastKnownPlayerPosition = Ctx.PlayerTransform.position;
            followTimer = s.followAfterLost;
            Ctx.AlertUI?.SetSeeingPlayer(canSee);
        }

        // Move toward last known position first
        if (!pathfinder.SetSafeDestination(LastKnownPlayerPosition))
            pathfinder.SetSafeDestinationNear(LastKnownPlayerPosition);

        if (Ctx.Agent.velocity.sqrMagnitude > 0.01f)
            Ctx.Facing.RotateFromVelocity(Ctx.Agent.velocity);

        // -> Prepare: player visible, attack off cooldown, and close enough to warrant charging
        float distToPlayer = Ctx.PlayerTransform != null
            ? Vector2.Distance(Ctx.Transform.position, Ctx.PlayerTransform.position)
            : float.MaxValue;

        bool closeEnough = distToPlayer <= s.detectDistance * 0.75f;
        if ((canSee || hitChaseTimer > 0f) && attackCooldown <= 0f && closeEnough && Fsm.HasState(EnemyStateTag.Prepare))
        {
            Fsm.TransitionTo(EnemyStateTag.Prepare);
            return;
        }

        // -> Search: arrived at last known pos and timers expired
        float distToLast = Vector2.Distance(Ctx.Transform.position, LastKnownPlayerPosition);
        bool timersExpired = followTimer <= 0f && hitChaseTimer <= 0f;
        if (distToLast < 0.5f && timersExpired && Fsm.HasState(EnemyStateTag.Search))
        {
            if (Fsm.GetState(EnemyStateTag.Search) is FishLSearchState search)
                search.SearchOrigin = LastKnownPlayerPosition;
            Fsm.TransitionTo(EnemyStateTag.Search);
            return;
        }
    }

    public override void Exit()
    {
        if (Ctx.AlertUI != null) Ctx.AlertUI.HideAttackWarning();
    }
}
