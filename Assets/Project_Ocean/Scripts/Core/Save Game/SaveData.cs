using System;
using System.Collections.Generic;

namespace Game.Testing
{
    [Serializable]
    public class SaveData
    {
        // Version tracking — bump SaveSystem.SAVE_VERSION when structure changes.
        public int saveVersion = 0;

        public GameState gameState        = GameState.Land;
        public string    currentScene     = "Land_Map";
        public float     posX, posY, posZ;
        public string    saveTime         = "";
        public bool      miniHubAvailable = false;

        public float maxRopeLength       = 150f;
        public float reduceMaxRopeLength = 0f;

        public int money = 100;

        public List<ItemSaveData> inventoryItems = new();

        public int   inventoryUpgradeLevel = 1;
        public int   damageUpgradeLevel    = 1;   // dead field (damage upgrade dropped) — kept for save compat
        public float currentDamage         = 10f; // dead field — kept for save compat
        public int   ropeUpgradeLevel      = 1;

        public int currentDay = 1;

        // ── v2: quest progress ──
        public List<QuestSaveData> quests = new();

        // ── v3: enemy kill persistence ──
        public List<string> killedEnemySpawnIds = new();
        public bool hasDivedToday = false;
    }

    [Serializable]
    public class ItemSaveData
    {
        public int    slotIndex;
        public string itemID;
        public int    count;
    }

    [Serializable]
    public class QuestSaveData
    {
        public int questId;
        public int currentCount; // kill-quest progress (defaults to 0 on old saves)

        public int status; // (int)QuestStatus
    }
}
