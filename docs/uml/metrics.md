# Metrics

[← index](README.md)

Generated. Coupling = distinct project types referenced (associations + usages).

## Largest types

| Type | LOC | Methods | Fields | Deps (fan-out) | Module |
|---|---|---|---|---|---|
| InventoryManager | 537 | 47 | 20 | 26 | UI/Inventory |
| OxygenRope | 536 | 25 | 33 | 7 | Gameplay/Entities/Players/OxygenTube |
| QuestStepDataDrawer | 528 | 10 | 2 | 5 | Editor |
| ObjectWrapPoint | 467 | 27 | 7 | 1 | Gameplay/InteractObjects |
| Player | 397 | 26 | 37 | 25 | Gameplay/Entities/Players |
| UIManager | 368 | 35 | 11 | 8 | UI/UIManager |
| KeyBindManager | 339 | 12 | 4 | 4 | Core/Inputs/KeyBind |
| QuestManager | 334 | 34 | 7 | 17 | Gameplay/QuestSystems/Quest |
| ShopManager | 299 | 37 | 18 | 13 | UI/Shop |
| UpgradeTooltip | 279 | 8 | 14 | 5 | UI/Upgrades |
| QuestBoardUI | 249 | 15 | 18 | 10 | UI/Quest |
| NPC | 243 | 21 | 18 | 17 | Gameplay/Entities/NPCs |
| EnemyNavAgentBase | 228 | 26 | 29 | 11 | Gameplay/Entities/Enemies/NavAgents/Core |
| EnemyFollowState | 223 | 11 | 16 | 9 | Gameplay/Entities/Enemies/Brain/FSM/States |
| PlayerGunHandler | 222 | 13 | 34 | 8 | Gameplay/Entities/Players |
| EnemySpawner | 209 | 12 | 17 | 10 | Gameplay/Spawning |
| SaveBridge | 192 | 15 | 4 | 21 | Gameplay/Save |
| GameHUD | 186 | 15 | 16 | 17 | UI |
| LinearPatrolBrain | 179 | 19 | 18 | 8 | Gameplay/Entities/Enemies/Brain/LinearPatrol |
| QuestProgressTracker | 179 | 12 | 3 | 18 | Gameplay/QuestSystems/Quest |

## Most depended-on (fan-in)

| Type | Used by | Examples |
|---|---|---|
| GameManager | 29 | Bullet, CheckPointItem, ChestInteract, Cutscene, DevPanel |
| TutorialActionId | 26 | Bullet, DayManager, DoorInteract, GameHUD, GameManager |
| QuestManager | 21 | CutsceneManager, DevPanel, DoorInteract, ExploreAreaTrigger, FeatureUnlockService |
| QuestType | 18 | DoorInteract, ExploreAreaTrigger, FieldVisibility, NPC, PlayerSelfDialogueUI |
| Quest | 16 | CutsceneManager, IQuestOfferView, NPC, PlayerSelfDialogueUI, QuestBoardUI |
| PlayerUIState | 16 | CollectableItemBase, ConfirmationDialog, Cutscene, CutsceneManager, EscClosablePanel |
| Item | 15 | DevItemButton, DevPanel, DraggableItem, EnemyLoot, GameObjectSpawnManager |
| NotificationManager | 15 | CutsceneSkipHint, GameManager, InteractUI, InventoryManager, LandUI |
| EnemyBrainContext | 15 | EnemyAttackState, EnemyBrain, EnemyBrainBase, EnemyFSM, EnemyFleeState |
| TutorialEvents | 14 | Bullet, DayManager, DoorInteract, GameManager, InteractUI |
| Inventory | 14 | CheckPointItem, DevPanel, EnemyLoot, InventoryManager, ItemDrop |
| SaveData | 12 | DayManager, EnemyKillTracker, FeatureUnlockService, ISaveParticipant, InventoryManager |
| Player | 12 | AmmoWheel, Bullet, CheckPointItem, DevPanel, Enemy |
| PlayerMoney | 12 | ChestInteract, DevPanel, MoneyUI, QuestProgressTracker, QuestStepEffects |
| GameFeature | 12 | CollectableItemBase, EnemySpawner, FeatureUnlockService, GameHUD, GameManager |

## Most dependencies (fan-out)

| Type | Depends on |
|---|---|
| InventoryManager | 26 |
| Player | 25 |
| SaveBridge | 21 |
| EnemyBrain | 19 |
| QuestProgressTracker | 18 |
| NPC | 17 |
| QuestManager | 17 |
| GameHUD | 17 |
| EnemyWanderState | 14 |
| GameManager | 14 |
| TutorialPromptUI | 14 |
| PlayerSelfDialogueUI | 13 |
| ShopManager | 13 |
| InteractUI | 12 |
| Quest | 12 |

## Static `Instance` singletons (23)

