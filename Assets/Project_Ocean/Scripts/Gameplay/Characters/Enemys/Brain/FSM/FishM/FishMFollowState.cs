using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishM Follow state: chases player and bites on contact.
    /// Tracks last known player position; transitions -> Search when sight is lost
    /// and all timers expire. Falls back to Wander if no Search state is registered.
    /// </summary>
    public class FishMFollowState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Follow;

        private readonly FishMBrainSettings settings;
        private readonly FishMVisionHelper  vision;

        private Vector2 currentDirection;
        private float   attackTimer;
        private float   hitChaseTimer;
        private float   followTimer;

        // FIX #3: private set — only this state should update the last known position.
        public Vector3 LastKnownPlayerPosition { get; private set; }

        public void SetHitChaseTimer(float duration) { hitChaseTimer = duration; }

        public FishMFollowState(FishMBrainSettings settings, FishMVisionHelper vision)
        {
            this.settings = settings;
            this.vision   = vision;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            SetAgentSpeed(settings.speed);
            followTimer = settings.followAfterLost;

            if (enemyBrainContext.PlayerTransform != null)
                LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;
        }

        public override void Tick(float deltaTime)
        {
            if (hitChaseTimer > 0f) hitChaseTimer -= deltaTime;
            if (followTimer   > 0f) followTimer   -= deltaTime;
            if (attackTimer   > 0f) attackTimer   -= deltaTime;

            if (enemyBrainContext.PlayerTransform == null)
            {
                TransitionOnLost();
                return;
            }

            bool canSee  = vision.CheckLineOfSight();
            bool alerted = hitChaseTimer > 0f;

            // Refresh last known position while player is visible or we're hit-alerted.
            if (canSee || alerted)
            {
                LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;
                followTimer = settings.followAfterLost;
            }

            if (!canSee && !alerted && followTimer <= 0f)
            {
                TransitionOnLost();
                return;
            }

            // Move toward last known position.
            enemyBrainContext.Agent.SetDestination(LastKnownPlayerPosition);

            UpdateDirectionFromAgentVelocity(ref currentDirection);

            // FIX #5: use LastKnownPlayerPosition consistently for attack range check.
            float dist = Vector2.Distance(
                enemyBrainContext.Transform.position,
                LastKnownPlayerPosition
            );

            if (dist <= settings.attackRange && attackTimer <= 0f)
            {
                enemyBrainContext.Player.TakeDamage(settings.attackDamage);
                attackTimer = settings.attackCooldown;
            }

            ApplyWallAvoidance(currentDirection, settings.wallDetectDistance, settings.wallLayer);

            RotateTowardsDirection(currentDirection);
        }

        // ── Helpers ─────────────────────────────────────────────────────────────────────

        private void TransitionOnLost()
        {
            // FIX #1: Try Search state first; fall back to Wander if it is not registered.
            TransitionToSearchOrWander(LastKnownPlayerPosition);
        }
    }
}
