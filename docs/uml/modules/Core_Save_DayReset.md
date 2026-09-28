# Core/Save/DayReset

3 types · 107 LOC (non-blank) · depends on: [Core/Save](Core_Save.md) ×6, [Core/Events](Core_Events.md) ×2, [Core](Core.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class DayManager {
    <<MB>>
    +DayManager Instance$
    +int CurrentDay
    +int MaxDivesPerDay$
    +int DivesToday
    +int TotalDives
    +int DivesRemaining
    +bool CanDive
    +bool HasDivedToday
    +UnityEvent~int~ onNewDay
    +event System.Action OnEnemyReset$
    ... +2 more fields
    +SetDay(int) void
    +SetDivesToday(int) void
    +SetTotalDives(int) void
    +TryRecordDive() bool
    +AdvanceDay() void
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class EnemyKillTracker {
    <<MB>>
    +EnemyKillTracker Instance$
    +RegisterKill(string) void
    +IsKilled(string) bool
    +ResetAll() void
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class OneTimePickupTracker {
    <<MB>>
    +OneTimePickupTracker Instance$
    +RegisterPickup(string) void
    +CountFor(string) int
    +ResetAll() void
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class ISaveParticipant {
    <<Core/Save>>
  }
  ISaveParticipant <|.. DayManager
  ISaveParticipant <|.. EnemyKillTracker
  ISaveParticipant <|.. OneTimePickupTracker
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| EnemyKillTracker | DayManager | OnEnemyReset |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DayManager | MB | `DayManager.cs:6` | 59 |
| EnemyKillTracker | MB | `EnemyKillTracker.cs:10` | 28 |
| OneTimePickupTracker | MB | `OneTimePickupTracker.cs:9` | 20 |
