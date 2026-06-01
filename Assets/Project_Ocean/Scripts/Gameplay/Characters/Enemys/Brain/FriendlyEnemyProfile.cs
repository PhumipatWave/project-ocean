using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Brain profile for friendly enemies — wanders away from the player when seen,
    /// panic-flees when hit by a bullet. No attack fields exposed.
    /// </summary>
    [CreateAssetMenu(fileName = "FriendlyEnemyProfile", menuName = "Enemy/Friendly Enemy Profile")]
    public class FriendlyEnemyProfile : EnemyBaseProfile
    {
        [Header("Pathfinding")]
        [Tooltip("Clearance radius used when sampling NavMesh positions.")]
        public float fishClearanceRadius = 0.5f;

        [Tooltip("Maximum attempts to find a valid wander point.")]
        public int maxWanderSampleTries = 20;

        [Header("Near-Wall Sampling")]
        [Tooltip("Radius used when searching for a position near the player around walls.")]
        public float nearPlayerSearchRadius = 5f;

        [Tooltip("Number of sample points cast around the player when searching near walls.")]
        public int nearPlayerSearchSamples = 24;

        [Tooltip("Step size between concentric sample rings near the player.")]
        public float nearPlayerSearchStep = 0.5f;
    }
}
