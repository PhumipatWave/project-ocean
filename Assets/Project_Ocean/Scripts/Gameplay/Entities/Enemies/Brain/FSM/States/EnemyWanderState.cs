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

        private float wanderTimer;
        private Vector3 wanderTarget;

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
        public EnemyWanderState(EnemyBaseProfile profile, EnemyNavPathfinder pathfinder, EnemyVision vision)
        {
            this.profile = profile;
            this.pathfinder = pathfinder;
            this.vision = vision;
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
            PickNewWanderTarget();
        }

        public override void Tick(float deltaTime)
        {
            // Check for player FIRST — before committing to any wander movement.
            // This eliminates the delay where the fish walks into the player before fleeing.
            if (ShouldTransitionAway())
            {
                OnPlayerDetected();
                return;
            }

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

            if (wanderTimer <= 0f || HasArrived())
            {
                PickNewWanderTarget();
                wanderTimer = profile.wanderInterval;
            }

            ApplyMovement();
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

        // Transforms sit off the NavMesh plane (fish z ≈ -1 vs mesh z ≈ 0): project before pathing.
        private bool TrySetFollowDestination(Vector3 point)
        {
            return NavMesh.SamplePosition(point, out NavMeshHit hit, FollowNavSampleDistance, NavMesh.AllAreas)
                && pathfinder.SetSafeDestination(hit.position);
        }

        private void PickNewWanderTarget()
        {
            bool foundTarget = pathfinder.TryPickWanderPoint(enemyBrainContext.Transform.position, profile.wanderRadius, out wanderTarget);

            if (!foundTarget)
                wanderTimer = Mathf.Min(wanderTimer, 0.25f);
        }

        private void ApplyMovement()
        {
            if (pathfinder.SetSafeDestination(wanderTarget))
                return;

            PickNewWanderTarget();
            pathfinder.SetSafeDestination(wanderTarget);
        }

        private bool HasArrived() => 
            Vector2.Distance(enemyBrainContext.Transform.position, wanderTarget) < 0.5f;

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

        private void OnPlayerDetected()
        {
            // Scared fish => immediately flee
            if (profile is ScaredEnemyProfile scaredProfile && enemyFSM.HasState(EnemyStateTag.Flee))
            {
                if (scaredProfile.alertNearbyAllies) AlertNearbyAllies(scaredProfile.groupAlertRadius);
                enemyFSM.TransitionTo(EnemyStateTag.Flee);
                return;
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
                return;
            }

            // Aggressive fish => Follow state
            if (profile is AggressiveEnemyProfile aggressiveProfile)
            {
                if (enemyFSM.GetState(EnemyStateTag.Follow) is EnemyFollowState follow)
                    follow.LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;

                if (aggressiveProfile.alertNearbyAllies) AlertNearbyAllies(aggressiveProfile.groupAlertRadius);
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
            }
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
