using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Brain profile for scared enemies — flees immediately on sight,
    /// panic-flees when hit by a bullet. Can alert nearby allies when fleeing.
    /// No attack fields exposed.
    /// </summary>
    [CreateAssetMenu(fileName = "ScaredEnemyProfile", menuName = "Enemy/Scared Enemy Profile")]
    public class ScaredEnemyProfile : EnemyBaseProfile
    {
        [Header("Behavior")]
        [Tooltip("If enabled, detecting the player alerts nearby allied enemies to flee as well.")]
        public bool alertNearbyAllies;

        [Header("Pathfinding")]
        [Tooltip("Clearance radius used when sampling NavMesh positions.")]
        public float fishClearanceRadius = 0.5f;

        [Tooltip("Maximum attempts to find a valid wander point.")]
        public int maxWanderSampleTries = 20;

        [Header("Group Alert")]
        [Tooltip("Radius within which nearby allies are alerted when this enemy flees.")]
        public float groupAlertRadius = 3f;

        [Header("Near-Wall Sampling")]
        [Tooltip("Radius used when searching for a position near the player around walls.")]
        public float nearPlayerSearchRadius = 5f;

        [Tooltip("Number of sample points cast around the player when searching near walls.")]
        public int nearPlayerSearchSamples = 24;

        [Tooltip("Step size between concentric sample rings near the player.")]
        public float nearPlayerSearchStep = 0.5f;
    }
}
