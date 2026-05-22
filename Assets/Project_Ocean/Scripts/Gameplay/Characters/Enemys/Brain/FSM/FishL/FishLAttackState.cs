using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishL Attack state: charges toward the player at boost speed and deals damage on contact.
    /// Transitions -> Follow when player is lost or after hitting.
    /// Transitions -> Wander if player transform is gone entirely.
    /// </summary>
    public class FishLAttackState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Attack;

        private readonly FishLBrainSettings settings;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly FishLVisionHelper vision;

        public bool IsAttacking { get; private set; }

        public FishLAttackState(FishLBrainSettings settings, EnemyNavPathfinder pathfinder, FishLVisionHelper vision)
        {
            this.settings   = settings;
            this.pathfinder = pathfinder;
            this.vision     = vision;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            IsAttacking = true;
            if (enemyBrainContext.Agent != null) enemyBrainContext.Agent.speed = settings.boostSpeed;
        }

        public override void Tick(float deltaTime)
        {
            if (enemyBrainContext.PlayerTransform == null)
            {
                EndAttack(false);
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
                return;
            }

            if (!vision.CheckLineOfSight())
            {
                EndAttack(false);
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return;
            }

            Vector3 playerPos = enemyBrainContext.PlayerTransform.position;
            float dist = Vector2.Distance(enemyBrainContext.Transform.position, playerPos);

            // Hit player
            if (dist <= settings.attackRange)
            {
                Player target = enemyBrainContext.Player
                    ?? enemyBrainContext.PlayerTransform.GetComponent<Player>();

                if (target != null)
                    target.TakeDamage(settings.attackDamage);

                EndAttack(true);
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return;
            }

            // Too far — give up charge
            if (dist > settings.detectDistance)
            {
                EndAttack(false);
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return;
            }

            if (!pathfinder.SetSafeDestination(playerPos))
                pathfinder.SetSafeDestinationNear(playerPos);

            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);
        }

        public override void Exit()
        {
            IsAttacking = false;
            if (enemyBrainContext.Agent != null) enemyBrainContext.Agent.speed = settings.speed;
            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.HideAttackWarning();
        }

        private void EndAttack(bool applyCooldown)
        {
            IsAttacking = false;
            if (applyCooldown && enemyFSM.GetState(EnemyStateTag.Follow) is FishLFollowState follow)
                follow.SetAttackCooldown(settings.attackCooldown);
        }
    }
}