using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishL Attack state: charges toward the player at boost speed and deals damage on contact.
    /// Transitions -> Follow when player is lost or after hitting.
    /// </summary>
    public class FishLAttackState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Attack;

        private FishLBrainSettings fishLBrainSettings;
        private EnemyNavPathfinder pathfinder;
        private FishLVisionHelper vision;

        public bool IsAttacking { get; private set; }

        public FishLAttackState(FishLBrainSettings settings, EnemyNavPathfinder pathfinder, FishLVisionHelper vision)
        {
            fishLBrainSettings = settings;
            this.pathfinder = pathfinder;
            this.vision = vision;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            IsAttacking = true;
            if (enemyBrainContext.Agent != null) enemyBrainContext.Agent.speed = fishLBrainSettings.boostSpeed;
        }

        public override void Tick(float deltaTime)
        {
            if (enemyBrainContext.PlayerTransform == null) { EndAttack(false); enemyFSM.TransitionTo(EnemyStateTag.Wander); return; }

            bool canSee = vision.CheckLineOfSight();
            if (!canSee)
            {
                EndAttack(false);
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return;
            }

            Vector3 playerPos = enemyBrainContext.PlayerTransform.position;
            float dist = Vector2.Distance(enemyBrainContext.Transform.position, playerPos);

            // Hit player
            if (dist <= fishLBrainSettings.attackRange)
            {
                // Try Ctx.Player first; fall back to component on the player transform
                Player target = enemyBrainContext.Player;
                if (target == null)
                    target = enemyBrainContext.PlayerTransform.GetComponent<Player>();

                if (target != null)
                    target.TakeDamage(fishLBrainSettings.attackDamage);

                EndAttack(true);
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return;
            }

            // Too far — give up charge
            if (dist > fishLBrainSettings.detectDistance)
            {
                EndAttack(false);
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return;
            }

            // Chase
            if (!pathfinder.SetSafeDestination(playerPos))
                pathfinder.SetSafeDestinationNear(playerPos);

            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);
        }

        public override void Exit()
        {
            IsAttacking = false;
            if (enemyBrainContext.Agent != null) enemyBrainContext.Agent.speed = fishLBrainSettings.speed;
            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.HideAttackWarning();
        }

        private void EndAttack(bool applyCooldown)
        {
            IsAttacking = false;
            // Pass cooldown back to FollowState so it won't immediately Prepare again
            if (applyCooldown && enemyFSM.GetState(EnemyStateTag.Follow) is FishLFollowState follow)
                follow.SetAttackCooldown(fishLBrainSettings.attackCooldown);
        }
    }
}