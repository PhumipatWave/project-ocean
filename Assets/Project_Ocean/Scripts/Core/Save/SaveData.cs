using System;
using System.Collections.Generic;

namespace Game.Core
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
        // Mini Hub (building) slot items; slotIndex = index into InventoryManager's building slots.
        public List<ItemSaveData> buildingItems = new();

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
        public int  divesToday    = 0; // supersedes hasDivedToday; absent in older saves (0)

        // ── v4: feature unlocks ── (GameFeature enum names, e.g. "Shop", "Ocean")
        public List<string> unlockedFeatures = new();

        // ── v5: lifetime dive count ── absent in older saves (0)
        public int totalDives = 0;

        // ── v6: feature locks ── (GameFeature enum names) — always wins over unlockedFeatures, see FeatureUnlockService
        public List<string> lockedFeatures = new();

        // ── v7: gun (NormalBullet) upgrade level ── it was never saved before, so it reset to 0 on load.
        // Absent in older saves (0), which is also the correct value for them — no migration needed.
        public int normalBulletUpgradeLevel = 0;

        // ── v8: one-time item pickups ── (ItemSpawner name per pickup, see OneTimePickupTracker)
        // Absent in older saves (empty), which is correct — nothing collected yet.
        public List<string> collectedPickupIds = new();
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

        public int currentStepIndex; // which checkpoint is active (0 on old saves — correct, they had only one step)

        public int dayStepBecameCurrent = -1; // WaitingQuest only; -1 = not recorded (old saves, or non-WaitingQuest steps)
    }
}
