using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Brain profile for friendly enemies — wanders away from the player when seen,
    /// panic-flees when hit by a bullet. No attack fields exposed.
    /// All shared pathfinding fields (fishClearanceRadius, maxWanderSampleTries,
    /// nearPlayerSearch*) live on EnemyBaseProfile.
    /// </summary>
    [CreateAssetMenu(fileName = "FriendlyEnemyProfile", menuName = "Enemy/Friendly Enemy Profile")]
    public class FriendlyEnemyProfile : EnemyBaseProfile
    {
        // No additional fields — everything is on EnemyBaseProfile.
    }
}
