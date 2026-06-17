using UnityEngine;

namespace Game.Testing
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
    public class Item : ScriptableObject
    {
        [Header("ID Items")]
        public int ItemID;

        [Header("Basic Info")]
        public string ItemName;
        [TextArea(2, 5)]
        public string ItemDescription;

        [Header("Visual")]
        public Sprite ItemVisual;
        public GameObject ItemPrefab;

        [Header("Economy")]
        [Range(0, 99999)]
        public int ItemPrice;

        [Range(1, 999)]
        public int MaxStackable = 1;

        [Header("Type")]
        public ItemType ItemType;

        [Header("Shop Settings")]
        public bool CanBuy = true;
        public bool CanSell = true;

    }

    public enum ItemType
    {
        Normal,
        Consumable,
        KeyItem,
        Building,
        Ammo,
    Quest,
        Checkpointuest
    }
}