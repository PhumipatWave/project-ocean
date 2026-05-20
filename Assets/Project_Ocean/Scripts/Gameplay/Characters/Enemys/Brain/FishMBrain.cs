using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Medium fish AI — FSM with 2 states: Wander and Follow (with built-in biting).
    /// </summary>
    public class FishMBrain : EnemyBrainBase, IBrainVision, IBrainAttack, IBrainHitReaction
    {
        private readonly FishMBrainSettings settings;
        private FishMFollowState followState;

        #region IBrainVision
        public float ViewAngle    => settings.viewAngle;
        public float ViewDistance => settings.viewDistance;

        public bool CanSeePlayer()
        {
            if (enemyBrainContext?.PlayerTransform == null) return false;

            // Use the agent's current velocity as the facing direction; fall back to Vector2.right.
            Vector2 facing = enemyBrainContext.Agent != null && enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f
                ? (Vector2)enemyBrainContext.Agent.velocity.normalized
                : Vector2.right;

            return EnemyVisionHelper.HasLineOfSightRay(
                enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position,
                facing,
                settings.viewDistance,
                settings.viewAngle,
                settings.obstacleLayer
            );
        }
        #endregion

        #region IBrainAttack
        // FishM has no sustained attack phase — biting is instant inside FollowState.
        public override bool IsAttacking => false;
        public float AttackRange => settings.attackRange;
        #endregion

        #region IBrainHitReaction
        public void OnBulletHit()
        {
            if (!settings.chasePlayerWhenHit) return;
            followState?.SetHitChaseTimer(settings.hitChaseDuration);
            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }

        public void OnPlayerCollision()
        {
            if (!settings.chasePlayerWhenHit) return;
            followState?.SetHitChaseTimer(settings.hitChaseDuration);
            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }
        #endregion

        public FishMBrain(FishMBrainSettings settings)
        {
            this.settings = settings;
        }

        public override void Initialize(EnemyBrainContext context)
        {
            base.Initialize(context);

            followState = new FishMFollowState(settings);

            enemyFSM.Register(new FishMWanderState(settings));
            enemyFSM.Register(followState);

            SetAgentSpeed(settings.speed);
            enemyFSM.Start(EnemyStateTag.Wander);
        }
    }
}