using Game.Core;

namespace Game.Gameplay
{
    public class EnemyPrepareAttackState : EnemyStateBase
    {
        #region FieldsProperties
        public override EnemyStateTag StateTag => EnemyStateTag.Prepare;

        private readonly AggressiveEnemyProfile profile;

        private float prepareTimer;
        #endregion

        #region Constructors
        public EnemyPrepareAttackState(AggressiveEnemyProfile profile)
        {
            this.profile = profile;
        }
        #endregion

        #region StateCallbacks
        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            prepareTimer = profile.prepareTime;
            StopAgent();
            enemyBrainContext.AlertUI?.ShowAttackWarning();
        }

        public override void Tick(float deltaTime)
        {
            StopAgent();
            prepareTimer -= deltaTime;

            // Only abort if the player has completely disappeared from the scene.
            // A brief LOS break (player steps behind a rock) should NOT cancel the
            // windup — the fish has already committed to this attack.
            if (enemyBrainContext.PlayerTransform == null)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
                return;
            }

            bool facingPlayer = RotateFacingToPlayer();

            if (prepareTimer <= 0f && facingPlayer)
                enemyFSM.TransitionTo(EnemyStateTag.Attack);
        }

        public override void Exit()
        {
            ResumeAgent(profile.boostSpeed);
        }
        #endregion
    }
}
