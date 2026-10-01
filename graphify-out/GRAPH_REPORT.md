# Graph Report - project-ocean  (2026-10-01)

## Corpus Check
- 415 files · ~377,645 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 2660 file(s) not represented in the graph (top: .meta 1814, .asset 511, .prefab 135)

## Summary
- 4814 nodes · 9817 edges · 246 communities (220 shown, 26 thin omitted)
- Extraction: 91% EXTRACTED · 9% INFERRED · 0% AMBIGUOUS · INFERRED: 929 edges (avg confidence: 0.9)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `ea1bf3fe`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- project-ocean.slnx
- Game.Core
- UIManager
- README.md
- OxygenRope
- manifest.json
- dependencies
- EnemyNavAgentBase
- InputReader
- AdditiveSceneSetupWindow
- SaveData
- ExploreAreaTrigger
- UI.md
- system_collections_generic
- ObjectWrapPoint
- Enemy
- packages-lock.json
- QuestProgressTracker
- com.unity.mathematics
- EnemyBrain
- ShopManager
- Game.UI
- Quest
- TutorialPromptUI
- InventoryManager
- QuestStep
- com.unity.modules.imgui
- LoadingManager
- com.unity.2d.common
- gen_uml.py
- com.unity.modules.animation
- FishMotion
- @InputSystem_Actions
- GameHUD
- CutsceneManager
- PlayerGunHandler
- com.unity.modules.jsonserialize
- Player
- NPC
- What You Must Do When Invoked
- com.unity.render-pipelines.core
- HierarchyCategory
- 3. Root causes
- GameFeature
- EnemyBrainContext
- TrashInteract
- dependencies
- com.unity.test-framework
- LinearPatrolBrain
- 03-all-scripts.md
- EntityHealth
- EnemySpawner
- Bullet
- CallbackContext
- TutorialActionId
- MonoBehaviour
- SpawnCatalog
- Player
- com.unity.modules.physics
- EnemyFacingController
- Gameplay_Entities_Enemies.md
- Gameplay_QuestSystems_Quest.md
- Entity
- PlayerHealth
- .FromSave
- ItemType
- DayManager
- ComponentInspectorPopup
- ComponentMinimap
- .DrawCategory
- Phase 2: put types in the right layer (medium risk, biggest win)
- PlayerSelfDialogueUI
- com.unity.editorcoroutines
- ItemSpawner
- unityengine_inputsystem
- UISelectOnOpen
- .HandleBulletHit
- ReachPoint
- PlayerActions
- .EnsureQuestTracked
- RopeBreakCountdownUI
- com.unity.2d.tilemap
- com.unity.dt.app-ui
- DraggableItem
- EnemyNavPathfinder
- QuestManager
- QuestBoardUI
- QuestCategory
- Boid
- com.unity.modules.uielements
- QuestStepDataDrawer
- Interactable
- com.unity.modules.subsystems
- project-ocean-worktree2.slnx
- EnemyFollowState
- EnemyBrainBase
- Avoidance
- AmmoWheel
- UIActions
- InventoryModel
- .Apply
- unityeditor
- gen_drawio.py
- HierarchyDataCache
- ShellPickup
- CheckPointItem
- InventorySlot
- DevPanel
- ItemDrop
- .Unregister
- com.unity.nuget.newtonsoft-json
- com.unity.ext.nunit
- CollectableItemBase
- PlayerInteriorState
- .UnregisterCallbacks
- ChestInteract
- Gameplay_Entities.md
- NPCManager
- .Pick
- ConfirmationDialog
- LandUI
- UNITY PROJECT AI GUIDE
- UpgradeSlot
- com.unity.modules.ai
- Core (51 files)
- HierarchyPlus — Installation Guide
- MotionBendTracking
- NextDayAnimationUI
- FishAlertUI
- ScriptNamingTests
- EnemyStateBase
- VerletRope
- QuestSplitTests
- SaveMenuUI
- IntroPlayer
- GrapplingHookBullet
- CategoryColorMap
- ObjectPoolManager
- AudioManager
- EnemyHitbox
- Bird
- Item
- graphify reference: extra exports and benchmark
- HierarchyPlusCore
- MissingScriptDetector
- Parallax
- SaveSystem
- DoorQuestMarker
- Phase 12: static classes, names, comments, dead button calls, folders
- parse_types
- UIType
- UI (58 files)
- CategoryIconMap
- QuestDetailPanel
- PlayerFeedbackVFX
- UISettingManager
- InteractableNextDay
- .TryOpenScript
- KeyAction
- PlayerMovement
- Gameplay_Services.md
- SimpleAnimLoop
- NotificationMessage
- graphify reference: query, path, explain
- .CompleteProgress
- .Awake
- .BeginReach
- SwimState
- DialogueState
- EnemyLoot
- RopeSplitTests
- .Info
- SettingTabManager
- ButtonHoverScale
- RebindButtonBase
- Singleton
- QuestSlot
- TrashBin
- .Notify
- graphify reference: add a URL and watch a folder
- graphify reference: commit hook and native CLAUDE.md integration
- graphify reference: incremental update and cluster-only
- HierarchyPlus — Folder Structure
- graphify reference: GitHub clone and cross-repo merge
- graphify reference: transcribe video and audio
- Metrics
- QuestCompleteUI
- .claude/CLAUDE.md
- extraction-spec.md
- IDialogueView
- .Close
- EntityDeathHandler
- PlayerSprintVFX
- SceneLoadContext
- SaveBridge
- TutorialGate
- .LoadSaveState
- UpgradeTooltip
- EnemyAgent
- .Pick
- AcceptedQuestListUI
- com.h8man.2d.navmeshplus
- PlayerMoney
- QuestTrackerUI
- GameLogTests
- VolumeSliderUI
- DocsInstaller
- Cutscene
- guid_snapshot.py
- Game.Tests
- Gameplay (131 files)
- 00-overview.md
- UpgradeServiceEventTests
- 1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)
- .AttachRope
- UIFadeBackground
- PunchScale
- ViewContractTests
- EnemyAttackState
- NotificationLine
- RopeCornerFinder
- EnemyWanderState
- Core.md
- NormalBullet
- DependencyFalsePositiveTest
- ParserTest
- GameManager
- UIType
- Gameplay_Entities_Players.md
- GameFlowEvents
- .LimitBreached
- Page
- DrawioTest
- GameSessionTests
- EventInteractable
- QuestCategorySlot
- PlayerStabAttack
- PlayerSplitTests
- tarjan
- EntityAnimation
- ScriptableObject

## God Nodes (most connected - your core abstractions)
1. `Game.Core` - 261 edges
2. `Game.Gameplay` - 218 edges
3. `Gameplay (131 files)` - 167 edges
4. `Game.UI` - 82 edges
5. `Item` - 81 edges
6. `InventoryManager` - 78 edges
7. `FishMotion` - 68 edges
8. `Core (51 files)` - 61 edges
9. `UI (58 files)` - 61 edges
10. `OxygenRope` - 60 edges

