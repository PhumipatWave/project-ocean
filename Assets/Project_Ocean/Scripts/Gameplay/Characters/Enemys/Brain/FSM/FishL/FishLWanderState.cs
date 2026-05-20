using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishL Wander state: picks random nav points and roams.
    /// Extends WanderStateBase — the timer loop and player-detection hook
    /// are inherited; this class only supplies FishL-specific implementations.
    /// Transitions -> Follow when player is seen or hit alert fires.
    /// </summary>
    public class FishLWanderState : WanderStateBase
    {
        private readonly FishLBrainSettings settings;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly FishLVisionHelper vision;

        private Vector3 wanderTarget;

        public FishLWanderState(FishLBrainSettings settings, EnemyNavPathfinder pathfinder, FishLVisionHelper vision)
        {
            this.settings   = settings;
            this.pathfinder = pathfinder;
            this.vision     = vision;
        }

        // ── WanderStateBase overrides ────────────────────────────────────────────

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
            if (enemyFSM.GetState(EnemyStateTag.Follow) is FishLFollowState follow)
                follow.LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;

            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }
    }
}