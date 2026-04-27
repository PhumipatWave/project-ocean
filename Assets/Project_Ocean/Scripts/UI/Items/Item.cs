using Codice.Client.BaseCommands.Merge.Xml;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Testing
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Item", menuName = "Object/Item")]
    public class Item : ScriptableObject
    {
        [Header("Only gameplay")]
        public string itemID;
        public  TileBase tile;
        public ItemType itemType;
        public GameObject itemPrefab;
        
        [Header("Only UI")]
        public bool stackable = true;
        
        [Header("Both")]
        public Sprite image;
        
        [Header("Shop")]
        public int buyPrice = 10;
        public int sellPrice = 5;
    }

    public enum ItemType
    {
        Normal,
        Quest,
        Consumable,
        Building,
        Ammo,
    }
}
