# Metrics

[← index](README.md)

Generated. Coupling = distinct project types referenced (associations + usages).

## Largest types

| Type | LOC | Methods | Fields | Deps (fan-out) | Module |
|---|---|---|---|---|---|
| QuestStepDataDrawer | 528 | 10 | 2 | 5 | Editor |
| OxygenRope | 370 | 14 | 31 | 9 | Gameplay/Entities/Players/OxygenTube |
| UIManager | 352 | 31 | 11 | 9 | UI/UIManager |
| KeyBindManager | 339 | 12 | 4 | 5 | Core/Inputs/KeyBind |
| Player | 333 | 19 | 35 | 24 | Gameplay/Entities/Players |
| QuestManager | 333 | 34 | 7 | 19 | Gameplay/QuestSystems/Quest |
| InventoryManager | 319 | 43 | 25 | 29 | UI/Inventory |
| ShopManager | 287 | 37 | 19 | 15 | UI/Shop |
| UpgradeTooltip | 279 | 8 | 14 | 5 | UI/Upgrades |
| QuestBoardUI | 249 | 15 | 18 | 11 | UI/Quest |
| NPC | 243 | 21 | 18 | 17 | Gameplay/Entities/NPCs |
| EnemyNavAgentBase | 228 | 26 | 29 | 12 | Gameplay/Entities/Enemies/NavAgents/Core |
| EnemyFollowState | 223 | 11 | 16 | 9 | Gameplay/Entities/Enemies/Brain/FSM/States |
| PlayerGunHandler | 222 | 13 | 34 | 8 | Gameplay/Entities/Players |
| EnemySpawner | 209 | 12 | 17 | 11 | Gameplay/Spawning |
| SaveBridge | 192 | 15 | 4 | 22 | Gameplay/Save |
| GameHUD | 186 | 15 | 16 | 19 | UI |
| ObjectWrapPoint | 182 | 10 | 7 | 1 | Gameplay/Entities/Players/OxygenTube |
| LinearPatrolBrain | 179 | 19 | 18 | 8 | Gameplay/Entities/Enemies/Brain/LinearPatrol |
| QuestProgressTracker | 179 | 12 | 3 | 18 | Gameplay/QuestSystems/Quest |

## Most depended-on (fan-in)

| Type | Used by | Examples |
|---|---|---|
| TutorialActionId | 27 | Bullet, DayManager, DoorInteract, GameHUD, GameManager |
| PlayerRegistry | 24 | Bullet, ChestInteract, Cutscene, DevPanel, DoorInteract |
| QuestManager | 20 | CutsceneManager, DevPanel, DoorInteract, ExploreAreaTrigger, FeatureUnlockService |
| GameLog | 19 | DayManager, Enemy, EnemyNavAgentBase, EnemySpawner, EnemyVision |
| Item | 18 | DevItemButton, DevPanel, DraggableItem, EnemyLoot, GameObjectSpawnManager |
| QuestType | 18 | DoorInteract, ExploreAreaTrigger, FieldVisibility, NPC, PlayerSelfDialogueUI |
| Quest | 16 | CutsceneManager, IQuestOfferView, NPC, PlayerSelfDialogueUI, QuestBoardUI |
| PlayerUIState | 16 | CollectableItemBase, ConfirmationDialog, Cutscene, CutsceneManager, EscClosablePanel |
| TutorialEvents | 15 | Bullet, DayManager, DoorInteract, GameManager, InteractUI |
| NotificationManager | 15 | CutsceneSkipHint, GameManager, InteractUI, InventoryManager, LandUI |
| EnemyBrainContext | 15 | EnemyAttackState, EnemyBrain, EnemyBrainBase, EnemyFSM, EnemyFleeState |
| Inventory | 14 | DevPanel, EnemyLoot, InventoryManager, ItemDrop, Player |
| SaveData | 12 | DayManager, EnemyKillTracker, FeatureUnlockService, ISaveParticipant, InventoryManager |
| Player | 12 | AmmoWheel, Bullet, DevPanel, Enemy, EnemyBrainContext |
| PlayerMoney | 12 | ChestInteract, DevPanel, MoneyUI, QuestProgressTracker, QuestStepEffects |

