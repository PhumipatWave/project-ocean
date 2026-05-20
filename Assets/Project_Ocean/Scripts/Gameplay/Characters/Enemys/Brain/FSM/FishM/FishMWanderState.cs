using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishM Wander state: random wandering with wall avoidance.
    /// Transitions -> Follow when line-of-sight or hit alert fires.
    /// </summary>
    public class FishMWanderState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Wander;

        private FishMBrainSettings fishMBrainSetting;

        private Vector2 currentDirection;
        public Vector2 CurrentDirection => currentDirection;

        private float wanderTimer;

        public FishMWanderState(FishMBrainSettings settings) { fishMBrainSetting = settings; }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            enemyBrainContext.Agent.speed = fishMBrainSetting.speed;
            wanderTimer = fishMBrainSetting.wanderInterval;
        }

        public override void Tick(float deltaTime)
        {
            wanderTimer -= deltaTime;

            // Spot player -> Follow
            if (enemyBrainContext.PlayerTransform != null)
            {
                Vector2 dir = currentDirection.sqrMagnitude > 0.01f ? currentDirection : Vector2.right;
                bool canSee = EnemyVisionHelper.HasLineOfSightRay(
                    enemyBrainContext.Transform.position, enemyBrainContext.PlayerTransform.position,
                    dir, fishMBrainSetting.viewDistance, fishMBrainSetting.viewAngle, fishMBrainSetting.obstacleLayer);
                if (canSee) { enemyFSM.TransitionTo(EnemyStateTag.Follow); return; }
            }

            if (wanderTimer <= 0f || enemyBrainContext.Agent.remainingDistance < 0.2f)
            {
                Vector2 rnd = UnityEngine.Random.insideUnitCircle * fishMBrainSetting.wanderRadius;
                enemyBrainContext.Agent.SetDestination(enemyBrainContext.Transform.position + new Vector3(rnd.x, rnd.y, 0f));
                wanderTimer = fishMBrainSetting.wanderInterval;
            }

            if (enemyBrainContext.Agent.velocity.magnitude > 0.1f)
                currentDirection = enemyBrainContext.Agent.velocity.normalized;

            TickWallAvoidance();
            if (currentDirection.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateTowards(currentDirection.normalized);
        }

        private void TickWallAvoidance()
        {
            if (currentDirection == Vector2.zero) return;
            RaycastHit2D hit = Physics2D.Raycast(enemyBrainContext.Transform.position, currentDirection, fishMBrainSetting.wallDetectDistance, fishMBrainSetting.wallLayer);
            if (hit.collider == null) return;
            Vector2 avoid = Vector2.Perpendicular(currentDirection) * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
            enemyBrainContext.Agent.SetDestination(enemyBrainContext.Transform.position + new Vector3(avoid.x, avoid.y, 0f));
        }
    }
}