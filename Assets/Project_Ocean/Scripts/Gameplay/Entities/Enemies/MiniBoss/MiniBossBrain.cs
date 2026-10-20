using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// FishL miniboss brain. Chases and lurks like an aggressive fish (EnemyBrain), and additionally:
    /// uses skills while chasing or lurking, swims home when it leaves its leash, and ignores hit reactions during skills.
    /// </summary>
    public class MiniBossBrain : EnemyBrain
    {
        private readonly MiniBossProfile bossProfile;

        private BossContext bossContext;
        private BossSkillSelector selector;
        private EnemyBossSkillState skillState;
        private float nextSkillTime;

        public MiniBossBrain(MiniBossProfile profile) : base(profile)
        {
            bossProfile = profile;
        }

        // The attack animation (and its hitbox) must also run during melee skills such as the dash.
        public override bool IsAttacking => base.IsAttacking || (skillState != null && skillState.IsMeleeActive);

        protected override void SetupExtraStates(EnemyNavPathfinder pathfinder)
        {
            bossContext = new BossContext(enemyBrainContext, pathfinder, bossProfile, enemyBrainContext.Transform.position);

            var skills = new List<BossSkill>();
            if (bossProfile.skills != null)
            {
                foreach (SkillSlot slot in bossProfile.skills)
                {
                    if (slot.data != null)
                        skills.Add(slot.data.CreateSkill(slot.unlockBelowHealth));
                }
            }

            selector = new BossSkillSelector(skills);

            skillState = new EnemyBossSkillState(bossContext);
            skillState.SkillEnded += OnSkillEnded;
            enemyFSM.Register(skillState);
            enemyFSM.Register(new EnemyReturnHomeState(bossContext));

            // The base class only prepares the hitbox when canAttack is on; the boss keeps canAttack off.
            var hitbox = enemyBrainContext.Owner.GetComponentInChildren<EnemyAttackHitbox>(true);
            hitbox?.Initialize(enemyBrainContext.Owner, bossProfile.attackDamage);

            nextSkillTime = Time.time + bossProfile.globalSkillCooldown;
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            if (enemyBrainContext.IsDead || bossContext == null)
                return;

            if (LeftLeash())
                return;

            TryStartSkill();
        }

        // Taking a hit must not pull the boss out of a skill or out of its way home.
        public override void OnHit(Transform attacker)
        {
            if (enemyFSM.IsIn(EnemyStateTag.Skill) || enemyFSM.IsIn(EnemyStateTag.ReturnHome))
                return;

            base.OnHit(attacker);
        }

        public override void OnDeath()
        {
            skillState?.CancelActive();
            base.OnDeath();
        }

        private bool LeftLeash()
        {
            if (enemyFSM.IsIn(EnemyStateTag.ReturnHome) || enemyFSM.IsIn(EnemyStateTag.Skill))
                return false;

            float distance = Vector2.Distance(enemyBrainContext.Transform.position, bossContext.Home);
            if (distance <= bossProfile.leashRadius)
                return false;

            enemyFSM.TransitionTo(EnemyStateTag.ReturnHome);
            return true;
        }

        // Skills start only while chasing or lurking: lurking is what lets the boss attack a player it cannot swim to.
        private void TryStartSkill()
        {
            bool canUseSkills = enemyFSM.IsIn(EnemyStateTag.Follow) || enemyFSM.IsIn(EnemyStateTag.Lurk);
            if (!canUseSkills || Time.time < nextSkillTime || bossContext.Player == null)
                return;

            BossSkill skill = selector.Pick(bossContext.HealthFraction, Time.time, bossContext);
            if (skill == null)
                return;

            skillState.StartSkill(skill);
            enemyFSM.TransitionTo(EnemyStateTag.Skill);
        }

        private void OnSkillEnded()
        {
            nextSkillTime = Time.time + bossProfile.globalSkillCooldown;
        }
    }
}
