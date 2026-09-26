# Gameplay/Entitys/Players

8 types · 925 LOC (non-blank) · depends on: [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×9, [Core](Core.md) ×6, [UI/Items](UI_Items.md) ×4, [Gameplay/Entitys](Gameplay_Entitys.md) ×4, [Core/Services](Core_Services.md) ×3, [Gameplay/Entitys/Players/OxygenTube](Gameplay_Entitys_Players_OxygenTube.md) ×3, [Core/Datas](Core_Datas.md) ×3, [UI/Upgrads](UI_Upgrads.md) ×2, [UI/Inventory](UI_Inventory.md) ×2, [Gameplay/InteractObjects](Gameplay_InteractObjects.md) ×2, [UI/VFX](UI_VFX.md) ×2, [Gameplay/AudioSetting](Gameplay_AudioSetting.md) ×2, [Gameplay/Projectiles](Gameplay_Projectiles.md) ×2, [Core/Inputs](Core_Inputs.md) ×1, [Core/Save Game](Core_Save_Game.md) ×1, [Gameplay/MainCameras](Gameplay_MainCameras.md) ×1, [Gameplay/RopeSystems](Gameplay_RopeSystems.md) ×1

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
    +InputReader input
    +PlayerGunHandler GunHandler
    +OxygenRope OxygenTube
    +int CheckpointCount
    +bool CanPlaceCheckpoint
    +CheckPointItem LastCheckpoint
    +Rigidbody2D RB
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
    #OnDamagedHandler() void
    #OnDamagedVisual() void
    #OnDeathHandler() void
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
  }
  class PlayerStabAttack {
    <<MB>>
  }
  class QuestCompass {
    <<MB>>
  }
  class Entity {
    <<Gameplay/Entitys>>
  }
  class EntityHealth {
    <<Gameplay/Entitys>>
  }
  DevPanel --> DevItemButton : devItemButtons
  Entity <|-- Player
  EntityHealth <|-- PlayerHealth
  Player --> PlayerGunHandler : gunHandler
  PlayerGunHandler --> Player : player
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| Player | InputReader | OnAttacked |
| Player | InputReader | OnInteracted |
| Player | InputReader | OnSprinted |
| Player | InputReader | OnTeleport |
| Player | InputReader | OnUseItem |
| PlayerMoney | QuestManager | OnMoneyRewarded |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DevItemButton | struct | `DevPanel.cs:18` | 6 |
| DevPanel | MB | `DevPanel.cs:16` | 163 |
| Player | MB | `Player.cs:12` | 403 |
| PlayerGunHandler | MB | `PlayerGunHandler.cs:7` | 222 |
| PlayerHealth | MB | `PlayerHealth.cs:5` | 64 |
| PlayerMoney | MB | `PlayerMoney.cs:5` | 30 |
| PlayerStabAttack | MB | `PlayerStabAttack.cs:6` | 18 |
| QuestCompass | MB | `QuestCompass.cs:20` | 25 |
