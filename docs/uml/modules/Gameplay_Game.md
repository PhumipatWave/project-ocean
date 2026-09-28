# Gameplay/Game

1 types · 127 LOC (non-blank) · depends on: [Core/Scenes](Core_Scenes.md) ×3, [Core](Core.md) ×3, [Core/Events](Core_Events.md) ×3, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×2, [Core/Save/DayReset](Core_Save_DayReset.md) ×1, [Gameplay/Services](Gameplay_Services.md) ×1, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class GameManager {
    <<MB>>
    +event System.Action OnReturningToMenu$
    +Transform PlayerSpawnPoint
    +Player CurPlayer$
    +GameState GameState
    +bool MiniHubAvaliable
    +Transform MiniHubLocation
    +GameManager Instance$
    +ToMainMenu() void
    +StartDive() void
    +ReturnToLand() void
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| GameManager | MB | `GameManager.cs:7` | 127 |