AudioManager, CameraShakeManager, ConfirmationDialog, CutsceneManager, DayManager, EnemyKillTracker, FeatureUnlockService, GameManager, GameObjectSpawnManager, InventoryManager, KeyBindManager, LoadingManager, NPCManager, ObjectPoolManager, OneTimePickupTracker, QuestCompleteUI, QuestManager, SaveBridge, ShopInventoryBuy, ShopManager, UIManager, UpgradeService, UpgradeTooltip

## Scene searches (`Find*`) — 13 in 8 files

| File | Count |
|---|---|
| Gameplay/Entities/Players/Player.cs | 3 |
| Core/Inputs/KeyBind/UIKeyBind/MoveKeyBindUI.cs | 2 |
| Gameplay/Environments/DoorInteract.cs | 2 |
| Gameplay/Game/GameManager.cs | 2 |
| Gameplay/Environments/NPCManager.cs | 1 |
| Gameplay/MainCameras/PlayerCamera.cs | 1 |
| UI/Shop/ShopManager.cs | 1 |
| UI/UIManager/UIManager.cs | 1 |

## `Debug.Log*` calls — 134 in 39 files

| File | Count |
|---|---|
| UI/Inventory/InventoryManager.cs | 15 |
| Core/Inputs/KeyBind/KeyBindManager.cs | 13 |
| Core/Save/SaveSystem.cs | 12 |
| Gameplay/Save/SaveBridge.cs | 11 |
| Gameplay/QuestSystems/Quest/QuestManager.cs | 10 |
| Gameplay/Entities/Players/Player.cs | 5 |
| Gameplay/Spawning/EnemySpawner.cs | 5 |
| Gameplay/Spawning/GameObjectSpawnManager.cs | 5 |
| Gameplay/Cutscenes/Cutscene.cs | 4 |
| Gameplay/Entities/Players/DevPanel.cs | 4 |
| Gameplay/Spawning/ItemSpawner.cs | 4 |
| UI/Loading/LoadingButton.cs | 4 |
| Core/Scenes/LoadingManager.cs | 3 |
| Gameplay/Entities/Enemies/Brain/EnemyVision.cs | 3 |
| UI/Quest/QuestDetailPanel.cs | 3 |
| UI/UIManager/UIManager.cs | 3 |
| Gameplay/AudioSetting/SoundChannel.cs | 2 |
| Gameplay/Entities/Enemies/Brain/FSM/EnemyFSM.cs | 2 |
| Gameplay/Entities/Enemies/NavAgents/Core/EnemyNavAgentBase.cs | 2 |
| Gameplay/Game/GameManager.cs | 2 |
| Gameplay/QuestSystems/Quest/ExploreAreaTrigger.cs | 2 |
| UI/Quest/QuestBoardUI.cs | 2 |
| UI/Save/SaveMenuUI.cs | 2 |
| Core/Save/DayReset/DayManager.cs | 1 |
| Core/VideoManager/IntroPlayer.cs | 1 |

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
| Gameplay/InteractObjects/CollectableItemBase.cs | 1 |
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
| 36 | Boid, Bullet, CheckPointItem, CollectableItemBase, Cutscene, CutsceneManager, DoorInteract, Enemy, EnemyBrainContext, EnemyLoot, EnemyNavAgentBase, Entity … +24 |
| 11 | AggressiveEnemyProfile, EnemyAttackState, EnemyBaseProfile, EnemyBrain, EnemyFleeState, EnemyFollowState, EnemyPrepareAttackState, EnemyVision, EnemyWanderState, FriendlyEnemyProfile, ScaredEnemyProfile |
| 4 | EnemySpawner, GameObjectSpawnManager, ItemDrop, ItemSpawner |
| 4 | DraggableItem, InventoryManager, InventoryQuestService, InventorySlot |
| 2 | EnemyFSM, IEnemyState |
| 2 | EnemySearchState, EnemyStateBase |
| 2 | UIManager, UIWindow |
| 2 | ShopBuySlot, ShopManager |
| 2 | LinearPatrolBrain, LinearPatrolProfile |
| 2 | SaveMenuUI, SaveSlotUI |

Modules in the largest cycle (17): Gameplay/Cutscenes, Gameplay/Entities, Gameplay/Entities/Enemies, Gameplay/Entities/Enemies/Behaviours, Gameplay/Entities/Enemies/Brain, Gameplay/Entities/Enemies/Brain/Interfaces, Gameplay/Entities/Enemies/NavAgents/Core, Gameplay/Entities/NPCs, Gameplay/Entities/Players, Gameplay/Entities/Players/OxygenTube, Gameplay/Environments, Gameplay/Game, Gameplay/InteractObjects, Gameplay/Interaction, Gameplay/Projectiles, Gameplay/QuestSystems/Quest, Gameplay/Services

## Plain types with no code reference

Non-MonoBehaviour, non-ScriptableObject types nothing in code refers to. MB/SO usage is checked against scene/prefab GUIDs separately.

none
