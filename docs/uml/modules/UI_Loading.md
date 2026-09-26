# UI/Loading

3 types · 189 LOC (non-blank) · depends on: [Core](Core.md) ×1, [UI/Loading/LoadingVisualController](UI_Loading_LoadingVisualController.md) ×1, [Core/Scenes](Core_Scenes.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class BootstrapLoader {
    <<MB>>
  }
  class LoadingButton {
    <<MB>>
    +StartGame() void
    +BackToMenu() void
    +GoToOcean() void
    +BackToLand() void
  }
  class LoadingManager {
    <<MB>>
    +LoadingManager Instance$
    +bool IsLoading$
    +event System.Action OnLoadingFinished$
    +Block(object)$ void
    +Unblock(object)$ void
    +LoadScene(string) void
    +StartGame() void
    +BackToMenu() void
    +GoToOcean() void
    +BackToLand() void
    +LoadScene(string, LoadingType) void
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| BootstrapLoader | MB | `BootstrapLoader.cs:6` | 7 |
| LoadingButton | MB | `LoadingButton.cs:5` | 39 |
| LoadingManager | MB | `LoadingManager.cs:9` | 143 |
