# Gameplay/Entitys/Enemys/NavAgents/Core

5 types · 464 LOC (non-blank) · depends on: [Gameplay/Entitys/Enemys/NavAgents](Gameplay_Entitys_Enemys_NavAgents.md) ×2, [Core/Interfaces](Core_Interfaces.md) ×1, [Gameplay/Entitys/Enemys/Brain](Gameplay_Entitys_Enemys_Brain.md) ×1, [Gameplay/Entitys/Enemys](Gameplay_Entitys_Enemys.md) ×1, [Gameplay/Entitys/Enemys/Beharviours](Gameplay_Entitys_Enemys_Beharviours.md) ×1, [Gameplay/Entitys/Players](Gameplay_Entitys_Players.md) ×1, [Core/Pool](Core_Pool.md) ×1, [Core](Core.md) ×1, [Core/Save Game/DayAndResetEnemys](Core_Save_Game_DayAndResetEnemys.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class EnemyFacingController {
    +bool FacingRight
    +Initialize() void
    +GetFacingDirection() Vector2
    +RotateFromVelocity(Vector2, float) void
    +RotateFromVelocity(Vector3, float) void
    +RotateTowards(Vector2) bool
  }
  class FacingTurnMode {
    <<enum>>
    SmoothTime
    TurnSpeed
  }
  class EnemyNavAgentBase {
    <<MB>>
    <<abstract>>
    #FishAlertUI fishAlertUI
    #string playerBulletTag
    #string playerTag
    #Transform visualFlipRoot
    #float turnSmoothTime
    #float maxTiltAngle
    #float flipDeadZone
    #float flipDelay
    #float flipCooldown
    #float spriteAngleOffset
    ... +15 more fields
    +SetSpawnId(string) void
    +NotifyKill() void
    #Awake() void
    #Start() void
    #Update() void
    #OnTriggerEnter2D(Collider2D) void
    #OnCollisionEnter2D(Collision2D) void
    +Initialize(Enemy) void
    #GetFishId() string
    #CreateBrain()* IEnemyBrain
    #OnBrainInitialized() void
    #OnTick() void
    ... +8 more methods()
  }
  class EnemyNavPathfinder {
    +IsPathComplete(Vector3) bool
    +SetSafeDestination(Vector3) bool
    +TryPickWanderPoint(Vector3, float, out Vector3) bool
    +SetSafeDestinationNear(Vector3) bool
  }
  class EnemyVisionHelper {
    <<static>>
    +HasLineOfSightRay(Vector2, Vector2, Vector2, float, float, LayerMask, Transform)$ bool
  }
  class IPoolable {
    <<Core/Pool>>
  }
  EnemyFacingController --> FacingTurnMode : turnMode
  EnemyNavAgentBase --> EnemyFacingController : facingTurnMode
  EnemyNavAgentBase --> FacingTurnMode : facingTurnMode
  IPoolable <|.. EnemyNavAgentBase
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| EnemyFacingController | class | `EnemyFacingController.cs:8` | 109 |
| EnemyNavAgentBase | MB, abstract | `EnemyNavAgentBase.cs:6` | 228 |
| EnemyNavPathfinder | class | `EnemyNavPathfinder.cs:9` | 106 |
| EnemyVisionHelper | static | `EnemyVisionHelper.cs:5` | 21 |
| FacingTurnMode | enum | `EnemyFacingController.cs:34` | 5 |
