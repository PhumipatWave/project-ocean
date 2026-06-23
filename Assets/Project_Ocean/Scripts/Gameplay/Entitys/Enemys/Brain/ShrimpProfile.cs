using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// ScriptableObject configuration for the Shrimp enemy.
    /// Create via: Right-click > Create > Game/Enemy/ShrimpProfile
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Enemy/ShrimpProfile", fileName = "ShrimpProfile")]
    public class ShrimpProfile : EnemyBaseProfile
    {
        [Header("Shrimp - Combat")]
        [Tooltip("Damage dealt on direct contact with the player.")]
        public float contactDamage = 10f;

        [Tooltip("Minimum seconds between contact damage ticks (prevent spam).")]
        public float damageCooldown = 1f;

        [Header("Shrimp - Patrol")]
        [Tooltip("ระยะเดินซ้าย-ขวาจากจุด spawn (เมตร)")]
        public float patrolRange = 5f;

        [Tooltip("ระยะหน่วงก่อนกลับทิศ (วิ)")]
        public float patrolTurnDelay = 0.3f;

        [Header("Shrimp - Loot / Pickup")]
        [Tooltip("Seconds the corpse stays interactable (press F) before auto-collecting.")]
        public float corpseInteractTime = 5f;

        [Tooltip("Total seconds the corpse lives before pool return (must be >= corpseInteractTime).")]
        public float corpseLingerTime = 8f;
    }
}
