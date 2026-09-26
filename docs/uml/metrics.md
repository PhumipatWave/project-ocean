# Metrics

[← index](README.md)

Generated. Coupling = distinct project types referenced (associations + usages).

## Largest types

| Type | LOC | Methods | Fields | Deps (fan-out) | Module |
|---|---|---|---|---|---|
| QuestManager | 588 | 47 | 6 | 30 | Gameplay/QuestSystems/Quest |
| OxygenRope | 536 | 25 | 33 | 7 | Gameplay/Entities/Players/OxygenTube |
| QuestStepDataDrawer | 528 | 10 | 2 | 6 | Editor |
| ObjectWrapPoint | 467 | 27 | 7 | 1 | Gameplay/InteractObjects |
| InventoryManager | 437 | 42 | 20 | 18 | UI/Inventory |
| Player | 403 | 27 | 38 | 26 | Gameplay/Entities/Players |
| UIManager | 362 | 34 | 11 | 6 | UI/UIManager |
| KeyBindManager | 339 | 12 | 4 | 5 | Core/Inputs/KeyBind |
| ShopManager | 299 | 37 | 18 | 13 | UI/Shop |
| SaveBridge | 288 | 21 | 4 | 23 | Core/Save |
| UpgradeTooltip | 279 | 8 | 14 | 5 | UI/Upgrades |
| QuestBoardUI | 249 | 15 | 18 | 10 | Core/Datas/ScriptableDatas/Quests/QuestBoard |
| NPC | 238 | 20 | 20 | 17 | Gameplay/Entities/NPCs |
| EnemyNavAgentBase | 228 | 26 | 29 | 11 | Gameplay/Entities/Enemies/NavAgents/Core |
| EnemyFollowState | 223 | 11 | 16 | 9 | Gameplay/Entities/Enemies/Brain/FSM/States |
| PlayerGunHandler | 222 | 13 | 34 | 10 | Gameplay/Entities/Players |
| EnemySpawner | 209 | 12 | 17 | 10 | Core/Save/DayReset |
| GameHUD | 186 | 15 | 16 | 18 | UI |
| LinearPatrolBrain | 179 | 19 | 18 | 8 | Gameplay/Entities/Enemies/Brain/LinearPatrol |
| ExploreAreaTrigger | 167 | 16 | 13 | 11 | Gameplay/QuestSystems/Quest |

## Most depended-on (fan-in)

| Type | Used by | Examples |
|---|---|---|
| GameManager | 29 | Bullet, CheckPointItem, ChestInteract, Cutscene, DevPanel |
| TutorialActionId | 26 | Bullet, DayManager, DoorInteract, GameHUD, GameManager |
| QuestManager | 20 | CutsceneManager, DevPanel, DoorInteract, ExploreAreaTrigger, FeatureUnlockService |
| InventoryManager | 17 | CheckPointItem, DevPanel, DraggableItem, EnemyLoot, InventorySlot |
| Quest | 16 | CutsceneManager, ItemType, NPC, PlayerSelfDialogueUI, QuestBoardUI |
| PlayerUIState | 16 | CollectableItemBase, ConfirmationDialog, Cutscene, CutsceneManager, EscClosablePanel |
| QuestType | 16 | DoorInteract, ExploreAreaTrigger, FieldVisibility, NPC, PlayerSelfDialogueUI |
| NotificationManager | 15 | CutsceneSkipHint, GameManager, InteractUI, InventoryManager, LandUI |
| EnemyBrainContext | 15 | EnemyAttackState, EnemyBrain, EnemyBrainBase, EnemyFSM, EnemyFleeState |
| TutorialEvents | 14 | Bullet, DayManager, DoorInteract, GameManager, InteractUI |
| Player | 14 | AmmoWheel, Bullet, CheckPointItem, DevPanel, Enemy |
| Item | 14 | DevItemButton, DevPanel, DraggableItem, EnemyLoot, GameObjectSpawnManager |
| GameFeature | 12 | CollectableItemBase, EnemySpawner, FeatureUnlockService, GameHUD, GameManager |
| LoadingManager | 12 | CutsceneManager, GameManager, InteractUI, IntroPlayer, LandUI |
| FeatureUnlockService | 11 | CollectableItemBase, EnemySpawner, GameHUD, GameManager, InteractUI |

