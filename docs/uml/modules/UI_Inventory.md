# UI/Inventory

4 types · 411 LOC (non-blank) · depends on: [Gameplay/Inventory](Gameplay_Inventory.md) ×5, [Core](Core.md) ×4, [Core/Datas/Items](Core_Datas_Items.md) ×3, [Core/Events](Core_Events.md) ×3, [UI/Inventory/InventoryPlayer](UI_Inventory_InventoryPlayer.md) ×3, [Core/Datas/Upgrades](Core_Datas_Upgrades.md) ×2, [Core/Save](Core_Save.md) ×2, [Gameplay/AudioSetting](Gameplay_AudioSetting.md) ×2, [Gameplay/Services](Gameplay_Services.md) ×2, [Core/Inputs](Core_Inputs.md) ×1, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×1, [Gameplay/Items](Gameplay_Items.md) ×1, [Gameplay/Spawning](Gameplay_Spawning.md) ×1, [UI](UI.md) ×1, [UI/UIManager](UI_UIManager.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class InventoryManager {
    <<MB>>
    +System.Action OnInventoryChanged$
    +System.Action OnInventoryClosed$
    +event System.Action~Item, int~ OnItemDelta$
    +GameObject InventoryMenu
    +IReadOnlyList~Item~ AllItems
    +int ActiveBuildingSlotIndex
    +int PlayerSlotCount
    #bool DestroyGameObjectOnDuplicate
    #Awake() void
    #OnDestroy() void
    +OpenInventory() void
    +CloseInventory() void
    +CloseInventoryViaButton() void
    +SelectSlot(int) void
    +AddItem(Item) bool
    +FindItemInDatabase(int) Item
    +GetStack(int) ItemStack
    +IsSlotUnlocked(int) bool
    +IndexOfSlot(Transform) int
    +RemoveOneAt(int) Item
    ... +15 more methods()
  }
  class InventoryUIController {
    <<MB>>
    +bool IsOpen
    +Initialize(GameObject) void
    +ToggleInventory() void
    +ShowInventory() void
    +HideInventory() void
  }
  class SlotBuilder {
    <<static>>
    +Build(InventorySlot, Transform, int, string)$ InventorySlot
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
  class Singleton {
    <<Core>>
  }
  IInventory <|.. InventoryManager
  ISaveParticipant <|.. InventoryManager
  InventoryManager --> InventoryUIController : uiController
  Singleton <|-- InventoryManager
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| InventoryManager | InputReader | OnInventoryToggled |
| InventoryManager | InventoryModel | Changed |
| InventoryManager | InventoryModel | ItemDelta |
| InventoryManager | UpgradeService | OnLevelGranted |
| InventoryManager | UpgradeService | OnLevelsLoaded |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| InventoryManager | MB | `InventoryManager.cs:10` | 315 |
| InventoryUIController | MB | `InventoryUIController.cs:7` | 57 |
| SlotBuilder | static | `SlotBuilder.cs:6` | 18 |
| UISlotHoverHighlight | MB | `UISlotHoverHighlight.cs:9` | 21 |
