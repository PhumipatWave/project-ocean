# UI/Loading

2 types · 46 LOC (non-blank) · depends on: [Core](Core.md) ×1, [Core/Scenes](Core_Scenes.md) ×1

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
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| BootstrapLoader | MB | `BootstrapLoader.cs:6` | 7 |
| LoadingButton | MB | `LoadingButton.cs:5` | 39 |
