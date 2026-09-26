# UI/VFX

2 types · 148 LOC (non-blank) · depends on: none

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class PlayerFeedbackVFX {
    <<MB>>
    +PlayDamageVFX() void
    +SetSprinting(bool) void
  }
  class PlayerSprintVFX {
    <<MB>>
    +SetDirection(Vector2) void
    +SetSprinting(bool) void
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| PlayerFeedbackVFX | MB | `PlayerFeedbackVFX.cs:7` | 100 |
| PlayerSprintVFX | MB | `PlayerSprintVFX.cs:3` | 48 |
