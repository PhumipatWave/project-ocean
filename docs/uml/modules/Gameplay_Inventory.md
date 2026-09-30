# Gameplay/Inventory

5 types · 224 LOC (non-blank) · depends on: [Core/Data/Items](Core_Data_Items.md) ×6, [Core/Save](Core_Save.md) ×1

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
    +event System.Action OnChanged$
    +RaiseChanged()$ void
    +Register(IInventory)$ void
    +Unregister(IInventory)$ void
  }
  class InventoryModel {
    +event Action Changed
    +event Action~Item, int~ ItemDelta
    +int PlayerSlotCount
    +int BuildingSlotCount
    +int SlotCount
    +InventoryModel(int, int)
    +BuildingSlot(int) int
    +Get(int) ItemStack
    +IsUnlocked(int) bool
    +Add(Item) bool
    +RemoveOne(int) Item
    +RemoveStack(int) ItemStack
    +CountOf(int) int
    +Remove(int, int) int
    +RemoveAllWhere(Predicate~Item~) Dictionary~Item, int~
    +Move(int, int) bool
    +UnlockNext() bool
    ... +5 more methods()
  }
  class InventorySaveMapper {
    <<static>>
    +ToSave(IReadOnlyList~ItemStack~)$ List~ItemSaveData~
    +FromSave(List~ItemSaveData~, int, System.Func~int, Item~, string)$ ItemStack
  }
  class ItemStack {
    <<struct>>
    +ItemStack Empty$
    +Item Item
    +int Count
    +bool IsEmpty
    +ItemStack(Item, int)
  }
  Inventory --> IInventory : Current
  InventoryModel --> ItemStack : stacks
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| IInventory | interface | `IInventory.cs:7` | 14 |
| Inventory | static | `Inventory.cs:6` | 11 |
| InventoryModel | class | `InventoryModel.cs:8` | 142 |
| InventorySaveMapper | static | `InventorySaveMapper.cs:8` | 45 |
| ItemStack | struct | `ItemStack.cs:6` | 12 |
