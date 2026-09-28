# UI/UIMenuSetting

3 types · 137 LOC (non-blank) · depends on: [UI/UIManager](UI_UIManager.md) ×2

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class SettingMenuButton {
    <<MB>>
    +OpenSetting() void
  }
  class SettingTabManager {
    <<MB>>
    +OpenSetting() void
    +OpenControls() void
  }
  class UISettingManager {
    <<MB>>
    +GameObject menuPanel
    +GameObject inventoryPanel
    +GameObject questPanel
    +GameObject settingPanel
    +bool IsOpen
    +ToggleMenu() void
    +OpenInventory() void
    +OpenQuest() void
    +OpenSetting() void
    +CloseAll() void
    +BackToMenu() void
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| SettingMenuButton | MB | `SettingMenuButton.cs:7` | 12 |
| SettingTabManager | MB | `SettingTabManager.cs:8` | 33 |
| UISettingManager | MB | `UISettingManager.cs:7` | 92 |
