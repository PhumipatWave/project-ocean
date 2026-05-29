using UnityEngine;

namespace Game.Testing
{
    public class ModularAttackState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Attack;

        private readonly EnemyBrainProfile profile;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly ModularEnemyVision vision;

        private bool damageApplied;
        private float attackTimer;

        public bool IsAttacking { get; private set; }

        public ModularAttackState(EnemyBrainProfile profile, EnemyNavPathfinder pathfinder, ModularEnemyVision vision)
        {
            this.profile = profile;
            this.pathfinder = pathfinder;
            this.vision = vision;
        }

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

            if (enemyBrainContext.PlayerTransform == null)
            {
                ReturnToFollow(false);
                return;
            }

            Vector3 playerPosition = enemyBrainContext.PlayerTransform.position;
            float distance = Vector2.Distance(enemyBrainContext.Transform.position, playerPosition);

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

            if (!pathfinder.SetSafeDestination(playerPosition))
                pathfinder.SetSafeDestinationNear(playerPosition);

            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);

            if (!vision.CheckLineOfSight() || attackTimer <= 0f)
                ReturnToFollow(false);
        }

        public override void Exit()
        {
            IsAttacking = false;
            SetAgentSpeed(profile.speed);
            enemyBrainContext.AlertUI?.HideAttackWarning();
        }

        private void ReturnToFollow(bool applyCooldown)
        {
            if (applyCooldown && enemyFSM.GetState(EnemyStateTag.Follow) is ModularFollowState follow)
                follow.SetHitChaseTimer(profile.attackCooldown);

            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }
    }
}
