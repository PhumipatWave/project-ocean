# Core/Scenes

2 types · 17 LOC (non-blank) · depends on: none

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class LoadingType {
    <<enum>>
    Menu
    Land
    Ocean
  }
  class SceneLoadContext {
    <<static>>
    +bool IsLoading$
    +Vector3 Position$
    +BeginLoad(Vector3)$ void
    +Clear()$ void
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| LoadingType | enum | `LoadingType.cs:3` | 6 |
| SceneLoadContext | static | `SceneLoadContext.cs:20` | 11 |
