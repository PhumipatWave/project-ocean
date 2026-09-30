# Gameplay/Entities/Enemies/NavAgents/Core

5 types · 478 LOC (non-blank) · depends on: [Gameplay/Entities/Enemies/NavAgents](Gameplay_Entities_Enemies_NavAgents.md) ×2, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×2, [Core](Core.md) ×1, [Core/Pool](Core_Pool.md) ×1, [Core/Save/DayReset](Core_Save_DayReset.md) ×1, [Gameplay/Entities/Enemies](Gameplay_Entities_Enemies.md) ×1, [Gameplay/Entities/Enemies/Behaviours](Gameplay_Entities_Enemies_Behaviours.md) ×1, [Gameplay/Entities/Enemies/Brain](Gameplay_Entities_Enemies_Brain.md) ×1, [Gameplay/Entities/Enemies/Brain/Interfaces](Gameplay_Entities_Enemies_Brain_Interfaces.md) ×1

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
    ... +9 more methods()
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
| EnemyFacingController | class | `EnemyFacingController.cs:7` | 109 |
| EnemyNavAgentBase | MB, abstract | `EnemyNavAgentBase.cs:7` | 242 |
| EnemyNavPathfinder | class | `EnemyNavPathfinder.cs:8` | 106 |
| EnemyVisionHelper | static | `EnemyVisionHelper.cs:6` | 21 |
| FacingTurnMode | enum | `EnemyFacingController.cs:33` | 5 |
