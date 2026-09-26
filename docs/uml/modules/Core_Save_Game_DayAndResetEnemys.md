# Core/Save Game/DayAndResetEnemys

11 types · 630 LOC (non-blank) · depends on: [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×4, [Core/Save Game](Core_Save_Game.md) ×4, [Core/Pool](Core_Pool.md) ×3, [Core](Core.md) ×3, [UI/Items](UI_Items.md) ×3, [Core/Services](Core_Services.md) ×2, [Gameplay/Entitys/Enemys/NavAgents/Core](Gameplay_Entitys_Enemys_NavAgents_Core.md) ×1, [Gameplay/Entitys/Enemys/Beharviours](Gameplay_Entitys_Enemys_Beharviours.md) ×1, [Gameplay/Entitys/Enemys/NavAgents](Gameplay_Entitys_Enemys_NavAgents.md) ×1

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
  class InteractableNextDay {
    <<MB>>
    +TrySleep() void
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
    <<Core/Save Game>>
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
| InteractableNextDay | DayManager | OnDayAdvanced |
| InteractableNextDay | DayManager | OnDivedToday |
| ItemSpawner | FeatureUnlockService | OnFeatureUnlocked |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DayManager | MB | `DayManager.cs:6` | 46 |
| EnemyKillTracker | MB | `EnemyKillTracker.cs:10` | 28 |
| EnemySpawnEntry | class | `GameObjectSpawnManager.cs:10` | 6 |
| EnemySpawner | MB | `EnemySpawner.cs:21` | 209 |
| GameObjectSpawnManager | MB | `GameObjectSpawnManager.cs:26` | 112 |
| InteractableNextDay | MB | `Interactable NextDay.cs:12` | 58 |
| ItemSpawnEntry | class | `GameObjectSpawnManager.cs:18` | 5 |
| ItemSpawner | MB | `ItemSpawner.cs:20` | 132 |
| OneTimePickupTracker | MB | `OneTimePickupTracker.cs:9` | 20 |
| ZoneItemEntry | class | `ItemSpawner.cs:8` | 7 |
| ZoneSpawnEntry | class | `EnemySpawner.cs:10` | 7 |
