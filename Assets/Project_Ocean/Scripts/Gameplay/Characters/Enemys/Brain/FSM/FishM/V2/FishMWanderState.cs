using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishM Wander state: random wandering with wall avoidance.
    /// Transitions -> Follow when line-of-sight or hit alert fires.
    /// Vision delegated to FishMVisionHelper (no raw physics calls here).
    /// </summary>
    public class FishMWanderState : WanderStateBase
    {
        private readonly FishMBrainSettings settings;
        private readonly FishMVisionHelper  vision;

        private Vector2 currentDirection;

        public FishMWanderState(FishMBrainSettings settings, FishMVisionHelper vision)
        {
            this.settings = settings;
            this.vision   = vision;
        }

        protected override void OnEnterWander()
        {
            enemyBrainContext.Agent.speed = settings.speed;
        }

        protected override float GetWanderInterval() => settings.wanderInterval;

        protected override bool HasArrived() =>
            enemyBrainContext.Agent.remainingDistance < 0.2f;

        protected override void PickNewWanderTarget()
        {
            Vector2 rnd = UnityEngine.Random.insideUnitCircle * settings.wanderRadius;
            enemyBrainContext.Agent.SetDestination(
                enemyBrainContext.Transform.position + new Vector3(rnd.x, rnd.y, 0f)
            );
        }

        protected override void ApplyMovement()
        {
            if (enemyBrainContext.Agent.velocity.magnitude > 0.1f)
                currentDirection = enemyBrainContext.Agent.velocity.normalized;

            ApplyWallAvoidance(currentDirection, settings.wallDetectDistance, settings.wallLayer);
        }

        protected override void UpdateFacing()
        {
            if (currentDirection.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateTowards(currentDirection.normalized);
        }

        protected override bool ShouldTransitionAway()
        {
            return vision.CheckLineOfSight();
        }

        protected override void OnPlayerDetected() =>
            enemyFSM.TransitionTo(EnemyStateTag.Follow);
    }
}