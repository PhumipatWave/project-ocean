using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Scared enemy: flees on sight, panics when hit, and can alert nearby allies.</summary>
    [CreateAssetMenu(fileName = "ScaredEnemyProfile", menuName = "ScriptableObjects/Enemies/Profiles/Scared Enemy Profile")]
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
