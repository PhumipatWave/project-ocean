# Core/Datas/Items

3 types · 50 LOC (non-blank) · depends on: none

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class InventoryItemDatabase {
    +InventoryItemDatabase(Item)
    +FindById(int) Item
  }
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
  InventoryItemDatabase --> Item : allItems
  Item --> ItemType : ItemType
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| InventoryItemDatabase | class | `InventoryItemDatabase.cs:3` | 18 |
| Item | SO | `Item.cs:6` | 22 |
| ItemType | enum | `Item.cs:36` | 10 |
