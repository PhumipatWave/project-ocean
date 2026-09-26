# Core/Save/DayReset

10 types · 572 LOC (non-blank) · depends on: [Core/Save](Core_Save.md) ×4, [Core/Pool](Core_Pool.md) ×3, [Core](Core.md) ×3, [UI/Items](UI_Items.md) ×3, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×2, [Core/Services](Core_Services.md) ×2, [Gameplay/Entities/Enemies/Behaviours](Gameplay_Entities_Enemies_Behaviours.md) ×1, [Gameplay/Entities/Enemies/NavAgents/Core](Gameplay_Entities_Enemies_NavAgents_Core.md) ×1, [Gameplay/Entities/Enemies/NavAgents](Gameplay_Entities_Enemies_NavAgents.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class DayManager {
    <<MB>>
    +DayManager Instance$
    +int CurrentDay
    +int MaxDivesPerDay$
    +int DivesToday
    +int TotalDives
    +int DivesRemaining
    +bool CanDive
    +bool HasDivedToday
    +UnityEvent~int~ onNewDay
    +event System.Action OnEnemyReset$
    ... +2 more fields
    +SetDay(int) void
    +SetDivesToday(int) void
    +SetTotalDives(int) void
    +TryRecordDive() bool
    +AdvanceDay() void
  }
  class EnemyKillTracker {
    <<MB>>
    +EnemyKillTracker Instance$
    +RegisterKill(string) void
    +IsKilled(string) bool
    +ResetAll() void
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class ZoneSpawnEntry {
    +string enemyId
    +float weight
  }
  class EnemySpawner {
    <<MB>>
    +ResetZone() void
  }
  class EnemySpawnEntry {
    +string id
    +GameObject prefab
  }
  class ItemSpawnEntry {
    +Item item
  }
  class GameObjectSpawnManager {
    <<MB>>
    +GameObjectSpawnManager Instance$
    +GetEnemyPrefab(string) GameObject
    +GetItem(int) Item
    +GetItemAt(int) Item
    +RegisterSpawner(EnemySpawner) void
    +UnregisterSpawner(EnemySpawner) void
    +RegisterItemSpawner(ItemSpawner) void
    +UnregisterItemSpawner(ItemSpawner) void
  }
  class ZoneItemEntry {
    +int itemIndex
    +float weight
  }
  class ItemSpawner {
    <<MB>>
    +ResetSpawner() void
    +OnItemCollected(GameObject) void
  }
  class OneTimePickupTracker {
    <<MB>>
    +OneTimePickupTracker Instance$
    +RegisterPickup(string) void
    +CountFor(string) int
    +ResetAll() void
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class ISaveParticipant {
    <<Core/Save>>
  }
  EnemySpawner --> ZoneSpawnEntry : spawnEntries
  GameObjectSpawnManager --> EnemySpawnEntry : allEnemies
  GameObjectSpawnManager --> EnemySpawner : enemySpawners
  GameObjectSpawnManager --> ItemSpawnEntry : allItems
  GameObjectSpawnManager --> ItemSpawner : itemSpawners
  ISaveParticipant <|.. EnemyKillTracker
  ISaveParticipant <|.. OneTimePickupTracker
  ItemSpawner --> ZoneItemEntry : spawnEntries
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| EnemyKillTracker | DayManager | OnEnemyReset |
| GameObjectSpawnManager | DayManager | OnDayAdvanced |
| GameObjectSpawnManager | DayManager | OnEnemyReset |
| ItemSpawner | FeatureUnlockService | OnFeatureUnlocked |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DayManager | MB | `DayManager.cs:6` | 46 |
| EnemyKillTracker | MB | `EnemyKillTracker.cs:10` | 28 |
| EnemySpawnEntry | class | `GameObjectSpawnManager.cs:10` | 6 |
| EnemySpawner | MB | `EnemySpawner.cs:21` | 209 |
| GameObjectSpawnManager | MB | `GameObjectSpawnManager.cs:26` | 112 |
| ItemSpawnEntry | class | `GameObjectSpawnManager.cs:18` | 5 |
| ItemSpawner | MB | `ItemSpawner.cs:20` | 132 |
| OneTimePickupTracker | MB | `OneTimePickupTracker.cs:9` | 20 |
| ZoneItemEntry | class | `ItemSpawner.cs:8` | 7 |
| ZoneSpawnEntry | class | `EnemySpawner.cs:10` | 7 |
