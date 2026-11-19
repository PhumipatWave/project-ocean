using UnityEngine;

namespace Game.Core
{
    public enum AmmoType
    {
        Normal,
        Tranquilizer,
        GrapplingHook,
        Granade
    }

    [CreateAssetMenu(fileName = "BulletData", menuName = "ScriptableObjects/Player/Bullet Data")]
    public class BulletData : ScriptableObject
    {
        public float attackDamage;
        public float qualityMultiplier;
        public float bulletSpeed;
        public float bulletMass;
    }
}
