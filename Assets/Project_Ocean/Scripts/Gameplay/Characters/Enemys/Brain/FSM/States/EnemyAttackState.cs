using UnityEngine;

namespace Game.Testing
{
    public class EnemyAttackState : EnemyStateBase
    {
        #region FieldsProperties
        public override EnemyStateTag StateTag => EnemyStateTag.Attack;

        private readonly AggressiveEnemyProfile profile;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly EnemyVision vision;

        private bool damageApplied;
        private float attackTimer;

        public bool IsAttacking { get; private set; }
        #endregion

        #region Constructors
        public EnemyAttackState(AggressiveEnemyProfile profile, EnemyNavPathfinder pathfinder, EnemyVision vision)
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
            SetAgentSpeed(profile.boostSpeed);
            damageApplied = false;
            attackTimer = profile.attackCooldown;
            IsAttacking = true;
            enemyBrainContext.AlertUI?.ShowAttackWarning();
        }

public override void Tick(float deltaTime)
        {
            attackTimer -= deltaTime;

            // Player left the scene entirely — abort.
            if (enemyBrainContext.PlayerTransform == null)
            {
                ReturnToFollow(false);
                return;
            }

            // Chase the last known position even through LOS breaks.
            // This keeps the dash committed — a fish that wound up won't give up
            // just because the player ducked behind a rock for a frame.
            Vector3 targetPosition = enemyBrainContext.PlayerTransform.position;

            float distance = Vector2.Distance(enemyBrainContext.Transform.position, targetPosition);

            if (distance <= profile.attackRange)
            {
                if (!damageApplied)
                {
                    enemyBrainContext.Player?.TakeDamage(profile.attackDamage);
                    damageApplied = true;
                }

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
            enemyBrainContext.AlertUI?.HideAttackWarning();
        }
        #endregion

        private void ReturnToFollow(bool applyCooldown)
        {
            if (applyCooldown && enemyFSM.GetState(EnemyStateTag.Follow) is EnemyFollowState follow)
                follow.SetHitChaseTimer(profile.attackCooldown);

            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }
    }
}
