using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Shared wander skeleton for enemy states.
    /// Handles the timer, arrival loop, movement, facing, and player-detection hook.
    /// </summary>
    public abstract class WanderStateBase : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Wander;

        protected float wanderTimer;
        protected Vector3 wanderTarget;

        private EnemyNavPathfinder pathfinder;
        private float pathfinderWanderRadius;
        private float pathfinderArrivalDistance = 0.5f;

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            OnEnterWander();
            wanderTimer = GetWanderInterval();
            PickNewWanderTarget();
        }

        public override void Tick(float deltaTime)
        {
            wanderTimer -= deltaTime;

            if (wanderTimer <= 0f || HasArrived())
            {
                PickNewWanderTarget();
                wanderTimer = GetWanderInterval();
            }

            ApplyMovement();
            UpdateFacing();

            if (ShouldTransitionAway())
                OnPlayerDetected();
        }

        protected void UsePathfinderWander(EnemyNavPathfinder wanderPathfinder, float wanderRadius, float arrivalDistance = 0.5f)
        {
            pathfinder = wanderPathfinder;
            pathfinderWanderRadius = wanderRadius;
            pathfinderArrivalDistance = arrivalDistance;
        }

        protected virtual void PickNewWanderTarget()
        {
            bool foundTarget = pathfinder.TryPickWanderPoint(
                enemyBrainContext.Transform.position,
                pathfinderWanderRadius,
                out wanderTarget
            );

            if (!foundTarget)
                wanderTimer = Mathf.Min(wanderTimer, 0.25f);
        }

        protected virtual void ApplyMovement()
        {
            if (pathfinder.SetSafeDestination(wanderTarget))
                return;

            PickNewWanderTarget();
            pathfinder.SetSafeDestination(wanderTarget);
        }

        protected abstract float GetWanderInterval();

        protected virtual bool HasArrived() =>
            Vector2.Distance(enemyBrainContext.Transform.position, wanderTarget) < pathfinderArrivalDistance;

        protected abstract bool ShouldTransitionAway();

        protected abstract void OnPlayerDetected();

        protected virtual void OnEnterWander() { }

        protected virtual void UpdateFacing()
        {
            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);
        }
    }
}
