using UnityEngine;
using UnityEngine.AI;
using Game.Core;

namespace Game.Gameplay
{
    public class EnemyWanderState : EnemyStateBase
    {
        #region FieldsProperties
        public override EnemyStateTag StateTag => EnemyStateTag.Wander;

        private readonly EnemyBaseProfile profile;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly EnemyVision vision;
        private readonly ChaseInterest chaseInterest;

        private float wanderTimer;
        private Vector3 wanderTarget;
        private bool hasWanderTarget;

        private RepathTimer repathTimer;
        private StuckDetector stuckDetector;
        private bool chaseAllowed = true;

        // School followers: re-aim at the leader a few times a second, not every frame (each re-aim runs a path check).
        private const float FollowRepathInterval = 0.25f;
        private const float FollowArriveDistance = 0.4f;
        private const float FollowCatchUpDistance = 3f;
        private const float FollowCatchUpSpeedMultiplier = 1.3f;
        private const float FollowNavSampleDistance = 2f;

        private float followRepathTimer;
        private bool wasFollowing;
        #endregion

        #region Constructors
        public EnemyWanderState(EnemyBaseProfile profile, EnemyNavPathfinder pathfinder, EnemyVision vision, ChaseInterest chaseInterest = null)
        {
            this.profile = profile;
            this.pathfinder = pathfinder;
            this.vision = vision;
            this.chaseInterest = chaseInterest;
        }
        #endregion

        #region StateCallbacks
        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            ResumeAgent(profile.speed);
            enemyBrainContext.AlertUI?.HideAll();
            wanderTimer = profile.wanderInterval;
            followRepathTimer = 0f;
            wasFollowing = false;
            hasWanderTarget = false;

            repathTimer = new RepathTimer(profile.repathInterval, Random.value * profile.repathInterval);
            stuckDetector = new StuckDetector(profile.stuckWindow, profile.stuckMinProgress);
            stuckDetector.Reset(enemyBrainContext.Transform.position);
            chaseAllowed = EvaluateChaseAllowed();

            PickNewWanderTarget();
        }

        public override void Tick(float deltaTime)
        {
            bool repath = repathTimer.Tick(deltaTime);
            if (repath)
                chaseAllowed = EvaluateChaseAllowed();

            // Check for player FIRST — before committing to any wander movement.
            // This eliminates the delay where the fish walks into the player before fleeing.
            if (ShouldTransitionAway() && OnPlayerDetected())
                return;

            var school = enemyBrainContext.School;
            if (school != null && school.HasLeader)
            {
                FollowLeader(school, deltaTime);
                UpdateFacing();
                return;
            }

            // Promoted to leader (or never had one): back to normal wander speed.
            if (wasFollowing)
            {
                wasFollowing = false;
                SetAgentSpeed(profile.speed);
            }

            wanderTimer -= deltaTime;
            bool stuck = stuckDetector.Tick(deltaTime, enemyBrainContext.Transform.position);

            if (wanderTimer <= 0f || HasArrived() || stuck)
            {
                wanderTimer = profile.wanderInterval;
                PickNewWanderTarget();
            }
            else if (repath)
            {
                ApplyMovement();
            }

            UpdateFacing();
        }
        #endregion

        // Swim to this follower's spot around the leader; speed up when left behind.
        private void FollowLeader(Boid school, float deltaTime)
        {
            wasFollowing = true;

            followRepathTimer -= deltaTime;
            if (followRepathTimer > 0f)
                return;

            followRepathTimer = FollowRepathInterval;

            Vector3 target = school.GetFollowTarget();
            float distance = Vector2.Distance(enemyBrainContext.Transform.position, target);

            SetAgentSpeed(distance > FollowCatchUpDistance ? profile.speed * FollowCatchUpSpeedMultiplier : profile.speed);

            if (distance <= FollowArriveDistance)
                return;

            // Spot blocked (wall, tight cave): fall back to the leader's own position.
            if (!TrySetFollowDestination(target))
                TrySetFollowDestination(school.Leader.transform.position);
        }

