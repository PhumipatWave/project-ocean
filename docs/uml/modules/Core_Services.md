# Core/Services

2 types · 170 LOC (non-blank) · depends on: [Core/Save Game](Core_Save_Game.md) ×4, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×3, [Core](Core.md) ×2, [UI/Upgrads](UI_Upgrads.md) ×2, [Gameplay/Entitys/Players](Gameplay_Entitys_Players.md) ×2, [Gameplay/Projectiles](Gameplay_Projectiles.md) ×1, [UI](UI.md) ×1, [UI/Inventory](UI_Inventory.md) ×1, [Gameplay/Entitys/Players/OxygenTube](Gameplay_Entitys_Players_OxygenTube.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class FeatureUnlockService {
    +FeatureUnlockService Instance$
    +event Action~GameFeature~ OnFeatureUnlocked$
    +event Action~GameFeature~ OnFeatureLocked$
    +IsUnlocked(GameFeature) bool
    +Unlock(GameFeature) void
    +Lock(GameFeature) void
    +ResetToDefault() void
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class UpgradeService {
    +UpgradeService Instance$
    +GetLevel(UpgradeType) int
    +SetLevel(UpgradeType, int) void
    +GetPrice(UpgradeData) int
    +TryBuyUpgrade(UpgradeData) bool
    +DevGrantNextLevel(UpgradeData, out string) bool
    +ApplyLoadedLevels(int, int) void
    +ResetToDefault() void
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class ISaveParticipant {
    <<Core/Save Game>>
  }
  ISaveParticipant <|.. FeatureUnlockService
  ISaveParticipant <|.. UpgradeService
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| FeatureUnlockService | class | `FeatureUnlockService.cs:16` | 66 |
| UpgradeService | class | `UpgradeService.cs:12` | 104 |
