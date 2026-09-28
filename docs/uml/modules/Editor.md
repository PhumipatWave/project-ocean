# Editor

3 types · 576 LOC (non-blank) · depends on: [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×6

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class QuestDataEditor {
    <<Editor>>
    +OnInspectorGUI() void
  }
  class QuestStepDataDrawer {
    <<Drawer>>
    +OnGUI(Rect, SerializedProperty, GUIContent) void
    +GetPropertyHeight(SerializedProperty, GUIContent) float
  }
  class FieldVisibility {
    <<struct>>
    +bool TargetCount
    +bool NotificationImages
    +bool Dialogue
    +bool Reward
    +bool UnlocksFeatures
    +bool LocksFeatures
    +bool TargetField
    +bool DeliverySource
    +bool TargetEnemyIds
    +bool TargetQuestItemIds
    ... +5 more fields
    +For(QuestType, DeliverySource, bool)$ FieldVisibility
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| FieldVisibility | struct | `QuestStepDataDrawer.cs:72` | 31 |
| QuestDataEditor | Editor | `QuestDataEditor.cs:19` | 48 |
| QuestStepDataDrawer | Drawer | `QuestStepDataDrawer.cs:67` | 528 |
