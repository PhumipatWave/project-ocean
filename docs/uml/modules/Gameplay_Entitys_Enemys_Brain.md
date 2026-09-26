# Gameplay/Entitys/Enemys/Brain

8 types · 363 LOC (non-blank) · depends on: [Gameplay/Entitys/Enemys/Brain/FSM/States](Gameplay_Entitys_Enemys_Brain_FSM_States.md) ×6, [Core/Interfaces](Core_Interfaces.md) ×5, [Gameplay/Entitys/Enemys/NavAgents/Core](Gameplay_Entitys_Enemys_NavAgents_Core.md) ×3, [Gameplay/Entitys/Enemys/Brain/FSM](Gameplay_Entitys_Enemys_Brain_FSM.md) ×2, [Gameplay/Entitys/Enemys/Partbraked](Gameplay_Entitys_Enemys_Partbraked.md) ×1, [Gameplay/Entitys/Enemys/Beharviours](Gameplay_Entitys_Enemys_Beharviours.md) ×1, [Gameplay/Entitys/Enemys](Gameplay_Entitys_Enemys.md) ×1, [Gameplay/Entitys/Players](Gameplay_Entitys_Players.md) ×1, [Gameplay/Entitys/Enemys/NavAgents](Gameplay_Entitys_Enemys_NavAgents.md) ×1

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
    <<Core/Interfaces>>
  }
  class IBrainHitReaction {
    <<Core/Interfaces>>
  }
  class IBrainVision {
    <<Core/Interfaces>>
  }
  class IEnemyBrain {
    <<Core/Interfaces>>
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
| AggressiveEnemyProfile | SO | `AggressiveEnemyProfile.cs:10` | 44 |
| EnemyBaseProfile | SO, abstract | `EnemyBaseProfile.cs:9` | 64 |
| EnemyBrain | class | `EnemyBrain.cs:5` | 134 |
| EnemyBrainBase | abstract | `EnemyBrainBase.cs:5` | 42 |
| EnemyBrainContext | class | `EnemyBrainContext.cs:9` | 13 |
| EnemyVision | class | `EnemyVision.cs:5` | 54 |
| FriendlyEnemyProfile | SO | `FriendlyEnemyProfile.cs:12` | 3 |
| ScaredEnemyProfile | SO | `ScaredEnemyProfile.cs:13` | 9 |
