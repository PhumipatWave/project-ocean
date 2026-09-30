# Metrics

[← index](README.md)

Generated. Coupling = distinct project types referenced (associations + usages).

## Largest types

| Type | LOC | Methods | Fields | Deps (fan-out) | Module |
|---|---|---|---|---|---|
| QuestStepDataDrawer | 528 | 10 | 2 | 5 | Editor |
| OxygenRope | 370 | 14 | 31 | 9 | Gameplay/Entities/Players/OxygenTube |
| UIManager | 348 | 31 | 11 | 10 | UI/UIManager |
| KeyBindManager | 334 | 12 | 4 | 6 | Core/Inputs/KeyBind |
| QuestManager | 328 | 34 | 7 | 20 | Gameplay/QuestSystems/Quest |
| InventoryManager | 315 | 43 | 23 | 30 | UI/Inventory |
| ShopManager | 280 | 36 | 19 | 15 | UI/Shop |
| UpgradeTooltip | 279 | 8 | 14 | 5 | UI/Upgrades |
| PlayerGunHandler | 260 | 15 | 37 | 11 | Gameplay/Entities/Players |
| QuestBoardUI | 249 | 15 | 18 | 11 | UI/Quest |
| EnemyNavAgentBase | 242 | 27 | 29 | 12 | Gameplay/Entities/Enemies/NavAgents/Core |
| EnemyFollowState | 223 | 11 | 16 | 9 | Gameplay/Entities/Enemies/Brain/FSM/States |
| EnemySpawner | 194 | 12 | 17 | 12 | Gameplay/Spawning |
| GameHUD | 186 | 15 | 16 | 19 | UI |
| SaveBridge | 183 | 14 | 4 | 23 | Gameplay/Save |
| ObjectWrapPoint | 182 | 10 | 7 | 1 | Gameplay/Entities/Players/OxygenTube |
| LinearPatrolBrain | 179 | 19 | 18 | 8 | Gameplay/Entities/Enemies/Brain/LinearPatrol |
| QuestProgressTracker | 179 | 12 | 3 | 17 | Gameplay/QuestSystems/Quest |
| ExploreAreaTrigger | 171 | 17 | 11 | 11 | Gameplay/QuestSystems/Quest |
| DevPanel | 163 | 10 | 18 | 11 | Gameplay/Entities/Players |

## Most depended-on (fan-in)

| Type | Used by | Examples |
|---|---|---|
| TutorialActionId | 27 | Bullet, DayManager, DoorQuestMarker, GameHUD, GameManager |
| Player | 21 | AmmoWheel, Bullet, ChestInteract, CollectableItemBase, DevPanel |
| PlayerRegistry | 20 | Bullet, Cutscene, DevPanel, Enemy, EnemyLoot |
| QuestManager | 20 | AcceptedQuestListUI, CutsceneManager, DevPanel, ExploreAreaTrigger, FeatureUnlockService |
| GameLog | 19 | DayManager, Enemy, EnemyNavAgentBase, EnemySpawner, EnemyVision |
| Item | 18 | DevItemButton, DevPanel, DraggableItem, EnemyLoot, IInventory |
| Singleton | 18 | AudioManager, CameraShakeManager, ConfirmationDialog, CutsceneManager, DayManager |
| Quest | 17 | AcceptedQuestSlot, CutsceneManager, IQuestOfferView, NPC, NpcQuestConversation |
| PlayerUIState | 16 | ConfirmationDialog, Cutscene, CutsceneManager, EscClosablePanel, GameHUD |
| NotificationManager | 15 | CutsceneSkipHint, GameManager, Interactable, InventoryManager, LandUI |
| QuestType | 15 | ExploreAreaTrigger, FieldVisibility, NpcQuestMarker, PlayerSelfDialogueUI, Quest |
| EnemyBrainContext | 15 | EnemyAttackState, EnemyBrain, EnemyBrainBase, EnemyFSM, EnemyFleeState |
| TutorialEvents | 14 | Bullet, DayManager, DoorQuestMarker, GameManager, Interactable |
| Inventory | 13 | DevPanel, EnemyLoot, InventoryManager, ItemDrop, PlayerItemUse |
| SaveData | 12 | DayManager, EnemyKillTracker, FeatureUnlockService, ISaveParticipant, InventoryManager |

## Most dependencies (fan-out)

| Type | Depends on |
|---|---|
| InventoryManager | 30 |
| SaveBridge | 23 |
| QuestManager | 20 |
| EnemyBrain | 19 |
| GameHUD | 19 |
| GameManager | 18 |
| QuestProgressTracker | 17 |
| Player | 16 |
| TutorialPromptUI | 15 |
| ShopManager | 15 |
| EnemyWanderState | 14 |
| NPC | 14 |
| PlayerMovement | 14 |
| PlayerSelfDialogueUI | 13 |
| Enemy | 12 |

## Static `Instance` singletons (6)