## Most dependencies (fan-out)

| Type | Depends on |
|---|---|
| QuestManager | 30 |
| Player | 26 |
| SaveBridge | 23 |
| EnemyBrain | 19 |
| GameHUD | 18 |
| InventoryManager | 18 |
| NPC | 17 |
| GameManager | 16 |
| TutorialPromptUI | 15 |
| EnemyWanderState | 14 |
| PlayerSelfDialogueUI | 14 |
| UpgradeService | 13 |
| Quest | 13 |
| ShopManager | 13 |
| InteractUI | 12 |

## Static `Instance` singletons (23)

AudioManager, CameraShakeManager, ConfirmationDialog, CutsceneManager, DayManager, EnemyKillTracker, FeatureUnlockService, GameManager, GameObjectSpawnManager, InventoryManager, KeyBindManager, LoadingManager, NPCManager, ObjectPoolManager, OneTimePickupTracker, QuestCompleteUI, QuestManager, SaveBridge, ShopInventoryBuy, ShopManager, UIManager, UpgradeService, UpgradeTooltip

## Scene searches (`Find*`) — 14 in 8 files

| File | Count |
|---|---|
| Gameplay/Entities/Players/Player.cs | 4 |
| Core/GameManager.cs | 2 |
| Core/Inputs/KeyBind/UIKeyBind/MoveKeyBindUI.cs | 2 |
| Gameplay/Environments/DoorInteract.cs | 2 |
| Gameplay/Environments/NPCManager.cs | 1 |
| Gameplay/MainCameras/PlayerCamera.cs | 1 |
| UI/Shop/ShopManager.cs | 1 |
| UI/UIManager/UIManager.cs | 1 |

## `Debug.Log*` calls — 137 in 40 files

| File | Count |
|---|---|
| Core/Save/SaveBridge.cs | 17 |
| Core/Inputs/KeyBind/KeyBindManager.cs | 13 |
| Core/Save/SaveSystem.cs | 12 |
| UI/Inventory/InventoryManager.cs | 11 |
| Gameplay/QuestSystems/Quest/QuestManager.cs | 10 |
| Core/Save/DayReset/EnemySpawner.cs | 5 |
| Core/Save/DayReset/GameObjectSpawnManager.cs | 5 |
| Gameplay/Entities/Players/Player.cs | 5 |
| Core/Save/DayReset/ItemSpawner.cs | 4 |
| Gameplay/Cutscenes/Cutscene.cs | 4 |
| Gameplay/Entities/Players/DevPanel.cs | 4 |
| UI/Loading/LoadingButton.cs | 4 |
| Core/Datas/ScriptableDatas/Quests/QuestBoard/QuestDetailPanel.cs | 3 |
| Gameplay/Entities/Enemies/Brain/EnemyVision.cs | 3 |
| UI/Loading/LoadingManager.cs | 3 |
| UI/UIManager/UIManager.cs | 3 |
| Core/Datas/ScriptableDatas/Quests/QuestBoard/QuestBoardUI.cs | 2 |
| Core/GameManager.cs | 2 |
| Core/Save/SaveMenuUI.cs | 2 |
| Gameplay/AudioSetting/SoundChannel.cs | 2 |
| Gameplay/Entities/Enemies/Brain/FSM/EnemyFSM.cs | 2 |
| Gameplay/Entities/Enemies/NavAgents/Core/EnemyNavAgentBase.cs | 2 |
| Gameplay/QuestSystems/Quest/ExploreAreaTrigger.cs | 2 |
| Core/Save/DayReset/DayManager.cs | 1 |
| Core/Services/UpgradeService.cs | 1 |

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
| UI/Interactions/InteractUI.cs | 1 |
| UI/Inventory/InventoryManager.cs | 1 |
| UI/Loading/LoadingTextAnimation/LoadingTextAnimation.cs | 1 |

