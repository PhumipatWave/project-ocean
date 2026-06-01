namespace Game.Testing
{
    public class EnemyPrepareAttackState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Prepare;

        private readonly AggressiveEnemyProfile profile;
        private readonly EnemyVision vision;

        private float prepareTimer;

        public EnemyPrepareAttackState(AggressiveEnemyProfile profile, EnemyVision vision)
        {
            this.profile = profile;
            this.vision = vision;
        }

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

            if (enemyBrainContext.PlayerTransform == null)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
                return;
            }

            if (!vision.CheckLineOfSight())
            {
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
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
    }
}
