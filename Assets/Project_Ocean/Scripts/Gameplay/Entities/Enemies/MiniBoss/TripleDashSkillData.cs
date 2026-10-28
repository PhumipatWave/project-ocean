using UnityEngine;
using UnityEngine.AI;
using Game.Core;

namespace Game.Gameplay
{
    [CreateAssetMenu(fileName = "TripleDashSkill", menuName = "ScriptableObjects/Enemies/Mini Boss/Skills/Triple Dash")]
    public class TripleDashSkillData : BossSkillData
    {
        [Header("Triple Dash")]
        [Min(1)] public int dashCount = 3;

        [Tooltip("Agent speed during a dash.")]
        [Min(0f)] public float dashSpeed = 8f;

        [Tooltip("Seconds per dash. One dash covers about Dash Speed x Dash Duration.")]
        [Min(0.1f)] public float dashDuration = 0.6f;

        [Tooltip("Idle time between two dashes, so the player can dodge.")]
        [Min(0f)] public float pauseBetween = 0.4f;

        [Min(0f)] public float damage = 20f;

        private void Reset()
        {
            telegraphTime = 0.6f;
            recoverTime = 0.5f;
            cooldown = 8f;
            maxRange = 8f;
        }

        public override BossSkill CreateSkill(float unlockBelowHealth) => new TripleDashSkill(this, unlockBelowHealth);
    }

    /// <summary>
    /// Dashes at the player several times in a row. Each dash is aimed again at the player's position,
    /// and stops at the first wall on its line. The hitbox windows are keyed in the TrippleAttack clip, so they must match dashDuration and pauseBetween.
    /// </summary>
    public class TripleDashSkill : BossSkill
    {
        private const float EndReachedDistance = 0.5f;

        private readonly TripleDashSkillData dash;

        private int dashesDone;
        private bool dashing;
        private float timer;

        private bool agentOverridden;
        private float savedAcceleration;
        private bool savedAutoBraking;

        public TripleDashSkill(TripleDashSkillData data, float unlockBelowHealth) : base(data, unlockBelowHealth)
        {
            dash = data;
        }

        public override bool CanUse(BossContext context) =>
            context.DistanceToPlayer() <= dash.maxRange &&
            context.Pathfinder.Resolve(context.Player.position).Status == ReachStatus.Reachable;

        protected override void OnTelegraph()
        {
            dashesDone = 0;
            dashing = false;

            // The boss keeps canAttack off, so the hitbox damage comes from this skill.
            ctx.Owner.GetComponentInChildren<EnemyAttackHitbox>(true)?.Initialize(ctx.Owner, dash.damage);
        }

        protected override void OnExecute()
        {
            var agent = ctx.Brain.Agent;
            if (agent != null)
            {
                // The normal acceleration is too low to reach dash speed within a dash, and auto braking slows it near the player.
                savedAcceleration = agent.acceleration;
                savedAutoBraking = agent.autoBraking;
                agent.acceleration = ctx.Profile.dashAcceleration;
                agent.autoBraking = false;
                agentOverridden = true;
            }

            StartDash();
        }

        protected override bool TickExecute(float deltaTime)
        {
            timer -= deltaTime;

            if (dashing)
            {
                var agent = ctx.Brain.Agent;
                if (agent != null && agent.velocity.sqrMagnitude > 0.01f)
                    ctx.Brain.Facing.RotateFromVelocity(agent.velocity);

                bool reachedEnd = agent != null && !agent.pathPending && agent.remainingDistance <= EndReachedDistance;
                if (timer > 0f && !reachedEnd)
                    return false;

                dashing = false;
                dashesDone++;
                HoldPosition();

                if (dashesDone >= dash.dashCount)
                {
                    RestoreAgent();
                    return true;
                }

                timer = dash.pauseBetween;
                return false;
            }

            FacePlayer();
            if (timer <= 0f)
                StartDash();

            return false;
        }

        protected override void OnCancel()
        {
            dashing = false;
            RestoreAgent();
        }

        // Aims once at the player's current position and commits to a straight line.
        private void StartDash()
        {
            dashing = true;
            timer = dash.dashDuration;

            var agent = ctx.Brain.Agent;
            if (agent == null || !agent.isOnNavMesh)
                return;

            ReleaseAgent();
            agent.speed = dash.dashSpeed;

            Vector3 start = ctx.Self.position;
            Vector2 direction = ctx.Player != null
                ? (Vector2)(ctx.Player.position - start)
                : ctx.Brain.Facing.GetFacingDirection();

            if (direction.sqrMagnitude < 0.0001f)
                direction = ctx.Brain.Facing.GetFacingDirection();

            Vector3 end = start + (Vector3)(direction.normalized * dash.dashSpeed * dash.dashDuration);
            end.z = start.z;

            // Stop the dash at the first wall on the line.
            if (NavMesh.Raycast(start, end, out NavMeshHit hit, NavMesh.AllAreas))
                end = hit.position;

            agent.SetDestination(end);
        }

        private void RestoreAgent()
        {
            if (!agentOverridden)
                return;

            agentOverridden = false;

            var agent = ctx.Brain.Agent;
            if (agent == null)
                return;

            agent.acceleration = savedAcceleration;
            agent.autoBraking = savedAutoBraking;
            agent.speed = ctx.Profile.speed;

            if (agent.isOnNavMesh)
                agent.velocity = Vector3.ClampMagnitude(agent.velocity, ctx.Profile.speed);
        }
    }
}
