# Gameplay/Entities/Enemies/Brain

8 types · 363 LOC (non-blank) · depends on: [Gameplay/Entities/Enemies/Brain/FSM/States](Gameplay_Entities_Enemies_Brain_FSM_States.md) ×6, [Gameplay/Entities/Enemies/Brain/Interfaces](Gameplay_Entities_Enemies_Brain_Interfaces.md) ×5, [Gameplay/Entities/Enemies/NavAgents/Core](Gameplay_Entities_Enemies_NavAgents_Core.md) ×3, [Gameplay/Entities/Enemies/Brain/FSM](Gameplay_Entities_Enemies_Brain_FSM.md) ×2, [Gameplay/Entities/Enemies/PartBreak](Gameplay_Entities_Enemies_PartBreak.md) ×1, [Gameplay/Entities/Enemies](Gameplay_Entities_Enemies.md) ×1, [Gameplay/Entities/Enemies/NavAgents](Gameplay_Entities_Enemies_NavAgents.md) ×1, [Gameplay/Entities/Enemies/Behaviours](Gameplay_Entities_Enemies_Behaviours.md) ×1, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class AggressiveEnemyProfile {
    <<SO>>
    +bool searchAfterLost
    +bool prepareBeforeAttack
    +bool canAttack
    +bool alertNearbyAllies
    +float searchDuration
    +float searchRadius
    +float prepareTime
    +float boostSpeed
    +float dashAcceleration
    +bool dashThrough
    ... +8 more fields
  }
  class EnemyBaseProfile {
    <<SO>>
    <<abstract>>
    +string fishId
    +int schoolMinFollowers
    +int schoolMaxFollowers
    +float speed
    +float viewDistance
    +float viewAngle
    +bool debugVision
    +LayerMask wallLayer
    +float wallDetectDistance
    +float wanderRadius
    ... +14 more fields
    +CreateBrain() IEnemyBrain
  }
  class EnemyBrain {
    +bool IsAttacking
    +float ViewDistance
    +float ViewAngle
    +float AttackRange
    +EnemyBrain(EnemyBaseProfile)
    +CanSeePlayer() bool
    +OnHit() void
    +OnPlayerCollision() void
    +Initialize(EnemyBrainContext) void
    +OnHit(Transform) void
  }
  class EnemyBrainBase {
    <<abstract>>
    #EnemyBrainContext enemyBrainContext
    #EnemyFSM enemyFSM
    +bool IsAttacking
    +Initialize(EnemyBrainContext) void
    +Tick(float) void
    +OnHit(Transform) void
    +OnDeath() void
    #ShowAttackWarning() void
    #HideAttackWarning() void
    #SetSeeingPlayer(bool) void
    #HideAllAlerts() void
  }
  class EnemyBrainContext {
    +Enemy Owner
    +Transform Transform
    +NavMeshAgent Agent
    +EnemyFacingController Facing
    +FishAlertUI AlertUI
    +MonoBehaviour Host
    +Boid School
    +Player Player
    +Transform PlayerTransform
    +bool IsDead
  }
  class EnemyVision {
    +EnemyVision(EnemyBaseProfile, EnemyBrainContext)
    +CheckLineOfSight() bool
  }
  class FriendlyEnemyProfile {
    <<SO>>
  }
  class ScaredEnemyProfile {
    <<SO>>
    +bool alertNearbyAllies
    +float groupAlertRadius
  }
  class IBrainAttack {
    <<Gameplay/Entities/Enemies/Brain/Interfaces>>
  }
  class IBrainHitReaction {
    <<Gameplay/Entities/Enemies/Brain/Interfaces>>
  }
  class IBrainVision {
    <<Gameplay/Entities/Enemies/Brain/Interfaces>>
  }
  class IEnemyBrain {
    <<Gameplay/Entities/Enemies/Brain/Interfaces>>
  }
  EnemyBaseProfile <|-- AggressiveEnemyProfile
  EnemyBaseProfile <|-- FriendlyEnemyProfile
  EnemyBaseProfile <|-- ScaredEnemyProfile
  EnemyBrain --> EnemyBaseProfile : profile
  EnemyBrain --> EnemyVision : vision
  EnemyBrainBase --> EnemyBrainContext : enemyBrainContext
  EnemyBrainBase <|-- EnemyBrain
  EnemyVision --> EnemyBaseProfile : profile
  EnemyVision --> EnemyBrainContext : context
  IBrainAttack <|.. EnemyBrain
  IBrainHitReaction <|.. EnemyBrain
  IBrainVision <|.. EnemyBrain
  IEnemyBrain <|.. EnemyBrainBase
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| AggressiveEnemyProfile | SO | `AggressiveEnemyProfile.cs:11` | 44 |
| EnemyBaseProfile | SO, abstract | `EnemyBaseProfile.cs:10` | 64 |
| EnemyBrain | class | `EnemyBrain.cs:6` | 134 |
| EnemyBrainBase | abstract | `EnemyBrainBase.cs:6` | 42 |
| EnemyBrainContext | class | `EnemyBrainContext.cs:10` | 13 |
| EnemyVision | class | `EnemyVision.cs:6` | 54 |
| FriendlyEnemyProfile | SO | `FriendlyEnemyProfile.cs:13` | 3 |
| ScaredEnemyProfile | SO | `ScaredEnemyProfile.cs:14` | 9 |
