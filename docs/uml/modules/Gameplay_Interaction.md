# Gameplay/Interaction

1 types · 90 LOC (non-blank) · depends on: [Core/Events](Core_Events.md) ×3, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×2, [Core](Core.md) ×2, [Core/Scenes](Core_Scenes.md) ×1, [Gameplay/Game](Gameplay_Game.md) ×1, [Core/Inputs](Core_Inputs.md) ×1, [Gameplay/Cutscenes](Gameplay_Cutscenes.md) ×1, [Gameplay/Services](Gameplay_Services.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
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
| InteractUI | MB | `InteractUI.cs:20` | 90 |