## Layer violations — 111 type links

Each row points the wrong way for Core ← Gameplay ← UI.

| From module | To module | From type | To type |
|---|---|---|---|
| Core | Gameplay/Entities/Players | GameManager | Player |
| Core | Gameplay/Entities/Players | PlayerRegistry | Player |
| Core | Gameplay/QuestSystems/Quest | GameManager | TutorialActionId |
| Core | Gameplay/QuestSystems/Quest | GameManager | TutorialEvents |
| Core | Gameplay/QuestSystems/Quest | GameManager | TutorialGate |
| Core | UI | GameManager | MainMenu |
| Core | UI | GameManager | NotificationManager |
| Core | UI | SceneNames | MainMenu |
| Core | UI/Loading | GameManager | LoadingManager |
| Core | UI/Loading/LoadingVisualController | GameManager | LoadingType |
| Core | UI/UIManager | GameManager | UIManager |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/Environments | QuestSlot | NPCManager |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestBoardUI | Quest |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestBoardUI | QuestCategory |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestBoardUI | QuestManager |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestBoardUI | TutorialActionId |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestBoardUI | TutorialEvents |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestCategorySlot | QuestCategory |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestCategoryUIData | QuestCategory |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestDetailPanel | Quest |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestDetailPanel | QuestManager |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestDetailPanel | QuestVisuals |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestDetailPanel | TutorialActionId |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestDetailPanel | TutorialEvents |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestSlot | Quest |
| Core/Datas/ScriptableDatas/Quests/QuestBoard | Gameplay/QuestSystems/Quest | QuestSlot | QuestVisuals |
| Core/Inputs | Gameplay/Entities/Players | InputReader | Player |
| Core/Inputs/KeyBind | Gameplay/Entities/Players | KeyBindManager | Player |
| Core/Interfaces | Gameplay/Entities/Enemies/Brain | IEnemyBrain | EnemyBrainContext |
| Core/Interfaces | Gameplay/Entities/Enemies/Brain | IEnemyState | EnemyBrainContext |
| Core/Interfaces | Gameplay/Entities/Enemies/Brain/FSM | IEnemyState | EnemyFSM |
| Core/Interfaces | Gameplay/Entities/Enemies/Brain/FSM | IEnemyState | EnemyStateTag |
| Core/Save | Gameplay/Cutscenes | SaveBridge | CutsceneManager |
| Core/Save | Gameplay/Entities/Players | SaveBridge | PlayerHealth |
| Core/Save | Gameplay/Entities/Players | SaveBridge | PlayerMoney |
| Core/Save | Gameplay/QuestSystems/Quest | EncryptionHelper | Mode |
| Core/Save | Gameplay/QuestSystems/Quest | SaveBridge | QuestManager |
| Core/Save | UI | SaveMenuUI | ConfirmationDialog |
| Core/Save | UI | SaveMenuUI | NotificationManager |
| Core/Save | UI/Inventory | SaveBridge | InventoryManager |
| Core/Save | UI/Inventory/InventoryPlayer | SaveBridge | DraggableItem |
| Core/Save | UI/Loading | SaveBridge | LoadingManager |
| Core/Save | UI/Loading | SaveMenuUI | LoadingManager |
| Core/Save | UI/Loading/LoadingVisualController | SaveBridge | LoadingType |
| Core/Save/DayReset | Gameplay/Entities/Enemies/Behaviours | EnemySpawner | Boid |
| Core/Save/DayReset | Gameplay/Entities/Enemies/NavAgents | EnemySpawner | EnemyAgent |
| Core/Save/DayReset | Gameplay/Entities/Enemies/NavAgents/Core | EnemySpawner | EnemyNavAgentBase |
| Core/Save/DayReset | Gameplay/QuestSystems/Quest | DayManager | TutorialActionId |
| Core/Save/DayReset | Gameplay/QuestSystems/Quest | DayManager | TutorialEvents |
| Core/Save/DayReset | UI/Items | GameObjectSpawnManager | Item |
| Core/Save/DayReset | UI/Items | ItemSpawnEntry | Item |
| Core/Save/DayReset | UI/Items | ItemSpawner | ItemDrop |
| Core/Services | Gameplay/Entities/Players | UpgradeService | PlayerHealth |
| Core/Services | Gameplay/Entities/Players | UpgradeService | PlayerMoney |
| Core/Services | Gameplay/Entities/Players/OxygenTube | UpgradeService | OxygenRope |
| Core/Services | Gameplay/Projectiles | UpgradeService | NormalBullet |
| Core/Services | Gameplay/QuestSystems/Quest | FeatureUnlockService | QuestManager |
| Core/Services | Gameplay/QuestSystems/Quest | UpgradeService | TutorialActionId |
| Core/Services | Gameplay/QuestSystems/Quest | UpgradeService | TutorialEvents |
| Core/Services | UI | UpgradeService | NotificationManager |
| Core/Services | UI/Inventory | UpgradeService | InventoryManager |
| Core/Services | UI/Upgrades | UpgradeService | UpgradeData |
| Core/Services | UI/Upgrades | UpgradeService | UpgradeType |
| Core/VideoManager | UI/Loading | IntroPlayer | LoadingManager |
| Core/VideoManager | UI/Loading/LoadingVisualController | IntroPlayer | LoadingType |
| Gameplay/Cutscenes | UI | Cutscene | ConfirmationDialog |
| Gameplay/Cutscenes | UI | CutsceneManager | MainMenu |
| Gameplay/Cutscenes | UI | CutsceneSkipHint | NotificationManager |
| Gameplay/Cutscenes | UI/Loading | CutsceneManager | LoadingManager |
| Gameplay/Cutscenes | UI/Notifications | CutsceneSkipHint | NotificationChannel |
| Gameplay/Cutscenes | UI/UIManager | Cutscene | UIManager |
| Gameplay/Entities | UI/Interactions | EntityDeathHandler | DamagePoint |
| Gameplay/Entities/Enemies | UI/Inventory | EnemyLoot | InventoryManager |
| Gameplay/Entities/Enemies | UI/Items | EnemyLoot | Item |
| Gameplay/Entities/NPCs | UI | NPC | DialogueSpeaker |
| Gameplay/Entities/NPCs | UI/Interactions | NPC | InteractUI |
| Gameplay/Entities/NPCs | UI/Loading | NPC | LoadingManager |
| Gameplay/Entities/Players | UI/Inventory | DevPanel | InventoryManager |
| Gameplay/Entities/Players | UI/Inventory | Player | InventoryManager |
| Gameplay/Entities/Players | UI/Items | DevItemButton | Item |
| Gameplay/Entities/Players | UI/Items | DevPanel | Item |
| Gameplay/Entities/Players | UI/Items | Player | Item |
| Gameplay/Entities/Players | UI/Items | Player | ItemType |
| Gameplay/Entities/Players | UI/Upgrades | DevPanel | UpgradeData |
| Gameplay/Entities/Players | UI/Upgrades | PlayerGunHandler | UpgradeType |
| Gameplay/Entities/Players | UI/VFX | Player | PlayerFeedbackVFX |
| Gameplay/Entities/Players | UI/VFX | Player | PlayerSprintVFX |
| Gameplay/Environments | UI/Interactions | ChestInteract | InteractUI |
| Gameplay/InteractObjects | UI/Inventory | CheckPointItem | InventoryManager |
| Gameplay/InteractObjects | UI/Items | ShelleController | ItemDrop |
| Gameplay/Projectiles | UI/Interactions | Bullet | DamagePoint |
| Gameplay/Projectiles | UI/Upgrades | NormalBullet | UpgradeType |
| Gameplay/QuestSystems/Quest | UI | ExploreAreaTrigger | DialogueSpeaker |
| Gameplay/QuestSystems/Quest | UI | PlayerSelfDialogueUI | DialogueSpeaker |
| Gameplay/QuestSystems/Quest | UI | QuestManager | NotificationManager |
| Gameplay/QuestSystems/Quest | UI | QuestTrackerUI | PunchScale |
| Gameplay/QuestSystems/Quest | UI | TutorialGate | NotificationManager |
| Gameplay/QuestSystems/Quest | UI | TutorialPromptUI | MainMenu |
| Gameplay/QuestSystems/Quest | UI | TutorialPromptUI | NotificationManager |
| Gameplay/QuestSystems/Quest | UI/Interactions | ExploreAreaTrigger | InteractUI |
| Gameplay/QuestSystems/Quest | UI/Inventory | QuestManager | InventoryManager |
| Gameplay/QuestSystems/Quest | UI/Inventory | QuestVisuals | InventoryManager |
| Gameplay/QuestSystems/Quest | UI/Inventory | TutorialPromptUI | InventoryManager |
| Gameplay/QuestSystems/Quest | UI/Items | TutorialPromptUI | ItemType |
| Gameplay/QuestSystems/Quest | UI/Loading | PlayerSelfDialogueUI | LoadingManager |
| Gameplay/QuestSystems/Quest | UI/Loading | QuestTrackerUI | LoadingManager |
| Gameplay/QuestSystems/Quest | UI/Loading | TutorialPromptUI | LoadingManager |
| Gameplay/QuestSystems/Quest | UI/Notifications | QuestManager | NotificationChannel |
| Gameplay/QuestSystems/Quest | UI/Notifications | TutorialGate | NotificationChannel |
| Gameplay/QuestSystems/Quest | UI/Notifications | TutorialPromptUI | NotificationChannel |
| Gameplay/QuestSystems/Quest | UI/QuestCompleteUI | QuestManager | QuestCompleteUI |

