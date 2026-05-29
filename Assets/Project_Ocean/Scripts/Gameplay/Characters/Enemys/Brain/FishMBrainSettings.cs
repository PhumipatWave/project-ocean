using UnityEngine;

namespace Game.Testing
{
    [CreateAssetMenu(fileName = "FishMBrainSettings", menuName = "Enemy/Fish M Brain Settings")]
    public class FishMBrainSettings : EnemyBrainSettingsBase
    {
        [Header("Attack")]
        [Tooltip("Distance from the player required before this fish can bite.")]
        public float attackRange = 1.5f;

        [Tooltip("Time in seconds before another bite can happen.")]
        public float attackCooldown = 2f;

        [Tooltip("Damage dealt to the player when the bite connects.")]
        public float attackDamage = 7.5f;
    }
}
