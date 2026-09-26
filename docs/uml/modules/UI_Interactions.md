# UI/Interactions

1 types · 26 LOC (non-blank) · depends on: [Gameplay/Entities](Gameplay_Entities.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class DamagePoint {
    <<MB>>
    +ShowDeath() void
    +ShowDamagePoint(string) void
  }
  class IDamagePopup {
    <<Gameplay/Entities>>
  }
  IDamagePopup <|.. DamagePoint
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DamagePoint | MB | `DamagePoint.cs:7` | 26 |
