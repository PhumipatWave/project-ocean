using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishL Prepare state: stops, faces the player, then charges.
    /// Transitions -> Attack when prepareTimer and facing are both satisfied.
    /// </summary>
    public class FishLPrepareState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Prepare;

        private FishLBrainSettings fishLBrainSettings;
        private FishLVisionHelper vision;

        private float prepareTimer;

        public float AttackCooldownTimer { get; set; }

        public FishLPrepareState(FishLBrainSettings settings, FishLVisionHelper vision)
        {
            fishLBrainSettings = settings;
            this.vision = vision;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            prepareTimer = fishLBrainSettings.prepareTime;
            AttackCooldownTimer = 0f;  // reset so a fresh prepare always leads to attack
            StopAgent();
            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.ShowAttackWarning();
        }

        public override void Tick(float deltaTime)
        {
            StopAgent();
            prepareTimer -= deltaTime;
            if (AttackCooldownTimer > 0f) AttackCooldownTimer -= deltaTime;

            if (enemyBrainContext.PlayerTransform == null) { enemyFSM.TransitionTo(EnemyStateTag.Wander); return; }

            // If player left sight during prepare, fall back to Follow
            if (!vision.CheckLineOfSight())
            {
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return;
            }

            Vector2 dir = enemyBrainContext.PlayerTransform.position - enemyBrainContext.Transform.position;
            if (dir.sqrMagnitude <= 0.01f) return;

            enemyBrainContext.Facing.SetSuppressRotation(true);
            bool facingPlayer = enemyBrainContext.Facing.RotateTowards(dir.normalized);
            enemyBrainContext.Facing.SetSuppressRotation(false);

            if (prepareTimer <= 0f && facingPlayer && AttackCooldownTimer <= 0f)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Attack);
            }
        }

        public override void Exit()
        {
            if (enemyBrainContext.Agent != null && enemyBrainContext.Agent.isOnNavMesh)
            {
                enemyBrainContext.Agent.isStopped = false;
                enemyBrainContext.Agent.speed = fishLBrainSettings.boostSpeed;
            }
        }

        private void StopAgent()
        {
            if (enemyBrainContext.Agent == null || !enemyBrainContext.Agent.isOnNavMesh) return;
            enemyBrainContext.Agent.isStopped = true;
            enemyBrainContext.Agent.ResetPath();
            enemyBrainContext.Agent.velocity = UnityEngine.Vector3.zero;
        }
    }
}