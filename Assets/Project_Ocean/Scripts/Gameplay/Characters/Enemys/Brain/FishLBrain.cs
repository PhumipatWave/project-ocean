using UnityEngine;

namespace Game.Testing
{
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
        public void OnBulletHit() => OnHit(enemyBrainContext?.PlayerTransform);
        public void OnPlayerCollision() => OnHit(enemyBrainContext?.PlayerTransform);
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

            vision = new FishLVisionHelper(settings, context);
            followState = new FishLFollowState(settings, pathfinder, vision);
            attackState = new FishLAttackState(settings, pathfinder, vision);

            enemyFSM.Register(new FishLWanderState(settings, pathfinder, vision));
            enemyFSM.Register(followState);
            enemyFSM.Register(new EnemySearchState(
                settings.speed,
                settings.searchDuration,
                settings.searchRadius,
                pathfinder,
                vision.CheckLineOfSight,
                position => followState.LastKnownPlayerPosition = position));
            enemyFSM.Register(new FishLPrepareState(settings, vision));
            enemyFSM.Register(attackState);

            HideAllAlerts();

            if (context.PlayerTransform != null)
                followState.LastKnownPlayerPosition = context.PlayerTransform.position;

            enemyFSM.Start(EnemyStateTag.Wander);
        }

        public override void OnHit(Transform attacker)
        {
            if (!settings.chasePlayerWhenHit) return;

            Vector3 knownPos = attacker != null ? attacker.position
                : enemyBrainContext.PlayerTransform != null ? enemyBrainContext.PlayerTransform.position
                : enemyBrainContext.Transform.position;

            followState.LastKnownPlayerPosition = knownPos;
            followState.SetHitChaseTimer(settings.hitChaseDuration);

            bool committed = enemyFSM.IsIn(EnemyStateTag.Prepare) || enemyFSM.IsIn(EnemyStateTag.Attack);
            if (!committed)
            {
                ShowAttackWarning();
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
            }
        }
    }
}