## Surprising Connections (you probably didn't know these)
- `5. Risks` --references--> `SaveData`  [INFERRED]
  docs/uml/findings.md → Assets/Project_Ocean/Scripts/Core/Save/SaveData.cs
- `Core (51 files)` --references--> `AmmoType`  [INFERRED]
  docs/uml/03-all-scripts.md → Assets/Project_Ocean/Scripts/Core/Data/BulletData.cs
- `Core (51 files)` --references--> `BulletData`  [INFERRED]
  docs/uml/03-all-scripts.md → Assets/Project_Ocean/Scripts/Core/Data/BulletData.cs
- `Core (51 files)` --references--> `InventoryItemDatabase`  [INFERRED]
  docs/uml/03-all-scripts.md → Assets/Project_Ocean/Scripts/Core/Data/Items/InventoryItemDatabase.cs
- `Phase 2: put types in the right layer (medium risk, biggest win)` --references--> `InventoryItemDatabase`  [INFERRED]
  docs/uml/findings.md → Assets/Project_Ocean/Scripts/Core/Data/Items/InventoryItemDatabase.cs

## Import Cycles
- None detected.

## Communities (246 total, 26 thin omitted)

### Community 0 - "project-ocean.slnx"
Cohesion: 0.03
Nodes (74): Assembly-CSharp, Assembly-CSharp-Editor, Game.Deprecated, Game.Editor, Game.Gameplay, Game.Tests.EditMode, Game.UI, glTFast (+66 more)

### Community 1 - "Game.Core"
Cohesion: 0.05
Nodes (9): Game.Core, Game.Gameplay, system_collections, Unity.Cinemachine, unityengine, unityengine_ai, unityengine_audio, unityengine_events (+1 more)

### Community 2 - "UIManager"
Cohesion: 0.10
Nodes (9): Dictionary, UIManager, CurrentUI, IsUIOpen, Persistent, UIWindow, Type, Phase 6: cleanup (low risk, any time after Phase 4) (+1 more)

### Community 3 - "README.md"
Cohesion: 0.10
Nodes (14): Inheritance & interfaces, Declared events with no `+=` subscriber found, Event catalogue, Event flow, Files, Gameplay/Entities/Players/OxygenTube, Event subscriptions, Files (+6 more)

### Community 4 - "OxygenRope"
Cohesion: 0.09
Nodes (22): Coroutine, GameObject, LayerMask, LineRenderer, List, Rigidbody2D, Transform, Vector2 (+14 more)

### Community 5 - "manifest.json"
Cohesion: 0.03
Nodes (60): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.director, com.unity.modules.imageconversion (+52 more)

### Community 6 - "dependencies"
Cohesion: 0.03
Nodes (61): dependencies, com.coplaydev.unity-mcp, com.h8man.2d.navmeshplus, com.unity.2d.animation, com.unity.2d.aseprite, com.unity.2d.psdimporter, com.unity.2d.sprite, com.unity.2d.spriteshape (+53 more)

### Community 7 - "EnemyNavAgentBase"
Cohesion: 0.07
Nodes (18): Transform, IEnemyBrain, IsAttacking, FacingTurnMode, NavMeshAgent, EnemyNavAgentBase, ActiveBrain, AutoReturnDelay (+10 more)

### Community 8 - "InputReader"
Cohesion: 0.09
Nodes (10): CallbackContext, Func, InputSystem_Actions, Vector2, InputReader, InputActions, MousePos, MovePos (+2 more)

### Community 9 - "AdditiveSceneSetupWindow"
Cohesion: 0.07
Nodes (23): AdditiveSceneEntry, Component, GUIStyle, Vector2, ComponentTooltipWindow, IsVisible, AdditiveSceneConfig, AdditiveSceneEntry (+15 more)

### Community 10 - "SaveData"
Cohesion: 0.08
Nodes (15): HashSet, EnemyKillTracker, DestroyGameObjectOnDuplicate, List, OneTimePickupTracker, DestroyGameObjectOnDuplicate, ISaveParticipant, List (+7 more)

### Community 11 - "ExploreAreaTrigger"
Cohesion: 0.10
Nodes (12): ExploreEvents, Collider2D, List, Transform, DialogueLine, ExploreAreaTrigger, View, Mode (+4 more)

### Community 12 - "UI.md"
Cohesion: 0.10
Nodes (15): Core/Data, Files, Files, UI/Dialogue, Event subscriptions, Files, Event subscriptions, Files (+7 more)

### Community 13 - "system_collections_generic"
Cohesion: 0.05
Nodes (9): Vector3, FishConstant, FishAlive, system, system_collections_generic, unityengine_inputsystem_utilities, unityengine_playables, unityengine_random (+1 more)

### Community 14 - "ObjectWrapPoint"
Cohesion: 0.17
Nodes (11): BoxCollider2D, Collider2D, IEnumerable, List, Vector2, ObjectWrapPoint, WrapPoint, CompositeCollider2D (+3 more)

### Community 15 - "Enemy"
Cohesion: 0.07
Nodes (15): EnemyEvents, Collision2D, IEnumerator, Enemy, Brain, EnemyId, IsAttacking, Player (+7 more)

### Community 16 - "packages-lock.json"
Cohesion: 0.05
Nodes (41): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.director, com.unity.modules.imageconversion (+33 more)

### Community 17 - "QuestProgressTracker"
Cohesion: 0.16
Nodes (5): Action, Func, List, Quest, QuestProgressTracker

### Community 18 - "com.unity.mathematics"
Cohesion: 0.07
Nodes (35): dependencies, dependencies, depth, source, url, version, dependencies, depth (+27 more)

### Community 19 - "EnemyBrain"
Cohesion: 0.08
Nodes (19): AggressiveEnemyProfile, detectDistance, LayerMask, EnemyBaseProfile, RequiresNavMesh, Transform, Vector3, EnemyBrain (+11 more)

### Community 20 - "ShopManager"
Cohesion: 0.09
Nodes (8): TutorialStep, Button, GameObject, Image, TMP_Text, Transform, ShopManager, Instance

### Community 21 - "Game.UI"
Cohesion: 0.05
Nodes (8): IHookable, Game.UI, Game.Deprecated, tmpro, unityengine_eventsystems, unityengine_inputsystem_ui, unityengine_serialization, unityengine_ui

### Community 22 - "Quest"
Cohesion: 0.06
Nodes (30): IEnumerable, List, Quest, Category, Classification, CurrentCount, CurrentStep, CurrentStepIndex (+22 more)

### Community 24 - "InventoryManager"
Cohesion: 0.06
Nodes (19): Inventory, Current, GameObject, IReadOnlyList, List, TextMeshProUGUI, Transform, InventoryManager (+11 more)

### Community 25 - "QuestStep"
Cohesion: 0.05
Nodes (41): DeliveryCountMode, CombinedTotal, PerItem, DialogueType, Accept, Finish, InProgress, NotFinish (+33 more)

### Community 26 - "com.unity.modules.imgui"
Cohesion: 0.09
Nodes (23): dependencies, depth, source, url, version, dependencies, depth, source (+15 more)

