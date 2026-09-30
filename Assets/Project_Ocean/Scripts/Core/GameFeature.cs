namespace Game.Core
{
    /// <summary>A system that quest steps unlock or lock (see FeatureUnlockService).</summary>
    public enum GameFeature
    {
        Interact,
        Shop,
        Upgrade,
        Ocean,
        Inventory,
        Combat,
        // Dead — kept for save/list-index compat. EnemySpawner now gates per-zone on
        // EnemySpawnLevel1/2/3 below instead of this one flat flag.
        EnemySpawn,
        // Added at the end — existing authored unlocksFeatures lists are serialized by enum index.
        Trash,
        QuestBoard,
        EnemySpawnLevel1,
        EnemySpawnLevel2,
        EnemySpawnLevel3,
        // Ocean world collectables (starfish, shells, etc.) — see CollectableItemBase.IsFeatureUnlocked.
        InteractItem,
        // Gates the Next Day button (the "bed"/sleep interaction) in the Home Panel —
        // see LandUI.TryAdvanceDay/RefreshNextDayButton.
        Sleep,
        // Gates the "Back to Land" button in the Ocean HUD — see GameHUD.RefreshLandButton.
        BackToLand,
        // Gates the F "use item" press — see Player.HandleUseItem.
        UseItem,
        // Rock Breaker material (item 80001) spawns only once MQ_1003 needs it.
        RockBreakerMaterial,
    }
}
