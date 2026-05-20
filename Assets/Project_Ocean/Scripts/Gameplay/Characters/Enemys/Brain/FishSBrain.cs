using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Small fish AI refactored to FSM.
    /// Registers 2 states: Flock (boids wander/lead) and Panic (flee).
    /// Public API (FleeFrom, ReflectDirection, etc.) forwards to the active state.
    /// </summary>
    public class FishSBrain : EnemyBrainBase, IBrainVision
    {
        private readonly FishSBrainSettings fishSBrainSettings;
        private FishS_Agent fishSAgent;
        private FishSFlockState flockState;
        private FishSPanicState panicState;
        private readonly List<FishS_Agent> neighbors = new List<FishS_Agent>();

        public Vector2 CurrentDirection => flockState?.CurrentDirection ?? Vector2.zero;

        #region IBrainVision
        public float ViewDistance => fishSBrainSettings.viewDistance;
        public float ViewAngle => fishSBrainSettings.viewAngle;
        public bool CanSeePlayer() => enemyBrainContext?.PlayerTransform != null;
        #endregion

        public FishSBrain(FishSBrainSettings settings)
        {
            this.fishSBrainSettings = settings;
        }

        public override void Initialize(EnemyBrainContext context)
        {
            base.Initialize(context);
            fishSAgent = context.Host as FishS_Agent;

            flockState = new FishSFlockState(fishSBrainSettings, fishSAgent, neighbors);
            panicState = new FishSPanicState(fishSBrainSettings, fishSAgent);

            enemyFSM.Register(flockState);
            enemyFSM.Register(panicState);

            SetAgentSpeed(fishSBrainSettings.speed);
            enemyFSM.Start(EnemyStateTag.Flock);
        }

        // ── Public API forwarded from FishS_Agent ────────────────────────────────

        public void SetPlayerTransform(Transform player) { /* Ctx.PlayerTransform is the live reference */ }

        public void FleeFrom(Vector2 targetPos)
        {
            // Transition first so Enter() sets Ctx on the panicState before SetFleeSource uses it
            enemyFSM.TransitionTo(EnemyStateTag.Panic);
            panicState.SetFleeSource(targetPos);
        }

        public void ReflectDirection(Vector2 normal, bool randomizePanic)
        {
            // Delegate to whichever state is active (only meaningful during Panic)
            if (enemyFSM.IsIn(EnemyStateTag.Panic))
                panicState.SetFleeSource(enemyBrainContext.Transform.position - (Vector3)normal);
        }

        public void ApplyBounceDestination()
        {
            enemyBrainContext.Agent.SetDestination(
                (Vector3)enemyBrainContext.Transform.position + (Vector3)CurrentDirection * fishSBrainSettings.navDestinationDistance
            );
        }

        public bool IsDirectionBlocked()
        {
            RaycastHit2D hit = Physics2D.Raycast(
                enemyBrainContext.Transform.position, CurrentDirection, fishSBrainSettings.wallDetectDistance, fishSBrainSettings.wallLayer
            );
            return hit.collider != null;
        }

        public void NudgeDirection(float degrees)
        {
            // Not needed with FSM wall avoidance built into states — kept for compatibility
        }
    }
}