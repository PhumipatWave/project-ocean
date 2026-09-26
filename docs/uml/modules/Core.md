# Core

5 types · 108 LOC (non-blank) · depends on: none

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
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DialogueState | static | `DialogueState.cs:13` | 36 |
| GameFeature | enum | `GameFeature.cs:7` | 20 |
| GameState | enum | `GameState.cs:3` | 6 |
| PlayerUIState | static | `PlayerUIState.cs:12` | 39 |
| SceneNames | static | `SceneNames.cs:7` | 7 |
