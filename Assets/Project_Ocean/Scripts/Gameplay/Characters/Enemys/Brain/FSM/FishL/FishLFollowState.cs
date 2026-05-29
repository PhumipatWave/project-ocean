using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishL Follow state: moves toward last known player position.
    /// Transitions -> Prepare only when player is visible AND attack is off cooldown.
    /// Transitions -> Search when follow timer and hit-chase timer both expire.
    /// </summary>
    public class FishLFollowState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Follow;

        private readonly FishLBrainSettings settings;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly FishLVisionHelper vision;

        private float followTimer;
        private float hitChaseTimer;
        private float attackCooldown;

        public Vector3 LastKnownPlayerPosition { get; set; }

        public void SetHitChaseTimer(float duration) { hitChaseTimer = duration; }
        public void SetAttackCooldown(float duration) { attackCooldown = duration; }

        public FishLFollowState(FishLBrainSettings settings, EnemyNavPathfinder pathfinder, FishLVisionHelper vision)
        {
            this.settings   = settings;
            this.pathfinder = pathfinder;
            this.vision     = vision;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            SetAgentSpeed(settings.speed);
            followTimer = settings.followAfterLost;
            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.ShowAttackWarning();
        }

        public override void Tick(float deltaTime)
        {
            if (hitChaseTimer > 0f) hitChaseTimer -= deltaTime;
            if (followTimer   > 0f) followTimer   -= deltaTime;
            if (attackCooldown > 0f) attackCooldown -= deltaTime;

            bool canSee = vision.CheckLineOfSight();

            if (canSee || hitChaseTimer > 0f)
            {
                LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;
                followTimer = settings.followAfterLost;
                enemyBrainContext.AlertUI?.SetSeeingPlayer(canSee);
            }

            if (!pathfinder.SetSafeDestination(LastKnownPlayerPosition))
                pathfinder.SetSafeDestinationNear(LastKnownPlayerPosition);

            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);

            // -> Prepare: player visible, attack off cooldown, close enough to warrant charging
            float distToPlayer = enemyBrainContext.PlayerTransform != null
                ? Vector2.Distance(enemyBrainContext.Transform.position, enemyBrainContext.PlayerTransform.position)
                : float.MaxValue;

            bool closeEnough = distToPlayer <= settings.detectDistance * 0.75f;
            if ((canSee || hitChaseTimer > 0f) && attackCooldown <= 0f && closeEnough && enemyFSM.HasState(EnemyStateTag.Prepare))
            {
                enemyFSM.TransitionTo(EnemyStateTag.Prepare);
                return;
            }

            // -> Search: arrived at last known pos and timers expired
            float distToLast  = Vector2.Distance(enemyBrainContext.Transform.position, LastKnownPlayerPosition);
            bool timersExpired = followTimer <= 0f && hitChaseTimer <= 0f;
            if (distToLast < 0.5f && timersExpired && enemyFSM.HasState(EnemyStateTag.Search))
            {
                TransitionToSearchOrWander(LastKnownPlayerPosition);
                return;
            }
        }

        public override void Exit()
        {
            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.HideAttackWarning();
        }
    }
}