### Community 27 - "LoadingManager"
Cohesion: 0.18
Nodes (8): GameObject, HashSet, IEnumerator, MonoBehaviour, Slider, LoadingManager, IsLoading, Persistent

### Community 28 - "com.unity.2d.common"
Cohesion: 0.06
Nodes (34): dependencies, depth, source, url, version, dependencies, depth, source (+26 more)

### Community 29 - "gen_uml.py"
Cohesion: 0.18
Nodes (22): class_block(), class_diagram(), edges_of(), load(), main(), md_link_module(), mm(), module_graph() (+14 more)

### Community 30 - "com.unity.modules.animation"
Cohesion: 0.07
Nodes (33): dependencies, depth, source, url, version, dependencies, depth, source (+25 more)

### Community 31 - "FishMotion"
Cohesion: 0.07
Nodes (18): AnimationClip, Action, Color, Dictionary, GameObject, IEnumerator, Quaternion, DrawDebug (+10 more)

### Community 32 - "@InputSystem_Actions"
Cohesion: 0.07
Nodes (26): IEnumerable, IEnumerator, InputAction, InputBinding, List, @InputSystem_Actions, asset, bindingMask (+18 more)

### Community 33 - "GameHUD"
Cohesion: 0.11
Nodes (10): Button, CanvasGroup, GameObject, IEnumerator, Image, LoadSceneMode, Scene, TextMeshProUGUI (+2 more)

### Community 34 - "CutsceneManager"
Cohesion: 0.15
Nodes (10): Action, Dictionary, GameObject, IEnumerator, List, LoadSceneMode, Quest, Scene (+2 more)

### Community 35 - "PlayerGunHandler"
Cohesion: 0.08
Nodes (20): Collider2D, IEnumerator, LayerMask, LineRenderer, ParticleSystem, Rigidbody2D, SpriteRenderer, Transform (+12 more)

### Community 36 - "com.unity.modules.jsonserialize"
Cohesion: 0.08
Nodes (27): dependencies, depth, source, version, dependencies, depth, source, version (+19 more)

### Community 37 - "Player"
Cohesion: 0.14
Nodes (11): GameObject, Rigidbody2D, Player, DashSpeed, FaceDir, GunHandler, Movement, OxygenTube (+3 more)

### Community 38 - "NPC"
Cohesion: 0.09
Nodes (14): NpcEvents, Collider2D, GameObject, Sprite, NPC, HasQuest, NpcId, NpcName (+6 more)

### Community 39 - "What You Must Do When Invoked"
Cohesion: 0.08
Nodes (24): For /graphify add and --watch, For /graphify query, For the commit hook and native CLAUDE.md integration, For --update and --cluster-only, /graphify, Honesty Rules, Interpreter guard for subcommands, Part A - Structural extraction for code files (+16 more)

### Community 40 - "com.unity.render-pipelines.core"
Cohesion: 0.09
Nodes (25): depth, source, version, dependencies, depth, source, version, dependencies (+17 more)

### Community 41 - "HierarchyCategory"
Cohesion: 0.09
Nodes (20): DevelopmentStatus, Complete, Deprecated, New, None, Problem, Review, WorkInProgress (+12 more)

### Community 42 - "3. Root causes"
Cohesion: 0.16
Nodes (13): List, UpgradeData, UpgradeType, Inventory, NormalBullet, Rope, UpgradeUnlock, IBrainAttack (+5 more)

### Community 43 - "GameFeature"
Cohesion: 0.09
Nodes (21): GameFeature, BackToLand, Combat, EnemySpawn, EnemySpawnLevel1, EnemySpawnLevel2, EnemySpawnLevel3, Interact (+13 more)

### Community 44 - "EnemyBrainContext"
Cohesion: 0.09
Nodes (22): MonoBehaviour, NavMeshAgent, Transform, EnemyBrainContext, IsDead, PlayerTransform, Dictionary, EnemyFSM (+14 more)

### Community 45 - "TrashInteract"
Cohesion: 0.19
Nodes (9): Coroutine, GameObject, IEnumerator, MonoBehaviour, Rigidbody2D, Sprite, Vector3, TrashInteract (+1 more)

### Community 46 - "dependencies"
Cohesion: 0.08
Nodes (24): dependencies, depth, source, url, version, dependencies, depth, source (+16 more)

### Community 47 - "com.unity.test-framework"
Cohesion: 0.09
Nodes (23): dependencies, dependencies, depth, source, url, version, dependencies, depth (+15 more)

### Community 48 - "LinearPatrolBrain"
Cohesion: 0.14
Nodes (7): Collider2D, Component, Rigidbody2D, Transform, Vector2, LinearPatrolBrain, IsAttacking

### Community 49 - "03-all-scripts.md"
Cohesion: 0.09
Nodes (15): Core/Data/ScriptableData/Characters, Files, Core/VideoManager, Files, Files, Gameplay/MainCameras, Files, UI/Loading/LoadingTextAnimation (+7 more)

### Community 50 - "EntityHealth"
Cohesion: 0.10
Nodes (12): Color, Coroutine, IEnumerator, SpriteRenderer, EntityHealth, CurrentHealth, IsDead, MaxHealth (+4 more)

### Community 51 - "EnemySpawner"
Cohesion: 0.13
Nodes (9): GameObject, IEnumerator, LayerMask, List, Rigidbody2D, Vector3, EnemySpawner, ZoneSpawnEntry (+1 more)

### Community 52 - "Bullet"
Cohesion: 0.15
Nodes (9): Collider2D, GameObject, IEnumerator, Rigidbody2D, SpriteRenderer, Vector2, Bullet, MaxRange (+1 more)

### Community 53 - "CallbackContext"
Cohesion: 0.17
Nodes (3): CallbackContext, IPlayerActions, IUIActions

### Community 54 - "TutorialActionId"
Cohesion: 0.07
Nodes (28): TutorialActionId, AcceptQuest, BackToLand, BreakRopeLimit, BuyItem, Dive, HitEnemy, Interact (+20 more)

### Community 55 - "MonoBehaviour"
Cohesion: 0.09
Nodes (13): Vector3, WaterShadowWiggle, Button, KeyBindResetAllButton, GameBootstrap, GameObject, QuestCompass, MiniHubItem (+5 more)

### Community 56 - "SpawnCatalog"
Cohesion: 0.15
Nodes (6): Dictionary, GameObject, List, EnemySpawnEntry, ItemSpawnEntry, SpawnCatalog

### Community 57 - "Player"
Cohesion: 0.12
Nodes (8): TutorialEvents, Player, PlayerRegistry, Local, IItemEffect, ActionHint, KeepInInventory, PlayerItemUse

### Community 58 - "com.unity.modules.physics"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, version, dependencies, depth, source, version (+13 more)

### Community 59 - "EnemyFacingController"
Cohesion: 0.12
Nodes (12): SpriteRenderer, Transform, CharacterFacing2D, FacingTurnMode, Transform, Vector2, Vector3, EnemyFacingController (+4 more)

