using System;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Shared search state used by enemies that investigate the last known player position.
    /// </summary>
    public class EnemySearchState : EnemyStateBase
    {
        #region FieldsProperties
        public override EnemyStateTag StateTag => EnemyStateTag.Search;

        private readonly float speed;
        private readonly float searchDuration;
        private readonly float searchRadius;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly Func<bool> canSeePlayer;
        private readonly Action<Vector3> onPlayerSpotted;

        private float searchTimer;
        private Vector3 wanderTarget;

        private Vector3 searchOrigin;
        public Vector3 SearchOrigin { get { return searchOrigin; } set { searchOrigin = value; } }
        #endregion

        #region Constructors
        public EnemySearchState(float speed, float searchDuration, float searchRadius, EnemyNavPathfinder pathfinder, Func<bool> canSeePlayer, Action<Vector3> onPlayerSpotted)
        {
            this.speed = speed;
            this.searchDuration = searchDuration;
            this.searchRadius = searchRadius;
            this.pathfinder = pathfinder;
            this.canSeePlayer = canSeePlayer;
            this.onPlayerSpotted = onPlayerSpotted;
        }
        #endregion

        #region StateCallbacks
        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            enemyBrainContext.Agent.speed = speed;
            searchTimer = searchDuration;
            PickSearchPoint();
            enemyBrainContext.AlertUI?.HideAttackWarning();
        }

        public override void Tick(float deltaTime)
        {
            searchTimer -= deltaTime;

            if (canSeePlayer())
            {
                if (enemyBrainContext.PlayerTransform != null)
                    onPlayerSpotted?.Invoke(enemyBrainContext.PlayerTransform.position);

                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return;
            }

            if (searchTimer <= 0f)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
                return;
            }

            if (Vector2.Distance(enemyBrainContext.Transform.position, wanderTarget) < 0.5f)
                PickSearchPoint();

            pathfinder.SetSafeDestination(wanderTarget);

            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);
        }
        #endregion

        private void PickSearchPoint()
        {
            pathfinder.TryPickWanderPoint(searchOrigin, searchRadius, out wanderTarget);
        }
    }
}
