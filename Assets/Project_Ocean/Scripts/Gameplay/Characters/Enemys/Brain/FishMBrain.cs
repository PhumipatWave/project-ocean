using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Medium fish AI — FSM with 2 states: Wander and Follow (with built-in biting).
    /// </summary>
    public class FishMBrain : EnemyBrainBase, IBrainVision, IBrainAttack, IBrainHitReaction
    {
        private readonly FishMBrainSettings settings;
        private FishMVisionHelper vision;
        private FishMFollowState  followState;

        #region IBrainVision
        public float ViewAngle    => settings.viewAngle;
        public float ViewDistance => settings.viewDistance;

        public bool CanSeePlayer() => vision?.CheckLineOfSight() ?? false;
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

            vision      = new FishMVisionHelper(settings, context);
            followState = new FishMFollowState(settings, vision);

            enemyFSM.Register(new FishMWanderState(settings, vision));
            enemyFSM.Register(followState);

            SetAgentSpeed(settings.speed);
            enemyFSM.Start(EnemyStateTag.Wander);
        }
    }
}