# UI/Upgrades

6 types · 446 LOC (non-blank) · depends on: [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×3, [Gameplay/Projectiles](Gameplay_Projectiles.md) ×2, [UI/Shop](UI_Shop.md) ×1, [Core/Services](Core_Services.md) ×1, [UI](UI.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class UpgradeType {
    <<enum>>
    Inventory
    Rope
    NormalBullet
  }
  class UpgradeUnlock {
    +string unlockName
    +int unlockLevel
  }
  class UpgradeData {
    <<SO>>
    +string upgradeName
    +string description
    +UpgradeType type
    +int maxLevel
    +int prices
    +float value
    +List~UpgradeUnlock~ unlocks
  }
  class UpgradeManager {
    <<MB>>
    +OpenUpgradeShop() void
    +OpenFromShop() void
    +GetLevel(UpgradeData) int
    +BuyUpgrade(UpgradeData) void
    +SetLevel(UpgradeData, int) void
    +GetPrice(UpgradeData) int
  }
  class UpgradeSlot {
    <<MB>>
    +UpgradeData data
    +TMP_Text nameText
    +TMP_Text priceText
    +Image levelBoxes
    +Color activeColor
    +Color inactiveColor
    +Upgrade() void
    +RefreshUI() void
    +GetCurrentPriceText() string
    +OnPointerEnter(PointerEventData) void
    +OnPointerExit(PointerEventData) void
  }
  class UpgradeTooltip {
    <<MB>>
    +UpgradeTooltip Instance$
    +Show(UpgradeData, UpgradeManager, RectTransform) void
    +Hide() void
  }
  UpgradeData --> UpgradeType : type
  UpgradeData --> UpgradeUnlock : unlocks
  UpgradeSlot --> UpgradeData : data
  UpgradeSlot --> UpgradeManager : upgradeManager
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| UpgradeSlot | QuestManager | OnQuestUpdated |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| UpgradeData | SO | `UpgradeData.cs:25` | 18 |
| UpgradeManager | MB | `UpgradeManager.cs:9` | 31 |
| UpgradeSlot | MB | `UpgradeSlot.cs:8` | 105 |
| UpgradeTooltip | MB | `UpgradeTooltip.cs:7` | 279 |
| UpgradeType | enum | `UpgradeData.cs:7` | 6 |
| UpgradeUnlock | class | `UpgradeData.cs:15` | 7 |
