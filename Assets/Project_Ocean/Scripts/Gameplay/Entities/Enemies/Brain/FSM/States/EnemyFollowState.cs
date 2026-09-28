using UnityEngine;
using UnityEngine.AI;
using Game.Core;

namespace Game.Gameplay
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

        // Post-attack reposition: hop between random spots away from the player during cooldown, then attack again.
        private const int RetreatSampleTries = 10;
        private const float RetreatSpreadAngle = 100f;
        private const float RetreatArriveDistance = 0.75f;
        private const float RetreatTimeout = 5f;

        private bool isRepositioning;
        private bool hasRetreatPoint;
        private float repositionTimer;

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
            else if ((isRepositioning || attackTimer > 0f) && profile is AggressiveEnemyProfile aggressive)
                Reposition(aggressive, deltaTime); // keeps hopping between spots until the cooldown ends
            else
                MoveTowardLastKnownPosition();

            UpdateDirectionFromAgentVelocity(ref currentDirection);
            ApplyWallAvoidance(currentDirection, profile.wallDetectDistance, profile.wallLayer);
            RotateTowardsDirection(currentDirection);

            TryTransitionToAttack(canSee || alerted);
        }

        public override void Exit()
        {
            // Drop an unfinished reposition when leaving for Search/Wander so a stale spot isn't reused.
            isRepositioning = false;
            enemyBrainContext.AlertUI?.HideAttackWarning();
        }
        #endregion

        public void SetHitChaseTimer(float duration)
        {
            hitChaseTimer = duration;
        }

        // Called by the Attack state after a bite; also starts the reposition.
        public void SetAttackCooldown(float duration)
        {
            attackTimer = duration;
            StartReposition();
        }

        private void StartReposition()
        {
            isRepositioning = true;
            hasRetreatPoint = false;
            repositionTimer = RetreatTimeout;
        }

        private void Reposition(AggressiveEnemyProfile aggressive, float deltaTime)
        {
            if (!isRepositioning)
            {
                // Never stand still during cooldown (e.g. re-entered Follow from Search mid-cooldown).
                if (attackTimer <= 0f)
                    return;

                StartReposition();
            }

            if (!hasRetreatPoint)
            {
                hasRetreatPoint = true;
                if (!TrySetRetreatPoint(aggressive))
                    BackOffFromPlayer(aggressive.retreatMinDistance);
                return;
            }

            repositionTimer -= deltaTime;

            var agent = enemyBrainContext.Agent;
            bool hopDone = repositionTimer <= 0f
                || (!agent.pathPending && (!agent.hasPath || agent.remainingDistance <= RetreatArriveDistance));

            // Cooldown over: stop roaming as soon as we're far enough away to dash, without finishing the hop.
            if (attackTimer <= 0f)
            {
                float distance = Vector2.Distance(enemyBrainContext.Transform.position, enemyBrainContext.PlayerTransform.position);
                if (hopDone || distance >= aggressive.retreatMinDistance)
                    isRepositioning = false;
                return;
            }

            // Cooldown still running: keep swimming to a fresh spot instead of holding still.
            if (hopDone)
            {
                hasRetreatPoint = false;
                repositionTimer = RetreatTimeout;
            }
        }

        // Random spot on the fish's side of the player (within ±RetreatSpreadAngle of "away"),
        // so it doesn't swim back through the player to get there. If that side is walled off,
        // the second half of the samples try any direction.
        private bool TrySetRetreatPoint(AggressiveEnemyProfile aggressive)
        {
            Vector3 playerPosition = enemyBrainContext.PlayerTransform.position;
            Vector2 away = enemyBrainContext.Transform.position - playerPosition;

            if (away.sqrMagnitude < 0.0001f)
                away = Random.insideUnitCircle;

            away.Normalize();

            for (int i = 0; i < RetreatSampleTries * 2; i++)
            {
                float spread = i < RetreatSampleTries ? RetreatSpreadAngle : 180f;
                Vector2 dir = Quaternion.Euler(0f, 0f, Random.Range(-spread, spread)) * away;
                float distance = Random.Range(aggressive.retreatMinDistance, aggressive.retreatMaxDistance);

                Vector3 candidate = playerPosition + (Vector3)(dir * distance);
                candidate.z = enemyBrainContext.Transform.position.z;

                // Straight line only: a spot behind a rock turns a short hop into a long detour.
                if (NavMesh.Raycast(enemyBrainContext.Transform.position, candidate, out _, NavMesh.AllAreas))
                    continue;

                if (pathfinder.SetSafeDestination(candidate))
                    return true;
            }

            return false;
        }

        private void MoveTowardLastKnownPosition()
        {
            if (!pathfinder.SetSafeDestination(LastKnownPlayerPosition))
                pathfinder.SetSafeDestinationNear(LastKnownPlayerPosition);
        }

        // Fallback when no random retreat spot is reachable: back straight away from the player.
        private void BackOffFromPlayer(float backOffDistance)
        {
            Vector3 playerPosition = enemyBrainContext.PlayerTransform.position;
            Vector3 awayDir = enemyBrainContext.Transform.position - playerPosition;
            awayDir.z = 0f;

            if (awayDir.sqrMagnitude < 0.0001f)
                awayDir = Random.insideUnitCircle;

            awayDir.Normalize();

            for (float distance = backOffDistance; distance >= 1f; distance -= 1f)
            {
                Vector3 candidate = playerPosition + awayDir * distance;
                candidate.z = enemyBrainContext.Transform.position.z;

                if (pathfinder.SetSafeDestination(candidate))
                    return;
            }
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
            if (!(profile is AggressiveEnemyProfile enemy) || !enemy.canAttack || !targetKnown)
                return;

            float distance = enemyBrainContext.PlayerTransform != null
                ? Vector2.Distance(enemyBrainContext.Transform.position, enemyBrainContext.PlayerTransform.position)
                : float.MaxValue;

            bool closeEnough = distance <= enemy.detectDistance * 0.75f;
            if (!closeEnough || attackTimer > 0f || isRepositioning)
                return;

            // A dash-through started inside attackRange has the mouth already past the player: reposition first.
            if (enemy.dashThrough && distance < enemy.attackRange)
            {
                StartReposition();
                return;
            }

            if (enemy.prepareBeforeAttack && enemyFSM.HasState(EnemyStateTag.Prepare))
                enemyFSM.TransitionTo(EnemyStateTag.Prepare);
            else if (enemyFSM.HasState(EnemyStateTag.Attack))
                enemyFSM.TransitionTo(EnemyStateTag.Attack);
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
