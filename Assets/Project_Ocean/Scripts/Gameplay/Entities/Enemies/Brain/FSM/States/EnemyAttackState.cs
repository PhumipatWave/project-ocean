using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class EnemyAttackState : EnemyStateBase
    {
        #region FieldsProperties
        public override EnemyStateTag StateTag => EnemyStateTag.Attack;

        private readonly AggressiveEnemyProfile profile;
        private readonly EnemyNavPathfinder pathfinder;

        // Minimum attack duration; covers the attack clip's HitBox window (animation enables it ~0.29-0.57s in).
        private const float MinCommitTime = 0.65f;

        private const float DashEndReachedDistance = 0.5f;

        private float attackTimer;
        private float dashTimer;
        private float savedAcceleration;
        private bool savedAutoBraking;

        public bool IsAttacking { get; private set; }
        #endregion

        #region Constructors
        public EnemyAttackState(AggressiveEnemyProfile profile, EnemyNavPathfinder pathfinder)
        {
            this.profile = profile;
            this.pathfinder = pathfinder;
        }
        #endregion

        #region StateCallbacks
        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            SetAgentSpeed(profile.boostSpeed);

            // Agent's normal acceleration is too low to ever reach boostSpeed within
            // the dash, and autoBraking slows it near the player. Override for the dash only.
            var agent = enemyBrainContext.Agent;
            savedAcceleration = agent.acceleration;
            savedAutoBraking = agent.autoBraking;
            agent.acceleration = profile.dashAcceleration;
            agent.autoBraking = false;

            attackTimer = profile.attackCooldown;
            IsAttacking = true;
            enemyBrainContext.AlertUI?.ShowAttackWarning();

            if (profile.dashThrough)
                StartDashThrough();
        }

        public override void Tick(float deltaTime)
        {
            if (profile.dashThrough)
            {
                TickDashThrough(deltaTime);
                return;
            }

            attackTimer -= deltaTime;

            // Player left the scene entirely — abort.
            if (enemyBrainContext.PlayerTransform == null)
            {
                ReturnToFollow(false);
                return;
            }

            // Keep chasing through brief line-of-sight breaks, so a committed dash isn't dropped behind a rock.
            Vector3 targetPosition = enemyBrainContext.PlayerTransform.position;
            float distance = Vector2.Distance(enemyBrainContext.Transform.position, targetPosition);

            // Stop at bite range: the mouth HitBox sits attackRange ahead of the body, so closer overshoots the player.
            if (distance <= profile.attackRange)
            {
                HoldPosition();
                RotateFacingToPlayer();

                if (profile.attackCooldown - attackTimer >= MinCommitTime)
                    ReturnToFollow(true);
                return;
            }

            if (!pathfinder.SetSafeDestination(targetPosition))
                pathfinder.SetSafeDestinationNear(targetPosition);

            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);

            // Only give up if the attack has run too long without connecting.
            if (attackTimer <= 0f)
                ReturnToFollow(false);
        }

        public override void Exit()
        {
            IsAttacking = false;
            SetAgentSpeed(profile.speed);

            // Restore normal handling and cap leftover dash velocity, otherwise the low
            // normal acceleration lets the fish slide at boostSpeed for seconds.
            var agent = enemyBrainContext.Agent;
            agent.acceleration = savedAcceleration;
            agent.autoBraking = savedAutoBraking;
            if (agent.isOnNavMesh)
                agent.velocity = Vector3.ClampMagnitude(agent.velocity, profile.speed);
            enemyBrainContext.AlertUI?.HideAttackWarning();
        }
        #endregion

        private void ReturnToFollow(bool applyCooldown)
        {
            if (applyCooldown && enemyFSM.GetState(EnemyStateTag.Follow) is EnemyFollowState follow)
            {
                // Stay alerted past the cooldown: the fish faces away while backing off,
                // so vision alone would lose the player right when the next attack is due.
                follow.SetHitChaseTimer(Mathf.Max(profile.hitChaseDuration, profile.attackCooldown));
                follow.SetAttackCooldown(profile.attackCooldown);
            }

            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }

        // Aim once at the player's current position and commit to a straight line
        // Boost Speed x Dash Duration long, so the fish passes through and the player can dodge sideways.
        private void StartDashThrough()
        {
            dashTimer = profile.dashDuration;

            var agent = enemyBrainContext.Agent;
            if (!agent.isOnNavMesh)
                return;

            Vector3 start = enemyBrainContext.Transform.position;
            Vector2 direction = enemyBrainContext.PlayerTransform != null
                ? (Vector2)(enemyBrainContext.PlayerTransform.position - start)
                : enemyBrainContext.Facing.GetFacingDirection();

            if (direction.sqrMagnitude < 0.0001f)
                direction = enemyBrainContext.Facing.GetFacingDirection();

            Vector3 end = start + (Vector3)(direction.normalized * profile.boostSpeed * profile.dashDuration);
            end.z = start.z;

            // Stop the dash at the first wall on the line.
            if (UnityEngine.AI.NavMesh.Raycast(start, end, out UnityEngine.AI.NavMeshHit hit, UnityEngine.AI.NavMesh.AllAreas))
                end = hit.position;

            agent.SetDestination(end);
        }

        private void TickDashThrough(float deltaTime)
        {
            dashTimer -= deltaTime;

            var agent = enemyBrainContext.Agent;
            if (agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(agent.velocity);

            bool reachedEnd = !agent.pathPending && agent.remainingDistance <= DashEndReachedDistance;
            if (dashTimer <= 0f || reachedEnd)
                ReturnToFollow(true);
        }

        // Not StopAgent(): that sets isStopped, and Follow never clears it.
        private void HoldPosition()
        {
            var agent = enemyBrainContext.Agent;
            if (!agent.isOnNavMesh)
                return;

            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }
}
