using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishM Follow state: chases player and bites on contact.
    /// Transitions -> Wander when sight is lost and hit timer expires.
    /// </summary>
    public class FishMFollowState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Follow;

        private FishMBrainSettings fishMBrainSetting;
        private Vector2 currentDirection;
        private float attackTimer;
        private float hitChaseTimer;

        public void SetHitChaseTimer(float duration) { hitChaseTimer = duration; }

        public FishMFollowState(FishMBrainSettings settings) { fishMBrainSetting = settings; }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            enemyBrainContext.Agent.speed = fishMBrainSetting.speed;
        }

        public override void Tick(float deltaTime)
        {
            if (hitChaseTimer > 0f) hitChaseTimer -= deltaTime;
            if (attackTimer > 0f) attackTimer -= deltaTime;

            if (enemyBrainContext.PlayerTransform == null) { enemyFSM.TransitionTo(EnemyStateTag.Wander); return; }

            bool canSee = CheckLineOfSight();
            bool alerted = hitChaseTimer > 0f;

            if (!canSee && !alerted)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
                return;
            }

            enemyBrainContext.Agent.SetDestination(enemyBrainContext.PlayerTransform.position);

            if (enemyBrainContext.Agent.velocity.magnitude > 0.1f)
                currentDirection = enemyBrainContext.Agent.velocity.normalized;

            float dist = Vector2.Distance(enemyBrainContext.Transform.position, enemyBrainContext.PlayerTransform.position);
            if (dist <= fishMBrainSetting.attackRange && attackTimer <= 0f)
            {
                enemyBrainContext.Player.TakeDamage(fishMBrainSetting.attackDamage);
                attackTimer = fishMBrainSetting.attackCooldown;
            }

            TickWallAvoidance();
            if (currentDirection.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateTowards(currentDirection.normalized);
        }

        private bool CheckLineOfSight()
        {
            return EnemyVisionHelper.HasLineOfSightRay(
                enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position,
                currentDirection,
                fishMBrainSetting.viewDistance,
                fishMBrainSetting.viewAngle,
                fishMBrainSetting.obstacleLayer
            );
        }

        private void TickWallAvoidance()
        {
            if (currentDirection == Vector2.zero) return;
            RaycastHit2D hit = Physics2D.Raycast(enemyBrainContext.Transform.position, currentDirection, fishMBrainSetting.wallDetectDistance, fishMBrainSetting.wallLayer);
            if (hit.collider == null) return;
            Vector2 avoid = Vector2.Perpendicular(currentDirection) * (Random.value > 0.5f ? 1f : -1f);
            enemyBrainContext.Agent.SetDestination(enemyBrainContext.Transform.position + new Vector3(avoid.x, avoid.y, 0f));
        }
    }
}