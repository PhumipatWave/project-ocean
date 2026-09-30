# Gameplay/MainCameras

2 types · 36 LOC (non-blank) · depends on: [Core](Core.md) ×1, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class CameraShakeManager {
    <<MB>>
    #Awake() void
    +SmallShake() void
    +NormalShake() void
    +HighShake() void
  }
  class PlayerCamera {
    <<MB>>
  }
  class Singleton {
    <<Core>>
  }
  Singleton <|-- CameraShakeManager
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| CameraShakeManager | MB | `CameraShakeManager.cs:7` | 13 |
| PlayerCamera | MB | `PlayerCamera.cs:9` | 23 |
