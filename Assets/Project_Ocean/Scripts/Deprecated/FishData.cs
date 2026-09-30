using UnityEngine;
using Game.Core;
using Game.Gameplay;
using Game.UI;

namespace Game.Deprecated
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
