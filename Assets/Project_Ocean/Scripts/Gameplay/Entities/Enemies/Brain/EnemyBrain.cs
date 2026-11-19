using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class EnemyBrain : EnemyBrainBase, IBrainVision, IBrainAttack, IBrainHitReaction
    {
        private readonly EnemyBaseProfile profile;

        private float lastContactDamageTime = float.MinValue;

        private EnemyVision vision;
        private EnemyFollowState followState;
        private EnemyAttackState attackState;
        private ChaseInterest chaseInterest;

        public override bool IsAttacking => attackState?.IsAttacking ?? false;
        public float ViewDistance => profile.viewDistance;
        public float ViewAngle => profile.viewAngle;
        public float AttackRange => (profile as AggressiveEnemyProfile)?.attackRange ?? 0f;

        public EnemyBrain(EnemyBaseProfile profile)
        {
            this.profile = profile;
        }

        public bool CanSeePlayer() => vision?.CheckLineOfSight() ?? false;
        public virtual void OnHit() => OnHit(enemyBrainContext?.PlayerTransform);

        public virtual void OnPlayerCollision()
        {
            // No contact damage: the animation-driven HitBox deals it.

            if (profile is FriendlyEnemyProfile || profile is ScaredEnemyProfile) return;
            OnHit(enemyBrainContext?.PlayerTransform);
        }

        public override void Initialize(EnemyBrainContext context)
        {
            base.Initialize(context);
            lastContactDamageTime = float.MinValue;

            if (profile == null)
            {
                Debug.LogError("[EnemyBrain] Missing brain profile.", context.Host);
                return;
            }

            var pathfinder = new EnemyNavPathfinder(
                context.Agent,
                profile.wallLayer,
                profile.fishClearanceRadius,
                profile.maxWanderSampleTries,
                profile.nearPlayerSearchRadius,
                profile.nearPlayerSearchSamples,
                profile.nearPlayerSearchStep,
                profile.edgeMargin,
                profile.reachTolerance,
                profile.debugNavigation
            );

            vision = new EnemyVision(profile, context);

            // Rebuilt on every Initialize, so a pooled fish never keeps a stale give-up cooldown.
            chaseInterest = CreateChaseInterest();

            followState = new EnemyFollowState(profile, pathfinder, vision, chaseInterest);

            enemyFSM.Register(new EnemyWanderState(profile, pathfinder, vision, chaseInterest));
            enemyFSM.Register(followState);

            switch (profile)
            {
                case AggressiveEnemyProfile aggressive:
                    SetupAggressiveStates(aggressive, pathfinder);
                    break;

                case FriendlyEnemyProfile friendly:
                    SetupFriendlyStates(friendly, pathfinder);
                    break;

                case ScaredEnemyProfile scared:
                    SetupScaredStates(scared, pathfinder);
                    break;

                default:
                    // Wander + Follow only — no extra states for this profile type.
                    break;
            }

            SetupExtraStates(pathfinder);

            HideAllAlerts();

            if (context.PlayerTransform != null)
                followState.LastKnownPlayerPosition = context.PlayerTransform.position;

            enemyFSM.Start(EnemyStateTag.Wander);
        }

        // Only aggressive fish give up on an unreachable player; other profiles keep their behavior.
        private ChaseInterest CreateChaseInterest()
        {
            if (!(profile is AggressiveEnemyProfile aggressive))
                return null;

            return new ChaseInterest(aggressive.unreachableBeforeLurk, aggressive.chaseGiveUpCooldown, () => Time.time);
        }

        private void SetupAggressiveStates(AggressiveEnemyProfile aggressive, EnemyNavPathfinder pathfinder)
        {
            if (aggressive.searchAfterLost)
            {
                enemyFSM.Register(new EnemySearchState(
                    profile.speed,
                    aggressive.searchDuration,
                    aggressive.searchRadius,
                    pathfinder,
                    vision.CheckLineOfSight,
                    position => followState.LastKnownPlayerPosition = position,
                    chaseInterest,
                    profile.repathInterval
                ));
            }

            if (aggressive.enableLurk)
                enemyFSM.Register(new EnemyLurkState(aggressive, pathfinder, chaseInterest));

            if (aggressive.canAttack)
            {
                var hitbox = enemyBrainContext.Owner.GetComponentInChildren<EnemyAttackHitbox>(true);
                hitbox?.Initialize(enemyBrainContext.Owner, aggressive.attackDamage);

                attackState = new EnemyAttackState(aggressive, pathfinder);

                if (aggressive.prepareBeforeAttack)
                    enemyFSM.Register(new EnemyPrepareAttackState(aggressive));

                enemyFSM.Register(attackState);
            }
        }

        private void SetupFriendlyStates(FriendlyEnemyProfile friendly, EnemyNavPathfinder pathfinder)
        {
            enemyFSM.Register(new EnemyFleeState(friendly, pathfinder));
        }

        private void SetupScaredStates(ScaredEnemyProfile scared, EnemyNavPathfinder pathfinder)
        {
            enemyFSM.Register(new EnemyFleeState(scared, pathfinder));
        }

        /// <summary>Hook for subclasses (e.g. a miniboss) to register extra states. Runs after the profile states, before the FSM starts.</summary>
        protected virtual void SetupExtraStates(EnemyNavPathfinder pathfinder) { }

        public override void OnHit(Transform attacker)
        {
            if (enemyBrainContext?.IsDead == true) return;
            if (followState == null) return;

            Vector3 knownPosition = ResolveKnownPosition(attacker);

            switch (profile)
            {
                case FriendlyEnemyProfile _:
                case ScaredEnemyProfile _:
                    TriggerFlee();
                    break;

                case AggressiveEnemyProfile aggressive when aggressive.chasePlayerWhenHit:
                    TriggerChase(aggressive, knownPosition);
                    break;
            }
        }

        private Vector3 ResolveKnownPosition(Transform attacker)
        {
            if (attacker != null)
                return attacker.position;

            if (enemyBrainContext.PlayerTransform != null)
                return enemyBrainContext.PlayerTransform.position;

            return enemyBrainContext.Transform.position;
        }

        private void TriggerFlee()
        {
            if (!enemyFSM.HasState(EnemyStateTag.Flee)) return;

            var fleeState = enemyFSM.GetState(EnemyStateTag.Flee) as EnemyFleeState;

            if (enemyFSM.IsIn(EnemyStateTag.Flee))
                fleeState?.EnterPanic();
            else
            {
                enemyFSM.TransitionTo(EnemyStateTag.Flee);
                fleeState?.EnterPanic();
            }
        }

        private void TriggerChase(AggressiveEnemyProfile aggressive, Vector3 knownPosition)
        {
            followState.LastKnownPlayerPosition = knownPosition;
            followState.SetHitChaseTimer(profile.hitChaseDuration);

            // Don't interrupt an ongoing attack or prepare phase.
            bool alreadyCommitted = enemyFSM.IsIn(EnemyStateTag.Prepare) || enemyFSM.IsIn(EnemyStateTag.Attack);
            if (alreadyCommitted) 
                return;

            ShowAttackWarning();
            enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }
    }
}
