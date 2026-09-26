# UI/UIManager

4 types · 399 LOC (non-blank) · depends on: [UI](UI.md) ×5, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×4, [Gameplay/Cutscenes](Gameplay_Cutscenes.md) ×1, [Core](Core.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class UIManager {
    <<MB>>
    +UIManager Instance$
    +UIType CurrentUI
    +bool IsUIOpen
    +Register(UIWindow) void
    +Unregister(UIWindow) void
    +Open(UIType) void
    +CloseCurrent() void
    +Close(UIType) void
    +Toggle(UIType) void
    +CloseAll() void
    +OpenMainMenu() void
    +ToggleMainMenu() void
    +OpenPauseMenu() void
    +TogglePauseMenu() void
    +OpenSettingMenu() void
    ... +8 more methods()
  }
  class UIManager_UIType {
    <<enum>>
    None
    MainMenu
    PauseMenu
    SettingMenu
    SettingPause
    Inventory
    Quest
  }
  class UIWindow {
    <<MB>>
    +UIType Type
  }
  class UIWindow_UIType {
    <<enum>>
    MainMenu
    PauseMenu
    SettingMenu
    SettingPause
    Inventory
    Quest
  }
  UIManager --> UIWindow : windows
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| UIManager | MB | `UIManager.cs:21` | 362 |
| UIManager_UIType | enum | `UIManager.cs:24` | 10 |
| UIWindow | MB | `UIWindow.cs:5` | 37 |
| UIWindow_UIType | enum | `UIWindow.cs:6` | 9 |
