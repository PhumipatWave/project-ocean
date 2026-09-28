# Gameplay/Entities/Enemies/Brain/LinearPatrol

5 types · 218 LOC (non-blank) · depends on: [Gameplay/Entities/Enemies/Brain/Interfaces](Gameplay_Entities_Enemies_Brain_Interfaces.md) ×3, [Gameplay/Entities/Enemies/Brain](Gameplay_Entities_Enemies_Brain.md) ×2, [Gameplay/Entities/Enemies/PartBreak](Gameplay_Entities_Enemies_PartBreak.md) ×1

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
    <<Gameplay/Entities/Enemies/Brain>>
  }
  class IBrainHitReaction {
    <<Gameplay/Entities/Enemies/Brain/Interfaces>>
  }
  class IEnemyBrain {
    <<Gameplay/Entities/Enemies/Brain/Interfaces>>
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
| LinearPatrolBrain | class | `LinearPatrolBrain.cs:6` | 179 |
| LinearPatrolProfile | SO | `LinearPatrolProfile.cs:17` | 30 |
| PatrolAxis | enum | `LinearPatrolProfile.cs:6` | 5 |
| PatrolMovementMode | enum | `LinearPatrolProfile.cs:11` | 4 |
| State | enum | `LinearPatrolBrain.cs:7` | 1 |
