using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishM Wander state: random wandering with wall avoidance.
    /// Transitions -> Follow when line-of-sight or hit alert fires.
    /// </summary>
    public class FishMWanderState : WanderStateBase
    {
        private readonly FishMBrainSettings settings;
        private Vector2 currentDirection;

        public FishMWanderState(FishMBrainSettings settings)
        {
            this.settings = settings;
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

            if (currentDirection == Vector2.zero) return;
            RaycastHit2D hit = Physics2D.Raycast(
                enemyBrainContext.Transform.position, currentDirection,
                settings.wallDetectDistance, settings.wallLayer
            );
            if (hit.collider != null)
            {
                Vector2 avoid = Vector2.Perpendicular(currentDirection)
                                * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
                enemyBrainContext.Agent.SetDestination(
                    enemyBrainContext.Transform.position + new Vector3(avoid.x, avoid.y, 0f)
                );
            }
        }

        protected override void UpdateFacing()
        {
            if (currentDirection.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateTowards(currentDirection.normalized);
        }

        protected override bool ShouldTransitionAway()
        {
            if (enemyBrainContext.PlayerTransform == null) return false;
            Vector2 dir = currentDirection.sqrMagnitude > 0.01f ? currentDirection : Vector2.right;
            return EnemyVisionHelper.HasLineOfSightRay(
                enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position,
                dir,
                settings.viewDistance,
                settings.viewAngle,
                settings.obstacleLayer
            );
        }

        protected override void OnPlayerDetected() =>
            enemyFSM.TransitionTo(EnemyStateTag.Follow);
    }
}