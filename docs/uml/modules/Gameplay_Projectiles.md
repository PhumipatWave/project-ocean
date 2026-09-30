# Gameplay/Projectiles

5 types · 222 LOC (non-blank) · depends on: [Gameplay/Entities](Gameplay_Entities.md) ×3, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×3, [Core/Events](Core_Events.md) ×2, [Core](Core.md) ×1, [Core/Datas/Upgrades](Core_Datas_Upgrades.md) ×1, [Gameplay/RopeSystems](Gameplay_RopeSystems.md) ×1, [Gameplay/Services](Gameplay_Services.md) ×1

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
  class GrapplingHookBullet {
    <<MB>>
    +bool AttachedToObject
    #FixedUpdate() void
    #OnTriggerEnter2D(Collider2D) void
    +HookDashRoutine(Vector2) IEnumerator
  }
  class GrenadeBullet {
    <<MB>>
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
  Bullet <|-- GrapplingHookBullet
  Bullet <|-- GrenadeBullet
  Bullet <|-- NormalBullet
  Bullet <|-- TranquilizerBullet
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| Bullet | MB, abstract | `Bullet.cs:7` | 69 |
| GrapplingHookBullet | MB | `GrapplingHookBullet.cs:7` | 99 |
| GrenadeBullet | MB | `GrenadeBullet.cs:5` | 1 |
| NormalBullet | MB | `NormalBullet.cs:6` | 52 |
| TranquilizerBullet | MB | `TranquilizerBullet.cs:5` | 1 |
