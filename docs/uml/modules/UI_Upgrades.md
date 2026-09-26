# UI/Upgrades

3 types · 415 LOC (non-blank) · depends on: [Core/Datas/Upgrades](Core_Datas_Upgrades.md) ×6, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×2, [UI/Shop](UI_Shop.md) ×1, [Core/Services](Core_Services.md) ×1, [UI](UI.md) ×1, [Core/Events](Core_Events.md) ×1, [Gameplay/Projectiles](Gameplay_Projectiles.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
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
  UpgradeSlot --> UpgradeManager : upgradeManager
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| UpgradeSlot | QuestManager | OnQuestUpdated |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| UpgradeManager | MB | `UpgradeManager.cs:9` | 31 |
| UpgradeSlot | MB | `UpgradeSlot.cs:8` | 105 |
| UpgradeTooltip | MB | `UpgradeTooltip.cs:7` | 279 |
