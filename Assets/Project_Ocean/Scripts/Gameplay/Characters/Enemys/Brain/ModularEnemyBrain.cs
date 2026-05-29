using UnityEngine;

namespace Game.Testing
{
    public class ModularEnemyBrain : EnemyBrainBase, IBrainVision, IBrainAttack, IBrainHitReaction
    {
        private readonly EnemyBrainProfile profile;

        private ModularEnemyVision vision;
        private ModularFollowState followState;
        private ModularAttackState attackState;

        public override bool IsAttacking => attackState?.IsAttacking ?? false;
        public float ViewDistance => profile.viewDistance;
        public float ViewAngle => profile.viewAngle;
        public float AttackRange => profile.attackRange;

        public bool CanSeePlayer() => vision?.CheckLineOfSight() ?? false;
        public void OnBulletHit() => OnHit(enemyBrainContext?.PlayerTransform);
        public void OnPlayerCollision() => OnHit(enemyBrainContext?.PlayerTransform);

        public ModularEnemyBrain(EnemyBrainProfile profile)
        {
            this.profile = profile;
        }

        public override void Initialize(EnemyBrainContext context)
        {
            base.Initialize(context);

            if (profile == null)
            {
                Debug.LogError("[ModularEnemyBrain] Missing EnemyBrainProfile.", context.Host);
                return;
            }

            var pathfinder = new EnemyNavPathfinder(
                context.Agent,
                profile.wallLayer,
                profile.fishClearanceRadius,
                profile.maxWanderSampleTries,
                profile.nearPlayerSearchRadius,
                profile.nearPlayerSearchSamples,
                profile.nearPlayerSearchStep
            );

            vision = new ModularEnemyVision(profile, context);
            followState = new ModularFollowState(profile, pathfinder, vision);
            attackState = profile.canAttack
                ? new ModularAttackState(profile, pathfinder, vision)
                : null;

            enemyFSM.Register(new ModularWanderState(profile, pathfinder, vision));
            enemyFSM.Register(followState);

            if (profile.searchAfterLost)
            {
                enemyFSM.Register(new EnemySearchState(
                    profile.speed,
                    profile.searchDuration,
                    profile.searchRadius,
                    pathfinder,
                    vision.CheckLineOfSight,
                    position => followState.LastKnownPlayerPosition = position));
            }

            if (profile.canAttack && profile.prepareBeforeAttack)
                enemyFSM.Register(new ModularPrepareState(profile, vision));

            if (attackState != null)
                enemyFSM.Register(attackState);

            HideAllAlerts();

            if (context.PlayerTransform != null)
                followState.LastKnownPlayerPosition = context.PlayerTransform.position;

            enemyFSM.Start(EnemyStateTag.Wander);
        }

        public override void OnHit(Transform attacker)
        {
            if (!profile.chasePlayerWhenHit || followState == null) return;

            Vector3 knownPosition = attacker != null ? attacker.position
                : enemyBrainContext.PlayerTransform != null ? enemyBrainContext.PlayerTransform.position
                : enemyBrainContext.Transform.position;

            followState.LastKnownPlayerPosition = knownPosition;
            followState.SetHitChaseTimer(profile.hitChaseDuration);

            bool committed = enemyFSM.IsIn(EnemyStateTag.Prepare) || enemyFSM.IsIn(EnemyStateTag.Attack);
            if (!committed)
            {
                ShowAttackWarning();
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
            }
        }
    }
}
