# UI/Inventory/InventoryPlayer

2 types · 105 LOC (non-blank) · depends on: [UI/Inventory](UI_Inventory.md) ×2, [UI/Items](UI_Items.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class DraggableItem {
    <<MB>>
    +Image image
    +TMP_Text countText
    +Transform parentAfterDrag
    +int count
    +InitialiseItem(Item, int) void
    +RefreshCount() void
    +OnBeginDrag(PointerEventData) void
    +OnDrag(PointerEventData) void
    +OnEndDrag(PointerEventData) void
    +GetItem() Item
  }
  class InventorySlot {
    <<MB>>
    +event Action~InventorySlot~ OnShopSlotClicked$
    +Image image
    +Color SelectedColor
    +Color notSelectedColor
    +bool isSelected
    +bool isUnlocked
    +bool isShopSlot
    +OnPointerClick(PointerEventData) void
    +Select() void
    +Deselect() void
    +OnDrop(PointerEventData) void
    +SetLocked(bool) void
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DraggableItem | MB | `DraggableItem.cs:8` | 43 |
| InventorySlot | MB | `InventorySlot.cs:8` | 62 |
