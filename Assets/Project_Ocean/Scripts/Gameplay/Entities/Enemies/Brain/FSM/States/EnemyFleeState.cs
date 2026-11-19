using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class EnemyFleeState : EnemyStateBase
    {
        #region FieldsProperties
        public override EnemyStateTag StateTag => EnemyStateTag.Flee;

        private readonly EnemyBaseProfile profile;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly FleeDestinationPicker fleePicker;
        private readonly RepathTimer repathTimer;
        private readonly StuckDetector stuckDetector;

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
            fleePicker = new FleeDestinationPicker(pathfinder.Resolve);
            repathTimer = new RepathTimer(profile.repathInterval);
            stuckDetector = new StuckDetector(profile.stuckWindow, profile.stuckMinProgress);
        }
        #endregion

        #region StateCallbacks
        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            ResumeAgent(profile.speed * (profile is ScaredEnemyProfile s ? s.fleeSpeedMultiplier : 1.5f));
            safeTimer = profile.followAfterLost;
            minFleeTimer = MinFleeDuration;
            repathTimer.Reset();
            stuckDetector.Reset(enemyBrainContext.Transform.position);
            enemyBrainContext.AlertUI?.ShowFleeWarning();
            PickFleeDestination();

            // One member spooked (seen the player or hit) = the whole school flees.
            enemyBrainContext.School?.AlertSchool();
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

            bool repath = repathTimer.Tick(deltaTime);
            bool stuck = stuckDetector.Tick(deltaTime, enemyBrainContext.Transform.position);

            float dist = Vector2.Distance(enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position);

            bool playerNearby = dist <= profile.viewDistance;
            if (playerNearby)
            {
                safeTimer = profile.followAfterLost;

                // Re-evaluate the escape route on the repath timer, not every frame.
                if (repath || stuck)
                {
                    float distToDest = Vector2.Distance(enemyBrainContext.Transform.position,
                        enemyBrainContext.Agent.destination);

                    // Re-route when the player moves into the flee path.
                    Vector3 selfPos = enemyBrainContext.Transform.position;
                    Vector3 playerPos = enemyBrainContext.PlayerTransform.position;
                    Vector3 toDest = ((Vector3)enemyBrainContext.Agent.destination - selfPos).normalized;
                    Vector3 toPlayer = (playerPos - selfPos).normalized;
                    float dot = Vector3.Dot(toDest, toPlayer);

                    // dot > 0.5 means player is roughly ahead on our escape route
                    bool playerBlockingPath = dot > 0.5f;

                    if (distToDest < 1.5f || playerBlockingPath || stuck)
                    {
                        PickFleeDestination();

                        // Give a little extra flee time when the player cuts us off
                        if (playerBlockingPath)
                            safeTimer += 0.8f;
                    }
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

            // Reachable spot with open water behind it (avoids dead ends), up to wanderRadius away.
            float maxDist = Mathf.Max(profile.wanderRadius, 4f);
            if (fleePicker.TryPick(selfPos, playerPos, maxDist, out Vector3 destination)
                && pathfinder.SetSafeDestination(destination))
                return;

            // Fallback: let pathfinder find any valid nearby point
            if (pathfinder.TryPickWanderPoint(selfPos, maxDist * 0.5f, out Vector3 fallback))
                pathfinder.SetSafeDestination(fallback);
        }

        public void EnterPanic()
        {
            SetAgentSpeed(profile.speed * (profile is ScaredEnemyProfile s ? s.panicSpeedMultiplier : 2.2f));
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
