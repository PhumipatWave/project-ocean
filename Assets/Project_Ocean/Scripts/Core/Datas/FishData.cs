using UnityEngine;

namespace Game.Testing
{
    public enum EnemyType
    {
        Normal,
        Special,
        Boss,
    }

    [CreateAssetMenu(fileName = "FishData", menuName = "Data/FishData")]
    public class FishData : ScriptableObject
    {
        public string fishName;
        public EnemyType fishType;
        public float deepLength;
        public float fishSize;
        public float fishQuality;
        public float fishPrice;
        public float maxHealth;
        public float maxSpeed;
    }
}
