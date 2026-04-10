using Codice.Client.BaseCommands.Merge.Xml;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Testing
{
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
        
        
    }


    public enum ItemType
    {
        BuildingBlock,
        Boom,
        Item,
        Tool
    }

    public enum ActionType
    {
        Dig,
        Mine
    }
    
    
}
