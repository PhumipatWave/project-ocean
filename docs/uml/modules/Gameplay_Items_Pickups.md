# Gameplay/Items/Pickups

3 types · 157 LOC (non-blank) · depends on: [Core/Pool](Core_Pool.md) ×3, [Gameplay/AudioSetting](Gameplay_AudioSetting.md) ×2, [Core](Core.md) ×1, [Core/Data/Items](Core_Data_Items.md) ×1, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×1, [Gameplay/Interaction](Gameplay_Interaction.md) ×1, [Gameplay/Inventory](Gameplay_Inventory.md) ×1, [Gameplay/Services](Gameplay_Services.md) ×1, [Gameplay/Spawning](Gameplay_Spawning.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class CollectableItemBase {
    <<abstract>>
    <<MB>>
    +bool IsCollectable
    #bool TakesPlayerLock
    #bool IsFeatureUnlocked
    +bool CanInteract
    #Interact(Player) void
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
  class ShellPickup {
    <<MB>>
    +OnGetFromPool() void
    +OnReturnToPool() void
  }
  class IPoolable {
    <<Core/Pool>>
  }
  class Interactable {
    <<Gameplay/Interaction>>
  }
  CollectableItemBase <|-- ItemDrop
  IPoolable <|.. ItemDrop
  IPoolable <|.. ShellPickup
  Interactable <|-- CollectableItemBase
  ShellPickup --> ItemDrop : itemDrop
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| ShellPickup | ItemDrop | OnCollected |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| CollectableItemBase | abstract, MB | `CollectableItemBase.cs:6` | 9 |
| ItemDrop | MB | `ItemDrop.cs:7` | 50 |
| ShellPickup | MB | `ShellPickup.cs:7` | 98 |
