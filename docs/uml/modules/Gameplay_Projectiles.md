# Gameplay/Projectiles

5 types · 226 LOC (non-blank) · depends on: [Gameplay/Entities/Enemies](Gameplay_Entities_Enemies.md) ×2, [Core/Events](Core_Events.md) ×2, [Gameplay/Game](Gameplay_Game.md) ×2, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×1, [Gameplay/Entities/Enemies/Brain/Interfaces](Gameplay_Entities_Enemies_Brain_Interfaces.md) ×1, [Gameplay/Entities](Gameplay_Entities.md) ×1, [Gameplay/RopeSystems](Gameplay_RopeSystems.md) ×1, [Gameplay/Services](Gameplay_Services.md) ×1, [Core/Datas/Upgrades](Core_Datas_Upgrades.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class Bullet {
    <<MB>>
    <<abstract>>
    #Rigidbody2D rb
    #Player player
    #float moveSpeed
    #float attackDamage
    #GameObject vfxObject
    #bool isHit
    #Vector2 shootDir
    +float MaxRange
    #Awake() void
    #Start() void
    +Initialize(Vector2) void
    #FixedUpdate() void
    #OnTriggerEnter2D(Collider2D) void
    #HideVisuals() void
    #BulletDirection() void
    #ShootCooldownRoutine() IEnumerator
  }
  class GranadeBullet {
    <<MB>>
  }
  class GrapplingHookBullet {
    <<MB>>
    +bool AttachedToObject
    #FixedUpdate() void
    #OnTriggerEnter2D(Collider2D) void
    +HookDashRoutine(Vector2) IEnumerator
  }
  class NormalBullet {
    <<MB>>
    +float MaxRange
    +RangeAt(int) float
    #Awake() void
    +GetDamage(int)$ float
  }
  class TranquilizerBullet {
    <<MB>>
  }
  Bullet <|-- GranadeBullet
  Bullet <|-- GrapplingHookBullet
  Bullet <|-- NormalBullet
  Bullet <|-- TranquilizerBullet
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| Bullet | MB, abstract | `Bullet.cs:7` | 72 |
| GranadeBullet | MB | `GranadeBullet.cs:5` | 1 |
| GrapplingHookBullet | MB | `GrapplingHookBullet.cs:7` | 100 |
| NormalBullet | MB | `NormalBullet.cs:6` | 52 |
| TranquilizerBullet | MB | `TranquilizerBullet.cs:5` | 1 |
