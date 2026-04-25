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
        public  TileBase tile;
        public ItemType type;
        public ActionType actionType;
        public Vector2Int range = new Vector2Int(5, 4);
        
        [Header("Only UI")]
        public bool stackable = true;
        
        [Header("Both")]
        public Sprite image;
        
        [Header("Shop")]
        public int buyPrice = 10;
        public int sellPrice = 5;

        public string itemName;
        public int amount;
        public int price;


    }


    public enum ItemType
    {
        FishSizeS,
        FishSizeM,
        FishSizeL,
        Item,
        Tool
    }

    public enum ActionType
    {
        Dig,
        Mine
    }

    
    
    
}
