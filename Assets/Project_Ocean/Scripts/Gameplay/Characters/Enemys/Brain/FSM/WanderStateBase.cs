using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Shared wander skeleton for FishM and FishL.
    /// Handles the timer + arrival loop and player-detection transition.
    /// Subclasses implement how targets are picked and how movement is applied.
    /// FishS is intentionally excluded — its Flock state is a steering system, not a wander.
    /// </summary>
    public abstract class WanderStateBase : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Wander;

        protected float wanderTimer;

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

        // ── Required overrides ───────────────────────────────────────────────────

        /// <summary>Pick and store the next wander destination.</summary>
        protected abstract void PickNewWanderTarget();

        /// <summary>Push the destination to the NavMeshAgent this frame.</summary>
        protected abstract void ApplyMovement();

        /// <summary>Returns the wander interval from this fish's settings.</summary>
        protected abstract float GetWanderInterval();

        /// <summary>Returns true when the fish is close enough to its current target.</summary>
        protected abstract bool HasArrived();

        /// <summary>Returns true when the player is detected and a transition should fire.</summary>
        protected abstract bool ShouldTransitionAway();

        /// <summary>Fire the FSM transition (and any bookkeeping) when the player is spotted.</summary>
        protected abstract void OnPlayerDetected();

        // ── Optional hooks ───────────────────────────────────────────────────────

        /// <summary>Called at the start of Enter before the timer and first target are set.
        /// Override to set agent speed, hide UI, etc.</summary>
        protected virtual void OnEnterWander() { }

        /// <summary>Rotate the facing controller from agent velocity. Override to change behaviour.</summary>
        protected virtual void UpdateFacing()
        {
            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);
        }
    }
}