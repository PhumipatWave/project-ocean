# Gameplay/Entities/Enemies/NavAgents

3 types · 162 LOC (non-blank) · depends on: [Gameplay/Entities/Enemies/Brain](Gameplay_Entities_Enemies_Brain.md) ×3, [Gameplay/Entities/Enemies/Brain/Interfaces](Gameplay_Entities_Enemies_Brain_Interfaces.md) ×2, [Gameplay/Entities/Enemies/NavAgents/Core](Gameplay_Entities_Enemies_NavAgents_Core.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class CharacterFacing2D {
    <<static>>
    +RecheckVisualRoot(Transform, Transform)$ Transform
    +IsFacingRight(Transform, Transform)$ bool
    +ApplyFlip(Transform, Transform, bool)$ void
  }
  class EnemyAgent {
    <<MB>>
    +EnemyBaseProfile BaseProfile
    #bool requiresNavMesh
    +float AutoReturnDelay
    #GetFishId() string
    #CreateBrain() IEnemyBrain
    #OnTick() void
    #HandlePlayerCollision() void
  }
  class FishAlertUI {
    <<MB>>
    +SetSeeingPlayer(bool) void
    +ShowAttackWarning() void
    +ShowFleeWarning() void
    +HideAttackWarning() void
    +HideAll() void
  }
  class EnemyNavAgentBase {
    <<Gameplay/Entities/Enemies/NavAgents/Core>>
  }
  EnemyNavAgentBase <|-- EnemyAgent
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| CharacterFacing2D | static | `CharacterFacing2D.cs:9` | 33 |
| EnemyAgent | MB | `EnemyAgent.cs:6` | 28 |
| FishAlertUI | MB | `FishAlertUI.cs:7` | 101 |
