using UnityEngine;

namespace Game.Testing
{
    public enum UpgradeType
    {
        Inventory,
        Rope
    }

    [CreateAssetMenu(menuName = "Game/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        public string upgradeName;

        [TextArea]
        public string description;

        public UpgradeType type;

        public int maxLevel = 5;
        public int basePrice;

        public float value;
    }
}