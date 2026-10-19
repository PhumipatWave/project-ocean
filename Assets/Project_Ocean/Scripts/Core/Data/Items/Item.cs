using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Core
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
    public class Item : ScriptableObject
    {
        [Header("ID Items")]
        [SerializeField, FormerlySerializedAs("ItemID")]
        private int itemID;

        [Header("Basic Info")]
        [SerializeField, FormerlySerializedAs("ItemName")]
        private string itemName;

        [TextArea(2, 5)]
        [SerializeField, FormerlySerializedAs("ItemDescription")]
        private string itemDescription;

        [Header("Visual")]
        [SerializeField, FormerlySerializedAs("ItemVisual")]
        private Sprite itemVisual;

        [SerializeField, FormerlySerializedAs("ItemPrefab")]
        private GameObject itemPrefab;

        [Header("Economy")]
        [Range(0, 99999)]
        [SerializeField, FormerlySerializedAs("ItemPrice")]
        private int itemPrice;

        [Range(1, 999)]
        [SerializeField, FormerlySerializedAs("MaxStackable")]
        private int maxStackable = 1;

        [Header("Type")]
        [SerializeField, FormerlySerializedAs("ItemType")]
        private ItemType itemType;

        [Header("Fish Settings")]
        [SerializeField]
        private FishSize fishSize = FishSize.None;

        [Header("Shop Settings")]
        [SerializeField, FormerlySerializedAs("CanBuy")]
        private bool canBuy = true;

        [SerializeField, FormerlySerializedAs("CanSell")]
        private bool canSell = true;

        public int ItemID => itemID;
        public string ItemName => itemName;
        public string ItemDescription => itemDescription;
        public Sprite ItemVisual => itemVisual;
        public GameObject ItemPrefab => itemPrefab;
        public int ItemPrice => itemPrice;
        public int MaxStackable => maxStackable;
        public ItemType ItemType => itemType;
        public FishSize FishSize => fishSize;
        public bool CanBuy => canBuy;
        public bool CanSell => canSell;
    }

    public enum FishSize
    {
        None,
        Small,
        Medium,
        Large
    }

    public enum ItemType
    {
        Normal,
        Consumable,
        KeyItem,
        Building,
        Ammo,
        Quest,
        CheckpointItem,
    }
}