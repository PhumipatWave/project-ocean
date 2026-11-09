using System;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    [Serializable]
    public struct SkillSlot
    {
        public BossSkillData data;

        [Tooltip("The skill becomes available when the boss health fraction is at or below this value. 1 = available from the start, 0.5 = below half health.")]
        [Range(0f, 1f)] public float unlockBelowHealth;
    }

    /// <summary>Profile of a fixed-position miniboss: chases like an aggressive fish, fights with skills, stays near its home.</summary>
    [CreateAssetMenu(fileName = "MiniBossProfile", menuName = "ScriptableObjects/Enemies/Mini Boss/Mini Boss Profile")]
    public class MiniBossProfile : AggressiveEnemyProfile
    {
        [Header("Skills")]
        [Tooltip("Set Unlock Below Health to 1 for skills available from the start.")]
        public SkillSlot[] skills;

        [Tooltip("Pause between two skills, during which the boss chases normally.")]
        [Min(0f)] public float globalSkillCooldown = 1.5f;

        [Header("Leash")]
        [Tooltip("Farther than this from its spawn point the boss swims home. Keep it larger than Wander Radius.")]
        [Min(1f)] public float leashRadius = 40f;

        [Tooltip("The boss counts as home within this distance of its spawn point.")]
        [Min(0.5f)] public float homeArriveDistance = 1.5f;

        [Tooltip("If enabled, the boss heals to full when it reaches home.")]
        public bool fullHealOnReset = true;

        public override IEnemyBrain CreateBrain() => new MiniBossBrain(this);

        private void OnValidate()
        {
            if (leashRadius <= wanderRadius)
                Debug.LogWarning($"[MiniBossProfile] '{name}': Leash Radius ({leashRadius}) should be larger than Wander Radius ({wanderRadius}), otherwise the boss keeps walking out of its leash.", this);

            if (skills == null) return;
            for (int i = 0; i < skills.Length; i++)
                if (skills[i].data == null)
                    Debug.LogWarning($"[MiniBossProfile] '{name}': skill slot {i} has no data.", this);
        }
    }
}
