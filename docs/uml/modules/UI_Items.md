# UI/Items

3 types · 83 LOC (non-blank) · depends on: [Core/Pool](Core_Pool.md) ×2, [Gameplay/AudioSetting](Gameplay_AudioSetting.md) ×2, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×1, [Gameplay/InteractObjects](Gameplay_InteractObjects.md) ×1, [Core/Save Game/DayAndResetEnemys](Core_Save_Game_DayAndResetEnemys.md) ×1, [UI/Inventory](UI_Inventory.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class Item {
    <<SO>>
    +int ItemID
    +string ItemName
    +string ItemDescription
    +Sprite ItemVisual
    +GameObject ItemPrefab
    +int ItemPrice
    +int MaxStackable
    +ItemType ItemType
    +bool CanBuy
    +bool CanSell
  }
  class ItemType {
    <<enum>>
    Normal
    Consumable
    KeyItem
    Building
    Ammo
    Quest
    CheckpointItem
  }
  class ItemDrop {
    <<MB>>
    +event System.Action OnCollected
    +Setup(Item, ItemSpawner) void
    +TryCollect() void
    +OnGetFromPool() void
    +OnReturnToPool() void
  }
  class CollectableItemBase {
    <<Gameplay/InteractObjects>>
  }
  class IPoolable {
    <<Core/Pool>>
  }
  CollectableItemBase <|-- ItemDrop
  IPoolable <|.. ItemDrop
  Item --> ItemType : ItemType
  ItemDrop --> Item : dropItem
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| Item | SO | `Item.cs:6` | 22 |
| ItemDrop | MB | `Itemdrop.cs:7` | 51 |
| ItemType | enum | `Item.cs:36` | 10 |
