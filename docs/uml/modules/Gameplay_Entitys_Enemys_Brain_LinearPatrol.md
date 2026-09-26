# Gameplay/Entitys/Enemys/Brain/LinearPatrol

5 types · 218 LOC (non-blank) · depends on: [Core/Interfaces](Core_Interfaces.md) ×3, [Gameplay/Entitys/Enemys/Brain](Gameplay_Entitys_Enemys_Brain.md) ×2, [Gameplay/Entitys/Enemys/Partbraked](Gameplay_Entitys_Enemys_Partbraked.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class LinearPatrolBrain {
    +bool IsAttacking
    +LinearPatrolBrain(LinearPatrolProfile)
    +Initialize(EnemyBrainContext) void
    +Tick(float) void
    +OnHit(Transform) void
    +OnDeath() void
    +OnHit() void
    +OnPlayerCollision() void
  }
  class State {
    <<enum>>
    Moving
    Pausing
  }
  class PatrolAxis {
    <<enum>>
    Horizontal
    Vertical
  }
  class PatrolMovementMode {
    <<enum>>
    FixedLength
    UntilWall
  }
  class LinearPatrolProfile {
    <<SO>>
    +PatrolAxis axis
    +PatrolMovementMode movementMode
    +float moveLength
    +float pauseDuration
    +float randomness
    +float driftAmplitude
    +float groundCheckOffset
    +float groundCheckDistance
    +LayerMask groundLayer
    +float attackDamage
    ... +1 more fields
    +CreateBrain() IEnemyBrain
  }
  class EnemyBaseProfile {
    <<Gameplay/Entitys/Enemys/Brain>>
  }
  class IBrainHitReaction {
    <<Core/Interfaces>>
  }
  class IEnemyBrain {
    <<Core/Interfaces>>
  }
  EnemyBaseProfile <|-- LinearPatrolProfile
  IBrainHitReaction <|.. LinearPatrolBrain
  IEnemyBrain <|.. LinearPatrolBrain
  LinearPatrolBrain --> LinearPatrolProfile : profile
  LinearPatrolBrain --> State : state
  LinearPatrolProfile --> PatrolAxis : axis
  LinearPatrolProfile --> PatrolMovementMode : movementMode
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| LinearPatrolBrain | class | `LinearPatrolBrain.cs:5` | 179 |
| LinearPatrolProfile | SO | `LinearPatrolProfile.cs:16` | 30 |
| PatrolAxis | enum | `LinearPatrolProfile.cs:5` | 5 |
| PatrolMovementMode | enum | `LinearPatrolProfile.cs:10` | 4 |
| State | enum | `LinearPatrolBrain.cs:6` | 1 |
