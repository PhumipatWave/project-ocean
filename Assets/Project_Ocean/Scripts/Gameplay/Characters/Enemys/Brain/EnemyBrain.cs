using UnityEngine;

namespace Game.Testing
{
    public class EnemyBrain : EnemyBrainBase, IBrainVision, IBrainAttack, IBrainHitReaction
    {
        private readonly EnemyBaseProfile profile;

        private EnemyVision vision;
        private EnemyFollowState followState;
        private EnemyAttackState attackState;

        public override bool IsAttacking => attackState?.IsAttacking ?? false;
        public float ViewDistance => profile.viewDistance;
        public float ViewAngle => profile.viewAngle;
        public float AttackRange => (profile as AggressiveEnemyProfile)?.attackRange ?? 0f;

        public bool CanSeePlayer() => vision?.CheckLineOfSight() ?? false;
        public void OnBulletHit() => OnHit(enemyBrainContext?.PlayerTransform);
        public void OnPlayerCollision()
        {
            // Friendly / Scared fish only flee from bullets, not body collision
            if (profile is FriendlyEnemyProfile || profile is ScaredEnemyProfile) return;
            OnHit(enemyBrainContext?.PlayerTransform);
        }

        public EnemyBrain(EnemyBaseProfile profile)
        {
            this.profile = profile;
        }

public override void Initialize(EnemyBrainContext context)
        {
            base.Initialize(context);

            if (profile == null)
            {
                Debug.LogError("[EnemyBrain] Missing brain profile.", context.Host);
                return;
            }

            var aggressiveProfile = profile as AggressiveEnemyProfile;

            // Pathfinding fields exist on all three profile types
            float clearanceRadius      = GetClearanceRadius();
            int   maxWanderTries       = GetMaxWanderSampleTries();
            float nearPlayerRadius     = GetNearPlayerSearchRadius();
            int   nearPlayerSamples    = GetNearPlayerSearchSamples();
            float nearPlayerStep       = GetNearPlayerSearchStep();

            var pathfinder = new EnemyNavPathfinder(
                context.Agent,
                profile.wallLayer,
                clearanceRadius,
                maxWanderTries,
                nearPlayerRadius,
                nearPlayerSamples,
                nearPlayerStep
            );

            vision      = new EnemyVision(profile, context);
            followState = new EnemyFollowState(profile, pathfinder, vision);
            attackState = (aggressiveProfile != null && aggressiveProfile.canAttack)
                ? new EnemyAttackState(aggressiveProfile, pathfinder, vision)
                : null;

            enemyFSM.Register(new EnemyWanderState(profile, pathfinder, vision));
            enemyFSM.Register(followState);

            // Friendly and Scared get a dedicated flee state
            if (profile is FriendlyEnemyProfile || profile is ScaredEnemyProfile)
                enemyFSM.Register(new EnemyFleeState(profile, pathfinder));

            if (aggressiveProfile != null && aggressiveProfile.searchAfterLost)
            {
                enemyFSM.Register(new EnemySearchState(
                    profile.speed,
                    aggressiveProfile.searchDuration,
                    aggressiveProfile.searchRadius,
                    pathfinder,
                    vision.CheckLineOfSight,
                    position => followState.LastKnownPlayerPosition = position));
            }

            if (aggressiveProfile != null && aggressiveProfile.canAttack && aggressiveProfile.prepareBeforeAttack)
                enemyFSM.Register(new EnemyPrepareAttackState(aggressiveProfile, vision));

            if (attackState != null)
                enemyFSM.Register(attackState);

            HideAllAlerts();

            if (context.PlayerTransform != null)
                followState.LastKnownPlayerPosition = context.PlayerTransform.position;

            enemyFSM.Start(EnemyStateTag.Wander);
        }

// ── Pathfinding field helpers (shared across all three profile types) ────
        private float GetClearanceRadius() => profile switch
        {
            AggressiveEnemyProfile a => a.fishClearanceRadius,
            FriendlyEnemyProfile   f => f.fishClearanceRadius,
            ScaredEnemyProfile     s => s.fishClearanceRadius,
            _                        => 0.5f
        };

        private int GetMaxWanderSampleTries() => profile switch
        {
            AggressiveEnemyProfile a => a.maxWanderSampleTries,
            FriendlyEnemyProfile   f => f.maxWanderSampleTries,
            ScaredEnemyProfile     s => s.maxWanderSampleTries,
            _                        => 20
        };

        private float GetNearPlayerSearchRadius() => profile switch
        {
            AggressiveEnemyProfile a => a.nearPlayerSearchRadius,
            FriendlyEnemyProfile   f => f.nearPlayerSearchRadius,
            ScaredEnemyProfile     s => s.nearPlayerSearchRadius,
            _                        => 5f
        };

        private int GetNearPlayerSearchSamples() => profile switch
        {
            AggressiveEnemyProfile a => a.nearPlayerSearchSamples,
            FriendlyEnemyProfile   f => f.nearPlayerSearchSamples,
            ScaredEnemyProfile     s => s.nearPlayerSearchSamples,
            _                        => 24
        };

        private float GetNearPlayerSearchStep() => profile switch
        {
            AggressiveEnemyProfile a => a.nearPlayerSearchStep,
            FriendlyEnemyProfile   f => f.nearPlayerSearchStep,
            ScaredEnemyProfile     s => s.nearPlayerSearchStep,
            _                        => 0.5f
        };

public override void OnHit(Transform attacker)
        {
            if (followState == null) return;

            Vector3 knownPosition = attacker != null ? attacker.position
                : enemyBrainContext.PlayerTransform != null ? enemyBrainContext.PlayerTransform.position
                : enemyBrainContext.Transform.position;

            // Friendly and Scared fish panic-flee when hit by bullet
            if (profile is FriendlyEnemyProfile || profile is ScaredEnemyProfile)
            {
                if (enemyFSM.HasState(EnemyStateTag.Flee))
                {
                    var fleeState = enemyFSM.GetState(EnemyStateTag.Flee) as EnemyFleeState;

                    if (enemyFSM.IsIn(EnemyStateTag.Flee))
                        fleeState?.EnterPanic();
                    else
                    {
                        enemyFSM.TransitionTo(EnemyStateTag.Flee);
                        fleeState?.EnterPanic();
                    }
                }
                return;
            }

            // Aggressive fish chase when hit
            var aggressiveProfile = profile as AggressiveEnemyProfile;
            if (aggressiveProfile == null || !aggressiveProfile.chasePlayerWhenHit) return;

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
