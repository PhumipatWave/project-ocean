using UnityEngine;

namespace Game.Testing
{
    public class EnemyFollowState : EnemyStateBase
    {
        #region FieldsProperties
        public override EnemyStateTag StateTag => EnemyStateTag.Follow;

        private readonly EnemyBaseProfile profile;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly EnemyVision vision;

        private Vector2 currentDirection;
        private float followTimer;
        private float hitChaseTimer;
        private float attackTimer;
        private bool hasSeenPlayer;

        public Vector3 LastKnownPlayerPosition { get; set; }
        #endregion

        #region Constructors
        public EnemyFollowState(EnemyBaseProfile profile, EnemyNavPathfinder pathfinder, EnemyVision vision)
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
            SetAgentSpeed(profile.speed);
            followTimer = profile.followAfterLost;
            hasSeenPlayer = false;

            if (enemyBrainContext.PlayerTransform != null)
                LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;

            enemyBrainContext.AlertUI?.ShowAttackWarning();
        }

        public override void Tick(float deltaTime)
        {
            if (hitChaseTimer > 0f) 
                hitChaseTimer -= deltaTime;

            if (followTimer > 0f) 
                followTimer -= deltaTime;

            if (attackTimer > 0f) 
                attackTimer -= deltaTime;

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

            if (profile is FriendlyEnemyProfile)
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
        #endregion

        public void SetHitChaseTimer(float duration)
        {
            hitChaseTimer = duration;
        }

        private void MoveTowardLastKnownPosition()
        {
            if (!pathfinder.SetSafeDestination(LastKnownPlayerPosition))
                pathfinder.SetSafeDestinationNear(LastKnownPlayerPosition);
        }

        private void MoveAwayFromLastKnownPosition()
        {
            // Always use the CURRENT player position for flee direction if we can still see them,
            // so the fish continuously re-evaluates away from where the player actually is.
            Vector3 fromPosition = (enemyBrainContext.PlayerTransform != null)
                ? enemyBrainContext.PlayerTransform.position : LastKnownPlayerPosition;

            Vector3 awayDir = (enemyBrainContext.Transform.position - fromPosition).normalized;
            if (awayDir == Vector3.zero)
                awayDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;

            // Only pick a new flee destination when close to (or past) the current one.
            // Use a generous threshold so we don't spam SetDestination every frame.
            float distToCurrentDest = Vector2.Distance(
                enemyBrainContext.Transform.position,
                enemyBrainContext.Agent.destination
            );

            if (distToCurrentDest > 1.5f)
                return;

            // Try increasing distances away from the player
            for (float distance = 5f; distance >= 1f; distance -= 1f)
            {
                Vector3 candidate = enemyBrainContext.Transform.position + awayDir * distance;
                candidate.z = enemyBrainContext.Transform.position.z;

                if (enemyBrainContext.Agent.SetDestination(candidate))
                    return;
            }
        }

        private void TryTransitionToAttack(bool targetKnown)
        {
            if (!(profile is AggressiveEnemyProfile enemy) || (!enemy.canAttack || !targetKnown)) 
                return;

            float distance = enemyBrainContext.PlayerTransform != null
                ? Vector2.Distance(enemyBrainContext.Transform.position, enemyBrainContext.PlayerTransform.position) : float.MaxValue;

            bool closeEnough = distance <= enemy.detectDistance * 0.75f;
            if (!closeEnough || attackTimer > 0f) 
                return;

            if (enemy.prepareBeforeAttack && enemyFSM.HasState(EnemyStateTag.Prepare))
            {
                enemyFSM.TransitionTo(EnemyStateTag.Prepare);
                return;
            }
        }

        private void TryBiteDuringFollow()
        {
            if (!(profile is AggressiveEnemyProfile enemy)) return;
            if (!enemy.canAttack || enemy.prepareBeforeAttack) return;

            float distance = Vector2.Distance(enemyBrainContext.Transform.position, LastKnownPlayerPosition);
            if (distance > enemy.attackRange || attackTimer > 0f) return;

            enemyBrainContext.Player?.TakeDamage(enemy.attackDamage);
            attackTimer = enemy.attackCooldown;
        }

        private void TransitionOnLost()
        {
            var enemy = profile as AggressiveEnemyProfile;
            if (hasSeenPlayer && enemy != null && enemy.searchAfterLost)
                TransitionToSearchOrWander(LastKnownPlayerPosition);
            else
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
        }
    }
}
