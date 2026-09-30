# Gameplay/Entities/Enemies/Brain/FSM/States

6 types · 712 LOC (non-blank) · depends on: [Gameplay/Entities/Enemies/Brain](Gameplay_Entities_Enemies_Brain.md) ×18, [Gameplay/Entities/Enemies/Brain/FSM](Gameplay_Entities_Enemies_Brain_FSM.md) ×18, [Gameplay/Entities/Enemies/NavAgents/Core](Gameplay_Entities_Enemies_NavAgents_Core.md) ×6, [Gameplay/Entities/Enemies/Behaviours](Gameplay_Entities_Enemies_Behaviours.md) ×1, [Gameplay/Entities/Enemies/Brain/Interfaces](Gameplay_Entities_Enemies_Brain_Interfaces.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class EnemyAttackState {
    +bool IsAttacking
    +Tick(float) void
    +Exit() void
  }
  class EnemyFleeState {
    +Tick(float) void
    +Exit() void
    +EnterPanic() void
  }
  class EnemyFollowState {
    +Vector3 LastKnownPlayerPosition
    +Tick(float) void
    +Exit() void
    +SetAttackCooldown(float) void
  }
  class EnemyPrepareAttackState {
    +Tick(float) void
    +Exit() void
  }
  class EnemySearchState {
    +Vector3 SearchOrigin
    +Tick(float) void
  }
  class EnemyWanderState {
    +Tick(float) void
  }
  class EnemyStateBase {
    <<Gameplay/Entities/Enemies/Brain/FSM>>
  }
  EnemyStateBase <|-- EnemyAttackState
  EnemyStateBase <|-- EnemyFleeState
  EnemyStateBase <|-- EnemyFollowState
  EnemyStateBase <|-- EnemyPrepareAttackState
  EnemyStateBase <|-- EnemySearchState
  EnemyStateBase <|-- EnemyWanderState
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| EnemyAttackState | class | `EnemyAttackState.cs:6` | 125 |
| EnemyFleeState | class | `EnemyFleeState.cs:6` | 108 |
| EnemyFollowState | class | `EnemyFollowState.cs:7` | 223 |
| EnemyPrepareAttackState | class | `EnemyPrepareAttackState.cs:5` | 40 |
| EnemySearchState | class | `EnemySearchState.cs:8` | 62 |
| EnemyWanderState | class | `EnemyWanderState.cs:7` | 154 |