## Dependency cycles (type level)

Types that transitively depend on each other. Small cycles are cheapest to break.

| Size | Types |
|---|---|
| 82 | AggressiveEnemyProfile, Boid, Bullet, CheckPointItem, CollectableItemBase, Cutscene, CutsceneManager, DialogueSpeaker, DialogueState, DoorInteract, DraggableItem, Enemy … +70 |
| 2 | ShopBuySlot, ShopManager |
| 2 | LinearPatrolBrain, LinearPatrolProfile |
| 2 | SaveMenuUI, SaveSlotUI |

Modules in the largest cycle (32): Core, Core/Datas/ScriptableDatas/Quests/QuestBoard, Core/Inputs, Core/Inputs/KeyBind, Core/Interfaces, Core/Save/DayReset, Core/Services, Gameplay/Cutscenes, Gameplay/Entities, Gameplay/Entities/Enemies, Gameplay/Entities/Enemies/Behaviours, Gameplay/Entities/Enemies/Brain, Gameplay/Entities/Enemies/Brain/FSM, Gameplay/Entities/Enemies/Brain/FSM/States, Gameplay/Entities/Enemies/NavAgents, Gameplay/Entities/Enemies/NavAgents/Core, Gameplay/Entities/Enemies/PartBreak, Gameplay/Entities/NPCs, Gameplay/Entities/Players, Gameplay/Entities/Players/OxygenTube, Gameplay/Environments, Gameplay/InteractObjects, Gameplay/Projectiles, Gameplay/QuestSystems/Quest, UI, UI/Interactions, UI/Inventory, UI/Inventory/InventoryPlayer, UI/Items, UI/QuestCompleteUI, UI/UIManager, UI/Upgrades

## Plain types with no code reference

Non-MonoBehaviour, non-ScriptableObject types nothing in code refers to. MB/SO usage is checked against scene/prefab GUIDs separately.

none
