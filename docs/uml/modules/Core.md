# Core

7 types · 252 LOC (non-blank) · depends on: [Core/Events](Core_Events.md) ×3, [Core/Scenes](Core_Scenes.md) ×2, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×2, [UI](UI.md) ×2, [Core/Inputs](Core_Inputs.md) ×1, [UI/UIManager](UI_UIManager.md) ×1, [Core/Save/DayReset](Core_Save_DayReset.md) ×1, [Core/Services](Core_Services.md) ×1, [UI/Loading](UI_Loading.md) ×1, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class DialogueState {
    <<static>>
    +event Action OnChanged$
    +bool IsShowing$
    +Show(GameObject)$ void
    +Hide(GameObject)$ void
    +ContainsShowingPanel(Transform)$ bool
  }
  class GameFeature {
    <<enum>>
    Interact
    Shop
    Upgrade
    Ocean
    Inventory
    Combat
    EnemySpawn
    Trash
    QuestBoard
    EnemySpawnLevel1
    ... +7 more
  }
  class GameState {
    <<enum>>
    Land
    Ocean
    Menu
  }
  class GameManager {
    <<MB>>
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
  class PlayerRegistry {
    <<static>>
    +Player Local$
    +Register(Player)$ void
    +Unregister(Player)$ void
    +GetNearest(Vector2)$ Player
  }
  class PlayerUIState {
    <<static>>
    +bool IsLocked$
    +int ResetCount$
    +event Action~bool~ OnLockChanged$
    +Open()$ void
    +Close()$ void
    +SetCloser(Action)$ void
    +CloseCurrent()$ void
    +Reset()$ void
  }
  class SceneNames {
    <<static>>
    +string Land$
    +string Ocean$
    +string MainMenu$
    +string Intro$
  }
  GameManager --> GameState : gameState
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DialogueState | static | `DialogueState.cs:13` | 36 |
| GameFeature | enum | `GameFeature.cs:7` | 20 |
| GameManager | MB | `GameManager.cs:13` | 126 |
| GameState | enum | `GameManager.cs:6` | 6 |
| PlayerRegistry | static | `PlayerRegistry.cs:5` | 18 |
| PlayerUIState | static | `PlayerUIState.cs:12` | 39 |
| SceneNames | static | `SceneNames.cs:7` | 7 |
