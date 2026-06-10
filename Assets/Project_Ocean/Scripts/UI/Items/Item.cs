using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Testing
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Item", menuName = "Object/Item")]
    public class Item : ScriptableObject
    {
        [Header("Item Identification")]
        public int ItemID;
        public string ItemName;

        [Header("Item Visuals")]
        public ItemType ItemType;

        public Sprite ItemVisual;
        public GameObject ItemPrefab;
        
        [Header("Item Stats")]

        [Range(1, 99)]
        public int MaxStackable = 1;

        [Range(1, 9999)]
        public int ItemPrice;
    }

    public enum ItemType
    {
        Normal,
        KeyItem,
        Consumable,
        Building,
        Ammo,
    Quest,
        Checkpointuest
    }
}