        // Transforms sit off the NavMesh plane (fish z ≈ -1 vs mesh z ≈ 0): Resolve projects before pathing.
        private bool TrySetFollowDestination(Vector3 point)
        {
            ReachResult result = pathfinder.Resolve(point);

            // A spot just off the mesh (rock edge) still counts: swim to the nearest mesh point.
            if (result.Status == ReachStatus.Unreachable && result.Gap > FollowNavSampleDistance)
                return false;

            return pathfinder.MoveTo(result, true);
        }

        // Keeps the previous target when no new point is found.
        private void PickNewWanderTarget()
        {
            if (pathfinder.TryPickWanderPoint(enemyBrainContext.Transform.position, profile.wanderRadius, out Vector3 point))
            {
                wanderTarget = point;
                hasWanderTarget = true;
                pathfinder.SetSafeDestination(wanderTarget);
                return;
            }

            // Nothing valid this time: try again soon.
            wanderTimer = Mathf.Min(wanderTimer, 0.25f);
        }

        // Re-issues the destination on the repath timer in case the path was lost.
        private void ApplyMovement()
        {
            if (!hasWanderTarget)
                return;

            if (!pathfinder.SetSafeDestination(wanderTarget))
                PickNewWanderTarget();
        }

        private bool HasArrived() =>
            hasWanderTarget && Vector2.Distance(enemyBrainContext.Transform.position, wanderTarget) < 0.5f;

        private void UpdateFacing()
        {
            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);
        }

        private bool ShouldTransitionAway()
        {
            return enemyBrainContext.PlayerTransform != null
                && vision != null
                && vision.CheckLineOfSight();
        }

        // Aggressive fish ignore a player they cannot reach for a while after giving up.
        private bool EvaluateChaseAllowed()
        {
            if (chaseInterest == null || !chaseInterest.IsOnCooldown)
                return true;

            Transform player = enemyBrainContext.PlayerTransform;
            if (player == null)
                return true;

            return pathfinder.Resolve(player.position).Status == ReachStatus.Reachable;
        }

        // Returns false when the fish keeps wandering (aggressive fish blocked by the give-up cooldown).
        private bool OnPlayerDetected()
        {
            // Scared fish => immediately flee
            if (profile is ScaredEnemyProfile scaredProfile && enemyFSM.HasState(EnemyStateTag.Flee))
            {
                if (scaredProfile.alertNearbyAllies) AlertNearbyAllies(scaredProfile.groupAlertRadius);
                enemyFSM.TransitionTo(EnemyStateTag.Flee);
                return true;
            }

            // Friendly fish => bias next wander point away from player, stay in Wander
            if (profile is FriendlyEnemyProfile)
            {
                if (enemyBrainContext.PlayerTransform != null)
                {
                    Vector3 selfPos   = enemyBrainContext.Transform.position;
                    Vector3 playerPos = enemyBrainContext.PlayerTransform.position;
                    Vector3 awayDir   = (selfPos - playerPos).normalized;
                    if (awayDir == Vector3.zero)
                        awayDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;

                    Vector3 candidate = selfPos + awayDir * profile.wanderRadius;
                    candidate.z = selfPos.z;

                    if (!pathfinder.SetSafeDestination(candidate))
                        pathfinder.TryPickWanderPoint(selfPos, profile.wanderRadius, out candidate);
                }
                return true;
            }

            // Aggressive fish => Follow state
            if (profile is AggressiveEnemyProfile aggressiveProfile)
            {
                if (!chaseAllowed)
                    return false;

                if (enemyFSM.GetState(EnemyStateTag.Follow) is EnemyFollowState follow)
                    follow.LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;

                if (aggressiveProfile.alertNearbyAllies) AlertNearbyAllies(aggressiveProfile.groupAlertRadius);
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
            }

            return true;
        }

        private void AlertNearbyAllies(float radius)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(enemyBrainContext.Transform.position, radius);

            foreach (var hit in hits)
            {
                if (hit.gameObject == enemyBrainContext.Host.gameObject || !hit.TryGetComponent<EnemyNavAgentBase>(out var otherAgent)) 
                    continue;

                (otherAgent.ActiveBrain as IBrainHitReaction)?.OnHit();
            }
        }
    }
}
