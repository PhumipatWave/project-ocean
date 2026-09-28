# Gameplay/RopeSystems

1 types · 49 LOC (non-blank) · depends on: [Core](Core.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class RopeSystemUtil {
    <<static>>
    +BreakNormalRopeLimit(GameObject, GameObject, ref bool, ref float, float, float)$ void
    +ClearNormalRope(LineRenderer)$ void
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| RopeSystemUtil | static | `RopeSystemUtil.cs:8` | 49 |