## Most dependencies (fan-out)

| Type | Depends on |
|---|---|
| InventoryManager | 29 |
| Player | 24 |
| SaveBridge | 22 |
| EnemyBrain | 19 |
| QuestManager | 19 |
| GameHUD | 19 |
| QuestProgressTracker | 18 |
| NPC | 17 |
| GameManager | 17 |
| TutorialPromptUI | 15 |
| ShopManager | 15 |
| EnemyWanderState | 14 |
| PlayerSelfDialogueUI | 13 |
| Enemy | 12 |
| EnemyNavAgentBase | 12 |

## Static `Instance` singletons (23)

AudioManager, CameraShakeManager, ConfirmationDialog, CutsceneManager, DayManager, EnemyKillTracker, FeatureUnlockService, GameManager, GameObjectSpawnManager, InventoryManager, KeyBindManager, LoadingManager, NPCManager, ObjectPoolManager, OneTimePickupTracker, QuestCompleteUI, QuestManager, SaveBridge, ShopInventoryBuy, ShopManager, UIManager, UpgradeService, UpgradeTooltip

## Scene searches (`Find*`) — 9 in 6 files

| File | Count |
|---|---|
| Gameplay/Entities/Players/Player.cs | 3 |
| Gameplay/Game/GameManager.cs | 2 |
| Gameplay/Environments/DoorInteract.cs | 1 |
| Gameplay/Environments/NPCManager.cs | 1 |
| Gameplay/MainCameras/PlayerCamera.cs | 1 |
| UI/UIManager/UIManager.cs | 1 |

## `Debug.Log*` calls — 85 in 35 files

| File | Count |
|---|---|
| Core/Save/SaveSystem.cs | 7 |
| Gameplay/Save/SaveBridge.cs | 7 |
| Core/Inputs/KeyBind/KeyBindManager.cs | 6 |
| Gameplay/Spawning/GameObjectSpawnManager.cs | 5 |
| Gameplay/Cutscenes/Cutscene.cs | 4 |
| Gameplay/Entities/Players/DevPanel.cs | 4 |
| Gameplay/QuestSystems/Quest/QuestManager.cs | 4 |
| Gameplay/Spawning/EnemySpawner.cs | 4 |
| Gameplay/Spawning/ItemSpawner.cs | 4 |
| UI/Loading/LoadingButton.cs | 4 |
| Core/Scenes/LoadingManager.cs | 3 |
| Gameplay/Inventory/InventorySaveMapper.cs | 3 |
| Gameplay/AudioSetting/SoundChannel.cs | 2 |
| Gameplay/Entities/Enemies/Brain/FSM/EnemyFSM.cs | 2 |
| Gameplay/Game/GameManager.cs | 2 |
| Gameplay/QuestSystems/Quest/ExploreAreaTrigger.cs | 2 |
| UI/Inventory/InventoryManager.cs | 2 |
| UI/Save/SaveMenuUI.cs | 2 |
| UI/UIManager/UIManager.cs | 2 |
| Core/GameLog.cs | 1 |
| Core/VideoManager/IntroPlayer.cs | 1 |
| Gameplay/Cutscenes/CutsceneManager.cs | 1 |
| Gameplay/Entities/Enemies/Brain/EnemyBrain.cs | 1 |
| Gameplay/Entities/Enemies/Brain/EnemyVision.cs | 1 |
| Gameplay/Entities/Enemies/NavAgents/Core/EnemyNavAgentBase.cs | 1 |

## Per-frame methods (`Update`/`FixedUpdate`/`LateUpdate`) — 33 in 31 files

