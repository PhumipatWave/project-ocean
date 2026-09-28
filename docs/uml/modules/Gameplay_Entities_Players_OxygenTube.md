# Gameplay/Entities/Players/OxygenTube

6 types · 575 LOC (non-blank) · depends on: [Core/Events](Core_Events.md) ×2, [Core](Core.md) ×1, [Gameplay/Game](Gameplay_Game.md) ×1, [Gameplay/InteractObjects](Gameplay_InteractObjects.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class OxygenRope {
    <<MB>>
    +float MaxRopeLength
    +bool IgnoreRopeLimit
    +GameObject StartPointObj
    +GameObject StartPointPrefab
    +bool HasCheckpoint
    +float HealthLimit
    +Vector2 CurrentAnchor
    +float UsedPinLength
    +event System.Action~float~ OnRopeBreachStart$
    +event System.Action OnRopeBreachEnd$
    +SetHealthLimit(float) void
    +ClampToRopeLimit(Vector2) Vector2
    +BuildRope() void
    +ResetRope() void
    +IncreaseMaxLength(float) void
    +AttachToCheckpoint(Vector2) void
    +AttachToHub(Vector2) void
    +DetachFromCheckpoint(Vector2) void
  }
  class WrapPin {
    <<struct>>
    +Vector2 pos
    +float side
    +bool isCheckpoint
    +bool isHub
  }
  class RopeCornerFinder {
    <<static>>
    +FindBestCorner(Collider2D, Vector2, Vector2, List~Vector2~, List~Vector2~)$ Vector2
  }
  class VerletRope {
    +int Count
    +Clear() void
    +Rebuild(Vector2, Vector2, float) void
    +Step(Vector2, Vector2, float, in Settings) void
    +AppendDrawPoints(List~Vector2~) void
  }
  class Settings {
    <<struct>>
    +float segmentLength
    +float damping
    +float segRadius
    +int solverIterations
    +int maxSimulatedSegs
    +Vector2 gravity
    +LayerMask collisionMask
  }
  class Seg {
    <<struct>>
    +Vector2 pos
    +Vector2 old
    +Seg(Vector2)
    +Pin(Vector2) void
  }
  OxygenRope --> Settings : SimSettings
  OxygenRope --> VerletRope : rope
  OxygenRope --> WrapPin : pins
  VerletRope --> Seg : segs
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| OxygenRope | MB | `OxygenRope.cs:8` | 364 |
| RopeCornerFinder | static | `RopeCornerFinder.cs:7` | 72 |
| Seg | struct | `VerletRope.cs:17` | 6 |
| Settings | struct | `VerletRope.cs:9` | 7 |
| VerletRope | class | `VerletRope.cs:7` | 139 |
| WrapPin | struct | `OxygenRope.cs:113` | 7 |
