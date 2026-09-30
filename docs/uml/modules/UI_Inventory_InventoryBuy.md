# UI/Inventory/InventoryBuy

2 types · 70 LOC (non-blank) · depends on: [UI/Inventory/InventoryPlayer](UI_Inventory_InventoryPlayer.md) ×2, [Core](Core.md) ×1, [Core/Data/Items](Core_Data_Items.md) ×1, [UI/Shop](UI_Shop.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class ShopBuySlot {
    <<MB>>
    +Image image
    +Color selectedColor
    +Color notSelectedColor
    +DraggableItem currentItem
    +bool isSelected
    +SetItem(DraggableItem) void
    +OnPointerClick(PointerEventData) void
    +Select() void
    +Deselect() void
  }
  class ShopInventoryBuy {
    <<MB>>
    +ShopInventoryBuy Instance$
    +ShopBuySlot shopbuySlots
    +GameObject shopItemPrefab
    +Item itemsToSell
  }
  ShopInventoryBuy --> ShopBuySlot : shopbuySlots
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| ShopBuySlot | MB | `ShopBuySlot.cs:9` | 35 |
| ShopInventoryBuy | MB | `ShopInventoryBuy.cs:7` | 35 |
