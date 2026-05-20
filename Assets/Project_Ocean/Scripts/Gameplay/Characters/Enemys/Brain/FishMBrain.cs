using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Medium fish AI refactored to FSM.
    /// Registers 2 states: Wander and Follow (with built-in biting).
    /// </summary>
    public class FishMBrain : EnemyBrainBase, IBrainVision, IBrainAttack, IBrainHitReaction
    {
        private readonly FishMBrainSettings settings;
        private FishMFollowState followState;

        #region IBrainVision
        public float ViewAngle => settings.viewAngle;
        public float ViewDistance => settings.viewDistance;
        public bool CanSeePlayer() => enemyBrainContext?.PlayerTransform != null;
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

        #region IBrainAttack
        public float AttackRange => settings.attackRange;
        public void TryAttack() { /* Handled inside FollowState */ }
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

            enemyBrainContext.Agent.speed = settings.speed;
            enemyFSM.Start(EnemyStateTag.Wander);
        }
    }
}