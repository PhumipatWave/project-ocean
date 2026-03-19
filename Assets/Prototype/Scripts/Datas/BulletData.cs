using UnityEngine;

namespace Game.Testing
{
    public enum AmmoType
    {
        Normal,
        Tranquilizer,
        GrapplingHook,
        Granade
    }

    [CreateAssetMenu(fileName = "BulletData", menuName = "Data/BulletData")]
    public class BulletData : MonoBehaviour
    {
        public float attackDamage;
        public float qualityMultiplier;
        public float bulletSpeed;
        public float bulletMass;
    }
}