| File | Count |
|---|---|
| Gameplay/Entities/Players/OxygenTube/OxygenRope.cs | 2 |
| Gameplay/Entities/Players/Player.cs | 2 |
| Core/VideoManager/IntroPlayer.cs | 1 |
| Gameplay/AudioSetting/AudioManager.cs | 1 |
| Gameplay/Entities/Enemies/Enemy.cs | 1 |
| Gameplay/Entities/Enemies/NavAgents/Core/EnemyNavAgentBase.cs | 1 |
| Gameplay/Entities/NPCs/NPC.cs | 1 |
| Gameplay/Entities/Players/DevPanel.cs | 1 |
| Gameplay/Entities/Players/PlayerGunHandler.cs | 1 |
| Gameplay/Entities/Players/QuestCompass.cs | 1 |
| Gameplay/Environments/Bird.cs | 1 |
| Gameplay/Environments/DoorInteract.cs | 1 |
| Gameplay/Interaction/InteractUI.cs | 1 |
| Gameplay/Items/Pickups/CollectableItemBase.cs | 1 |
| Gameplay/Projectiles/Bullet.cs | 1 |
| Gameplay/Projectiles/GrapplingHookBullet.cs | 1 |
| Gameplay/QuestSystems/Quest/ExploreAreaTrigger.cs | 1 |
| Gameplay/QuestSystems/Quest/TutorialActionMarker.cs | 1 |
| Gameplay/QuestSystems/Quest/TutorialPromptUI.cs | 1 |
| UI/AmmoWheel.cs | 1 |
| UI/ButtonHoverScale.cs | 1 |
| UI/EnemyHealthDisplay.cs | 1 |
| UI/GameHUD.cs | 1 |
| UI/Inventory/InventoryManager.cs | 1 |
| UI/Loading/LoadingTextAnimation/LoadingTextAnimation.cs | 1 |

## Layer violations — 0 type links

Each row points the wrong way for Core ← Gameplay ← UI.

| From module | To module | From type | To type |
|---|---|---|---|

## Dependency cycles (type level)

Types that transitively depend on each other. Small cycles are cheapest to break.

| Size | Types |
|---|---|
| 33 | Boid, Bullet, CollectableItemBase, Cutscene, CutsceneManager, DoorInteract, Enemy, EnemyBrainContext, EnemyLoot, EnemyNavAgentBase, Entity, EntityDeathHandler … +21 |
| 11 | AggressiveEnemyProfile, EnemyAttackState, EnemyBaseProfile, EnemyBrain, EnemyFleeState, EnemyFollowState, EnemyPrepareAttackState, EnemyVision, EnemyWanderState, FriendlyEnemyProfile, ScaredEnemyProfile |
| 4 | EnemySpawner, GameObjectSpawnManager, ItemDrop, ItemSpawner |
| 4 | DraggableItem, InventoryManager, InventorySlot, SlotBuilder |
| 2 | EnemyFSM, IEnemyState |
| 2 | EnemySearchState, EnemyStateBase |
| 2 | UIManager, UIWindow |
| 2 | ShopBuySlot, ShopManager |
| 2 | LinearPatrolBrain, LinearPatrolProfile |
| 2 | SaveMenuUI, SaveSlotUI |

Modules in the largest cycle (16): Gameplay/Cutscenes, Gameplay/Entities, Gameplay/Entities/Enemies, Gameplay/Entities/Enemies/Behaviours, Gameplay/Entities/Enemies/Brain, Gameplay/Entities/Enemies/Brain/Interfaces, Gameplay/Entities/Enemies/NavAgents/Core, Gameplay/Entities/NPCs, Gameplay/Entities/Players, Gameplay/Environments, Gameplay/Interaction, Gameplay/Items, Gameplay/Items/Pickups, Gameplay/Projectiles, Gameplay/QuestSystems/Quest, Gameplay/Services

## Plain types with no code reference

Non-MonoBehaviour, non-ScriptableObject types nothing in code refers to. MB/SO usage is checked against scene/prefab GUIDs separately.

none
