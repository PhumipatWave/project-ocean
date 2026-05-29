using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Convenience base for all concrete enemy states.
    /// Caches Ctx and Fsm on Enter so subclasses don't repeat the boilerplate.
    /// </summary>
    public abstract class EnemyStateBase : IEnemyState
    {
        protected EnemyBrainContext enemyBrainContext { get; private set; }
        protected EnemyFSM enemyFSM { get; private set; }

        public abstract EnemyStateTag StateTag { get; }

        public virtual void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            enemyBrainContext = ctx;
            enemyFSM = fsm;
        }

        public abstract void Tick(float deltaTime);

        public virtual void Exit() { }

        // ── Shared Utilities ─────────────────────────────────────────────────────────

        /// <summary>
        /// Casts a ray in <paramref name="direction"/> and steers the agent sideways
        /// when a wall is detected within <paramref name="detectDistance"/>.
        /// Call every Tick after the agent destination has been set.
        /// </summary>
        protected void ApplyWallAvoidance(Vector2 direction, float detectDistance, LayerMask wallLayer)
        {
            if (direction == Vector2.zero) return;

            RaycastHit2D hit = Physics2D.Raycast(
                enemyBrainContext.Transform.position,
                direction,
                detectDistance,
                wallLayer
            );

            if (hit.collider == null) return;

            Vector2 avoid = Vector2.Perpendicular(direction)
                            * (UnityEngine.Random.value > 0.5f ? 1f : -1f);

            enemyBrainContext.Agent.SetDestination(
                enemyBrainContext.Transform.position + new Vector3(avoid.x, avoid.y, 0f)
            );
        }
    }
}
