using UnityEngine;

/// <summary>
/// Large fish AI — FSM with 5 states: Wander, Follow, Search, Prepare, Attack.
/// </summary>
public class FishLBrain : EnemyBrainBase, IBrainVision, IBrainAttack, IBrainHitReaction
{
    private readonly FishLBrainSettings settings;

    private FishLVisionHelper vision;
    private FishLFollowState followState;
    private FishLAttackState attackState;

    public override bool IsAttacking => attackState?.IsAttacking ?? false;

    #region IBrainVision
    public float ViewDistance => settings.viewDistance;
    public float ViewAngle => settings.viewAngle;
    public bool CanSeePlayer() => vision?.CheckLineOfSight() ?? false;
    #endregion

    #region IBrainHitReaction
    public void OnBulletHit() => OnHit(Ctx?.PlayerTransform);
    public void OnPlayerCollision() => OnHit(Ctx?.PlayerTransform);
    #endregion

    #region IBrainAttack
    public float AttackRange => settings.attackRange;
    public void TryAttack() { }
    #endregion

    public FishLBrain(FishLBrainSettings settings)
    {
        this.settings = settings;
    }

public override void Initialize(EnemyBrainContext context)
    {
        base.Initialize(context);

        var pathfinder = new EnemyNavPathfinder(
            context.Agent,
            settings.wallLayer,
            settings.fishClearanceRadius,
            settings.maxWanderSampleTries,
            settings.nearPlayerSearchRadius,
            settings.nearPlayerSearchSamples,
            settings.nearPlayerSearchStep
        );

        vision      = new FishLVisionHelper(settings, context);
        followState = new FishLFollowState(settings, pathfinder, vision);
        attackState = new FishLAttackState(settings, pathfinder, vision);

        FSM.Register(new FishLWanderState(settings, pathfinder, vision));
        FSM.Register(followState);
        FSM.Register(new FishLSearchState(settings, pathfinder, vision));
        FSM.Register(new FishLPrepareState(settings, vision));
        FSM.Register(attackState);

        HideAllAlerts();

        if (context.PlayerTransform != null)
            followState.LastKnownPlayerPosition = context.PlayerTransform.position;

        FSM.Start(EnemyStateTag.Wander);
    }

    public override void OnHit(Transform attacker)
    {
        if (!settings.chasePlayerWhenHit) return;

        Vector3 knownPos = attacker != null ? attacker.position
            : Ctx.PlayerTransform != null ? Ctx.PlayerTransform.position
            : Ctx.Transform.position;

        followState.LastKnownPlayerPosition = knownPos;
        followState.SetHitChaseTimer(settings.hitChaseDuration);

        bool committed = FSM.IsIn(EnemyStateTag.Prepare) || FSM.IsIn(EnemyStateTag.Attack);
        if (!committed)
        {
            ShowAttackWarning();
            FSM.TransitionTo(EnemyStateTag.Follow);
        }
    }
}
