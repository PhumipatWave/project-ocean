# UI/Interactions

2 types · 116 LOC (non-blank) · depends on: [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×4, [Core](Core.md) ×3, [UI](UI.md) ×1, [UI/LoadingSecne](UI_LoadingSecne.md) ×1, [Core/Services](Core_Services.md) ×1, [Core/Inputs](Core_Inputs.md) ×1, [Gameplay/Cutscenes](Gameplay_Cutscenes.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class DamagePoint {
    <<MB>>
    +ShowDeath() void
    +ShowDamagePoint(string) void
  }
  class InteractUI {
    <<MB>>
    +UnityEvent onInteract
    +event System.Action PlayerExited
    +ReleasePlayerLock() void
    +HideIcon() void
  }
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| InteractUI | InputReader | OnInteracted |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DamagePoint | MB | `DamagePoint.cs:7` | 26 |
| InteractUI | MB | `InteractUI.cs:20` | 90 |
