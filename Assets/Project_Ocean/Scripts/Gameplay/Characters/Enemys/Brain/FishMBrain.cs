using UnityEngine;

/// <summary>
/// Medium fish AI refactored to FSM.
/// Registers 2 states: Wander and Follow (with built-in biting).
/// </summary>
public class FishMBrain : EnemyBrainBase, IBrainVision, IBrainAttack, IBrainHitReaction
{
    private readonly FishMBrainSettings settings;
    private FishMFollowState followState;

    #region IBrainVision
    public float ViewAngle => settings.viewAngle;
    public float ViewDistance => settings.viewDistance;
    public bool CanSeePlayer() => Ctx?.PlayerTransform != null;
    #endregion

    #region IBrainHitReaction
    public void OnBulletHit()
    {
        if (!settings.chasePlayerWhenHit) return;
        followState?.SetHitChaseTimer(settings.hitChaseDuration);
        FSM.TransitionTo(EnemyStateTag.Follow);
    }

    public void OnPlayerCollision()
    {
        if (!settings.chasePlayerWhenHit) return;
        followState?.SetHitChaseTimer(settings.hitChaseDuration);
        FSM.TransitionTo(EnemyStateTag.Follow);
    }
    #endregion

    #region IBrainAttack
    public float AttackRange => settings.attackRange;
    public void TryAttack() { /* Handled inside FollowState */ }
    #endregion

    public FishMBrain(FishMBrainSettings settings)
    {
        this.settings = settings;
    }

    public override void Initialize(EnemyBrainContext context)
    {
        base.Initialize(context);

        followState = new FishMFollowState(settings);

        FSM.Register(new FishMWanderState(settings));
        FSM.Register(followState);

        Ctx.Agent.speed = settings.speed;
        FSM.Start(EnemyStateTag.Wander);
    }
}
