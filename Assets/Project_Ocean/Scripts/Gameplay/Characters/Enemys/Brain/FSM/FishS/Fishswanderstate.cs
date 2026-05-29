using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishS Wander state: uses shared pathfinder wander, then alerts nearby small fish when the player is spotted.
    /// </summary>
    public class FishSWanderState : WanderStateBase
    {
        private readonly FishSBrainSettings settings;
        private readonly FishSVisionHelper vision;

        public FishSWanderState(FishSBrainSettings settings, EnemyNavPathfinder pathfinder, FishSVisionHelper vision)
        {
            this.settings = settings;
            this.vision = vision;
            UsePathfinderWander(pathfinder, settings.wanderRadius);
        }

        protected override void OnEnterWander()
        {
            SetAgentSpeed(settings.speed);
            enemyBrainContext.AlertUI?.HideAll();
        }

        protected override float GetWanderInterval() => settings.wanderInterval;

        protected override bool ShouldTransitionAway()
        {
            return enemyBrainContext.PlayerTransform != null
                && vision != null
                && vision.CheckLineOfSight();
        }

        protected override void OnPlayerDetected()
        {
            if (enemyFSM.GetState(EnemyStateTag.Follow) is FishSFollowState follow)
                follow.LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;

            AlertNearbySmallFish();
            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }

        private void AlertNearbySmallFish()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                enemyBrainContext.Transform.position,
                settings.groupRadius
            );

            foreach (var hit in hits)
            {
                if (hit.gameObject == enemyBrainContext.Host.gameObject) continue;
                if (!hit.TryGetComponent<FishS_Agent>(out var other)) continue;

                var otherAgent = other.GetComponent<EnemyNavAgentBase>();
                if (otherAgent == null) continue;

                (otherAgent.ActiveBrain as IBrainHitReaction)?.OnBulletHit();
            }
        }
    }
}
