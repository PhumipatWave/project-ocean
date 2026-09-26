# UI/Loading/LoadingVisualController

2 types · 34 LOC (non-blank) · depends on: [Core/Scenes](Core_Scenes.md) ×3

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class LoadingVisualController {
    <<MB>>
    +ApplyProfile(LoadingType) void
  }
  class LoadingProfile {
    +LoadingType type
    +Sprite sprite
    +string stateName
  }
  class ILoadingVisual {
    <<Core/Scenes>>
  }
  ILoadingVisual <|.. LoadingVisualController
  LoadingVisualController --> LoadingProfile : profiles
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| LoadingProfile | class | `LoadingVisualController.cs:8` | 7 |
| LoadingVisualController | MB | `LoadingVisualController.cs:6` | 34 |
