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
        private readonly EnemyBaseProfile profile;
        private readonly EnemyNavPathfinder pathfinder;

        // ── Runtime ──────────────────────────────────────────────────────
        private Vector2 currentDirection;
        private float   safeTimer;       // counts down while player is out of range
        private float   minFleeTimer;    // must flee for at least this long before returning to Wander
        private const float MinFleeDuration = 1.5f; // counts down while player is out of range

        // ── Constructor ──────────────────────────────────────────────────
        public EnemyFleeState(EnemyBaseProfile profile, EnemyNavPathfinder pathfinder)
        {
            this.profile    = profile;
            this.pathfinder = pathfinder;
        }

        // ── Enter ─────────────────────────────────────────────────────────
public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            ResumeAgent(profile.speed * 1.5f);  // flee boost
            safeTimer    = profile.followAfterLost;
            minFleeTimer = MinFleeDuration;
            enemyBrainContext.AlertUI?.ShowFleeWarning();
            PickFleeDestination();
        }

        // ── Tick ──────────────────────────────────────────────────────────
public override void Tick(float deltaTime)
        {
            if (minFleeTimer > 0f) minFleeTimer -= deltaTime;

            if (enemyBrainContext.PlayerTransform == null)
            {
                if (minFleeTimer <= 0f) ReturnToWander();
                return;
            }

            float dist = Vector2.Distance(
                enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position
            );

            bool playerNearby = dist <= profile.viewDistance;

            if (playerNearby)
            {
                safeTimer = profile.followAfterLost;

                float distToDest = Vector2.Distance(
                    enemyBrainContext.Transform.position,
                    enemyBrainContext.Agent.destination
                );

                // Check if the player is cutting into the flee path:
                // project player-to-fish direction onto current flee direction.
                // If the dot is negative the player is moving into our path — re-route.
                Vector3 selfPos      = enemyBrainContext.Transform.position;
                Vector3 playerPos    = enemyBrainContext.PlayerTransform.position;
                Vector3 toDest       = ((Vector3)enemyBrainContext.Agent.destination - selfPos).normalized;
                Vector3 toPlayer     = (playerPos - selfPos).normalized;
                float   dot          = Vector3.Dot(toDest, toPlayer);

                // dot > 0.5 means player is roughly ahead on our escape route
                bool playerBlockingPath = dot > 0.5f;

                if (distToDest < 1.5f || playerBlockingPath)
                {
                    PickFleeDestination();

                    // Give a little extra flee time when the player cuts us off
                    if (playerBlockingPath)
                        safeTimer += 0.8f;
                }
            }
            else
            {
                safeTimer -= deltaTime;
                if (safeTimer <= 0f && minFleeTimer <= 0f)
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

        /// <summary>
        /// Called when the fish is hit mid-flee or triggered by a bullet.
        /// Bumps speed higher than the normal flee boost for a panic burst.
        /// </summary>
        public void EnterPanic()
        {
            SetAgentSpeed(profile.speed * 2.2f);  // panic boost, higher than normal flee (1.5x)
            minFleeTimer = Mathf.Max(minFleeTimer, MinFleeDuration);  // reset min flee time
            safeTimer    = Mathf.Max(safeTimer, profile.followAfterLost);
            PickFleeDestination();  // immediately re-route away
        }

        private void ReturnToWander()
        {
            enemyFSM.TransitionTo(EnemyStateTag.Wander);
        }
    }
}
