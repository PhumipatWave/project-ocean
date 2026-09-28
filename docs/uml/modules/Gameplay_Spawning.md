# Gameplay/Spawning

7 types · 478 LOC (non-blank) · depends on: [Core/Pool](Core_Pool.md) ×3, [Core/Save/DayReset](Core_Save_DayReset.md) ×3, [Core](Core.md) ×2, [Gameplay/Services](Gameplay_Services.md) ×2, [Core/Datas/Items](Core_Datas_Items.md) ×2, [Gameplay/Entities/Enemies/NavAgents/Core](Gameplay_Entities_Enemies_NavAgents_Core.md) ×1, [Gameplay/Game](Gameplay_Game.md) ×1, [Gameplay/Entities/Enemies/Behaviours](Gameplay_Entities_Enemies_Behaviours.md) ×1, [Gameplay/Entities/Enemies/NavAgents](Gameplay_Entities_Enemies_NavAgents.md) ×1, [Gameplay/InteractObjects](Gameplay_InteractObjects.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
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
  EnemySpawner --> ZoneSpawnEntry : spawnEntries
  GameObjectSpawnManager --> EnemySpawnEntry : allEnemies
  GameObjectSpawnManager --> EnemySpawner : enemySpawners
  GameObjectSpawnManager --> ItemSpawnEntry : allItems
  GameObjectSpawnManager --> ItemSpawner : itemSpawners
  ItemSpawner --> ZoneItemEntry : spawnEntries
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| GameObjectSpawnManager | DayManager | OnDayAdvanced |
| GameObjectSpawnManager | DayManager | OnEnemyReset |
| ItemSpawner | FeatureUnlockService | OnFeatureUnlocked |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| EnemySpawnEntry | class | `GameObjectSpawnManager.cs:10` | 6 |
| EnemySpawner | MB | `EnemySpawner.cs:21` | 209 |
| GameObjectSpawnManager | MB | `GameObjectSpawnManager.cs:26` | 112 |
| ItemSpawnEntry | class | `GameObjectSpawnManager.cs:18` | 5 |
| ItemSpawner | MB | `ItemSpawner.cs:20` | 132 |
| ZoneItemEntry | class | `ItemSpawner.cs:8` | 7 |
| ZoneSpawnEntry | class | `EnemySpawner.cs:10` | 7 |
