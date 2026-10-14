using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// The player is out of reach: swim slowly around the nearest reachable water for a while,
    /// then give up and wander. A reachable player ends the lurk immediately.
    /// </summary>
    public class EnemyLurkState : EnemyStateBase
    {
        #region FieldsProperties
        public override EnemyStateTag StateTag => EnemyStateTag.Lurk;

        private const float LurkSpeedMultiplier = 0.7f;

        private readonly AggressiveEnemyProfile profile;
        private readonly EnemyNavPathfinder pathfinder;
        private readonly ChaseInterest chaseInterest;

        private LurkOrbit orbit;
        private RepathTimer repathTimer;
        private StuckDetector stuckDetector;
        private float lurkTimer;
        private float sinceRepath;
        #endregion

        #region Constructors
        public EnemyLurkState(AggressiveEnemyProfile profile, EnemyNavPathfinder pathfinder, ChaseInterest chaseInterest)
        {
            this.profile = profile;
            this.pathfinder = pathfinder;
            this.chaseInterest = chaseInterest;
        }
        #endregion

        #region StateCallbacks
        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            ResumeAgent(profile.speed * LurkSpeedMultiplier);

            lurkTimer = profile.lurkDuration;
            sinceRepath = 0f;

            // Each fish starts at its own angle so a school does not stack on one spot.
            orbit = new LurkOrbit(profile.lurkOrbitRadius, profile.lurkOrbitSpeed, Random.Range(0f, 360f));
            repathTimer = new RepathTimer(profile.repathInterval, profile.repathInterval);
            stuckDetector = new StuckDetector(profile.stuckWindow, profile.stuckMinProgress);
            stuckDetector.Reset(enemyBrainContext.Transform.position);
        }

        public override void Tick(float deltaTime)
        {
            if (enemyBrainContext.PlayerTransform == null)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
                return;
            }

            lurkTimer -= deltaTime;
            if (lurkTimer <= 0f)
            {
                GiveUp();
                return;
            }

            sinceRepath += deltaTime;
            bool stuck = stuckDetector.Tick(deltaTime, enemyBrainContext.Transform.position);

            if (repathTimer.Tick(deltaTime) || stuck)
            {
                if (!Repath(sinceRepath))
                    return; // transitioned away

                sinceRepath = 0f;
            }

            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);
        }
        #endregion

        // Returns false when the state changed (player became reachable).
        private bool Repath(float elapsed)
        {
            Vector3 playerPosition = enemyBrainContext.PlayerTransform.position;
            ReachResult playerReach = pathfinder.Resolve(playerPosition);

            if (playerReach.Status == ReachStatus.Reachable)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return false;
            }

            // Orbit the nearest reachable water to the player.
            ReachResult picked = default;
            bool Accept(Vector3 candidate)
            {
                ReachResult result = pathfinder.Resolve(candidate);
                if (result.Status != ReachStatus.Reachable)
                    return false;

                picked = result;
                return true;
            }

            if (orbit.TryPick(playerReach.Position, elapsed, Accept, out _))
                pathfinder.MoveTo(picked);

            return true;
        }

        private void GiveUp()
        {
            chaseInterest?.StartCooldown();
            enemyFSM.TransitionTo(EnemyStateTag.Wander);
        }
    }
}
