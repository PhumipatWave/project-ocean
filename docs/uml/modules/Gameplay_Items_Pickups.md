# Gameplay/Items/Pickups

3 types · 219 LOC (non-blank) · depends on: [Core/Pool](Core_Pool.md) ×3, [Core](Core.md) ×2, [Gameplay/AudioSetting](Gameplay_AudioSetting.md) ×2, [Gameplay/Services](Gameplay_Services.md) ×1, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×1, [Core/Inputs](Core_Inputs.md) ×1, [Core/Datas/Items](Core_Datas_Items.md) ×1, [Gameplay/Spawning](Gameplay_Spawning.md) ×1, [Gameplay/Inventory](Gameplay_Inventory.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class CollectableItemBase {
    <<MB>>
    <<abstract>>
    #bool playerInRange
    +bool IsCollectable
    #bool CanCollect
    #OnEnable() void
    #OnDisable() void
    #Update() void
    #OnTriggerEnter2D(Collider2D) void
    #OnTriggerExit2D(Collider2D) void
    +TryCollect()* void
  }
  class ItemDrop {
    <<MB>>
    +event System.Action OnCollected
    +Setup(Item, ItemSpawner) void
    +TryCollect() void
    +OnGetFromPool() void
    +OnReturnToPool() void
  }
  class ShelleController {
    <<MB>>
    +OnGetFromPool() void
    +OnReturnToPool() void
  }
  class IPoolable {
    <<Core/Pool>>
  }
  CollectableItemBase <|-- ItemDrop
  IPoolable <|.. ItemDrop
  IPoolable <|.. ShelleController
  ShelleController --> ItemDrop : itemDrop
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| CollectableItemBase | InputReader | OnInteracted |
| ShelleController | ItemDrop | OnCollected |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| CollectableItemBase | MB, abstract | `CollectableItemBase.cs:7` | 70 |
| ItemDrop | MB | `ItemDrop.cs:7` | 51 |
| ShelleController | MB | `ShelleController.cs:7` | 98 |
