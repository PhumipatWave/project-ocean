using UnityEngine;

namespace Game.Testing
{
    public class EnemyFleeState : EnemyStateBase
    {
        #region FieldsProperties
        public override EnemyStateTag StateTag => EnemyStateTag.Flee;

        private readonly EnemyBaseProfile profile;
        private readonly EnemyNavPathfinder pathfinder;

        private Vector2 currentDirection;
        private float safeTimer;
        private float minFleeTimer;
        private const float MinFleeDuration = 1.5f;
        #endregion

        #region Constructors
        public EnemyFleeState(EnemyBaseProfile profile, EnemyNavPathfinder pathfinder)
        {
            this.profile = profile;
            this.pathfinder = pathfinder;
        }
        #endregion

        #region StateCallbacks
        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            ResumeAgent(profile.speed * 1.5f);  // flee boost
            safeTimer = profile.followAfterLost;
            minFleeTimer = MinFleeDuration;
            enemyBrainContext.AlertUI?.ShowFleeWarning();
            PickFleeDestination();
        }

        public override void Tick(float deltaTime)
        {
            if (minFleeTimer > 0f) minFleeTimer -= deltaTime;

            if (enemyBrainContext.PlayerTransform == null)
            {
                if (minFleeTimer <= 0f) 
                    ReturnToWander();

                return;
            }

            float dist = Vector2.Distance(enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position);

            bool playerNearby = dist <= profile.viewDistance;
            if (playerNearby)
            {
                safeTimer = profile.followAfterLost;

                float distToDest = Vector2.Distance(enemyBrainContext.Transform.position,
                    enemyBrainContext.Agent.destination);

                // Check if the player is cutting into the flee path:
                // project player-to-fish direction onto current flee direction.
                // If the dot is negative the player is moving into our path — re-route.
                Vector3 selfPos = enemyBrainContext.Transform.position;
                Vector3 playerPos = enemyBrainContext.PlayerTransform.position;
                Vector3 toDest = ((Vector3)enemyBrainContext.Agent.destination - selfPos).normalized;
                Vector3 toPlayer = (playerPos - selfPos).normalized;
                float dot = Vector3.Dot(toDest, toPlayer);

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

        public override void Exit()
        {
            enemyBrainContext.AlertUI?.HideAll();
        }
        #endregion

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

        public void EnterPanic()
        {
            SetAgentSpeed(profile.speed * 2.2f);  // panic boost, higher than normal flee (1.5x)
            minFleeTimer = Mathf.Max(minFleeTimer, MinFleeDuration);  // reset min flee time
            safeTimer = Mathf.Max(safeTimer, profile.followAfterLost);
            PickFleeDestination();  // immediately re-route away
        }

        private void ReturnToWander()
        {
            enemyFSM.TransitionTo(EnemyStateTag.Wander);
        }
    }
}
