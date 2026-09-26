# Core/Interfaces

5 types · 30 LOC (non-blank) · depends on: [Gameplay/Entities/Enemies/Brain](Gameplay_Entities_Enemies_Brain.md) ×2, [Gameplay/Entities/Enemies/Brain/FSM](Gameplay_Entities_Enemies_Brain_FSM.md) ×2

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class IBrainAttack {
    <<interface>>
    +float AttackRange
  }
  class IBrainHitReaction {
    <<interface>>
    +OnHit()* void
    +OnPlayerCollision()* void
  }
  class IBrainVision {
    <<interface>>
    +float ViewDistance
    +float ViewAngle
    +CanSeePlayer()* bool
  }
  class IEnemyBrain {
    <<interface>>
    +bool IsAttacking
    +Initialize(EnemyBrainContext)* void
    +Tick(float)* void
    +OnHit(Transform)* void
    +OnDeath()* void
  }
  class IEnemyState {
    <<interface>>
    +EnemyStateTag StateTag
    +Enter(EnemyBrainContext, EnemyFSM)* void
    +Tick(float)* void
    +Exit()* void
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| IBrainAttack | interface | `IBrainAttack.cs:3` | 4 |
| IBrainHitReaction | interface | `IBrainHitReaction.cs:5` | 5 |
| IBrainVision | interface | `IBrainVision.cs:10` | 6 |
| IEnemyBrain | interface | `IEnemyBrain.cs:5` | 8 |
| IEnemyState | interface | `IEnemyState.cs:8` | 7 |
