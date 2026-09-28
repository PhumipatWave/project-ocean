# Gameplay/Entities/Enemies/Brain/FSM

3 types · 143 LOC (non-blank) · depends on: [Gameplay/Entities/Enemies/Brain/Interfaces](Gameplay_Entities_Enemies_Brain_Interfaces.md) ×2, [Gameplay/Entities/Enemies/Brain](Gameplay_Entities_Enemies_Brain.md) ×2, [Gameplay/Entities/Enemies/Brain/FSM/States](Gameplay_Entities_Enemies_Brain_FSM_States.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class EnemyFSM {
    +EnemyStateTag CurrentTag
    +IEnemyState CurrentState
    +Initialize(EnemyBrainContext) void
    +Register(IEnemyState) void
    +Start(EnemyStateTag) void
    +TransitionTo(EnemyStateTag) void
    +Tick(float) void
    +IsIn(EnemyStateTag) bool
    +GetState(EnemyStateTag) IEnemyState
    +HasState(EnemyStateTag) bool
  }
  class EnemyStateBase {
    <<abstract>>
    #EnemyBrainContext enemyBrainContext
    #EnemyFSM enemyFSM
    +EnemyStateTag StateTag
    +Enter(EnemyBrainContext, EnemyFSM) void
    +Tick(float)* void
    +Exit() void
    #SetAgentSpeed(float) void
    #StopAgent() void
    #ResumeAgent(float) void
    #RotateFacingToPlayer() bool
    #UpdateDirectionFromAgentVelocity(ref Vector2, float) void
    #RotateTowardsDirection(Vector2) void
    #TransitionToSearchOrWander(Vector3) void
    #ApplyWallAvoidance(Vector2, float, LayerMask) void
  }
  class EnemyStateTag {
    <<enum>>
    Wander
    Follow
    Search
    Prepare
    Attack
    Panic
    Flee
    Flock
  }
  class IEnemyState {
    <<Gameplay/Entities/Enemies/Brain/Interfaces>>
  }
  EnemyFSM --> EnemyStateTag : states
  EnemyStateBase --> EnemyFSM : enemyFSM
  EnemyStateBase --> EnemyStateTag : StateTag
  IEnemyState <|.. EnemyStateBase
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| EnemyFSM | class | `EnemyFSM.cs:7` | 50 |
| EnemyStateBase | abstract | `EnemyStateBase.cs:10` | 82 |
| EnemyStateTag | enum | `EnemyStateTag.cs:10` | 11 |
