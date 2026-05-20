using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishL Search state: wanders around the last seen position for a limited time.
    /// Transitions -> Follow if player spotted again.
    /// Transitions -> Wander when timer expires.
    /// </summary>
    public class FishLSearchState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Search;

        private FishLBrainSettings fishLBrainSettings;
        private EnemyNavPathfinder pathfinder;
        private FishLVisionHelper vision;

        private float searchTimer;
        private Vector3 wanderTarget;

        public Vector3 SearchOrigin { get; set; }

        public FishLSearchState(FishLBrainSettings settings, EnemyNavPathfinder pathfinder, FishLVisionHelper vision)
        {
            fishLBrainSettings = settings;
            this.pathfinder = pathfinder;
            this.vision = vision;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            enemyBrainContext.Agent.speed = fishLBrainSettings.speed;
            searchTimer = fishLBrainSettings.searchDuration;
            PickSearchPoint();
            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.HideAttackWarning();
        }

        public override void Tick(float deltaTime)
        {
            searchTimer -= deltaTime;

            // Re-spot player -> Follow
            if (vision.CheckLineOfSight())
            {
                if (enemyFSM.GetState(EnemyStateTag.Follow) is FishLFollowState follow)
                    follow.LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return;
            }

            if (searchTimer <= 0f)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
                return;
            }

            if (Vector2.Distance(enemyBrainContext.Transform.position, wanderTarget) < 0.5f)
                PickSearchPoint();

            pathfinder.SetSafeDestination(wanderTarget);

            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);
        }

        private void PickSearchPoint()
        {
            pathfinder.TryPickWanderPoint(SearchOrigin, fishLBrainSettings.searchRadius, out wanderTarget);
        }
    }
}