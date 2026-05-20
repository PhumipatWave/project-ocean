using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishL Wander state: picks random nav points and roams.
    /// Transitions -> Follow when player is seen or hit alert fires.
    /// </summary>
    public class FishLWanderState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Wander;

        private FishLBrainSettings fishLBrainSettings;
        private EnemyNavPathfinder pathfinder;
        private FishLVisionHelper vision;


        private float wanderTimer;
        private Vector3 wanderTarget;

        public FishLWanderState(FishLBrainSettings settings, EnemyNavPathfinder pathfinder, FishLVisionHelper vision)
        {
            fishLBrainSettings = settings;
            this.pathfinder = pathfinder;
            this.vision = vision;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            enemyBrainContext.Agent.speed = fishLBrainSettings.speed;
            wanderTimer = fishLBrainSettings.wanderInterval;
            PickWanderPoint();
            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.HideAll();
        }

        public override void Tick(float deltaTime)
        {
            wanderTimer -= deltaTime;
            if (wanderTimer <= 0f || Vector2.Distance(enemyBrainContext.Transform.position, wanderTarget) < 0.5f)
            {
                PickWanderPoint();
                wanderTimer = fishLBrainSettings.wanderInterval;
            }
            pathfinder.SetSafeDestination(wanderTarget);

            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);

            // Detect player -> Follow
            if (enemyBrainContext.PlayerTransform != null && vision != null && vision.CheckLineOfSight())
            {
                if (enemyFSM.GetState(EnemyStateTag.Follow) is FishLFollowState follow)
                    follow.LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
            }
        }

        private void PickWanderPoint()
        {
            pathfinder.TryPickWanderPoint(enemyBrainContext.Transform.position, fishLBrainSettings.wanderRadius, out wanderTarget);
        }
    }
}