using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishL Prepare state: stops, faces the player, then transitions to Attack.
    /// Transitions -> Attack when prepareTimer and facing are both satisfied.
    /// Transitions -> Follow if player leaves sight during preparation.
    /// </summary>
    public class FishLPrepareState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Prepare;

        private readonly FishLBrainSettings settings;
        private readonly FishLVisionHelper vision;

        private float prepareTimer;

        public float AttackCooldownTimer { get; set; }

        public FishLPrepareState(FishLBrainSettings settings, FishLVisionHelper vision)
        {
            this.settings = settings;
            this.vision   = vision;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            prepareTimer        = settings.prepareTime;
            AttackCooldownTimer = 0f; // fresh prepare always leads to attack
            StopAgent();
            enemyBrainContext.AlertUI?.ShowAttackWarning();
        }

        public override void Tick(float deltaTime)
        {
            StopAgent();
            prepareTimer -= deltaTime;
            if (AttackCooldownTimer > 0f) AttackCooldownTimer -= deltaTime;

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

            if (prepareTimer <= 0f && facingPlayer && AttackCooldownTimer <= 0f)
                enemyFSM.TransitionTo(EnemyStateTag.Attack);
        }

        public override void Exit()
        {
            ResumeAgent(settings.boostSpeed);
        }
    }
}
