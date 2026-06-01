using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Dedicated flee state. The fish runs directly away from the player's
    /// CURRENT position every tick, using raw distance detection (NOT a
    /// facing cone) so the player can never become "invisible" simply by
    /// standing behind the fish while it is already running away.
    ///
    /// Transitions OUT to Wander once the player has been outside viewDistance
    /// for longer than followAfterLost seconds.
    /// </summary>
    public class EnemyFleeState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Flee;

        // ── Dependencies ─────────────────────────────────────────────────
        private readonly EnemyBrainProfile  profile;
        private readonly EnemyNavPathfinder pathfinder;

        // ── Runtime ──────────────────────────────────────────────────────
        private Vector2 currentDirection;
        private float   safeTimer; // counts down while player is out of range

        // ── Constructor ──────────────────────────────────────────────────
        public EnemyFleeState(EnemyBrainProfile profile, EnemyNavPathfinder pathfinder)
        {
            this.profile    = profile;
            this.pathfinder = pathfinder;
        }

        // ── Enter ─────────────────────────────────────────────────────────
        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            ResumeAgent(profile.speed);
            safeTimer = profile.followAfterLost;
            enemyBrainContext.AlertUI?.ShowFleeWarning();
            PickFleeDestination(); // immediate first flee point
        }

        // ── Tick ──────────────────────────────────────────────────────────
        public override void Tick(float deltaTime)
        {
            if (enemyBrainContext.PlayerTransform == null)
            {
                ReturnToWander();
                return;
            }

            // Distance-only detection — no facing cone.
            // This means the player is ALWAYS visible to a fleeing fish,
            // regardless of which direction the fish is facing.
            float dist = Vector2.Distance(
                enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position
            );

            bool playerNearby = dist <= profile.viewDistance;

            if (playerNearby)
            {
                safeTimer = profile.followAfterLost; // reset countdown

                // Re-pick a flee point whenever we are close to the current destination
                float distToDest = Vector2.Distance(
                    enemyBrainContext.Transform.position,
                    enemyBrainContext.Agent.destination
                );

                if (distToDest < 1.5f)
                    PickFleeDestination();
            }
            else
            {
                safeTimer -= deltaTime;
                if (safeTimer <= 0f)
                {
                    ReturnToWander();
                    return;
                }
            }

            UpdateDirectionFromAgentVelocity(ref currentDirection);
            ApplyWallAvoidance(currentDirection, profile.wallDetectDistance, profile.wallLayer);
            RotateTowardsDirection(currentDirection);
        }

        // ── Exit ──────────────────────────────────────────────────────────
        public override void Exit()
        {
            enemyBrainContext.AlertUI?.HideAll();
        }

        // ── Private helpers ───────────────────────────────────────────────

        /// <summary>
        /// Sets a NavMesh destination in the direction directly opposite the
        /// player. Tries progressively shorter distances as fallback.
        /// </summary>
        private void PickFleeDestination()
        {
            if (enemyBrainContext.PlayerTransform == null) return;

            Vector3 playerPos = enemyBrainContext.PlayerTransform.position;
            Vector3 selfPos   = enemyBrainContext.Transform.position;

            Vector3 awayDir = (selfPos - playerPos).normalized;
            if (awayDir == Vector3.zero)
                awayDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;

            // Try distances from wanderRadius down to 2 units
            float maxDist = Mathf.Max(profile.wanderRadius, 4f);
            for (float d = maxDist; d >= 2f; d -= 2f)
            {
                Vector3 candidate = selfPos + awayDir * d;
                candidate.z = selfPos.z;
                if (enemyBrainContext.Agent.SetDestination(candidate))
                    return;
            }

            // Fallback: let pathfinder find any valid nearby point
            if (pathfinder.TryPickWanderPoint(selfPos, maxDist * 0.5f, out Vector3 fallback))
                enemyBrainContext.Agent.SetDestination(fallback);
        }

        private void ReturnToWander()
        {
            enemyFSM.TransitionTo(EnemyStateTag.Wander);
        }
    }
}
