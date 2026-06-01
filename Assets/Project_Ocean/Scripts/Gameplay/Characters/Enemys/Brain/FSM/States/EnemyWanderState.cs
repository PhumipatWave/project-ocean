using UnityEngine;

namespace Game.Testing
{
    public class EnemyWanderState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Wander;

        private readonly EnemyBrainProfile profile;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly EnemyVision vision;

        private float wanderTimer;
        private Vector3 wanderTarget;

        public EnemyWanderState(EnemyBrainProfile profile, EnemyNavPathfinder pathfinder, EnemyVision vision)
        {
            this.profile = profile;
            this.pathfinder = pathfinder;
            this.vision = vision;
        }

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

        private void PickNewWanderTarget()
        {
            bool foundTarget = pathfinder.TryPickWanderPoint(
                enemyBrainContext.Transform.position,
                profile.wanderRadius,
                out wanderTarget
            );

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
            // Flee fish go directly to the dedicated EnemyFleeState
            if (profile.followBehavior == EnemyFollowBehavior.Flee
                && enemyFSM.HasState(EnemyStateTag.Flee))
            {
                if (profile.alertNearbyAllies)
                    AlertNearbyAllies();

                enemyFSM.TransitionTo(EnemyStateTag.Flee);
                return;
            }

            // All other behaviors use the Follow state as before
            if (enemyFSM.GetState(EnemyStateTag.Follow) is EnemyFollowState follow)
                follow.LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;

            if (profile.alertNearbyAllies)
                AlertNearbyAllies();

            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }

        private void AlertNearbyAllies()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                enemyBrainContext.Transform.position,
                profile.groupAlertRadius
            );

            foreach (var hit in hits)
            {
                if (hit.gameObject == enemyBrainContext.Host.gameObject) continue;
                if (!hit.TryGetComponent<EnemyNavAgentBase>(out var otherAgent)) continue;

                (otherAgent.ActiveBrain as IBrainHitReaction)?.OnBulletHit();
            }
        }
    }
}
