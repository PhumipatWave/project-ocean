using UnityEngine;

namespace Game.Testing
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Project Ocean/Enemy/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Health")]
        public float maxHealth = 100f;

        [Header("Movement")]
        public float moveSpeed = 2f;
    }
}
