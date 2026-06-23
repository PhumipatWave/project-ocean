using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// ScriptableObject that holds all tunable data for the Crab enemy.
    /// Create via: Assets → Create → Game/Enemies/CrabProfile
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Enemy/CrabProfile", fileName = "CrabProfile")]
    public class CrabProfile : EnemyBaseProfile
    {
        [Header("Crab Movement")]
        [Tooltip("Horizontal walk speed (units/sec). Overrides base speed if > 0.")]
        public float walkSpeed = 2f;

        [Tooltip("How long (seconds) the crab pauses before turning around.")]
        public float turnPauseDuration = 0.4f;

        [Header("Edge / Wall Detection")]
        [Tooltip("LayerMask used for ground-edge and wall raycasts.")]
        public LayerMask groundLayer;

        [Tooltip("How far ahead the crab checks for ground (edge detection).")]
        public float edgeCheckDistance = 0.3f;

        [Tooltip("How far ahead the crab checks for a wall.")]
        public float wallCheckDistance = 0.2f;

        [Header("Combat")]
        [Tooltip("Damage dealt on contact with the player.")]
        public float contactDamage = 10f;

        [Tooltip("Minimum seconds between contact-damage hits.")]
        public float contactDamageCooldown = 1f;
    }
}