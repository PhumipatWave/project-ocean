# Gameplay/Entities/Players

10 types · 913 LOC (non-blank) · depends on: [Core](Core.md) ×6, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×5, [Core/Datas/Upgrades](Core_Datas_Upgrades.md) ×4, [Core/Events](Core_Events.md) ×4, [Core/Save](Core_Save.md) ×4, [Gameplay/Entities](Gameplay_Entities.md) ×4, [Gameplay/Services](Gameplay_Services.md) ×4, [Core/Datas/Items](Core_Datas_Items.md) ×3, [Gameplay/Entities/Players/OxygenTube](Gameplay_Entities_Players_OxygenTube.md) ×3, [Core/Datas](Core_Datas.md) ×2, [Gameplay/AudioSetting](Gameplay_AudioSetting.md) ×2, [Gameplay/Inventory](Gameplay_Inventory.md) ×2, [Core/Inputs](Core_Inputs.md) ×1, [Core/Scenes](Core_Scenes.md) ×1, [Gameplay/Items](Gameplay_Items.md) ×1, [Gameplay/MainCameras](Gameplay_MainCameras.md) ×1, [Gameplay/Projectiles](Gameplay_Projectiles.md) ×1, [Gameplay/RopeSystems](Gameplay_RopeSystems.md) ×1

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
    +int CheckpointCount
    +bool CanPlaceCheckpoint
    +CheckPointItem LastCheckpoint
    +GameObject PlayerSprite
    +int FaceDir
    +float MoveSpeed
    ... +3 more fields
    +RegisterCheckpoint(CheckPointItem) void
    +UnregisterCheckpoint(CheckPointItem) void
    +ClearAllCheckpoints() void
    +CanPlaceCheckpointFromSlot(int) bool
    #Awake() void
    #ProvideHealthSource() IEntityHealth
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
    ... +1 more fields
    +ResetGunRotation(int) void
    +Aimming(ref int, Vector2) void
    +Shooting() void
    +AttachRope(GameObject) void
  }
  class PlayerHealth {
    <<MB>>
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
    +int Money
    +int CurrentMoney
    +AddMoney(int) void
    +SetMoneySilent(int) void
    +Use(int) bool
    +Collect(SaveData) void
    +Apply(SaveData) void
  }
  class PlayerRegistry {
    <<static>>
    +Player Local$
    +Register(Player)$ void
    +Unregister(Player)$ void
    +GetNearest(Vector2)$ Player
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
  Player --> PlayerSprintVFX : playerSprintVFX
  PlayerGunHandler --> Player : player
  PlayerRegistry --> Player : Local
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| Player | InputReader | OnAttacked |
| Player | InputReader | OnSprinted |
| Player | InputReader | OnTeleport |
| Player | InputReader | OnUseItem |
| PlayerHealth | UpgradeService | OnLevelGranted |
| PlayerMoney | RewardEvents | OnMoneyRewarded |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DevItemButton | struct | `DevPanel.cs:19` | 6 |
| DevPanel | MB | `DevPanel.cs:17` | 163 |
| Player | MB | `Player.cs:12` | 305 |
| PlayerGunHandler | MB | `PlayerGunHandler.cs:8` | 222 |
| PlayerHealth | MB | `PlayerHealth.cs:6` | 82 |
| PlayerMoney | MB | `PlayerMoney.cs:6` | 32 |
| PlayerRegistry | static | `PlayerRegistry.cs:6` | 18 |
| PlayerSprintVFX | MB | `PlayerSprintVFX.cs:6` | 48 |
| PlayerStabAttack | MB | `PlayerStabAttack.cs:8` | 18 |
| QuestCompass | MB | `QuestCompass.cs:21` | 25 |
