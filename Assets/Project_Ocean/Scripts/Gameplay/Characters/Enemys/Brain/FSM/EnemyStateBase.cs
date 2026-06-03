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

        protected void SetAgentSpeed(float speed)
        {
            if (enemyBrainContext.Agent != null)
                enemyBrainContext.Agent.speed = speed;
        }

        protected void StopAgent()
        {
            if (enemyBrainContext.Agent == null || !enemyBrainContext.Agent.isOnNavMesh)
                return;

            enemyBrainContext.Agent.isStopped = true;
            enemyBrainContext.Agent.ResetPath();
            enemyBrainContext.Agent.velocity = Vector3.zero;
        }

        protected void ResumeAgent(float speed)
        {
            if (enemyBrainContext.Agent == null || !enemyBrainContext.Agent.isOnNavMesh)
                return;

            enemyBrainContext.Agent.isStopped = false;
            enemyBrainContext.Agent.speed = speed;
        }

        protected bool RotateFacingToPlayer()
        {
            if (enemyBrainContext.PlayerTransform == null)
                return false;

            Vector2 direction = enemyBrainContext.PlayerTransform.position - enemyBrainContext.Transform.position;
            if (direction.sqrMagnitude <= 0.01f)
                return false;

            enemyBrainContext.Facing.SetSuppressRotation(true);
            bool facingTarget = enemyBrainContext.Facing.RotateTowards(direction.normalized);
            enemyBrainContext.Facing.SetSuppressRotation(false);
            return facingTarget;
        }

        protected void UpdateDirectionFromAgentVelocity(ref Vector2 currentDirection, float minSpeed = 0.1f)
        {
            if (enemyBrainContext.Agent != null && enemyBrainContext.Agent.velocity.magnitude > minSpeed)
                currentDirection = enemyBrainContext.Agent.velocity.normalized;
        }

        protected void RotateTowardsDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateTowards(direction.normalized);
        }

        protected void TransitionToSearchOrWander(Vector3 searchOrigin)
        {
            if (enemyFSM.HasState(EnemyStateTag.Search))
            {
                if (enemyFSM.GetState(EnemyStateTag.Search) is EnemySearchState search)
                    search.SearchOrigin = searchOrigin;

                enemyFSM.TransitionTo(EnemyStateTag.Search);
                return;
            }

            enemyFSM.TransitionTo(EnemyStateTag.Wander);
        }

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
