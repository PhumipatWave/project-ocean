using System;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Searches the player's last known position.</summary>
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
        private readonly ChaseInterest chaseInterest;
        private readonly float repathInterval;

        private float searchTimer;
        private Vector3 wanderTarget;
        private bool hasTarget;
        private bool chaseAllowed;
        private RepathTimer repathTimer;

        private Vector3 searchOrigin;
        public Vector3 SearchOrigin { get { return searchOrigin; } set { searchOrigin = value; } }
        #endregion

        #region Constructors
        public EnemySearchState(float speed, float searchDuration, float searchRadius, EnemyNavPathfinder pathfinder, Func<bool> canSeePlayer, Action<Vector3> onPlayerSpotted, ChaseInterest chaseInterest = null, float repathInterval = 0.25f)
        {
            this.speed = speed;
            this.searchDuration = searchDuration;
            this.searchRadius = searchRadius;
            this.pathfinder = pathfinder;
            this.canSeePlayer = canSeePlayer;
            this.onPlayerSpotted = onPlayerSpotted;
            this.chaseInterest = chaseInterest;
            this.repathInterval = repathInterval;
        }
        #endregion

        #region StateCallbacks
        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            enemyBrainContext.Agent.speed = speed;
            searchTimer = searchDuration;
            hasTarget = false;
            repathTimer = new RepathTimer(repathInterval, repathInterval);
            chaseAllowed = EvaluateChaseAllowed();
            PickSearchPoint();
            enemyBrainContext.AlertUI?.HideAttackWarning();
        }

        public override void Tick(float deltaTime)
        {
            searchTimer -= deltaTime;

            bool repath = repathTimer.Tick(deltaTime);
            if (repath)
                chaseAllowed = EvaluateChaseAllowed();

            // Spotting the player resumes the chase, unless we just gave up on a player we cannot reach.
            if (canSeePlayer() && enemyBrainContext.PlayerTransform != null && chaseAllowed)
            {
                onPlayerSpotted?.Invoke(enemyBrainContext.PlayerTransform.position);
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return;
            }

            if (searchTimer <= 0f)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
                return;
            }

            if (!hasTarget)
            {
                if (repath)
                    PickSearchPoint();
            }
            else if (Vector2.Distance(enemyBrainContext.Transform.position, wanderTarget) < 0.5f)
            {
                PickSearchPoint();
            }
            else if (repath)
            {
                pathfinder.SetSafeDestination(wanderTarget);
            }

            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);
        }
        #endregion

        // Keeps the previous target when no new point is found (never falls back to the origin).
        private void PickSearchPoint()
        {
            if (!pathfinder.TryPickWanderPoint(searchOrigin, searchRadius, out Vector3 point))
                return;

            wanderTarget = point;
            hasTarget = true;
            pathfinder.SetSafeDestination(wanderTarget);
        }

        private bool EvaluateChaseAllowed()
        {
            if (chaseInterest == null || !chaseInterest.IsOnCooldown)
                return true;

            Transform player = enemyBrainContext.PlayerTransform;
            if (player == null)
                return true;

            return pathfinder.Resolve(player.position).Status == ReachStatus.Reachable;
        }
    }
}
