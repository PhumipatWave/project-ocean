using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishM Wander state: random wandering with wall avoidance.
    /// Transitions -> Follow when line-of-sight OR hit alert fires.
    /// Vision delegated to FishMVisionHelper (no raw physics calls here).
    /// </summary>
    public class FishMWanderState : WanderStateBase
    {
        private readonly FishMBrainSettings settings;
        private readonly FishMVisionHelper  vision;

        // FIX #4: reference to FollowState so we can push the hit-chase timer
        // before transitioning, ensuring Follow knows the player was just hit.
        private readonly FishMFollowState followState;

        private Vector2 currentDirection;

        // FIX #4: hitChaseTimer is set externally (e.g. by the damage receiver)
        //         so Wander can react to hits even without line-of-sight.
        private float hitAlertTimer;

        public void SetHitAlertTimer(float duration) { hitAlertTimer = duration; }

        public FishMWanderState(FishMBrainSettings settings, FishMVisionHelper vision, FishMFollowState followState)
        {
            this.settings    = settings;
            this.vision      = vision;
            this.followState = followState;
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

        public override void Tick(float deltaTime)
        {
            // FIX #4: count down the hit-alert timer each tick.
            if (hitAlertTimer > 0f) hitAlertTimer -= deltaTime;

            base.Tick(deltaTime);
        }

        // FIX #4: detect either line-of-sight OR an active hit-alert.
        protected override bool ShouldTransitionAway()
        {
            return vision.CheckLineOfSight() || hitAlertTimer > 0f;
        }

        protected override void OnPlayerDetected()
        {
            // FIX #4: forward the remaining hit-alert time into FollowState so it
            // keeps chasing even if it loses sight immediately after the transition.
            if (hitAlertTimer > 0f)
                followState.SetHitChaseTimer(hitAlertTimer);

            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }
    }
}
