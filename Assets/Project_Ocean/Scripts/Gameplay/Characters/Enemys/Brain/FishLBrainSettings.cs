using UnityEngine;

namespace Game.Testing
{
    [CreateAssetMenu(fileName = "FishLBrainSettings", menuName = "Enemy/Fish L Brain Settings")]
    public class FishLBrainSettings : EnemyBrainSettingsBase
    {
        [Header("Movement (Large Fish)")]
        [Tooltip("High movement speed used during a committed attack charge.")]
        public float boostSpeed = 7f;

        [Header("Wall Avoidance (Large Fish)")]
        [Tooltip("Circle radius used when checking whether the fish has enough clearance.")]
        public float fishClearanceRadius = 0.8f;

        [Tooltip("Distance checked in front of the fish for nearby walls via CircleCast.")]
        public float wallCheckDistance = 0.8f;

        [Tooltip("Maximum attempts used when sampling a safe wander point.")]
        public int maxWanderSampleTries = 20;

        [Header("Search")]
        [Tooltip("How long the fish searches the area after losing sight of the player.")]
        public float searchDuration = 4f;

        [Tooltip("Radius around the last seen player position used while searching.")]
        public float searchRadius = 2.5f;

        [Header("Attack")]
        [Tooltip("Distance from the player required for the attack to deal damage.")]
        public float attackRange = 1.5f;

        [Tooltip("Time spent aiming at the player before the charge begins.")]
        public float prepareTime = 2f;

        [Tooltip("Time in seconds before the fish can attack again after a hit.")]
        public float attackCooldown = 2f;

        [Tooltip("Damage dealt to the player when the attack connects.")]
        public float attackDamage = 10.25f;

        [Header("Near-Wall Attack Sampling")]
        [Tooltip("Radius used to find a reachable attack destination near the player.")]
        public float nearPlayerSearchRadius = 5f;

        [Tooltip("Number of sample directions checked around the player for safe attack movement.")]
        public int nearPlayerSearchSamples = 24;

        [Tooltip("Distance between sample rings when searching around the player.")]
        public float nearPlayerSearchStep = 0.5f;

        public float detectDistance => viewDistance;
    }
}