### Community 60 - "Gameplay_Entities_Enemies.md"
Cohesion: 0.08
Nodes (25): Core/Pool, Files, Files, Gameplay/Entities/Enemies/Behaviours, Files, Files, Gameplay/Entities/Enemies/Brain/FSM, Files (+17 more)

### Community 61 - "Gameplay_QuestSystems_Quest.md"
Cohesion: 0.08
Nodes (29): Core/Events, Files, Core/Inputs, Event subscriptions, Files, Editor, Files, Files (+21 more)

### Community 62 - "Entity"
Cohesion: 0.14
Nodes (5): Rigidbody2D, Entity, CurrentHealth, IsDead, MaxHealth

### Community 63 - "PlayerHealth"
Cohesion: 0.09
Nodes (8): PlayerHealth, IsInvincible, PlayerSceneMode, GameObject, SetUp, TearDown, Test, PlayerInvincibilityTests

### Community 64 - ".FromSave"
Cohesion: 0.13
Nodes (13): ItemSaveData, Func, IReadOnlyList, List, InventorySaveMapper, SlotBuilder, TearDown, TearDown (+5 more)

### Community 65 - "ItemType"
Cohesion: 0.25
Nodes (8): ItemType, Ammo, Building, CheckpointItem, Consumable, KeyItem, Normal, Quest

### Community 66 - "DayManager"
Cohesion: 0.18
Nodes (8): UnityEvent, DayManager, CanDive, CurrentDay, DivesRemaining, DivesToday, HasDivedToday, TotalDives

### Community 67 - "ComponentInspectorPopup"
Cohesion: 0.17
Nodes (7): Component, Editor, GUIStyle, Rect, Vector2, ComponentInspectorPopup, PopupWindowContent

### Community 68 - "ComponentMinimap"
Cohesion: 0.22
Nodes (10): Component, Dictionary, GameObject, HashSet, List, Rect, Texture2D, Type (+2 more)

### Community 69 - ".DrawCategory"
Cohesion: 0.18
Nodes (11): GameObject, GUIStyle, Rect, HierarchyPlusDrawer, BadgeStyle, IconStyle, Color, DevelopmentStatus (+3 more)

### Community 70 - "Phase 2: put types in the right layer (medium risk, biggest win)"
Cohesion: 0.13
Nodes (13): CutsceneEvents, ILoadingVisual, LoadingType, Land, Menu, Ocean, Animator, Image (+5 more)

### Community 71 - "PlayerSelfDialogueUI"
Cohesion: 0.20
Nodes (7): Line, IEnumerator, List, Quest, Line, PlayerSelfDialogueUI, View

### Community 72 - "com.unity.editorcoroutines"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, url, version, dependencies, depth, source (+10 more)

### Community 73 - "ItemSpawner"
Cohesion: 0.17
Nodes (6): GameObject, List, Transform, ItemSpawner, ZoneItemEntry, Weight

### Community 75 - "UISelectOnOpen"
Cohesion: 0.21
Nodes (8): Button, GameObject, IEnumerator, List, UISelectOnOpen, EventSystem, InputActionReference&gt;, Selectable

### Community 76 - ".HandleBulletHit"
Cohesion: 0.22
Nodes (4): Collider2D, Collision2D, GameObject, Transform

### Community 77 - "ReachPoint"
Cohesion: 0.16
Nodes (13): ReachMode, Position, PositionToBite, Transform, Wander, Quaternion, Vector3, ReachPoint (+5 more)

### Community 78 - "PlayerActions"
Cohesion: 0.12
Nodes (15): PlayerActions, @Aim, @Attack, enabled, @Interact, @Inventory, @ItemConsume, @Menu (+7 more)

### Community 79 - ".EnsureQuestTracked"
Cohesion: 0.16
Nodes (3): RewardEvents, Test, RewardEventsTests

### Community 80 - "RopeBreakCountdownUI"
Cohesion: 0.18
Nodes (7): CanvasGroup, Coroutine, IEnumerator, Image, LoadSceneMode, Scene, RopeBreakCountdownUI

### Community 81 - "com.unity.2d.tilemap"
Cohesion: 0.08
Nodes (27): dependencies, depth, source, url, version, dependencies, depth, dependencies (+19 more)

### Community 82 - "com.unity.dt.app-ui"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, url, version, dependencies, depth, source (+8 more)

### Community 83 - "DraggableItem"
Cohesion: 0.09
Nodes (17): Color, Image, PointerEventData, ShopBuySlot, GameObject, ShopInventoryBuy, Instance, Image (+9 more)

### Community 84 - "EnemyNavPathfinder"
Cohesion: 0.18
Nodes (11): Action, Func, Vector3, EnemySearchState, SearchOrigin, StateTag, LayerMask, NavMeshAgent (+3 more)

### Community 85 - "QuestManager"
Cohesion: 0.16
Nodes (6): Coroutine, IEnumerator, List, Quest, QuestManager, Persistent

### Community 86 - "QuestBoardUI"
Cohesion: 0.14
Nodes (9): Button, GameObject, List, Quest, Sprite, TextMeshProUGUI, QuestBoardUI, QuestsPerPage (+1 more)

### Community 87 - "QuestCategory"
Cohesion: 0.09
Nodes (23): DeliverySource, EnemyKills, InventoryHeld, NextQuestMode, QuestType, Cutscene, Delivery, ExploreArea (+15 more)

### Community 88 - "Boid"
Cohesion: 0.15
Nodes (7): List, Vector2, Vector3, Boid, HasLeader, Leader, Phase 10: break the 36-type gameplay cycle

### Community 89 - "com.unity.modules.uielements"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, url, version, depth, source, version (+7 more)

### Community 90 - "QuestStepDataDrawer"
Cohesion: 0.11
Nodes (21): GUIStyle, SceneAsset, ReadmeReaderEditor, SerializedProperty, QuestDataEditor, characterId, Dictionary, GUIContent (+13 more)

### Community 91 - "Interactable"
Cohesion: 0.10
Nodes (12): Collider2D, Collider2D, GameObject, Interactable, CanInteract, HoldsLock, IsFeatureUnlocked, IsReady (+4 more)

### Community 92 - "com.unity.modules.subsystems"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 93 - "project-ocean-worktree2.slnx"
Cohesion: 0.03
Nodes (74): Game.Core, Game.Testing, glTFast.dots, glTFast.Editor, glTFast.Newtonsoft, HierarchyPlus.Editor, HierarchyPlus.Runtime, MCPForUnity.Editor (+66 more)

### Community 94 - "EnemyFollowState"
Cohesion: 0.18
Nodes (6): EnemyVision, Vector2, Vector3, EnemyFollowState, LastKnownPlayerPosition, StateTag

### Community 95 - "EnemyBrainBase"
Cohesion: 0.24
Nodes (4): Transform, EnemyBrainBase, enemyFSM, IsAttacking

### Community 96 - "Avoidance"
Cohesion: 0.21
Nodes (7): Avoidance, Avoiding, Parent, found, numTries, RaycastHit, wayOut

