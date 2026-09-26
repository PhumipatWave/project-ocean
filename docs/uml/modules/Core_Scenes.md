# Core/Scenes

4 types · 169 LOC (non-blank) · depends on: none

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class ILoadingVisual {
    <<interface>>
    +ApplyProfile(LoadingType)* void
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
| ILoadingVisual | interface | `ILoadingVisual.cs:4` | 4 |
| LoadingManager | MB | `LoadingManager.cs:9` | 148 |
| LoadingType | enum | `LoadingType.cs:3` | 6 |
| SceneLoadContext | static | `SceneLoadContext.cs:20` | 11 |
