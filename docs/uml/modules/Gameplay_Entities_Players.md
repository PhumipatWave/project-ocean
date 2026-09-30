# Gameplay/Entities/Players

12 types · 871 LOC (non-blank) · depends on: [Core](Core.md) ×10, [Gameplay/AudioSetting](Gameplay_AudioSetting.md) ×6, [Gameplay/Entities](Gameplay_Entities.md) ×6, [Core/Events](Core_Events.md) ×5, [Core/Datas/Upgrades](Core_Datas_Upgrades.md) ×4, [Core/Save](Core_Save.md) ×4, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×4, [Gameplay/Services](Gameplay_Services.md) ×4, [Core/Datas/Items](Core_Datas_Items.md) ×3, [Gameplay/Entities/Players/OxygenTube](Gameplay_Entities_Players_OxygenTube.md) ×3, [Core/Datas](Core_Datas.md) ×2, [Core/Inputs](Core_Inputs.md) ×2, [Core/Scenes](Core_Scenes.md) ×1, [Gameplay/Inventory](Gameplay_Inventory.md) ×1, [Gameplay/Items](Gameplay_Items.md) ×1, [Gameplay/MainCameras](Gameplay_MainCameras.md) ×1, [Gameplay/Projectiles](Gameplay_Projectiles.md) ×1, [Gameplay/RopeSystems](Gameplay_RopeSystems.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class DevPanel {
    <<MB>>
  }
  class DevItemButton {
    <<struct>>
    +Button button
    +Item item
    +int amount
  }
  class Player {
    <<MB>>
    +event System.Action OnPlayerDamaged$
    +InputReader input
    +PlayerGunHandler GunHandler
    +OxygenRope OxygenTube
    +PlayerMovement Movement
    +Rigidbody2D RB
    +GameObject PlayerSprite
    +int FaceDir
    +float DashSpeed
    #Awake() void
    #ProvideHealthSource() IEntityHealth
    #OnDestroy() void
    #OnDamagedHandler() void
    #OnDamagedVisual() void
  }
  class PlayerGunHandler {
    <<MB>>
    +LineRenderer Line
    +bool IsHooked
    +bool IsPullBack
    +Transform GunSprite
    +GameObject FirePoint
    +Transform CurrentBulletObj
    +GameObject CurrentHookObj
    +Vector2 AimDir
    +bool IsAimming
    +bool CanAttack
    ... +2 more fields
    +ResetGunRotation(int) void
    +Aimming(ref int, Vector2) void
    +HandleAttackInput(bool) void
    +ResetAim() void
    +Shooting() void
    +AttachRope(GameObject) void
  }
  class PlayerHealth {
    <<MB>>
    +bool IsInvincible
    +SetOxygenRope(OxygenRope) void
    +Initialize() void
    +ResetToOriginal() void
    +IncreaseOriginalMaxLength(float) void
    +SetOriginalMaxLength(float) void
    +TakeDamage(float) void
    +Heal(float) void
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class PlayerMoney {
    <<MB>>
    +event System.Action~int~ OnMoneyChanged$
    +event System.Action~int~ OnMoneyDelta$
    +PlayerMoney Current$
    +int Money
    +int CurrentMoney
    +AddMoney(int) void
    +SetMoneySilent(int) void
    +Use(int) bool
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class PlayerMovement {
    <<MB>>
    +bool IsLand
    +int FaceDir
    +float MoveSpeed
    +float DashSpeed
    +float SprintSpeed
    +SetLand(bool) void
    +Stop() void
  }
  class PlayerRegistry {
    <<static>>
    +Player Local$
    +Register(Player)$ void
    +Unregister(Player)$ void
    +GetNearest(Vector2)$ Player
  }
  class PlayerSceneMode {
    <<MB>>
  }
  class PlayerSprintVFX {
    <<MB>>
    +SetDirection(Vector2) void
    +SetSprinting(bool) void
  }
  class PlayerStabAttack {
    <<MB>>
  }
  class QuestCompass {
    <<MB>>
  }
  class Entity {
    <<Gameplay/Entities>>
  }
  class EntityHealth {
    <<Gameplay/Entities>>
  }
  class ISaveParticipant {
    <<Core/Save>>
  }
  DevPanel --> DevItemButton : devItemButtons
  Entity <|-- Player
  EntityHealth <|-- PlayerHealth
  ISaveParticipant <|.. PlayerHealth
  ISaveParticipant <|.. PlayerMoney
  Player --> PlayerGunHandler : gunHandler
  Player --> PlayerMovement : movement
  PlayerGunHandler --> Player : player
  PlayerMovement --> Player : player
  PlayerMovement --> PlayerSprintVFX : playerSprintVFX
  PlayerRegistry --> Player : Local
  PlayerSceneMode --> Player : player
  PlayerSceneMode --> PlayerHealth : playerHealth
  PlayerSceneMode --> PlayerMovement : movement
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| Player | InputReader | OnAttacked |
| Player | InputReader | OnUseItem |
| PlayerHealth | UpgradeService | OnLevelGranted |
| PlayerMoney | RewardEvents | OnMoneyRewarded |
| PlayerMovement | InputReader | OnSprinted |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DevItemButton | struct | `DevPanel.cs:19` | 6 |
| DevPanel | MB | `DevPanel.cs:17` | 163 |
| Player | MB | `Player.cs:7` | 62 |
| PlayerGunHandler | MB | `PlayerGunHandler.cs:9` | 260 |
| PlayerHealth | MB | `PlayerHealth.cs:6` | 88 |
| PlayerMoney | MB | `PlayerMoney.cs:6` | 41 |
| PlayerMovement | MB | `PlayerMovement.cs:7` | 99 |
| PlayerRegistry | static | `PlayerRegistry.cs:6` | 18 |
| PlayerSceneMode | MB | `PlayerSceneMode.cs:8` | 49 |
| PlayerSprintVFX | MB | `PlayerSprintVFX.cs:6` | 48 |
| PlayerStabAttack | MB | `PlayerStabAttack.cs:8` | 18 |
| QuestCompass | MB | `QuestCompass.cs:21` | 25 |