### Community 97 - "AmmoWheel"
Cohesion: 0.17
Nodes (9): AmmoType, Granade, GrapplingHook, Normal, Tranquilizer, GameObject, RectTransform, AmmoWheel (+1 more)

### Community 98 - "UIActions"
Cohesion: 0.21
Nodes (8): UIActions, @Cancel, @Click, enabled, @MiddleClick, @RightClick, @ScrollWheel, @Submit

### Community 99 - "InventoryModel"
Cohesion: 0.16
Nodes (9): InventoryModel, BuildingSlotCount, PlayerSlotCount, SlotCount, ItemStack, IsEmpty, Test, InventoryModelTests (+1 more)

### Community 100 - ".Apply"
Cohesion: 0.22
Nodes (5): Quest, Image, Quest, Sprite, QuestVisuals

### Community 101 - "unityeditor"
Cohesion: 0.07
Nodes (15): MenuItem, AsmdefInstaller, SceneAsset, ReadmeReader, DenysAlmaral, HierarchyPlus.Editor, Game.Editor, system_io (+7 more)

### Community 102 - "gen_drawio.py"
Cohesion: 0.17
Nodes (19): collections, html, math, build(), class_box(), colors_of(), feature_page(), measure_container() (+11 more)

### Community 103 - "HierarchyDataCache"
Cohesion: 0.27
Nodes (7): DevelopmentStatusTag, Dictionary, GameObject, Cache, HierarchyDataCache, Cache, HierarchyCategoryTag

### Community 104 - "ShellPickup"
Cohesion: 0.23
Nodes (4): Animator, Coroutine, IEnumerator, ShellPickup

### Community 105 - "CheckPointItem"
Cohesion: 0.22
Nodes (7): IEnumerator, LineRenderer, Rigidbody2D, CheckPointItem, ActionHint, KeepInInventory, SourceSlotIndex

### Community 106 - "InventorySlot"
Cohesion: 0.09
Nodes (15): PointerEventData, SellSlot, Color, Image, PointerEventData, InventorySlot, Transform, GameObject (+7 more)

### Community 107 - "DevPanel"
Cohesion: 0.16
Nodes (8): Button, GameObject, List, DevItemButton, DevPanel, DevItemButton, TMP_Dropdown, Toggle

### Community 108 - "ItemDrop"
Cohesion: 0.29
Nodes (3): IEnumerator, ItemDrop, ActiveItem

### Community 109 - ".Unregister"
Cohesion: 0.18
Nodes (5): TearDown, Test, InventoryAccessTests, SetUp, SetUp

### Community 110 - "com.unity.nuget.newtonsoft-json"
Cohesion: 0.11
Nodes (18): dependencies, depth, hash, source, version, dependencies, depth, source (+10 more)

### Community 111 - "com.unity.ext.nunit"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, version, dependencies, depth, source, url (+4 more)

### Community 112 - "CollectableItemBase"
Cohesion: 0.33
Nodes (5): CollectableItemBase, CanInteract, IsCollectable, IsFeatureUnlocked, TakesPlayerLock

### Community 113 - "PlayerInteriorState"
Cohesion: 0.13
Nodes (14): Transform, Vector3, PlayerInterior, House, None, Shop, UpgradeShop, PlayerInteriorState (+6 more)

### Community 115 - "ChestInteract"
Cohesion: 0.11
Nodes (16): Sprite, IInteractPopupView, IsOpen, Collider2D, HashSet, MonoBehaviour, Sprite, ChestInteract (+8 more)

### Community 116 - "Gameplay_Entities.md"
Cohesion: 0.29
Nodes (5): Event subscriptions, Files, Gameplay/Entities, Files, UI/Interactions

### Community 117 - "NPCManager"
Cohesion: 0.16
Nodes (9): Sprite, CharacterData, Dictionary, List, CharacterDatabase, Dictionary, Sprite, NPCManager (+1 more)

### Community 118 - ".Pick"
Cohesion: 0.25
Nodes (9): IReadOnlyList, IWeightedEntry, Weight, WeightedPicker, Test, Entry, Weight, WeightedPickerTests (+1 more)

### Community 119 - "ConfirmationDialog"
Cohesion: 0.14
Nodes (11): Action, IModalPrompt, CanShow, ModalPrompt, Current, Action, Button, GameObject (+3 more)

### Community 120 - "LandUI"
Cohesion: 0.14
Nodes (9): alpha, Button, CanvasGroup, Dictionary, interactable, TMP_Text, LandUI, blocksRaycasts (+1 more)

### Community 121 - "UNITY PROJECT AI GUIDE"
Cohesion: 0.17
Nodes (11): AI Workflow, Architecture Rules, Comment Rules, Existing Managers, Git Rules, graphify, Project Overview, Save System Rules (+3 more)

### Community 122 - "UpgradeSlot"
Cohesion: 0.13
Nodes (8): UpgradeManager, Button, Color, Image, PointerEventData, RectTransform, TMP_Text, UpgradeSlot

### Community 123 - "com.unity.modules.ai"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 124 - "Core (51 files)"
Cohesion: 0.12
Nodes (14): GameLog, GameSession, State, GameState, Land, Menu, Ocean, List (+6 more)

### Community 125 - "HierarchyPlus — Installation Guide"
Cohesion: 0.20
Nodes (9): 1. Copy the Package, 2. Install Assembly Definitions, 3. Verify, 4. Tag Your GameObjects, 5. Component Minimap, 6. Unity Version Notes, Category Tag, Development Status Tag (+1 more)

### Community 126 - "MotionBendTracking"
Cohesion: 0.27
Nodes (7): Queue, Vector3, MotionBendTracking, Capacity, Count, MotionData, MotionData

### Community 127 - "NextDayAnimationUI"
Cohesion: 0.17
Nodes (6): Animator, CanvasGroup, Coroutine, IEnumerator, NextDayAnimationUI, IsPlaying

### Community 128 - "FishAlertUI"
Cohesion: 0.23
Nodes (4): Coroutine, GameObject, IEnumerator, FishAlertUI

### Community 130 - "EnemyStateBase"
Cohesion: 0.12
Nodes (9): LayerMask, Vector2, EnemyStateBase, enemyBrainContext, enemyFSM, StateTag, Vector2, EnemyFleeState (+1 more)

### Community 131 - "VerletRope"
Cohesion: 0.21
Nodes (9): Settings, LayerMask, List, Vector2, Seg, Settings, VerletRope, Count (+1 more)

### Community 132 - "QuestSplitTests"
Cohesion: 0.29
Nodes (4): TearDown, Test, QuestSplitTests, RecordingInventory

### Community 133 - "SaveMenuUI"
Cohesion: 0.20
Nodes (6): GameObject, SaveMenuUI, Button, GameObject, TMP_Text, SaveSlotUI

### Community 134 - "IntroPlayer"
Cohesion: 0.09
Nodes (12): Button, IntroPlayer, Animator, GameObject, IEnumerator, LandOpeningController, Animator, CanvasGroup (+4 more)

### Community 135 - "GrapplingHookBullet"
Cohesion: 0.31
Nodes (6): Collider2D, IEnumerator, Transform, Vector2, GrapplingHookBullet, AttachedToObject

