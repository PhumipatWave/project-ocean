using UnityEngine;

namespace Game.Testing
{
    public class ModularFollowState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Follow;

        private readonly EnemyBrainProfile profile;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly ModularEnemyVision vision;

        private Vector2 currentDirection;
        private float followTimer;
        private float hitChaseTimer;
        private float attackTimer;
        private bool hasSeenPlayer;

        public Vector3 LastKnownPlayerPosition { get; set; }

        public void SetHitChaseTimer(float duration) { hitChaseTimer = duration; }

        public ModularFollowState(EnemyBrainProfile profile, EnemyNavPathfinder pathfinder, ModularEnemyVision vision)
        {
            this.profile = profile;
            this.pathfinder = pathfinder;
            this.vision = vision;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            SetAgentSpeed(profile.speed);
            followTimer = profile.followAfterLost;
            hasSeenPlayer = false;

            if (enemyBrainContext.PlayerTransform != null)
                LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;

            enemyBrainContext.AlertUI?.ShowAttackWarning();
        }

        public override void Tick(float deltaTime)
        {
            if (hitChaseTimer > 0f) hitChaseTimer -= deltaTime;
            if (followTimer > 0f) followTimer -= deltaTime;
            if (attackTimer > 0f) attackTimer -= deltaTime;

            if (enemyBrainContext.PlayerTransform == null)
            {
                TransitionOnLost();
                return;
            }

            bool canSee = vision.CheckLineOfSight();
            bool alerted = hitChaseTimer > 0f;

            if (canSee || alerted)
            {
                hasSeenPlayer = true;
                LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;
                followTimer = profile.followAfterLost;
                enemyBrainContext.AlertUI?.SetSeeingPlayer(canSee);
            }

            if (!canSee && !alerted && followTimer <= 0f)
            {
                TransitionOnLost();
                return;
            }

            if (profile.followBehavior == EnemyFollowBehavior.Flee)
                MoveAwayFromLastKnownPosition();
            else
                MoveTowardLastKnownPosition();

            UpdateDirectionFromAgentVelocity(ref currentDirection);
            ApplyWallAvoidance(currentDirection, profile.wallDetectDistance, profile.wallLayer);
            RotateTowardsDirection(currentDirection);

            TryTransitionToAttack(canSee || alerted);
            TryBiteDuringFollow();
        }

        public override void Exit()
        {
            enemyBrainContext.AlertUI?.HideAttackWarning();
        }

        private void MoveTowardLastKnownPosition()
        {
            if (!pathfinder.SetSafeDestination(LastKnownPlayerPosition))
                pathfinder.SetSafeDestinationNear(LastKnownPlayerPosition);
        }

        private void MoveAwayFromLastKnownPosition()
        {
            Vector3 awayDir = (enemyBrainContext.Transform.position - LastKnownPlayerPosition).normalized;
            if (awayDir == Vector3.zero)
                awayDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;

            if (Vector2.Distance(enemyBrainContext.Transform.position, enemyBrainContext.Agent.destination) >= 0.5f)
                return;

            for (float distance = 3f; distance >= 1f; distance -= 1f)
            {
                Vector3 candidate = enemyBrainContext.Transform.position + awayDir * distance;
                candidate.z = enemyBrainContext.Transform.position.z;
                if (enemyBrainContext.Agent.SetDestination(candidate))
                    return;
            }

            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
            enemyBrainContext.Agent.SetDestination(enemyBrainContext.Transform.position + randomDir * 2f);
        }

        private void TryTransitionToAttack(bool targetKnown)
        {
            if (!profile.canAttack || !targetKnown || profile.followBehavior == EnemyFollowBehavior.Flee)
                return;

            float distance = enemyBrainContext.PlayerTransform != null
                ? Vector2.Distance(enemyBrainContext.Transform.position, enemyBrainContext.PlayerTransform.position)
                : float.MaxValue;

            bool closeEnough = distance <= profile.detectDistance * 0.75f;
            if (!closeEnough || attackTimer > 0f)
                return;

            if (profile.prepareBeforeAttack && enemyFSM.HasState(EnemyStateTag.Prepare))
            {
                enemyFSM.TransitionTo(EnemyStateTag.Prepare);
                return;
            }
        }

        private void TryBiteDuringFollow()
        {
            if (!profile.canAttack || profile.prepareBeforeAttack || profile.followBehavior == EnemyFollowBehavior.Flee)
                return;

            float distance = Vector2.Distance(enemyBrainContext.Transform.position, LastKnownPlayerPosition);
            if (distance > profile.attackRange || attackTimer > 0f)
                return;

            enemyBrainContext.Player?.TakeDamage(profile.attackDamage);
            attackTimer = profile.attackCooldown;
        }

        private void TransitionOnLost()
        {
            if (hasSeenPlayer && profile.searchAfterLost)
                TransitionToSearchOrWander(LastKnownPlayerPosition);
            else
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
        }
    }
}
