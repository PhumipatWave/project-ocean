# Core/Save/DayReset

3 types · 91 LOC (non-blank) · depends on: [Core/Save](Core_Save.md) ×6, [Core](Core.md) ×4, [Core/Events](Core_Events.md) ×2

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class DayManager {
    <<MB>>
    +int CurrentDay
    +int MaxDivesPerDay$
    +int DivesToday
    +int TotalDives
    +int DivesRemaining
    +bool CanDive
    +bool HasDivedToday
    +UnityEvent~int~ onNewDay
    +event System.Action OnEnemyReset$
    +event System.Action OnDayAdvanced$
    ... +1 more fields
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
    #bool DestroyGameObjectOnDuplicate
    +RegisterKill(string) void
    +IsKilled(string) bool
    +ResetAll() void
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class OneTimePickupTracker {
    <<MB>>
    #bool DestroyGameObjectOnDuplicate
    +RegisterPickup(string) void
    +CountFor(string) int
    +ResetAll() void
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class ISaveParticipant {
    <<Core/Save>>
  }
  class Singleton {
    <<Core>>
  }
  ISaveParticipant <|.. DayManager
  ISaveParticipant <|.. EnemyKillTracker
  ISaveParticipant <|.. OneTimePickupTracker
  Singleton <|-- DayManager
  Singleton <|-- EnemyKillTracker
  Singleton <|-- OneTimePickupTracker
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| EnemyKillTracker | DayManager | OnEnemyReset |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DayManager | MB | `DayManager.cs:6` | 53 |
| EnemyKillTracker | MB | `EnemyKillTracker.cs:8` | 23 |
| OneTimePickupTracker | MB | `OneTimePickupTracker.cs:7` | 15 |