### Community 136 - "CategoryColorMap"
Cohesion: 0.42
Nodes (5): Color, Dictionary, HierarchyCategory, Texture2D, CategoryColorMap

### Community 138 - "ObjectPoolManager"
Cohesion: 0.21
Nodes (8): IPoolable, Dictionary, GameObject, Quaternion, Queue, Vector3, ObjectPoolManager, DestroyGameObjectOnDuplicate

### Community 139 - "AudioManager"
Cohesion: 0.10
Nodes (14): AudioSource, LoadSceneMode, Scene, AudioChannel, BGM, Master, SFX, AudioManager (+6 more)

### Community 140 - "EnemyHitbox"
Cohesion: 0.16
Nodes (10): Collider2D, EnemyHitbox, HitPart, Body, Head, Tail, GameObject, Vector2 (+2 more)

### Community 141 - "Bird"
Cohesion: 0.43
Nodes (3): SpriteRenderer, Transform, Bird

### Community 142 - "Item"
Cohesion: 0.04
Nodes (35): InventoryItemDatabase, GameObject, Sprite, Item, CanBuy, CanSell, ItemDescription, ItemID (+27 more)

### Community 143 - "graphify reference: extra exports and benchmark"
Cohesion: 0.22
Nodes (8): graphify reference: extra exports and benchmark, Step 6b - Wiki (only if --wiki flag), Step 7 - Neo4j export (only if --neo4j or --neo4j-push flag), Step 7a - FalkorDB export (only if --falkordb or --falkordb-push flag), Step 7b - SVG export (only if --svg flag), Step 7c - GraphML export (only if --graphml flag), Step 7d - MCP server (only if --mcp flag), Step 8 - Token reduction benchmark (only if total_words > 5000)

### Community 144 - "HierarchyPlusCore"
Cohesion: 0.25
Nodes (3): GameObject, Rect, HierarchyPlusCore

### Community 145 - "MissingScriptDetector"
Cohesion: 0.32
Nodes (6): Dictionary, GameObject, GUIContent, Rect, MissingScriptDetector, WarningContent

### Community 146 - "Parallax"
Cohesion: 0.25
Nodes (5): SpriteRenderer, Transform, Vector2, Vector3, Parallax

### Community 147 - "SaveSystem"
Cohesion: 0.27
Nodes (4): EncryptionHelper, List, SaveSystem, LegacyPath

### Community 148 - "DoorQuestMarker"
Cohesion: 0.12
Nodes (10): CanvasGroup, IEnumerator, Transform, DoorInteract, List, DoorQuestMarker, TutorialAction, GameObject (+2 more)

### Community 149 - "Phase 12: static classes, names, comments, dead button calls, folders"
Cohesion: 0.14
Nodes (9): IDamagePopup, GrenadeBullet, IEnumerator, TextMeshProUGUI, DamagePopup, KeepUpright, Test, GameplayUiEventTests (+1 more)

