using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Friendly enemy: wanders away from the player on sight and panics when hit.</summary>
    [CreateAssetMenu(fileName = "FriendlyEnemyProfile", menuName = "ScriptableObjects/Enemies/Profiles/Friendly Enemy Profile")]
    public class FriendlyEnemyProfile : EnemyBaseProfile
    {
        // No additional fields — everything is on EnemyBaseProfile.
    }
}
