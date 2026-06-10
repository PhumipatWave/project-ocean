using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// ScriptableObject profile for JellyfishEnemy.
    /// Extends EnemyBaseProfile so it works with the existing brain pipeline.
    /// Create via: Assets → Create → Game/Enemy/JellyfishProfile
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Enemy/JellyfishProfile", fileName = "JellyfishProfile")]
    public class JellyfishProfile : EnemyBaseProfile
    {
        [Header("Bob Movement")]
        [Tooltip("How fast the jellyfish bobs up and down (frequency).")]
        public float bobSpeed     = 1.2f;

        [Tooltip("Max distance from origin the jellyfish travels up or down.")]
        public float bobAmplitude = 1.5f;

        [Header("Contact Damage")]
        [Tooltip("Damage dealt to the player on body collision.")]
        public int contactDamage = 1;
    }
}
