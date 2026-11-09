using System;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Runs one boss skill from start to finish, then goes back to chasing. The brain hands it the skill with StartSkill.</summary>
    public class EnemyBossSkillState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Skill;

        private readonly BossContext boss;
        private BossSkill active;

        /// <summary>Raised when a skill ends, finished or cancelled.</summary>
        public event Action SkillEnded;

        public EnemyBossSkillState(BossContext boss)
        {
            this.boss = boss;
        }

        /// <summary>Call before transitioning to this state.</summary>
        public void StartSkill(BossSkill skill)
        {
            active = skill;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);

            if (active == null)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
                return;
            }

            active.Begin(boss, UnityEngine.Time.time);
        }

        public override void Tick(float deltaTime)
        {
            if (active == null)
                return;

            // Player gone (died, destroyed): drop the skill and let Follow decide what comes next.
            if (enemyBrainContext.PlayerTransform == null)
            {
                End(cancel: true);
                return;
            }

            if (active.Tick(deltaTime))
                End(cancel: false);
        }

        public override void Exit()
        {
            // Left by something other than the skill ending (e.g. the leash): clean up.
            if (active != null)
                End(cancel: true, transition: false);
        }

        /// <summary>Stops the running skill (boss died or was reset).</summary>
        public void CancelActive()
        {
            if (active != null)
                End(cancel: true, transition: false);
        }

        private void End(bool cancel, bool transition = true)
        {
            BossSkill skill = active;
            active = null;

            if (cancel)
                skill.Cancel();

            SkillEnded?.Invoke();

            if (transition)
                enemyFSM.TransitionTo(EnemyStateTag.Follow);
        }
    }

    /// <summary>The boss left its leash: swim home ignoring the player, heal, then wander again.</summary>
    public class EnemyReturnHomeState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.ReturnHome;

        // If home cannot be reached for this long, count it as arrived so the boss never hangs here.
        private const float GiveUpAfterSeconds = 20f;

        private readonly BossContext boss;

        private RepathTimer repathTimer;
        private StuckDetector stuckDetector;
        private float elapsed;

        public EnemyReturnHomeState(BossContext boss)
        {
            this.boss = boss;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            ResumeAgent(boss.Profile.speed);
            enemyBrainContext.AlertUI?.HideAttackWarning();
            enemyBrainContext.AlertUI?.SetSeeingPlayer(false);

            repathTimer = new RepathTimer(boss.Profile.repathInterval, boss.Profile.repathInterval);
            stuckDetector = new StuckDetector(boss.Profile.stuckWindow, boss.Profile.stuckMinProgress);
            stuckDetector.Reset(enemyBrainContext.Transform.position);
            elapsed = 0f;
        }

        public override void Tick(float deltaTime)
        {
            elapsed += deltaTime;

            float distance = Vector2.Distance(enemyBrainContext.Transform.position, boss.Home);
            if (distance <= boss.Profile.homeArriveDistance || elapsed >= GiveUpAfterSeconds)
            {
                Arrive();
                return;
            }

            bool stuck = stuckDetector.Tick(deltaTime, enemyBrainContext.Transform.position);
            if (repathTimer.Tick(deltaTime) || stuck)
                boss.Pathfinder.MoveTo(boss.Pathfinder.Resolve(boss.Home), allowUnreachable: true);

            if (enemyBrainContext.Agent.velocity.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateFromVelocity(enemyBrainContext.Agent.velocity);
        }

        private void Arrive()
        {
            if (boss.Profile.fullHealOnReset)
                boss.Owner.Heal(boss.Owner.MaxHealth);

            enemyFSM.TransitionTo(EnemyStateTag.Wander);
        }
    }
}
