using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    [CreateAssetMenu(fileName = "ShockwaveSkill", menuName = "Enemy/Boss Skills/Shockwave")]
    public class ShockwaveSkillData : BossSkillData
    {
        [Header("Shockwave")]
        [Tooltip("Prefab with a ShockwaveProjectile, a trigger collider and a kinematic Rigidbody2D.")]
        public GameObject projectilePrefab;

        [Min(1)] public int projectileCount = 3;

        [Tooltip("Angle between the centre wave and each outer wave.")]
        [Min(0f)] public float spreadAngle = 25f;

        [Min(0f)] public float projectileSpeed = 9f;

        [Tooltip("Seconds before a wave that hit nothing disappears.")]
        [Min(0.1f)] public float projectileLifetime = 3f;

        [Min(0f)] public float damage = 15f;

        private void Reset()
        {
            telegraphTime = 0.8f;
            recoverTime = 0.5f;
            cooldown = 6f;
            maxRange = 10f;
        }

        public override BossSkill CreateSkill(float unlockBelowHealth) => new ShockwaveSkill(this, unlockBelowHealth);
    }

    /// <summary>Fires a fan of waves toward the player's position at the moment the telegraph ends.</summary>
    public class ShockwaveSkill : BossSkill
    {
        private readonly ShockwaveSkillData shockwave;
        private readonly List<GameObject> live = new List<GameObject>();

        public ShockwaveSkill(ShockwaveSkillData data, float unlockBelowHealth) : base(data, unlockBelowHealth)
        {
            shockwave = data;
        }

        public override bool CanUse(BossContext context) =>
            context.DistanceToPlayer() <= shockwave.maxRange && context.HasClearLineToPlayer();

        protected override void OnExecute()
        {
            live.Clear();

            if (ObjectPoolManager.Instance == null || shockwave.projectilePrefab == null || ctx.Player == null)
                return;

            Vector2 aim = ctx.Player.position - ctx.Self.position;
            Vector2[] directions = ShockwaveFan.Directions(aim, shockwave.projectileCount, shockwave.spreadAngle);

            foreach (Vector2 direction in directions)
            {
                GameObject go = ObjectPoolManager.Instance.Get(shockwave.projectilePrefab, ctx.Self.position, Quaternion.identity);
                if (go == null)
                    continue;

                go.GetComponent<ShockwaveProjectile>()?.Initialize(
                    direction,
                    shockwave.projectileSpeed,
                    shockwave.projectileLifetime,
                    shockwave.damage,
                    ctx.Profile.wallLayer);

                live.Add(go);
            }
        }

        // The waves fly on their own; the skill only waits for its recover time.
        protected override bool TickExecute(float deltaTime) => true;

        protected override void OnCancel()
        {
            if (ObjectPoolManager.Instance == null)
                return;

            foreach (GameObject go in live)
                ObjectPoolManager.Instance.Return(go);

            live.Clear();
        }
    }

    public static class ShockwaveFan
    {
        /// <summary>
        /// Directions of a fan centred on aim. The outer directions are spreadDegrees to each side of aim;
        /// with an odd count the middle one points exactly at aim.
        /// </summary>
        public static Vector2[] Directions(Vector2 aim, int count, float spreadDegrees)
        {
            count = Mathf.Max(1, count);
            aim = aim.sqrMagnitude > 0.0001f ? aim.normalized : Vector2.right;

            var result = new Vector2[count];
            if (count == 1)
            {
                result[0] = aim;
                return result;
            }

            float step = 2f * spreadDegrees / (count - 1);
            for (int i = 0; i < count; i++)
            {
                float angle = -spreadDegrees + step * i;
                result[i] = Quaternion.Euler(0f, 0f, angle) * aim;
            }

            return result;
        }
    }
}
