using System;
using System.Collections.Generic;

namespace Game.Testing
{
    [Serializable]
    public class SaveData
    {
        public GameState gameState = GameState.Land;
        public string currentScene = "Land_Map";
        public float posX, posY, posZ;
        public string saveTime = "";
        public bool miniHubAvailable = false;
        public float maxRopeLength = 150f;
        public float reduceMaxRopeLength = 0f;
        public int money = 100;
        public List<ItemSaveData> inventoryItems = new();

        public int inventoryUpgradeLevel = 1;
        public int damageUpgradeLevel = 1;
        public float currentDamage = 10f;
        public int ropeUpgradeLevel = 1;
    }

    [Serializable]
    public class ItemSaveData
    {
        public int slotIndex;
        public string itemID;
        public int count;
    }
}