# UI/Inventory

5 types · 584 LOC (non-blank) · depends on: [UI/Inventory/InventoryPlayer](UI_Inventory_InventoryPlayer.md) ×4, [UI/Items](UI_Items.md) ×3, [Core](Core.md) ×3, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×2, [UI](UI.md) ×2, [Gameplay/AudioSetting](Gameplay_AudioSetting.md) ×2, [Core/Save Game/DayAndResetEnemys](Core_Save_Game_DayAndResetEnemys.md) ×1, [Gameplay/InteractObjects](Gameplay_InteractObjects.md) ×1, [Core/Services](Core_Services.md) ×1, [Core/Inputs](Core_Inputs.md) ×1, [UI/UIManager](UI_UIManager.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class InventoryItemDatabase {
    +InventoryItemDatabase(Item)
    +FindById(int) Item
  }
  class InventoryManager {
    <<MB>>
    +System.Action OnInventoryChanged$
    +System.Action OnInventoryClosed$
    +GameObject InventoryMenu
    +InventorySlot InventorySlots
    +System.Collections.Generic.IReadOnlyList~Item~ AllItems
    +InventoryManager Instance$
    +int ActiveBuildingSlotIndex
    +event System.Action~Item, int~ OnItemDelta$
    +OpenInventory() void
    +CloseInventory() void
    +CloseInventoryViaButton() void
    +SelectSlot(int) void
    +RaiseItemDelta(Item, int) void
    +AddItem(Item) bool
    +AddSlot(InventorySlot) void
    +FindItem(ItemType, int) Item
    +FindItemInDatabase(int) Item
    +SpawnItemAtSlot(Item, int, int) void
    +GetSelectedItem() Item
    +ConsumeItem(ItemType, out Item) bool
    ... +14 more methods()
  }
  class InventoryQuestService {
    <<static>>
    +HasQuestItem(InventorySlot, int)$ bool
    +GetQuestItemCount(InventorySlot, int)$ int
    +RemoveQuestItem(InventorySlot, int, int)$ bool
  }
  class InventoryUIController {
    <<MB>>
    +bool IsOpen
    +Initialize(GameObject) void
    +ToggleInventory() void
    +ShowInventory() void
    +HideInventory() void
  }
  class UISlotHoverHighlight {
    <<MB>>
    +Image targetImage
    +Color hoverColor
    +Color normalColor
    +OnPointerEnter(PointerEventData) void
    +OnPointerExit(PointerEventData) void
  }
  InventoryManager --> InventoryItemDatabase : itemDatabase
  InventoryManager --> InventoryUIController : uiController
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| InventoryManager | InputReader | OnInventoryToggled |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| InventoryItemDatabase | class | `InventoryItemDatabase.cs:3` | 18 |
| InventoryManager | MB | `InventoryManager.cs:8` | 437 |
| InventoryQuestService | static | `InventoryQuestService.cs:5` | 51 |
| InventoryUIController | MB | `InventoryUIController.cs:5` | 57 |
| UISlotHoverHighlight | MB | `UISlotHoverHighlight.cs:7` | 21 |
