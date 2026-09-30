# UI/Quest

6 types · 493 LOC (non-blank) · depends on: [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×13, [Core/Events](Core_Events.md) ×4, [Core](Core.md) ×3, [Gameplay/Environments](Gameplay_Environments.md) ×2, [Core/Scenes](Core_Scenes.md) ×1, [Gameplay/Contracts](Gameplay_Contracts.md) ×1, [UI](UI.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class QuestBoardUI {
    <<MB>>
    +CloseBoard() void
    +NavigateToNextPage() void
    +NavigateToPreviousPage() void
  }
  class QuestCategoryUIData {
    +QuestCategory category
  }
  class QuestCategorySlot {
    <<MB>>
    +Setup(QuestCategory, Action~QuestCategory~) void
    +Clear() void
  }
  class QuestDetailPanel {
    <<MB>>
    +ShowPanel(Quest, Action) void
    +HidePanel() void
  }
  class QuestSlot {
    <<MB>>
    +SetupSlot(Quest, Action~Quest~, Sprite) void
    +ClearSlot() void
  }
  class QuestTrackerUI {
    <<MB>>
    +OnClickCancelQuest() void
  }
  class IQuestOfferView {
    <<Gameplay/Contracts>>
  }
  IQuestOfferView <|.. QuestDetailPanel
  QuestBoardUI --> QuestCategorySlot : categorySlots
  QuestBoardUI --> QuestCategoryUIData : categories
  QuestBoardUI --> QuestDetailPanel : questDetailPanel
  QuestBoardUI --> QuestSlot : questSlots
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| QuestBoardUI | QuestManager | OnQuestUpdated |
| QuestTrackerUI | QuestManager | OnQuestUpdated |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| QuestBoardUI | MB | `QuestBoardUI.cs:10` | 249 |
| QuestCategorySlot | MB | `QuestCategorySlot.cs:10` | 28 |
| QuestCategoryUIData | class | `QuestBoardUI.cs:55` | 4 |
| QuestDetailPanel | MB | `QuestDetailPanel.cs:10` | 70 |
| QuestSlot | MB | `QuestSlot.cs:11` | 61 |
| QuestTrackerUI | MB | `QuestTrackerUI.cs:9` | 85 |
