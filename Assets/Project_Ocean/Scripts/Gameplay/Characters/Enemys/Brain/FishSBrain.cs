using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Small fish AI with FishL-style FSM: Wander, Follow, Search, Prepare.
    /// No attack — Prepare leads to scatter (Wander) instead.
    /// Retains group/flock behavior via FishS_Agent steering.
    /// </summary>
    public class FishSBrain : EnemyBrainBase, IBrainVision, IBrainHitReaction
    {
        private readonly FishSBrainSettings settings;

        private FishSVisionHelper vision;
        private FishSFollowState followState;

        #region IBrainVision
        public float ViewDistance => settings.viewDistance;
        public float ViewAngle    => settings.viewAngle;
        public bool CanSeePlayer() => vision?.CheckLineOfSight() ?? false;
        #endregion

        #region IBrainHitReaction
        public void OnBulletHit()       => OnHit(enemyBrainContext?.PlayerTransform);
        public void OnPlayerCollision() => OnHit(enemyBrainContext?.PlayerTransform);
        #endregion

        public FishSBrain(FishSBrainSettings settings)
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

            vision      = new FishSVisionHelper(settings, context);
            followState = new FishSFollowState(settings, vision);

            enemyFSM.Register(new FishSWanderState(settings, pathfinder, vision));
            enemyFSM.Register(followState);
            enemyFSM.Register(new EnemySearchState(
                settings.speed,
                settings.searchDuration,
                settings.searchRadius,
                pathfinder,
                vision.CheckLineOfSight,
                position => followState.LastKnownPlayerPosition = position));

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

            bool committed = false;
            if (!committed)
            {
                ShowAttackWarning();
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
            }
        }
    }
}
