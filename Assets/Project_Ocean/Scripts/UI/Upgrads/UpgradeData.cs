using UnityEngine;

namespace Game.Testing
{
    public enum UpgradeType
    {
        Inventory,
        Rope,
        NormalBullet
    }

    [CreateAssetMenu(menuName = "Game/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        public string upgradeName;

        [TextArea]
        public string description;

        public UpgradeType type;

        [Header("Upgrade Settings")]
        public int maxLevel = 6;

        [Header("Upgrade Prices")]
        public int[] prices;

        public float value;
    }
}