FeatureUnlockService, ShopInventoryBuy, ShopManager, Singleton, UpgradeService, UpgradeTooltip

## Scene searches (`Find*`) — 8 in 5 files

| File | Count |
|---|---|
| Gameplay/Entities/Players/PlayerSceneMode.cs | 3 |
| Gameplay/Game/GameManager.cs | 2 |
| Gameplay/Environments/NPCManager.cs | 1 |
| Gameplay/MainCameras/PlayerCamera.cs | 1 |
| UI/UIManager/UIManager.cs | 1 |

## `Debug.Log*` calls — 85 in 35 files

| File | Count |
|---|---|
| Core/Save/SaveSystem.cs | 7 |
| Gameplay/Save/SaveBridge.cs | 7 |
| Core/Inputs/KeyBind/KeyBindManager.cs | 6 |
| Gameplay/Spawning/SpawnCatalog.cs | 5 |
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

## Per-frame methods (`Update`/`FixedUpdate`/`LateUpdate`) — 31 in 30 files

| File | Count |
|---|---|
| Gameplay/Entities/Players/OxygenTube/OxygenRope.cs | 2 |
| Core/VideoManager/IntroPlayer.cs | 1 |
| Gameplay/AudioSetting/AudioManager.cs | 1 |
| Gameplay/Entities/Enemies/Enemy.cs | 1 |
| Gameplay/Entities/Enemies/NavAgents/Core/EnemyNavAgentBase.cs | 1 |
| Gameplay/Entities/NPCs/NPC.cs | 1 |
| Gameplay/Entities/Players/DevPanel.cs | 1 |
| Gameplay/Entities/Players/Player.cs | 1 |
| Gameplay/Entities/Players/PlayerGunHandler.cs | 1 |
| Gameplay/Entities/Players/PlayerMovement.cs | 1 |
| Gameplay/Entities/Players/QuestCompass.cs | 1 |
| Gameplay/Environments/Bird.cs | 1 |
| Gameplay/Interaction/PlayerInteractor.cs | 1 |
| Gameplay/Projectiles/Bullet.cs | 1 |
| Gameplay/Projectiles/GrapplingHookBullet.cs | 1 |
| Gameplay/QuestSystems/Quest/ExploreAreaTrigger.cs | 1 |
| Gameplay/QuestSystems/Quest/QuestMarkerBase.cs | 1 |
| Gameplay/QuestSystems/Quest/TutorialPromptUI.cs | 1 |
| UI/AmmoWheel.cs | 1 |
| UI/ButtonHoverScale.cs | 1 |
| UI/EnemyHealthDisplay.cs | 1 |
| UI/GameHUD.cs | 1 |
| UI/Inventory/InventoryManager.cs | 1 |
| UI/KeepUpright.cs | 1 |
| UI/Loading/LoadingTextAnimation/LoadingTextAnimation.cs | 1 |

## Layer violations — 0 type links

Each row points the wrong way for Core ← Gameplay ← UI.

| From module | To module | From type | To type |
|---|---|---|---|

## Dependency cycles (type level)

Types that transitively depend on each other. Small cycles are cheapest to break.

| Size | Types |
|---|---|
| 11 | AggressiveEnemyProfile, EnemyAttackState, EnemyBaseProfile, EnemyBrain, EnemyFleeState, EnemyFollowState, EnemyPrepareAttackState, EnemyVision, EnemyWanderState, FriendlyEnemyProfile, ScaredEnemyProfile |
| 10 | Cutscene, CutsceneManager, DoorQuestMarker, NPC, NPCManager, NpcQuestConversation, NpcQuestMarker, PlayerInteriorState, QuestNavigation, TutorialActionMarker |
| 9 | Bullet, IItemEffect, Player, PlayerGunHandler, PlayerHealth, PlayerItemUse, PlayerMovement, PlayerRegistry, UpgradeService |
| 6 | Boid, Enemy, EnemyBrainContext, EnemyLoot, EnemyNavAgentBase, IEnemyBrain |
| 4 | FeatureUnlockService, QuestManager, QuestProgressTracker, QuestStepEffects |
| 4 | EnemySpawner, ItemDrop, ItemSpawner, SpawnCatalog |
| 4 | DraggableItem, InventoryManager, InventorySlot, SlotBuilder |
| 2 | Interactable, PlayerInteractor |
| 2 | EnemyFSM, IEnemyState |
| 2 | EnemySearchState, EnemyStateBase |
| 2 | UIManager, UIWindow |
| 2 | ShopBuySlot, ShopManager |
| 2 | LinearPatrolBrain, LinearPatrolProfile |
| 2 | SaveMenuUI, SaveSlotUI |

Modules in the largest cycle (2): Gameplay/Entities/Enemies/Brain, Gameplay/Entities/Enemies/Brain/FSM/States

## Plain types with no code reference

Non-MonoBehaviour, non-ScriptableObject types nothing in code refers to. MB/SO usage is checked against scene/prefab GUIDs separately.

none
