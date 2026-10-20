using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Everything a boss skill needs to know about the fight. One instance per boss brain.</summary>
    public sealed class BossContext
    {
        public EnemyBrainContext Brain { get; }
        public EnemyNavPathfinder Pathfinder { get; }
        public MiniBossProfile Profile { get; }

        /// <summary>Where the boss spawned; the leash and ReturnHome use it.</summary>
        public Vector3 Home { get; }

        public Enemy Owner => Brain.Owner;
        public Transform Self => Brain.Transform;
        public Transform Player => Brain.PlayerTransform;

        public float HealthFraction => Owner != null && Owner.MaxHealth > 0f ? Owner.CurrentHealth / Owner.MaxHealth : 1f;

        public BossContext(EnemyBrainContext brain, EnemyNavPathfinder pathfinder, MiniBossProfile profile, Vector3 home)
        {
            Brain = brain;
            Pathfinder = pathfinder;
            Profile = profile;
            Home = home;
        }

        public float DistanceToPlayer() =>
            Player != null ? Vector2.Distance(Self.position, Player.position) : float.MaxValue;

        /// <summary>True when no wall layer collider sits between the boss and the player.</summary>
        public bool HasClearLineToPlayer() =>
            Player != null && Physics2D.Linecast(Self.position, Player.position, Profile.wallLayer).collider == null;
    }

    /// <summary>
    /// A boss skill runs a fixed timeline: telegraph, execute, recover.
    /// Subclasses only describe what happens when the telegraph ends (OnExecute / TickExecute) and what to clean up (OnCancel).
    /// </summary>
    public abstract class BossSkill
    {
        private enum Phase { Idle, Telegraph, Execute, Recover }

        protected readonly BossSkillData data;
        private readonly float unlockBelowHealth;

        protected BossContext ctx { get; private set; }

        private Phase phase;
        private float timer;
        private float readyAt;

        public virtual bool IsMelee => false;
        public float Weight => data.weight;
        public bool IsActive => phase != Phase.Idle;

        protected BossSkill(BossSkillData data, float unlockBelowHealth)
        {
            this.data = data;
            this.unlockBelowHealth = unlockBelowHealth;
        }

        // ---- Selection ----

        public bool IsUnlocked(float healthFraction) => healthFraction <= unlockBelowHealth;
        public bool IsReady(float now) => now >= readyAt;

        /// <summary>Range and line-of-sight rule of this skill. Called only when it is unlocked and off cooldown.</summary>
        public abstract bool CanUse(BossContext context);

        // ---- Lifecycle ----

        public void Begin(BossContext context, float now)
        {
            ctx = context;
            phase = Phase.Telegraph;
            timer = data.telegraphTime;

            HoldPosition();
            ctx.Brain.AlertUI?.ShowAttackWarning();
            OnTelegraph();
        }

        /// <summary>Advances the skill. Returns true when it has finished (including recovery).</summary>
        public bool Tick(float deltaTime)
        {
            switch (phase)
            {
                case Phase.Telegraph:
                    FacePlayer();
                    timer -= deltaTime;
                    if (timer <= 0f)
                    {
                        ctx.Brain.AlertUI?.HideAttackWarning();
                        phase = Phase.Execute;
                        OnExecute();
                    }
                    return false;

                case Phase.Execute:
                    if (TickExecute(deltaTime))
                    {
                        phase = Phase.Recover;
                        timer = data.recoverTime;
                    }
                    return false;

                case Phase.Recover:
                    timer -= deltaTime;
                    if (timer > 0f)
                        return false;

                    Finish();
                    return true;

                default:
                    return true;
            }
        }

        /// <summary>Stops the skill early (boss died or was reset) and cleans up.</summary>
        public void Cancel()
        {
            if (phase == Phase.Idle)
                return;

            OnCancel();
            Finish();
        }

        private void Finish()
        {
            phase = Phase.Idle;
            readyAt = Time.time + data.cooldown;
            ctx.Brain.AlertUI?.HideAttackWarning();
            ReleaseAgent();
        }

        // ---- Hooks for subclasses ----

        protected virtual void OnTelegraph() { }
        protected virtual void OnExecute() { }

        /// <summary>Return true when the main part of the skill is over.</summary>
        protected abstract bool TickExecute(float deltaTime);

        protected virtual void OnCancel() { }

        // ---- Helpers ----

        protected void HoldPosition()
        {
            var agent = ctx.Brain.Agent;
            if (agent == null || !agent.isOnNavMesh)
                return;

            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        protected void ReleaseAgent()
        {
            var agent = ctx.Brain.Agent;
            if (agent != null && agent.isOnNavMesh)
                agent.isStopped = false;
        }

        protected void FacePlayer()
        {
            if (ctx.Player == null)
                return;

            Vector2 direction = ctx.Player.position - ctx.Self.position;
            if (direction.sqrMagnitude <= 0.01f)
                return;

            ctx.Brain.Facing.SetSuppressRotation(true);
            ctx.Brain.Facing.RotateTowards(direction.normalized);
            ctx.Brain.Facing.SetSuppressRotation(false);
        }
    }

    /// <summary>Picks the next boss skill: unlocked, off cooldown and usable, chosen by weight, never the same one twice in a row when another is available.</summary>
    public sealed class BossSkillSelector
    {
        private readonly IReadOnlyList<BossSkill> skills;
        private readonly List<BossSkill> candidates = new List<BossSkill>();
        private BossSkill last;

        public BossSkillSelector(IReadOnlyList<BossSkill> skills)
        {
            this.skills = skills;
        }

        /// <summary>Returns null when no skill qualifies.</summary>
        public BossSkill Pick(float healthFraction, float now, BossContext context)
        {
            candidates.Clear();

            for (int i = 0; i < skills.Count; i++)
            {
                BossSkill skill = skills[i];
                if (skill.IsUnlocked(healthFraction) && skill.IsReady(now) && skill.CanUse(context))
                    candidates.Add(skill);
            }

            if (candidates.Count == 0)
                return null;

            if (candidates.Count > 1)
                candidates.Remove(last);

            last = PickByWeight(candidates);
            return last;
        }

        private static BossSkill PickByWeight(List<BossSkill> list)
        {
            float total = 0f;
            for (int i = 0; i < list.Count; i++)
                total += list[i].Weight;

            if (total <= 0f)
                return list[0];

            float roll = Random.value * total;
            for (int i = 0; i < list.Count; i++)
            {
                roll -= list[i].Weight;
                if (roll <= 0f)
                    return list[i];
            }

            return list[list.Count - 1];
        }
    }
}
