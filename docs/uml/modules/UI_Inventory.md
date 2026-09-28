# UI/Inventory

4 types · 666 LOC (non-blank) · depends on: [UI/Inventory/InventoryPlayer](UI_Inventory_InventoryPlayer.md) ×4, [Core/Save](Core_Save.md) ×3, [Core/Datas/Items](Core_Datas_Items.md) ×3, [Core/Events](Core_Events.md) ×3, [Gameplay/Inventory](Gameplay_Inventory.md) ×2, [Core/Datas/Upgrades](Core_Datas_Upgrades.md) ×2, [Core](Core.md) ×2, [Gameplay/Services](Gameplay_Services.md) ×2, [Gameplay/AudioSetting](Gameplay_AudioSetting.md) ×2, [Gameplay/InteractObjects](Gameplay_InteractObjects.md) ×1, [Gameplay/Game](Gameplay_Game.md) ×1, [UI](UI.md) ×1, [Gameplay/Spawning](Gameplay_Spawning.md) ×1, [Core/Inputs](Core_Inputs.md) ×1, [UI/UIManager](UI_UIManager.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
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
    ... +16 more methods()
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
  class IInventory {
    <<Gameplay/Inventory>>
  }
  class ISaveParticipant {
    <<Core/Save>>
  }
  IInventory <|.. InventoryManager
  ISaveParticipant <|.. InventoryManager
  InventoryManager --> InventoryUIController : uiController
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| InventoryManager | InputReader | OnInventoryToggled |
| InventoryManager | UpgradeService | OnLevelGranted |
| InventoryManager | UpgradeService | OnLevelsLoaded |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| InventoryManager | MB | `InventoryManager.cs:9` | 537 |
| InventoryQuestService | static | `InventoryQuestService.cs:7` | 51 |
| InventoryUIController | MB | `InventoryUIController.cs:7` | 57 |
| UISlotHoverHighlight | MB | `UISlotHoverHighlight.cs:9` | 21 |
