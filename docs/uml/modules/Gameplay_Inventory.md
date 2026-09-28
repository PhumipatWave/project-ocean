# Gameplay/Inventory

2 types · 24 LOC (non-blank) · depends on: [Core/Datas/Items](Core_Datas_Items.md) ×2

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class IInventory {
    <<interface>>
    +IReadOnlyList~Item~ AllItems
    +int ActiveBuildingSlotIndex
    +AddItem(Item)* bool
    +FindItemInDatabase(int)* Item
    +ConsumeItem(ItemType, out Item)* bool
    +PeekItem(ItemType)* Item
    +GetQuestItemCount(int)* int
    +RemoveQuestItem(int, int)* void
    +ClearBuildingSlot(int)* void
    +RefreshBuildingHints()* void
    +ResetToDefault()* void
  }
  class Inventory {
    <<static>>
    +IInventory Current$
    +System.Action OnChanged$
    +Register(IInventory)$ void
    +Unregister(IInventory)$ void
  }
  Inventory --> IInventory : Current
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| IInventory | interface | `IInventory.cs:7` | 14 |
| Inventory | static | `Inventory.cs:6` | 10 |
