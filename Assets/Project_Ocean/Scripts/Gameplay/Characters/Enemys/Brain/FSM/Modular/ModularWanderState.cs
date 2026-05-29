using UnityEngine;

namespace Game.Testing
{
    public class ModularWanderState : WanderStateBase
    {
        private readonly EnemyBrainProfile profile;
        private readonly ModularEnemyVision vision;

        public ModularWanderState(EnemyBrainProfile profile, EnemyNavPathfinder pathfinder, ModularEnemyVision vision)
        {
            this.profile = profile;
            this.vision = vision;
            UsePathfinderWander(pathfinder, profile.wanderRadius);
        }

        protected override void OnEnterWander()
        {
            ResumeAgent(profile.speed);
            enemyBrainContext.AlertUI?.HideAll();
        }

        protected override float GetWanderInterval() => profile.wanderInterval;

        protected override bool ShouldTransitionAway()
        {
            return enemyBrainContext.PlayerTransform != null
                && vision != null
                && vision.CheckLineOfSight();
        }

        protected override void OnPlayerDetected()
        {
            if (enemyFSM.GetState(EnemyStateTag.Follow) is ModularFollowState follow)
                follow.LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;

            if (profile.alertNearbyAllies)
                AlertNearbyAllies();

            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }

        private void AlertNearbyAllies()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                enemyBrainContext.Transform.position,
                profile.groupAlertRadius
            );

            foreach (var hit in hits)
            {
                if (hit.gameObject == enemyBrainContext.Host.gameObject) continue;
                if (!hit.TryGetComponent<EnemyNavAgentBase>(out var otherAgent)) continue;

                (otherAgent.ActiveBrain as IBrainHitReaction)?.OnBulletHit();
            }
        }
    }
}
