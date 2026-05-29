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
            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.ShowAttackWarning();
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

            // Face the player while preparing
            Vector2 dir = enemyBrainContext.PlayerTransform.position - enemyBrainContext.Transform.position;
            if (dir.sqrMagnitude > 0.01f)
            {
                enemyBrainContext.Facing.SetSuppressRotation(true);
                enemyBrainContext.Facing.RotateTowards(dir.normalized);
                enemyBrainContext.Facing.SetSuppressRotation(false);
            }

            // Timer done -> scatter (Wander away from player)
            if (prepareTimer <= 0f)
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
        }

        public override void Exit()
        {
            if (enemyBrainContext.Agent != null && enemyBrainContext.Agent.isOnNavMesh)
            {
                enemyBrainContext.Agent.isStopped = false;
                enemyBrainContext.Agent.speed     = settings.speed;
            }

            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.HideAttackWarning();
        }

        private void StopAgent()
        {
            if (enemyBrainContext.Agent == null || !enemyBrainContext.Agent.isOnNavMesh) return;
            enemyBrainContext.Agent.isStopped = true;
            enemyBrainContext.Agent.ResetPath();
            enemyBrainContext.Agent.velocity  = Vector3.zero;
        }
    }
}