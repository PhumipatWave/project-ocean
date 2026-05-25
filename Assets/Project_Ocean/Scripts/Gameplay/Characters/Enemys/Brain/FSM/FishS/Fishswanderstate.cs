using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishS Wander state: picks random nav points and roams.
    /// Transitions -> Follow when player is spotted.
    /// </summary>
    public class FishSWanderState : WanderStateBase
    {
        private readonly FishSBrainSettings settings;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly FishSVisionHelper vision;

        private Vector3 wanderTarget;

        public FishSWanderState(FishSBrainSettings settings, EnemyNavPathfinder pathfinder, FishSVisionHelper vision)
        {
            this.settings   = settings;
            this.pathfinder = pathfinder;
            this.vision     = vision;
        }

        protected override void OnEnterWander()
        {
            enemyBrainContext.Agent.speed = settings.speed;
            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.HideAll();
        }

        protected override float GetWanderInterval() => settings.wanderInterval;

        protected override bool HasArrived() =>
            Vector2.Distance(enemyBrainContext.Transform.position, wanderTarget) < 0.5f;

        protected override void PickNewWanderTarget()
        {
            pathfinder.TryPickWanderPoint(
                enemyBrainContext.Transform.position,
                settings.wanderRadius,
                out wanderTarget
            );
        }

        protected override void ApplyMovement()
        {
            pathfinder.SetSafeDestination(wanderTarget);
        }

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

            // แจ้ง neighbor ในรัศมี groupRadius
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

            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }
    }
}