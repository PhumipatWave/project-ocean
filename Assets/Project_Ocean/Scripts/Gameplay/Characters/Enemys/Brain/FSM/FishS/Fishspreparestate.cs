using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishS Prepare state: stops and faces the player briefly, then scatters away.
    /// No attack — transitions -> Wander (flee) when prepare timer expires.
    /// Transitions -> Follow if player leaves sight during preparation.
    /// </summary>
    public class FishSPrepareState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Prepare;

        private readonly FishSBrainSettings settings;
        private readonly FishSVisionHelper vision;

        private float prepareTimer;

        public FishSPrepareState(FishSBrainSettings settings, FishSVisionHelper vision)
        {
            this.settings = settings;
            this.vision   = vision;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            prepareTimer = settings.prepareTime;
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

            RotateFacingToPlayer();

            // Timer done -> scatter (Wander away from player)
            if (prepareTimer <= 0f)
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
        }

        public override void Exit()
        {
            ResumeAgent(settings.speed);

            enemyBrainContext.AlertUI?.HideAttackWarning();
        }
    }
}
