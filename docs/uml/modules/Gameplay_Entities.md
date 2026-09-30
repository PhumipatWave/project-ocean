# Gameplay/Entities

7 types · 279 LOC (non-blank) · depends on: [Core/Pool](Core_Pool.md) ×2

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class AnimHash {
    <<static>>
    +int TakeDamage$
    +int IsDeath$
    +int IsAttack$
    +int VelX$
    +int VelY$
    +int IsAim$
    +int IsLand$
    +int Stab$
  }
  class Entity {
    <<MB>>
    <<abstract>>
    #EntityAnimation entityAnimation
    #EntityDeathHandler entityDeathHandler
    #Rigidbody2D rb
    #EntityHealth entityHealth
    #IEntityHealth healthSource
    +float CurrentHealth
    +float MaxHealth
    +bool IsDead
    #OnDamagedHandler()* void
    #OnDeathHandler() void
    #OnDamagedVisual() void
    #ProvideHealthSource() IEntityHealth
    #Awake() void
    #OnDestroy() void
    +TakeDamage(float) void
    +Heal(float) void
    #StopMovement() void
  }
  class EntityAnimation {
    <<MB>>
    +PlayAnim(int) void
    +PlayAnim(int, int) void
    +PlayAnim(int, float) void
    +PlayAnim(int, bool) void
  }
  class EntityDeathHandler {
    <<MB>>
    +OnGetFromPool() void
    +OnReturnToPool() void
  }
  class EntityHealth {
    <<MB>>
    +event Action OnDamaged
    +event Action OnDied
    +float CurrentHealth
    +float MaxHealth
    +bool IsDead
    #RaiseOnDamaged() void
    #RaiseOnDied() void
    #SetCurrentHealth(float) void
    +Initialize() void
    +TakeDamage(float) void
    +Heal(float) void
    #FlashHitColor() void
    +OnGetFromPool() void
    +OnReturnToPool() void
  }
  class IDamagePopup {
    <<interface>>
    +ShowDamagePoint(string)* void
    +ShowDeath()* void
  }
  class IEntityHealth {
    <<interface>>
    +float CurrentHealth
    +float MaxHealth
    +bool IsDead
    +event Action OnDamaged
    +event Action OnDied
    +TakeDamage(float)* void
    +Heal(float)* void
    +Initialize()* void
  }
  class IPoolable {
    <<Core/Pool>>
  }
  Entity --> EntityAnimation : entityAnimation
  Entity --> EntityDeathHandler : entityDeathHandler
  Entity --> EntityHealth : entityHealth
  Entity --> IEntityHealth : healthSource
  IEntityHealth <|.. EntityHealth
  IPoolable <|.. EntityDeathHandler
  IPoolable <|.. EntityHealth
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| Entity | IEntityHealth | OnDamaged |
| Entity | IEntityHealth | OnDied |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| AnimHash | static | `AnimHash.cs:7` | 11 |
| Entity | MB, abstract | `Entity.cs:7` | 78 |
| EntityAnimation | MB | `EntityAnimation.cs:6` | 43 |
| EntityDeathHandler | MB | `EntityDeathHandler.cs:6` | 56 |
| EntityHealth | MB | `EntityHealth.cs:9` | 75 |
| IDamagePopup | interface | `IDamagePopup.cs:6` | 5 |
| IEntityHealth | interface | `IEntityHealth.cs:6` | 11 |