### Community 150 - "parse_types"
Cohesion: 0.17
Nodes (9): match_brace(), Member, member_headers(), parse_members(), parse_types(), Find type declarations in code; returns list of TypeInfo (nested types…, Yield top-level member declaration headers of a type body., split_top() (+1 more)

### Community 151 - "UIType"
Cohesion: 0.29
Nodes (7): UIType, Inventory, MainMenu, PauseMenu, Quest, SettingMenu, SettingPause

### Community 152 - "UI (58 files)"
Cohesion: 0.10
Nodes (11): TMP_Text, LoadingTextAnimation, MainMenu, IEnumerator, NotificationUI, SaveNotificationUI, Button, GameObject (+3 more)

### Community 153 - "CategoryIconMap"
Cohesion: 0.48
Nodes (4): Dictionary, HierarchyCategory, Texture2D, CategoryIconMap

### Community 155 - "QuestDetailPanel"
Cohesion: 0.16
Nodes (9): Action, Quest, IQuestOfferView, Action, Button, Image, Quest, TextMeshProUGUI (+1 more)

### Community 156 - "PlayerFeedbackVFX"
Cohesion: 0.28
Nodes (4): Coroutine, IEnumerator, Image, PlayerFeedbackVFX

### Community 157 - "UISettingManager"
Cohesion: 0.39
Nodes (3): GameObject, UISettingManager, IsOpen

### Community 158 - "InteractableNextDay"
Cohesion: 0.22
Nodes (4): Button, Collider2D, GameObject, InteractableNextDay

### Community 159 - ".TryOpenScript"
Cohesion: 0.60
Nodes (3): Component, ScriptNavigator, MonoScript

### Community 160 - "KeyAction"
Cohesion: 0.18
Nodes (11): KeyAction, Aim, Attack, Interact, Inventory, Move, Sprint, SwitchItem (+3 more)

### Community 161 - "PlayerMovement"
Cohesion: 0.10
Nodes (10): Rigidbody2D, Vector2, PlayerMovement, DashSpeed, FaceDir, IsLand, MoveSpeed, SprintSpeed (+2 more)

### Community 162 - "Gameplay_Services.md"
Cohesion: 0.15
Nodes (11): Core/Data/Upgrades, Files, Files, Gameplay/Projectiles, Files, Gameplay/RopeSystems, Files, Gameplay/Services (+3 more)

### Community 163 - "SimpleAnimLoop"
Cohesion: 0.47
Nodes (3): Animator, IEnumerator, SimpleAnimLoop

### Community 164 - "NotificationMessage"
Cohesion: 0.08
Nodes (20): NotificationChannel, Center, General, Resource, System, Sprite, NotificationMessage, Channel (+12 more)

### Community 165 - "graphify reference: query, path, explain"
Cohesion: 0.33
Nodes (5): For /graphify explain, For /graphify path, graphify reference: query, path, explain, Step 0 — Constrained query expansion (REQUIRED before traversal), Step 1 — Traversal

### Community 169 - "SwimState"
Cohesion: 0.40
Nodes (5): SwimState, Biting, Reaching, Stopped, Swimming

### Community 170 - "DialogueState"
Cohesion: 0.21
Nodes (7): GameObject, HashSet, Transform, DialogueState, BlocksInteraction, IsShowing, RuntimeInitializeOnLoadMethod

### Community 171 - "EnemyLoot"
Cohesion: 0.33
Nodes (3): EnemyLoot, CanInteract, IsFeatureUnlocked

### Community 172 - "RopeSplitTests"
Cohesion: 0.21
Nodes (7): BoxCollider2D, GameObject, Settings, TearDown, Test, Vector2, RopeSplitTests

### Community 174 - ".Info"
Cohesion: 0.10
Nodes (20): Object, Action, InputAction, InputSystem_Actions, KeyBindManager, Actions, Persistent, SavePath (+12 more)

### Community 175 - "SettingTabManager"
Cohesion: 0.39
Nodes (4): Button, Color, GameObject, SettingTabManager

### Community 176 - "ButtonHoverScale"
Cohesion: 0.29
Nodes (4): PointerEventData, TMP_Text, Vector3, ButtonHoverScale

### Community 177 - "RebindButtonBase"
Cohesion: 0.15
Nodes (8): Coroutine, IEnumerator, TMP_Text, KeyBindErrorUI, Action, Button, TMP_Text, RebindButtonBase

### Community 178 - "Singleton"
Cohesion: 0.14
Nodes (7): Singleton, DestroyGameObjectOnDuplicate, Instance, IsDuplicate, Persistent, CameraShakeManager, CinemachineImpulseSource

### Community 179 - "QuestSlot"
Cohesion: 0.24
Nodes (8): Action, Button, Image, Quest, Sprite, TextMeshProUGUI, TMP_Text, QuestSlot

### Community 180 - "TrashBin"
Cohesion: 0.15
Nodes (11): Image, PointerEventData, Sprite, Vector3, TrashBin, Color, Image, PointerEventData (+3 more)

### Community 181 - ".Notify"
Cohesion: 0.11
Nodes (9): Sprite, NotificationManager, CutsceneSkipHint, Sprite, ResourceNotificationPresenter, SetUp, TearDown, Test (+1 more)

### Community 182 - "graphify reference: add a URL and watch a folder"
Cohesion: 0.50
Nodes (3): For /graphify add, For --watch, graphify reference: add a URL and watch a folder

### Community 183 - "graphify reference: commit hook and native CLAUDE.md integration"
Cohesion: 0.50
Nodes (3): For git commit hook, For native CLAUDE.md integration, graphify reference: commit hook and native CLAUDE.md integration

### Community 184 - "graphify reference: incremental update and cluster-only"
Cohesion: 0.50
Nodes (3): For --cluster-only, For --update (incremental re-extraction), graphify reference: incremental update and cluster-only

### Community 188 - "Metrics"
Cohesion: 0.18
Nodes (11): `Debug.Log*` calls — 85 in 35 files, Dependency cycles (type level), Largest types, Layer violations — 0 type links, Metrics, Most depended-on (fan-in), Most dependencies (fan-out), Per-frame methods (`Update`/`FixedUpdate`/`LateUpdate`) — 31 in 30 files (+3 more)

### Community 189 - "QuestCompleteUI"
Cohesion: 0.31
Nodes (6): Coroutine, GameObject, IEnumerator, Quest, TextMeshProUGUI, QuestCompleteUI

### Community 192 - "IDialogueView"
Cohesion: 0.09
Nodes (16): GameObject, Sprite, IDialogueView, IsOpen, Panel, GameObject, Image, Sprite (+8 more)

### Community 193 - ".Close"
Cohesion: 0.18
Nodes (6): Action, PlayerUIState, IsLocked, ResetCount, EscClosablePanel, Stack

### Community 194 - "EntityDeathHandler"
Cohesion: 0.29
Nodes (4): Color, Rigidbody2D, SpriteRenderer, EntityDeathHandler

### Community 195 - "PlayerSprintVFX"
Cohesion: 0.31
Nodes (5): ParticleSystem, Quaternion, Vector2, Vector3, PlayerSprintVFX

### Community 196 - "SceneLoadContext"
Cohesion: 0.40
Nodes (4): Vector3, SceneLoadContext, IsLoading, Position

### Community 197 - "SaveBridge"
Cohesion: 0.15
Nodes (8): SaveSlotMeta, IEnumerator, List, LoadSceneMode, Scene, SaveBridge, CurrentSlot, DestroyGameObjectOnDuplicate

### Community 198 - "TutorialGate"
Cohesion: 0.24
Nodes (4): TutorialGate, CurrentPrompt, IsRestricting, RequiredAction

### Community 199 - ".LoadSaveState"
Cohesion: 0.24
Nodes (6): QuestSaveData, Func, IEnumerable, List, Quest, QuestSaveMapper

### Community 200 - "UpgradeTooltip"
Cohesion: 0.20
Nodes (9): CanvasGroup, GameObject, List, RectTransform, TMP_Text, Transform, UpgradeTooltip, Canvas (+1 more)

### Community 201 - "EnemyAgent"
Cohesion: 0.33
Nodes (4): EnemyAgent, AutoReturnDelay, BaseProfile, requiresNavMesh

### Community 202 - ".Pick"
Cohesion: 0.24
Nodes (9): Func, IEnumerable, Vector2, InteractionTargeting, Test, Vector2, InteractionTargetingTests, Target (+1 more)

### Community 203 - "AcceptedQuestListUI"
Cohesion: 0.15
Nodes (11): TextMeshProUGUI, Transform, AcceptedQuestListUI, CanvasGroup, Image, TextMeshProUGUI, AcceptedQuestSlot, QuestClassification (+3 more)

### Community 204 - "com.h8man.2d.navmeshplus"
Cohesion: 0.33
Nodes (6): dependencies, depth, hash, source, version, com.h8man.2d.navmeshplus

### Community 205 - "PlayerMoney"
Cohesion: 0.08
Nodes (14): PlayerMoney, Current, CurrentMoney, Money, Dictionary, Predicate, Dictionary, Predicate (+6 more)

### Community 206 - "QuestTrackerUI"
Cohesion: 0.33
Nodes (4): GameObject, Image, TextMeshProUGUI, QuestTrackerUI

### Community 207 - "GameLogTests"
Cohesion: 0.50
Nodes (3): Test, GameLogTests, ConditionalAttribute

### Community 209 - "VolumeSliderUI"
Cohesion: 0.40
Nodes (3): Slider, TMP_Text, VolumeSliderUI

### Community 211 - "Cutscene"
Cohesion: 0.18
Nodes (8): Action, Component, List, Rigidbody2D, Cutscene, CutsceneId, CutsceneTrackBinding, PlayableDirector

### Community 212 - "guid_snapshot.py"
Cohesion: 0.20
Nodes (7): json, pathlib, re, sys, Snapshot and compare .meta GUIDs under Assets/Project_Ocean/Scripts. Moves must…, Self-test for gen_uml's parser. Run: python tools/uml/test_gen_uml.py, unittest

### Community 214 - "Game.Tests"
Cohesion: 0.15
Nodes (4): Game.Tests, nunit_framework, system_text_regularexpressions, unityengine_testtools

### Community 215 - "Gameplay (131 files)"
Cohesion: 0.07
Nodes (25): CutsceneGate, IsPlaying, AnimHash, IBrainHitReaction, State, Moving, Pausing, LayerMask (+17 more)

### Community 216 - "00-overview.md"
Cohesion: 0.11
Nodes (15): Layer dependencies, Module dependencies, Module table, Overview — layers & modules, Core/Inputs/KeyBind, Files, Core/Inputs/KeyBind/MoveDirection, Files (+7 more)

### Community 218 - "UpgradeServiceEventTests"
Cohesion: 0.22
Nodes (4): SetUp, TearDown, Test, UpgradeServiceEventTests

### Community 219 - "1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)"
Cohesion: 0.12
Nodes (7): Slider, AudioSliderBinder, SoundKeys, RevealAfterTotalDives, BootstrapLoader, LoadingButton, 1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)

