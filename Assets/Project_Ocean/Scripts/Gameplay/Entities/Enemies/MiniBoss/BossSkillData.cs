using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Numbers shared by every boss skill. A subclass adds its own fields and creates the runtime skill.</summary>
    public abstract class BossSkillData : ScriptableObject
    {
        [Header("Timing")]
        [Tooltip("The boss stops and shows a warning for this long before the skill deals damage.")]
        [Min(0f)] public float telegraphTime = 0.8f;

        [Tooltip("Idle time after the skill, so the player gets an opening.")]
        [Min(0f)] public float recoverTime = 0.5f;

        [Tooltip("Seconds before this skill can be used again.")]
        [Min(0f)] public float cooldown = 6f;

        [Header("Selection")]
        [Tooltip("Higher weight = picked more often when several skills are ready.")]
        [Min(0f)] public float weight = 1f;

        [Tooltip("The skill is only used when the player is within this distance.")]
        [Min(0f)] public float maxRange = 10f;

        [Header("Animation")]
        [Tooltip("Animator trigger fired when the telegraph starts. Leave empty for none.")]
        public string telegraphTrigger;

        [Tooltip("Animator trigger fired when the telegraph ends and the skill executes. Leave empty for none.")]
        public string executeTrigger;

        /// <summary>Builds the runtime skill. unlockBelowHealth comes from the profile slot (1 = always available).</summary>
        public abstract BossSkill CreateSkill(float unlockBelowHealth);
    }
}
