# Gameplay/Game

1 types · 132 LOC (non-blank) · depends on: [Core](Core.md) ×5, [Core/Events](Core_Events.md) ×4, [Core/Scenes](Core_Scenes.md) ×3, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×2, [Core/Save/DayReset](Core_Save_DayReset.md) ×1, [Gameplay/Services](Gameplay_Services.md) ×1, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class GameManager {
    <<MB>>
    +event System.Action OnReturningToMenu$
    +Transform PlayerSpawnPoint
    +GameState GameState
    +bool MiniHubAvaliable
    +Transform MiniHubLocation
    +GameManager Instance$
    +ToMainMenu() void
    +StartDive() void
    +ReturnToLand() void
  }
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| GameManager | GameFlowEvents | OnReturnToLandRequested |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| GameManager | MB | `GameManager.cs:7` | 132 |
