# UI/Interactions

1 types · 26 LOC (non-blank) · depends on: [Gameplay/Entities](Gameplay_Entities.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class DamagePopup {
    <<MB>>
    +ShowDeath() void
    +ShowDamagePoint(string) void
  }
  class IDamagePopup {
    <<Gameplay/Entities>>
  }
  IDamagePopup <|.. DamagePopup
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DamagePopup | MB | `DamagePopup.cs:9` | 26 |
