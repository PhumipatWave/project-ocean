# Gameplay/Entities/Players/OxygenTube

3 types · 536 LOC (non-blank) · depends on: [Core/Events](Core_Events.md) ×2, [Gameplay/InteractObjects](Gameplay_InteractObjects.md) ×1, [Gameplay/Game](Gameplay_Game.md) ×1, [Core](Core.md) ×1

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
  class Seg {
    <<struct>>
    +Vector2 pos
    +Vector2 old
    +Seg(Vector2)
    +Pin(Vector2) void
  }
  class WrapPin {
    <<struct>>
    +Vector2 pos
    +float side
    +bool isCheckpoint
    +bool isHub
  }
  OxygenRope --> Seg : segs
  OxygenRope --> WrapPin : pins
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| OxygenRope | MB | `OxygenRope.cs:8` | 536 |
| Seg | struct | `OxygenRope.cs:113` | 6 |
| WrapPin | struct | `OxygenRope.cs:120` | 7 |