### Community 220 - ".AttachRope"
Cohesion: 0.32
Nodes (5): GameObject, GameObject, LineRenderer, Rigidbody2D, RopeSystemUtil

### Community 223 - "PunchScale"
Cohesion: 0.28
Nodes (4): Coroutine, IEnumerator, Vector3, PunchScale

### Community 225 - "EnemyAttackState"
Cohesion: 0.33
Nodes (3): EnemyAttackState, IsAttacking, StateTag

### Community 226 - "NotificationLine"
Cohesion: 0.39
Nodes (5): CanvasGroup, IEnumerator, Image, TextMeshProUGUI, NotificationLine

### Community 227 - "RopeCornerFinder"
Cohesion: 0.60
Nodes (4): Collider2D, List, Vector2, RopeCornerFinder

### Community 228 - "EnemyWanderState"
Cohesion: 0.27
Nodes (3): Vector3, EnemyWanderState, StateTag

### Community 231 - "Core.md"
Cohesion: 0.11
Nodes (19): Core, Files, Core/Save, Core/Save/DayReset, Event subscriptions, Files, Files, Core/Scenes (+11 more)

### Community 233 - "NormalBullet"
Cohesion: 0.43
Nodes (3): Rigidbody2D, NormalBullet, MaxRange

### Community 237 - "GameManager"
Cohesion: 0.14
Nodes (10): GameState, LoadSceneMode, Rigidbody2D, Scene, Transform, Vector3, GameManager, GameState (+2 more)

### Community 239 - "UIType"
Cohesion: 0.25
Nodes (8): UIType, Inventory, MainMenu, None, PauseMenu, Quest, SettingMenu, SettingPause

### Community 240 - "Gameplay_Entities_Players.md"
Cohesion: 0.07
Nodes (29): Core/Data/Items, Files, Files, Gameplay/AudioSetting, Event subscriptions, Files, Gameplay/Entities/Players, Files (+21 more)

### Community 241 - "GameFlowEvents"
Cohesion: 0.33
Nodes (3): GameFlowEvents, Test, GameFlowEventsTests

### Community 248 - "GameSessionTests"
Cohesion: 0.29
Nodes (4): SetUp, TearDown, Test, GameSessionTests

### Community 250 - "EventInteractable"
Cohesion: 0.20
Nodes (5): Quest, GameObject, NpcQuestMarker, UnityEvent, EventInteractable

### Community 252 - "QuestCategorySlot"
Cohesion: 0.38
Nodes (3): Action, Button, QuestCategorySlot

### Community 254 - "PlayerStabAttack"
Cohesion: 0.50
Nodes (3): GameObject, IEnumerator, PlayerStabAttack

### Community 255 - "PlayerSplitTests"
Cohesion: 0.24
Nodes (7): GameObject, List, Object, TearDown, Test, PlayerSplitTests, OneItemInventory

### Community 261 - "EntityAnimation"
Cohesion: 0.38
Nodes (4): Animator, HashSet, EntityAnimation, RuntimeAnimatorController

### Community 262 - "ScriptableObject"
Cohesion: 0.18
Nodes (12): SwimConfig, BulletData, RopeStructureData, EnemyType, Boss, Normal, Special, FishData (+4 more)

## Knowledge Gaps
- **1104 isolated node(s):** `IsVisible`, `IconStyle`, `BadgeStyle`, `WarningContent`, `None` (+1099 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1919 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **26 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Gameplay (131 files)` connect `Gameplay (131 files)` to `OxygenRope`, `EnemyNavAgentBase`, `ExploreAreaTrigger`, `ObjectWrapPoint`, `Enemy`, `QuestProgressTracker`, `EnemyBrain`, `ShopManager`, `Quest`, `TutorialPromptUI`, `InventoryManager`, `QuestStep`, `CutsceneManager`, `PlayerGunHandler`, `Player`, `NPC`, `3. Root causes`, `GameFeature`, `EnemyBrainContext`, `TrashInteract`, `LinearPatrolBrain`, `EntityHealth`, `EnemySpawner`, `Bullet`, `TutorialActionId`, `MonoBehaviour`, `SpawnCatalog`, `Player`, `EnemyFacingController`, `Entity`, `PlayerHealth`, `.FromSave`, `PlayerSelfDialogueUI`, `ItemSpawner`, `EnemyNavPathfinder`, `QuestManager`, `QuestCategory`, `Boid`, `QuestStepDataDrawer`, `Interactable`, `EnemyFollowState`, `EnemyBrainBase`, `InventoryModel`, `.Apply`, `ShellPickup`, `DevPanel`, `ItemDrop`, `CollectableItemBase`, `PlayerInteriorState`, `ChestInteract`, `NPCManager`, `.Pick`, `ConfirmationDialog`, `FishAlertUI`, `EnemyStateBase`, `VerletRope`, `GrapplingHookBullet`, `AudioManager`, `Bird`, `Item`, `DoorQuestMarker`, `Phase 12: static classes, names, comments, dead button calls, folders`, `QuestDetailPanel`, `PlayerMovement`, `SimpleAnimLoop`, `.CompleteProgress`, `EnemyLoot`, `Singleton`, `.Notify`, `IDialogueView`, `EntityDeathHandler`, `PlayerSprintVFX`, `SaveBridge`, `.LoadSaveState`, `EnemyAgent`, `.Pick`, `AcceptedQuestListUI`, `PlayerMoney`, `VolumeSliderUI`, `Cutscene`, `1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)`, `.AttachRope`, `EnemyAttackState`, `RopeCornerFinder`, `EnemyWanderState`, `NormalBullet`, `GameManager`, `EventInteractable`, `PlayerStabAttack`, `EntityAnimation`?**
  _High betweenness centrality (0.225) - this node is a cross-community bridge._
- **Why does `All scripts` connect `QuestStepDataDrawer` to `UI (58 files)`, `03-all-scripts.md`, `Core (51 files)`, `Gameplay (131 files)`?**
  _High betweenness centrality (0.097) - this node is a cross-community bridge._
- **Why does `FishMotion` connect `FishMotion` to `Avoidance`, `ScriptableObject`, `.Awake`, `.BeginReach`, `.Motion`, `SwimState`, `system_collections_generic`, `ReachPoint`, `MonoBehaviour`, `.PingTarget`, `MotionBendTracking`?**
  _High betweenness centrality (0.047) - this node is a cross-community bridge._
- **Are the 166 inferred relationships involving `Gameplay (131 files)` (e.g. with `AudioChannel` and `AudioManager`) actually correct?**
  _`Gameplay (131 files)` has 166 INFERRED edges - model-reasoned connections that need verification._
- **What connects `IsVisible`, `IconStyle`, `BadgeStyle` to the rest of the system?**
  _1104 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `project-ocean.slnx` be split into smaller, more focused modules?**
  _Cohesion score 0.02666666666666667 - nodes in this community are weakly interconnected._
- **Should `Game.Core` be split into smaller, more focused modules?**
  _Cohesion score 0.05 - nodes in this community are weakly interconnected._