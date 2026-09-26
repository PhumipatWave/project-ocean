# Gameplay/Entitys/Enemys

3 types · 173 LOC (non-blank) · depends on: [Gameplay/Entitys](Gameplay_Entitys.md) ×3, [Core/Interfaces](Core_Interfaces.md) ×2, [Core/Pool](Core_Pool.md) ×2, [Core](Core.md) ×2, [Gameplay/AudioSetting](Gameplay_AudioSetting.md) ×2, [Gameplay/Entitys/Enemys/NavAgents/Core](Gameplay_Entitys_Enemys_NavAgents_Core.md) ×1, [Gameplay/Entitys/Players](Gameplay_Entitys_Players.md) ×1, [Gameplay/InteractObjects](Gameplay_InteractObjects.md) ×1, [UI/Inventory](UI_Inventory.md) ×1, [UI/Items](UI_Items.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class EnemyEvents {
    <<static>>
    +event System.Action~string~ OnEnemyKilled$
    +RaiseEnemyKilled(string)$ void
  }
  class Enemy {
    <<MB>>
    +float SleepDistanceSqr
    +string EnemyId
    +Player Player
    +IEnemyBrain Brain
    +bool IsAttacking
    #Awake() void
    #OnDestroy() void
    #OnDamagedHandler() void
    #OnDeathHandler() void
    +RegisterNavAgent(EnemyNavAgentBase) void
  }
  class EnemyLoot {
    <<MB>>
    #bool CanCollect
    #OnEnable() void
    +TryCollect() void
    +Collect() void
  }
  class CollectableItemBase {
    <<Gameplay/InteractObjects>>
  }
  class Entity {
    <<Gameplay/Entitys>>
  }
  CollectableItemBase <|-- EnemyLoot
  Enemy --> EnemyLoot : loot
  EnemyLoot --> Enemy : enemy
  Entity <|-- Enemy
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| Enemy | MB | `Enemy.cs:17` | 124 |
| EnemyEvents | static | `Enemy.cs:6` | 8 |
| EnemyLoot | MB | `EnemyLoot.cs:6` | 41 |
