using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishM Follow state: chases player and bites on contact.
    /// Extends EnemyStateBase directly — wall avoidance and line-of-sight
    /// are handled inline, removing the need for FishMStateBase.
    /// Transitions -> Wander when sight is lost and hit timer expires.
    /// </summary>
    public class FishMFollowState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Follow;

        private readonly FishMBrainSettings settings;
        private Vector2 currentDirection;
        private float attackTimer;
        private float hitChaseTimer;

        public void SetHitChaseTimer(float duration) { hitChaseTimer = duration; }

        public FishMFollowState(FishMBrainSettings settings)
        {
            this.settings = settings;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            enemyBrainContext.Agent.speed = settings.speed;
        }

        public override void Tick(float deltaTime)
        {
            if (hitChaseTimer > 0f) hitChaseTimer -= deltaTime;
            if (attackTimer  > 0f) attackTimer  -= deltaTime;

            if (enemyBrainContext.PlayerTransform == null)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
                return;
            }

            bool canSee  = CheckLineOfSight(currentDirection);
            bool alerted = hitChaseTimer > 0f;

            if (!canSee && !alerted)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
                return;
            }

            enemyBrainContext.Agent.SetDestination(enemyBrainContext.PlayerTransform.position);

            if (enemyBrainContext.Agent.velocity.magnitude > 0.1f)
                currentDirection = enemyBrainContext.Agent.velocity.normalized;

            float dist = Vector2.Distance(
                enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position
            );

            if (dist <= settings.attackRange && attackTimer <= 0f)
            {
                enemyBrainContext.Player.TakeDamage(settings.attackDamage);
                attackTimer = settings.attackCooldown;
            }

            TickWallAvoidance(currentDirection);

            if (currentDirection.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateTowards(currentDirection.normalized);
        }

        // ── Vision ───────────────────────────────────────────────────────────────────────

        private bool CheckLineOfSight(Vector2 facingDirection)
        {
            if (enemyBrainContext.PlayerTransform == null) return false;

            Vector2 dir = facingDirection.sqrMagnitude > 0.01f ? facingDirection : Vector2.right;

            return EnemyVisionHelper.HasLineOfSightRay(
                enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position,
                dir,
                settings.viewDistance,
                settings.viewAngle,
                settings.obstacleLayer
            );
        }

        // ── Wall Avoidance ────────────────────────────────────────────────────────────

        private void TickWallAvoidance(Vector2 currentDir)
        {
            if (currentDir == Vector2.zero) return;

            RaycastHit2D hit = Physics2D.Raycast(
                enemyBrainContext.Transform.position,
                currentDir,
                settings.wallDetectDistance,
                settings.wallLayer
            );

            if (hit.collider == null) return;

            Vector2 avoid = Vector2.Perpendicular(currentDir)
                            * (UnityEngine.Random.value > 0.5f ? 1f : -1f);

            enemyBrainContext.Agent.SetDestination(
                enemyBrainContext.Transform.position + new Vector3(avoid.x, avoid.y, 0f)
            );
        }
    }
}