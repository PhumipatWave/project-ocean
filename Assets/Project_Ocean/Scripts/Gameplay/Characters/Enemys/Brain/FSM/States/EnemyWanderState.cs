using UnityEngine;

namespace Game.Testing
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
            if (enemyBrainContext.Leader != null)
            {
                enemyBrainContext.Leader.AlertAll();
                return;
            }

            if (enemyBrainContext.Flock != null)
            {
                enemyBrainContext.Flock.AlertAll();
                return;
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(enemyBrainContext.Transform.position, radius);

            foreach (var hit in hits)
            {
                if (hit.gameObject == enemyBrainContext.Host.gameObject || !hit.TryGetComponent<EnemyNavAgentBase>(out var otherAgent)) 
                    continue;

                (otherAgent.ActiveBrain as IBrainHitReaction)?.OnBulletHit();
            }
        }
    }
}
