namespace Game.Testing
{
    /// <summary>
    /// FishL Wander state: uses shared pathfinder wander and transitions to Follow when the player is spotted.
    /// </summary>
    public class FishLWanderState : WanderStateBase
    {
        private readonly FishLBrainSettings settings;
        private readonly FishLVisionHelper vision;

        public FishLWanderState(FishLBrainSettings settings, EnemyNavPathfinder pathfinder, FishLVisionHelper vision)
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
            if (enemyFSM.GetState(EnemyStateTag.Follow) is FishLFollowState follow)
                follow.LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;

            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }
    }
}
