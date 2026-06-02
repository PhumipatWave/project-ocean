using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Brain profile for scared enemies — flees immediately on sight,
    /// panic-flees when hit by a bullet. Can alert nearby allies when fleeing.
    /// No attack fields exposed.
    /// All shared pathfinding fields (fishClearanceRadius, maxWanderSampleTries,
    /// nearPlayerSearch*) live on EnemyBaseProfile.
    /// </summary>
    [CreateAssetMenu(fileName = "ScaredEnemyProfile", menuName = "Enemy/Scared Enemy Profile")]
    public class ScaredEnemyProfile : EnemyBaseProfile
    {
        [Header("Behavior")]
        [Tooltip("If enabled, detecting the player alerts nearby allied enemies to flee as well.")]
        public bool alertNearbyAllies;

        [Header("Group Alert")]
        [Tooltip("Radius within which nearby allies are alerted when this enemy flees.")]
        public float groupAlertRadius = 3f;
    }
}
