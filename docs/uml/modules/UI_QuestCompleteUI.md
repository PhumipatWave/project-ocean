# UI/QuestCompleteUI

1 types · 100 LOC (non-blank) · depends on: [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×2, [Core](Core.md) ×1, [UI](UI.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class QuestCompleteUI {
    <<MB>>
    #Awake() void
    #OnDestroy() void
    +Show(Quest) void
  }
  class Singleton {
    <<Core>>
  }
  Singleton <|-- QuestCompleteUI
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| QuestCompleteUI | QuestManager | OnQuestCompleted |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| QuestCompleteUI | MB | `QuestCompleteUI.cs:9` | 100 |
