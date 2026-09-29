# Core

8 types · 123 LOC (non-blank) · depends on: none

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
  class GameLog {
    <<static>>
    +Info(object, UnityEngine.Object)$ void
  }
  class GameSession {
    <<static>>
    +GameState State$
  }
  class GameState {
    <<enum>>
    Land
    Ocean
    Menu
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
  class Tags {
    <<static>>
    +string PlayerSpawnPoint$
    +string OxygenTubePoint$
  }
  GameSession --> GameState : State
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DialogueState | static | `DialogueState.cs:13` | 36 |
| GameFeature | enum | `GameFeature.cs:7` | 20 |
| GameLog | static | `GameLog.cs:6` | 6 |
| GameSession | static | `GameSession.cs:4` | 4 |
| GameState | enum | `GameState.cs:3` | 6 |
| PlayerUIState | static | `PlayerUIState.cs:12` | 39 |
| SceneNames | static | `SceneNames.cs:7` | 7 |
| Tags | static | `Tags.cs:4` | 5 |
