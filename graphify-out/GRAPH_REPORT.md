# Graph Report - project-ocean  (2026-10-01)

## Corpus Check
- 438 files · ~427,554 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 2653 file(s) not represented in the graph (top: .meta 1807, .asset 511, .prefab 135)

## Summary
- 4985 nodes · 10085 edges · 258 communities (229 shown, 29 thin omitted)
- Extraction: 88% EXTRACTED · 12% INFERRED · 0% AMBIGUOUS · INFERRED: 1240 edges (avg confidence: 0.92)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `60fb6628`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- project-ocean.slnx
- Game.Core
- UIManager
- KeyBindManager
- OxygenRope
- manifest.json
- dependencies
- EnemyNavAgentBase
- InputReader
- Phase 6: cleanup (low risk, any time after Phase 4)
- Player
- ExploreAreaTrigger
- AdditiveSceneSetupWindow
- system_collections_generic
- ObjectWrapPoint
- Enemy
- packages-lock.json
- Refactor Phase 5a: Split QuestManager — Implementation Plan
- com.unity.mathematics
- 3. Root causes
- ShopManager
- Game.UI
- Quest
- TutorialActionId
- Phase 7a: decouple the GameManager hub and player-side upward links
- QuestStep
- com.unity.modules.imgui
- Phase 3: cut remaining wrong-way links (medium risk)
- com.unity.2d.common
- gen_uml.py
- com.unity.modules.animation
- FishMotion
- @InputSystem_Actions
- GameHUD
- CutsceneManager
- PlayerGunHandler
- com.unity.collections
- unityengine
- NPC
- What You Must Do When Invoked
- com.unity.render-pipelines.core
- HierarchyCategory
- UpgradeService
- GameFeature
- EnemyFSM
- TrashInteract
- dependencies
- com.unity.test-framework
- LinearPatrolBrain
- README.md
- QuestData.cs
- EnemySpawner
- EntityDeathHandler
- CallbackContext
- IEnemyBrain
- MonoBehaviour
- UpgradeData
- DayManager
- com.unity.modules.physics
- EnemyFacingController
- Gameplay_Entities_Enemies.md
- UI.md
- Entity
- EntityHealth
- .FromSave
- Player
- EnemyKillTracker
- ComponentInspectorPopup
- ComponentMinimap
- .DrawCategory
- NotificationMessage
- PlayerSelfDialogueUI
- com.unity.editorcoroutines
- ItemSpawner
- unityengine_inputsystem
- Refactor Phase 5c: Split Player — Implementation Plan
- InventoryManager
- ReachPoint
- PlayerActions
- ChestInteract
- RopeBreakCountdownUI
- com.unity.2d.tilemap
- com.unity.dt.app-ui
- DraggableItem
- EnemyNavPathfinder
- QuestManager
- QuestBoardUI
- Bullet
- Boid
- com.unity.modules.uielements
- QuestStepDataDrawer
- Interactable
- com.unity.modules.subsystems
- project-ocean-worktree2.slnx
- EnemyFollowState
- EnemyBrainContext
- Avoidance
- AmmoWheel
- UIActions
- InventoryModel
- QuestSlot
- PlayerHealth
- gen_drawio.py
- HierarchyDataCache
- DamagePoint
- NextDayAnimationUI
- SaveMenuUI
- DevPanel
- ItemDrop
- PlayerSplitTests
- com.unity.nuget.newtonsoft-json
- com.unity.ext.nunit
- ReadmeReaderEditor
- .InteractPosition
- .UnregisterCallbacks
- Phase 8: one Interactable hierarchy
- EnemyLoot
- ShelleController
- QuestCompleteUI
- ConfirmationDialog
- LandUI
- UNITY PROJECT AI GUIDE
- PlayerFeedbackVFX
- com.unity.modules.ai
- UNITY PROJECT AI GUIDE
- HierarchyPlus — Installation Guide
- MotionBendTracking
- .Close
- FishAlertUI
- ScriptNamingTests
- KeyAction
- VerletRope
- FakeInventory
- .Info
- LoadingUI
- GrapplingHookBullet
- CategoryColorMap
- ButtonHoverScale
- AudioManager
- EnemyHitbox
- Bird
- Item
- graphify reference: extra exports and benchmark
- HierarchyPlusCore
- MissingScriptDetector
- Parallax
- GameObjectSpawnManager
- TutorialActionMarker
- SlotBuilderTests
- parse_types
- QuestType
- TipPanelUI
- CategoryIconMap
- QuestDetailPanel
- NPCManager
- DialogueState
- InteractableNextDay
- .TryOpenScript
- com.unity.modules.physics2d
- PlayerMovement
- Gameplay_Entities_Players.md
- SimpleAnimLoop
- Phase 2: put types in the right layer (medium risk, biggest win)
- graphify reference: query, path, explain
- MoveKeyBindUI
- .Awake
- .BeginReach
- SwimState
- TutorialPromptUI
- IntroPlayer
- RopeSplitTests
- Notification Channels Implementation Plan
- UISettingManager
- SettingTabManager
- DoorQuestMarker
- KeyBindUI
- Notification Channels — Design
- UIWindow
- TrashBin
- .Notify
- graphify reference: add a URL and watch a folder
- graphify reference: commit hook and native CLAUDE.md integration
- graphify reference: incremental update and cluster-only
- HierarchyPlus — Folder Structure
- graphify reference: GitHub clone and cross-repo merge
- graphify reference: transcribe video and audio
- Metrics
- Gameplay/QuestSystems/Quest
- .claude/CLAUDE.md
- extraction-spec.md
- IDialogueView
- NotificationView
- LoadingManager
- .HandleBulletHit
- TutorialUIMarker
- QuestSaveData
- UML refresh & refactor roadmap: design
- PlayerInteriorState
- UpgradeTooltip
- GameManager
- .PickClosest
- QuestCategory
- SaveBridge
- .LimitBreached
- LandOpeningController
- Risks
- EnemyBaseProfile
- VolumeSliderUI
- Refactor Phase 3c-1: Move Gameplay Orchestrators Out of Core — Implementation Plan
- Cutscene
- guid_snapshot.py
- ReviceQuestListUI
- Game.Tests
- 1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)
- 00-overview.md
- .MovePlayerTo
- PlayerCamera
- LoadingButton
- .AttachRope
- SaveData
- .Shooting
- Package assembly GUIDs (resolved from Library/PackageCache)
- QuestStatus
- EnemyAttackState
- NotificationLine
- Refactor Phase 5b: Split OxygenRope — Implementation Plan
- EnemyWanderState
- InteractUI
- SceneLoadContext
- Core.md
- unityeditor
- ResourceNotificationPresenter
- Task 1: Stop counting member names as type references
- ParserTest
- com.unity.collab-proxy
- Cleanup Phase 6e: Items Feature Folder, Checkpoint to Deprecated — Implementation Plan
- .RemoveAllWhere
- .RaiseMoneyRewarded_PassesAmount
- UI_Inventory.md
- UIFadeBackground
- EncryptionHelper
- Page
- DrawioTest
- NotificationUI
- GameSessionTests
- UIType
- Cleanup Phase 6c: Find* Calls — Implementation Plan
- NormalBullet
- Settings
- .LogWarning
- CameraShakeManager
- AsmdefInstaller
- UpgradeServiceEventTests
- PlayerMoney

## God Nodes (most connected - your core abstractions)
1. `Game.Core` - 259 edges
2. `Game.Gameplay` - 215 edges
3. `InventoryManager` - 108 edges
4. `Item` - 87 edges
5. `Game.UI` - 81 edges
6. `FishMotion` - 68 edges
7. `ShopManager` - 66 edges
8. `OxygenRope` - 64 edges
9. `QuestManager` - 60 edges
10. `EnemyNavAgentBase` - 59 edges

## Surprising Connections (you probably didn't know these)
- `5. Risks` --references--> `SaveData`  [INFERRED]
  docs/uml/findings.md → Assets/Project_Ocean/Scripts/Core/Save/SaveData.cs
- `Task 3: LoadingManager to Core behind ILoadingVisual` --references--> `LoadingManager`  [INFERRED]
  docs/superpowers/plans/2026-09-26-refactor-phase3ab-metrics-loading.md → Assets/Project_Ocean/Scripts/Core/Scenes/LoadingManager.cs
- `Task 3: Verify live, docs` --references--> `OxygenRope`  [INFERRED]
  docs/superpowers/plans/2026-09-29-refactor-phase5b-oxygenrope.md → Assets/Project_Ocean/Scripts/Gameplay/Entities/Players/OxygenTube/OxygenRope.cs
- `Moves (Unity `AssetDatabase.MoveAsset`, so GUIDs and prefab/scene links are kept)` --references--> `RopeCornerFinder`  [INFERRED]
  docs/superpowers/plans/2026-09-29-cleanup-phase6e-items-folder.md → Assets/Project_Ocean/Scripts/Gameplay/Entities/Players/OxygenTube/RopeCornerFinder.cs
- `Comment updates (text only)` --references--> `Player`  [INFERRED]
  docs/superpowers/plans/2026-09-29-cleanup-phase6e-items-folder.md → Assets/Project_Ocean/Scripts/Gameplay/Entities/Players/Player.cs

## Import Cycles
- None detected.

## Communities (258 total, 29 thin omitted)

### Community 0 - "project-ocean.slnx"
Cohesion: 0.03
Nodes (74): Assembly-CSharp, Assembly-CSharp-Editor, Game.Deprecated, Game.Editor, Game.Gameplay, Game.Tests.EditMode, Game.UI, glTFast (+66 more)

### Community 1 - "Game.Core"
Cohesion: 0.05
Nodes (7): Tags, AnimHash, Game.Core, Game.Gameplay, system_collections, Unity.Cinemachine, unityengine_ai

### Community 2 - "UIManager"
Cohesion: 0.13
Nodes (6): Dictionary, UIManager, CurrentUI, Instance, IsUIOpen, UIType

### Community 3 - "KeyBindManager"
Cohesion: 0.14
Nodes (15): Action, InputAction, InputSystem_Actions, KeyBindManager, Actions, Instance, SavePath, MoveDirection (+7 more)

### Community 4 - "OxygenRope"
Cohesion: 0.10
Nodes (19): Coroutine, GameObject, LayerMask, LineRenderer, List, Rigidbody2D, Transform, OxygenRope (+11 more)

### Community 5 - "manifest.json"
Cohesion: 0.03
Nodes (60): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.director, com.unity.modules.imageconversion (+52 more)

### Community 6 - "dependencies"
Cohesion: 0.03
Nodes (61): dependencies, com.coplaydev.unity-mcp, com.h8man.2d.navmeshplus, com.unity.2d.animation, com.unity.2d.aseprite, com.unity.2d.psdimporter, com.unity.2d.sprite, com.unity.2d.spriteshape (+53 more)

### Community 7 - "EnemyNavAgentBase"
Cohesion: 0.08
Nodes (15): FacingTurnMode, NavMeshAgent, EnemyNavAgentBase, ActiveBrain, AutoReturnDelay, defaultFacingTurnMode, enemy, facingCtr (+7 more)

### Community 8 - "InputReader"
Cohesion: 0.05
Nodes (27): SwimConfig, BulletData, RopeStructureData, Sprite, CharacterData, Dictionary, List, CharacterDatabase (+19 more)

### Community 9 - "Phase 6: cleanup (low risk, any time after Phase 4)"
Cohesion: 0.11
Nodes (15): Action, Inventory, Current, GameObject, InventoryUIController, IsOpen, SetUp, SetUp (+7 more)

### Community 10 - "Player"
Cohesion: 0.14
Nodes (11): GameObject, Rigidbody2D, Player, DashSpeed, FaceDir, GunHandler, Movement, OxygenTube (+3 more)

### Community 11 - "ExploreAreaTrigger"
Cohesion: 0.12
Nodes (9): ExploreEvents, Collider2D, List, Transform, DialogueLine, ExploreAreaTrigger, View, DialogueLine (+1 more)

### Community 12 - "AdditiveSceneSetupWindow"
Cohesion: 0.05
Nodes (31): AdditiveSceneEntry, Component, GUIStyle, Vector2, ComponentTooltipWindow, IsVisible, IPoolable, Dictionary (+23 more)

### Community 13 - "system_collections_generic"
Cohesion: 0.04
Nodes (13): Vector3, FishConstant, CutsceneGate, IsPlaying, FishAlive, system, system_collections_generic, system_security_cryptography (+5 more)

### Community 14 - "ObjectWrapPoint"
Cohesion: 0.15
Nodes (15): BoxCollider2D, Collider2D, IEnumerable, List, Vector2, ObjectWrapPoint, WrapPoint, CompositeCollider2D (+7 more)

### Community 15 - "Enemy"
Cohesion: 0.09
Nodes (14): Collision2D, Enemy, Brain, EnemyId, IsAttacking, Player, SleepDistanceSqr, GameObject (+6 more)

### Community 16 - "packages-lock.json"
Cohesion: 0.05
Nodes (41): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.director, com.unity.modules.imageconversion (+33 more)

### Community 17 - "Refactor Phase 5a: Split QuestManager — Implementation Plan"
Cohesion: 0.18
Nodes (9): Action, Func, List, Quest, QuestProgressTracker, QuestSaveMapper, QuestStepEffects, Refactor Phase 5a: Split QuestManager — Implementation Plan (+1 more)

### Community 18 - "com.unity.mathematics"
Cohesion: 0.06
Nodes (40): dependencies, depth, source, url, version, dependencies, depth, source (+32 more)

### Community 19 - "3. Root causes"
Cohesion: 0.11
Nodes (14): Transform, Vector3, EnemyBrain, AttackRange, IsAttacking, ViewAngle, ViewDistance, IBrainAttack (+6 more)

### Community 20 - "ShopManager"
Cohesion: 0.07
Nodes (16): Color, Image, PointerEventData, InventorySlot, Transform, Button, GameObject, IEnumerator (+8 more)

### Community 21 - "Game.UI"
Cohesion: 0.05
Nodes (6): IHookable, Game.UI, Game.Deprecated, tmpro, unityengine_eventsystems, unityengine_ui

### Community 22 - "Quest"
Cohesion: 0.08
Nodes (24): IEnumerable, List, Quest, Category, Classification, CurrentCount, CurrentStep, CurrentStepIndex (+16 more)

### Community 23 - "TutorialActionId"
Cohesion: 0.07
Nodes (28): TutorialActionId, AcceptQuest, BackToLand, BreakRopeLimit, BuyItem, Dive, HitEnemy, Interact (+20 more)

### Community 24 - "Phase 7a: decouple the GameManager hub and player-side upward links"
Cohesion: 0.12
Nodes (16): GameFlowEvents, RewardEvents, GameSession, State, GameState, Land, Menu, Ocean (+8 more)

### Community 25 - "QuestStep"
Cohesion: 0.06
Nodes (30): DialogueType, Accept, Finish, InProgress, NotFinish, Offer, Reject, characterId (+22 more)

### Community 26 - "com.unity.modules.imgui"
Cohesion: 0.06
Nodes (35): dependencies, depth, source, url, version, dependencies, depth, source (+27 more)

### Community 27 - "Phase 3: cut remaining wrong-way links (medium risk)"
Cohesion: 0.10
Nodes (20): ILoadingVisual, LoadingType, Land, Menu, Ocean, Mode, Interact, TriggerZone (+12 more)

### Community 28 - "com.unity.2d.common"
Cohesion: 0.06
Nodes (34): dependencies, depth, source, url, version, dependencies, depth, source (+26 more)

### Community 29 - "gen_uml.py"
Cohesion: 0.17
Nodes (21): draw.io Project Structure Diagram — Implementation Plan, class_block(), class_diagram(), edges_of(), main(), md_link_module(), mm(), module_graph() (+13 more)

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
Cohesion: 0.13
Nodes (11): CutsceneEvents, Action, Dictionary, GameObject, IEnumerator, List, LoadSceneMode, Quest (+3 more)

### Community 35 - "PlayerGunHandler"
Cohesion: 0.10
Nodes (17): LayerMask, LineRenderer, ParticleSystem, SpriteRenderer, Transform, Vector2, PlayerGunHandler, AimDir (+9 more)

### Community 36 - "com.unity.collections"
Cohesion: 0.12
Nodes (17): dependencies, dependencies, depth, source, url, version, depth, source (+9 more)

### Community 37 - "unityengine"
Cohesion: 0.07
Nodes (5): unityengine, unityengine_audio, unityengine_events, unityengine_scenemanagement, unityengine_serialization

### Community 38 - "NPC"
Cohesion: 0.08
Nodes (16): Collider2D, GameObject, Quest, Sprite, NPC, HasQuest, NpcId, NpcName (+8 more)

### Community 39 - "What You Must Do When Invoked"
Cohesion: 0.08
Nodes (24): For /graphify add and --watch, For /graphify query, For the commit hook and native CLAUDE.md integration, For --update and --cluster-only, /graphify, Honesty Rules, Interpreter guard for subcommands, Part A - Structural extraction for code files (+16 more)

### Community 40 - "com.unity.render-pipelines.core"
Cohesion: 0.09
Nodes (25): depth, source, version, dependencies, depth, source, version, dependencies (+17 more)

### Community 41 - "HierarchyCategory"
Cohesion: 0.09
Nodes (20): DevelopmentStatus, Complete, Deprecated, New, None, Problem, Review, WorkInProgress (+12 more)

### Community 42 - "UpgradeService"
Cohesion: 0.16
Nodes (13): UpgradeType, Inventory, NormalBullet, Rope, Dictionary, UpgradeService, Instance, File map (paths relative to `Assets/Project_Ocean/Scripts/`) (+5 more)

### Community 43 - "GameFeature"
Cohesion: 0.09
Nodes (21): GameFeature, BackToLand, Combat, EnemySpawn, EnemySpawnLevel1, EnemySpawnLevel2, EnemySpawnLevel3, Interact (+13 more)

### Community 44 - "EnemyFSM"
Cohesion: 0.08
Nodes (22): Dictionary, EnemyFSM, CurrentState, CurrentTag, Vector3, EnemyStateBase, enemyBrainContext, enemyFSM (+14 more)

### Community 45 - "TrashInteract"
Cohesion: 0.19
Nodes (9): Coroutine, GameObject, IEnumerator, MonoBehaviour, Rigidbody2D, Sprite, Vector3, TrashInteract (+1 more)

### Community 46 - "dependencies"
Cohesion: 0.08
Nodes (24): dependencies, depth, hash, source, version, dependencies, depth, source (+16 more)

### Community 47 - "com.unity.test-framework"
Cohesion: 0.09
Nodes (23): dependencies, dependencies, depth, source, url, version, dependencies, depth (+15 more)

### Community 48 - "LinearPatrolBrain"
Cohesion: 0.12
Nodes (10): Collider2D, Component, Rigidbody2D, Transform, Vector2, LinearPatrolBrain, IsAttacking, State (+2 more)

### Community 49 - "README.md"
Cohesion: 0.06
Nodes (21): Inheritance & interfaces, Declared events with no `+=` subscriber found, Event catalogue, Event flow, Core/Datas/ScriptableDatas/Characters, Files, Core/VideoManager, Files (+13 more)

### Community 50 - "QuestData.cs"
Cohesion: 0.09
Nodes (27): DeliverySource, EnemyKills, InventoryHeld, NextQuestMode, List, Sprite, DeliveryCountMode, CombinedTotal (+19 more)

### Community 51 - "EnemySpawner"
Cohesion: 0.14
Nodes (8): GameObject, IEnumerator, LayerMask, List, Rigidbody2D, Vector3, EnemySpawner, ZoneSpawnEntry

### Community 52 - "EntityDeathHandler"
Cohesion: 0.12
Nodes (14): Color, Rigidbody2D, SpriteRenderer, EntityDeathHandler, PlayerRegistry, Local, 10a: `EntityDeathHandler` stops knowing enemies (was deferred 9c), 10c: the self-dialogue "blocks interaction" flag moves to Core (+6 more)

### Community 53 - "CallbackContext"
Cohesion: 0.17
Nodes (3): CallbackContext, IPlayerActions, IUIActions

### Community 54 - "IEnemyBrain"
Cohesion: 0.09
Nodes (16): Transform, IEnemyBrain, IsAttacking, LayerMask, LinearPatrolProfile, RequiresNavMesh, PatrolAxis, Horizontal (+8 more)

### Community 55 - "MonoBehaviour"
Cohesion: 0.08
Nodes (12): Vector3, WaterShadowWiggle, Button, KeyBindResetAllButton, ItemUI, RevealAfterTotalDives, TMP_Text, LoadingTextAnimation (+4 more)

### Community 56 - "UpgradeData"
Cohesion: 0.13
Nodes (10): List, UpgradeData, UpgradeManager, Button, Color, Image, PointerEventData, RectTransform (+2 more)

### Community 57 - "DayManager"
Cohesion: 0.16
Nodes (9): UnityEvent, DayManager, CanDive, CurrentDay, DivesRemaining, DivesToday, HasDivedToday, Instance (+1 more)

### Community 58 - "com.unity.modules.physics"
Cohesion: 0.10
Nodes (20): dependencies, depth, source, version, dependencies, depth, source, version (+12 more)

### Community 59 - "EnemyFacingController"
Cohesion: 0.12
Nodes (12): SpriteRenderer, Transform, CharacterFacing2D, FacingTurnMode, Transform, Vector2, Vector3, EnemyFacingController (+4 more)

### Community 60 - "Gameplay_Entities_Enemies.md"
Cohesion: 0.07
Nodes (30): Core/Pool, Files, Files, Gameplay/AudioSetting, Files, Gameplay/Entities/Enemies/Behaviours, Files, Files (+22 more)

### Community 61 - "UI.md"
Cohesion: 0.08
Nodes (31): Core/Events, Files, Core/Scenes, Files, Editor, Files, Files, Gameplay/Contracts (+23 more)

### Community 62 - "Entity"
Cohesion: 0.14
Nodes (5): Rigidbody2D, Entity, CurrentHealth, IsDead, MaxHealth

### Community 63 - "EntityHealth"
Cohesion: 0.10
Nodes (12): Color, Coroutine, IEnumerator, SpriteRenderer, EntityHealth, CurrentHealth, IsDead, MaxHealth (+4 more)

### Community 64 - ".FromSave"
Cohesion: 0.08
Nodes (22): ItemSaveData, Func, IReadOnlyList, List, InventorySaveMapper, SlotBuilder, TearDown, TearDown (+14 more)

### Community 65 - "Player"
Cohesion: 0.11
Nodes (15): IEnumerator, LineRenderer, Rigidbody2D, CheckPointItem, ActionHint, KeepInInventory, SourceSlotIndex, Player (+7 more)

### Community 66 - "EnemyKillTracker"
Cohesion: 0.18
Nodes (3): HashSet, EnemyKillTracker, Instance

### Community 67 - "ComponentInspectorPopup"
Cohesion: 0.17
Nodes (7): Component, Editor, GUIStyle, Rect, Vector2, ComponentInspectorPopup, PopupWindowContent

### Community 68 - "ComponentMinimap"
Cohesion: 0.22
Nodes (10): Component, Dictionary, GameObject, HashSet, List, Rect, Texture2D, Type (+2 more)

### Community 69 - ".DrawCategory"
Cohesion: 0.18
Nodes (11): GameObject, GUIStyle, Rect, HierarchyPlusDrawer, BadgeStyle, IconStyle, Color, DevelopmentStatus (+3 more)

### Community 70 - "NotificationMessage"
Cohesion: 0.14
Nodes (12): Sprite, NotificationMessage, Channel, Icon, Sticky, Text, CanvasGroup, Coroutine (+4 more)

### Community 71 - "PlayerSelfDialogueUI"
Cohesion: 0.18
Nodes (7): Line, IEnumerator, List, Quest, Line, PlayerSelfDialogueUI, View

### Community 72 - "com.unity.editorcoroutines"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, url, version, dependencies, depth, source (+10 more)

### Community 73 - "ItemSpawner"
Cohesion: 0.18
Nodes (5): GameObject, List, Transform, ItemSpawner, ZoneItemEntry

### Community 75 - "Refactor Phase 5c: Split Player — Implementation Plan"
Cohesion: 0.23
Nodes (5): Vector2, Vector3, WrapPin, Global Constraints, Refactor Phase 5c: Split Player — Implementation Plan

### Community 76 - "InventoryManager"
Cohesion: 0.06
Nodes (32): Action, GameObject, IReadOnlyList, List, TextMeshProUGUI, Transform, InventoryManager, ActiveBuildingSlotIndex (+24 more)

### Community 77 - "ReachPoint"
Cohesion: 0.16
Nodes (13): ReachMode, Position, PositionToBite, Transform, Wander, Quaternion, Vector3, ReachPoint (+5 more)

### Community 78 - "PlayerActions"
Cohesion: 0.12
Nodes (15): PlayerActions, @Aim, @Attack, enabled, @Interact, @Inventory, @ItemConsume, @Menu (+7 more)

### Community 79 - "ChestInteract"
Cohesion: 0.24
Nodes (6): Collider2D, HashSet, MonoBehaviour, Sprite, ChestInteract, Popup

### Community 80 - "RopeBreakCountdownUI"
Cohesion: 0.18
Nodes (7): CanvasGroup, Coroutine, IEnumerator, Image, LoadSceneMode, Scene, RopeBreakCountdownUI

### Community 81 - "com.unity.2d.tilemap"
Cohesion: 0.13
Nodes (16): dependencies, depth, dependencies, depth, source, url, version, source (+8 more)

### Community 82 - "com.unity.dt.app-ui"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, url, version, dependencies, depth, source (+8 more)

### Community 83 - "DraggableItem"
Cohesion: 0.09
Nodes (17): Color, Image, PointerEventData, ShopBuySlot, GameObject, ShopInventoryBuy, Instance, Image (+9 more)

### Community 84 - "EnemyNavPathfinder"
Cohesion: 0.13
Nodes (13): Action, Func, Vector3, EnemySearchState, SearchOrigin, StateTag, LayerMask, NavMeshAgent (+5 more)

### Community 85 - "QuestManager"
Cohesion: 0.11
Nodes (9): Coroutine, IEnumerator, List, Quest, QuestManager, Instance, Risks, Task 2: Extract (+1 more)

### Community 86 - "QuestBoardUI"
Cohesion: 0.14
Nodes (9): Button, GameObject, List, Quest, Sprite, TextMeshProUGUI, QuestBoardUI, QuestsPerPage (+1 more)

### Community 87 - "Bullet"
Cohesion: 0.14
Nodes (10): Collider2D, GameObject, IEnumerator, Rigidbody2D, SpriteRenderer, Vector2, Bullet, MaxRange (+2 more)

### Community 88 - "Boid"
Cohesion: 0.17
Nodes (6): List, Vector2, Vector3, Boid, HasLeader, Leader

### Community 89 - "com.unity.modules.uielements"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, url, version, depth, source, version (+7 more)

### Community 90 - "QuestStepDataDrawer"
Cohesion: 0.20
Nodes (13): characterId, Dictionary, GUIContent, List, Rect, SerializedProperty, FieldVisibility, QuestStepDataDrawer (+5 more)

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
Cohesion: 0.15
Nodes (9): AggressiveEnemyProfile, detectDistance, LayerMask, Vector2, Vector2, Vector3, EnemyFollowState, LastKnownPlayerPosition (+1 more)

### Community 95 - "EnemyBrainContext"
Cohesion: 0.13
Nodes (10): Transform, EnemyBrainBase, enemyFSM, IsAttacking, MonoBehaviour, NavMeshAgent, Transform, EnemyBrainContext (+2 more)

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
Cohesion: 0.15
Nodes (11): InventoryModel, BuildingSlotCount, PlayerSlotCount, SlotCount, ItemStack, IsEmpty, Test, InventoryModelTests (+3 more)

### Community 100 - "QuestSlot"
Cohesion: 0.07
Nodes (21): Image, Quest, Sprite, QuestVisuals, Quest, Coroutine, IEnumerator, Vector3 (+13 more)

### Community 101 - "PlayerHealth"
Cohesion: 0.13
Nodes (7): PlayerHealth, IsInvincible, GameObject, SetUp, TearDown, Test, PlayerInvincibilityTests

### Community 102 - "gen_drawio.py"
Cohesion: 0.17
Nodes (19): collections, html, math, build(), class_box(), colors_of(), feature_page(), measure_container() (+11 more)

### Community 103 - "HierarchyDataCache"
Cohesion: 0.27
Nodes (7): DevelopmentStatusTag, Dictionary, GameObject, Cache, HierarchyDataCache, Cache, HierarchyCategoryTag

### Community 104 - "DamagePoint"
Cohesion: 0.17
Nodes (7): IDamagePopup, IEnumerator, TextMeshProUGUI, DamagePoint, Test, GameplayUiEventTests, Refactor Phase 3f-1: Gameplay Stops Calling UI Directly — Implementation Plan

### Community 105 - "NextDayAnimationUI"
Cohesion: 0.17
Nodes (6): Animator, CanvasGroup, Coroutine, IEnumerator, NextDayAnimationUI, IsPlaying

### Community 106 - "SaveMenuUI"
Cohesion: 0.21
Nodes (6): GameObject, SaveMenuUI, Button, GameObject, TMP_Text, SaveSlotUI

### Community 107 - "DevPanel"
Cohesion: 0.16
Nodes (8): Button, GameObject, List, DevItemButton, DevPanel, DevItemButton, TMP_Dropdown, Toggle

### Community 108 - "ItemDrop"
Cohesion: 0.29
Nodes (3): IEnumerator, ItemDrop, ActiveItem

### Community 109 - "PlayerSplitTests"
Cohesion: 0.18
Nodes (9): PlayerItemUse, GameObject, List, Object, TearDown, Test, PlayerSplitTests, Task 2: Encapsulate (+1 more)

### Community 110 - "com.unity.nuget.newtonsoft-json"
Cohesion: 0.17
Nodes (12): dependencies, depth, hash, source, version, dependencies, depth, source (+4 more)

### Community 111 - "com.unity.ext.nunit"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, version, dependencies, depth, source, url (+4 more)

### Community 112 - "ReadmeReaderEditor"
Cohesion: 0.22
Nodes (6): GUIStyle, SceneAsset, ReadmeReaderEditor, SerializedProperty, QuestDataEditor, Editor

### Community 113 - ".InteractPosition"
Cohesion: 0.20
Nodes (8): GameObject, QuestCompass, Transform, Vector3, Collider2D, Transform, Vector3, QuestNavigation

### Community 115 - "Phase 8: one Interactable hierarchy"
Cohesion: 0.19
Nodes (10): Sprite, IInteractPopupView, IsOpen, GameObject, Image, Sprite, TMP_Text, InteractPopupView (+2 more)

### Community 116 - "EnemyLoot"
Cohesion: 0.18
Nodes (8): EnemyLoot, CanInteract, IsFeatureUnlocked, CollectableItemBase, CanInteract, IsCollectable, IsFeatureUnlocked, TakesPlayerLock

### Community 117 - "ShelleController"
Cohesion: 0.23
Nodes (4): Animator, Coroutine, IEnumerator, ShelleController

### Community 118 - "QuestCompleteUI"
Cohesion: 0.22
Nodes (7): Coroutine, GameObject, IEnumerator, Quest, TextMeshProUGUI, QuestCompleteUI, Instance

### Community 119 - "ConfirmationDialog"
Cohesion: 0.11
Nodes (17): Action, IModalPrompt, CanShow, ModalPrompt, Current, IQuestOfferView, Action, Button (+9 more)

### Community 120 - "LandUI"
Cohesion: 0.14
Nodes (9): alpha, Button, CanvasGroup, Dictionary, interactable, TMP_Text, LandUI, blocksRaycasts (+1 more)

### Community 121 - "UNITY PROJECT AI GUIDE"
Cohesion: 0.17
Nodes (11): AI Workflow, Architecture Rules, Comment Rules, Existing Managers, Git Rules, graphify, Project Overview, Save System Rules (+3 more)

### Community 122 - "PlayerFeedbackVFX"
Cohesion: 0.21
Nodes (8): Coroutine, IEnumerator, Image, PlayerFeedbackVFX, Global Constraints, Task 1: GameState to Core, InteractUI to Gameplay, Task 2: Events and interface replace direct UI calls, Task 3: Verify live, measure, docs

### Community 123 - "com.unity.modules.ai"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 124 - "UNITY PROJECT AI GUIDE"
Cohesion: 0.20
Nodes (9): AI Workflow, Architecture Rules, Existing Managers, Git Rules, Project Overview, Save System Rules, Scene Structure, UI Rules (+1 more)

### Community 125 - "HierarchyPlus — Installation Guide"
Cohesion: 0.20
Nodes (9): 1. Copy the Package, 2. Install Assembly Definitions, 3. Verify, 4. Tag Your GameObjects, 5. Component Minimap, 6. Unity Version Notes, Category Tag, Development Status Tag (+1 more)

### Community 126 - "MotionBendTracking"
Cohesion: 0.27
Nodes (7): Queue, Vector3, MotionBendTracking, Capacity, Count, MotionData, MotionData

### Community 127 - ".Close"
Cohesion: 0.15
Nodes (7): Action, PlayerUIState, IsLocked, ResetCount, EscClosablePanel, Behaviour changes (accepted), Stack

### Community 128 - "FishAlertUI"
Cohesion: 0.23
Nodes (4): Coroutine, GameObject, IEnumerator, FishAlertUI

### Community 129 - "ScriptNamingTests"
Cohesion: 0.22
Nodes (10): Test, ScriptNamingTests, File map, Global Constraints, Refactor Phase 1: Names Implementation Plan, Task 1: Safety net (GUID snapshot + naming test), Task 2: Fix file/class name mismatches, Task 3: Rename typo folders (+2 more)

### Community 130 - "KeyAction"
Cohesion: 0.18
Nodes (11): KeyAction, Aim, Attack, Interact, Inventory, Move, Sprint, SwitchItem (+3 more)

### Community 131 - "VerletRope"
Cohesion: 0.24
Nodes (7): Settings, List, Vector2, Seg, VerletRope, Count, Seg

### Community 132 - "FakeInventory"
Cohesion: 0.10
Nodes (8): IReadOnlyList, TearDown, Test, FakeInventory, ActiveBuildingSlotIndex, AllItems, InventoryAccessTests, TearDown

### Community 133 - ".Info"
Cohesion: 0.21
Nodes (8): Object, List, SaveSlotMeta, SaveSystem, LegacyPath, List, Conditional, HideInCallstack

### Community 134 - "LoadingUI"
Cohesion: 0.15
Nodes (8): GameBootstrap, Animator, CanvasGroup, Slider, TMP_Text, LoadingUI, BootstrapLoader, Task 4: Decide the four unreferenced scripts

### Community 135 - "GrapplingHookBullet"
Cohesion: 0.31
Nodes (6): Collider2D, IEnumerator, Transform, Vector2, GrapplingHookBullet, AttachedToObject

### Community 136 - "CategoryColorMap"
Cohesion: 0.42
Nodes (5): Color, Dictionary, HierarchyCategory, Texture2D, CategoryColorMap

### Community 138 - "ButtonHoverScale"
Cohesion: 0.14
Nodes (10): PointerEventData, TMP_Text, Vector3, ButtonHoverScale, Color, Image, PointerEventData, UISlotHoverHighlight (+2 more)

### Community 139 - "AudioManager"
Cohesion: 0.08
Nodes (17): AudioSource, LoadSceneMode, Scene, AudioChannel, BGM, Master, SFX, AudioManager (+9 more)

### Community 140 - "EnemyHitbox"
Cohesion: 0.16
Nodes (10): Collider2D, EnemyHitbox, HitPart, Body, Head, Tail, GameObject, Vector2 (+2 more)

### Community 141 - "Bird"
Cohesion: 0.43
Nodes (3): SpriteRenderer, Transform, Bird

### Community 142 - "Item"
Cohesion: 0.04
Nodes (34): InventoryItemDatabase, GameObject, Sprite, Item, CanBuy, CanSell, ItemDescription, ItemID (+26 more)

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

### Community 147 - "GameObjectSpawnManager"
Cohesion: 0.14
Nodes (7): Dictionary, GameObject, List, EnemySpawnEntry, GameObjectSpawnManager, Instance, ItemSpawnEntry

### Community 148 - "TutorialActionMarker"
Cohesion: 0.24
Nodes (4): GameObject, List, Transform, TutorialActionMarker

### Community 149 - "SlotBuilderTests"
Cohesion: 0.25
Nodes (5): GameObject, SetUp, TearDown, Test, SlotBuilderTests

### Community 150 - "parse_types"
Cohesion: 0.14
Nodes (12): load(), match_brace(), Member, member_headers(), parse_members(), parse_types(), Find type declarations in code; returns list of TypeInfo (nested types…, Yield top-level member declaration headers of a type body. (+4 more)

### Community 151 - "QuestType"
Cohesion: 0.14
Nodes (12): QuestType, Cutscene, Delivery, ExploreArea, Talk, Tutorial, WaitingQuest, Quest (+4 more)

### Community 152 - "TipPanelUI"
Cohesion: 0.43
Nodes (3): Button, GameObject, TipPanelUI

### Community 153 - "CategoryIconMap"
Cohesion: 0.48
Nodes (4): Dictionary, HierarchyCategory, Texture2D, CategoryIconMap

### Community 155 - "QuestDetailPanel"
Cohesion: 0.17
Nodes (8): Action, Quest, Action, Button, Image, Quest, TextMeshProUGUI, QuestDetailPanel

### Community 156 - "NPCManager"
Cohesion: 0.19
Nodes (6): NpcEvents, Dictionary, Sprite, NPCManager, Instance, Task 4: Verify live, docs

### Community 157 - "DialogueState"
Cohesion: 0.19
Nodes (8): GameObject, HashSet, Transform, DialogueState, BlocksInteraction, IsShowing, Task 3: Switch the code to the view and remove the old fields, RuntimeInitializeOnLoadMethod

### Community 158 - "InteractableNextDay"
Cohesion: 0.22
Nodes (4): Button, Collider2D, GameObject, InteractableNextDay

### Community 159 - ".TryOpenScript"
Cohesion: 0.60
Nodes (3): Component, ScriptNavigator, MonoScript

### Community 160 - "com.unity.modules.physics2d"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 161 - "PlayerMovement"
Cohesion: 0.06
Nodes (20): Animator, HashSet, EntityAnimation, Rigidbody2D, Vector2, PlayerMovement, DashSpeed, FaceDir (+12 more)

### Community 162 - "Gameplay_Entities_Players.md"
Cohesion: 0.06
Nodes (26): Core/Datas, Files, Core/Datas/Upgrades, Files, Event subscriptions, Files, Gameplay/Entities, Event subscriptions (+18 more)

### Community 163 - "SimpleAnimLoop"
Cohesion: 0.47
Nodes (3): Animator, IEnumerator, SimpleAnimLoop

### Community 164 - "Phase 2: put types in the right layer (medium risk, biggest win)"
Cohesion: 0.12
Nodes (16): ItemType, Ammo, Building, CheckpointItem, Consumable, KeyItem, Normal, Quest (+8 more)

### Community 165 - "graphify reference: query, path, explain"
Cohesion: 0.33
Nodes (5): For /graphify explain, For /graphify path, graphify reference: query, path, explain, Step 0 — Constrained query expansion (REQUIRED before traversal), Step 1 — Traversal

### Community 166 - "MoveKeyBindUI"
Cohesion: 0.18
Nodes (7): Button, TMP_Text, MoveKeyBindUI, Global Constraints, Task 1: Code changes, Task 2: Verify live, Task 3: Docs

### Community 169 - "SwimState"
Cohesion: 0.40
Nodes (5): SwimState, Biting, Reaching, Stopped, Swimming

### Community 171 - "IntroPlayer"
Cohesion: 0.31
Nodes (3): Button, IntroPlayer, VideoPlayer

### Community 172 - "RopeSplitTests"
Cohesion: 0.21
Nodes (8): BoxCollider2D, GameObject, Settings, TearDown, Test, Vector2, RopeSplitTests, Task 1: Tests and a baseline

### Community 173 - "Notification Channels Implementation Plan"
Cohesion: 0.12
Nodes (10): Transform, StackNotificationView, SaveNotificationUI, File map, Global Constraints, Notification Channels Implementation Plan, Task 1: Channel bus and toast view, Task 3: Stack view and line (+2 more)

### Community 174 - "UISettingManager"
Cohesion: 0.39
Nodes (3): GameObject, UISettingManager, IsOpen

### Community 175 - "SettingTabManager"
Cohesion: 0.39
Nodes (4): Button, Color, GameObject, SettingTabManager

### Community 176 - "DoorQuestMarker"
Cohesion: 0.17
Nodes (8): CanvasGroup, IEnumerator, Transform, DoorInteract, GameObject, List, DoorQuestMarker, TutorialAction

### Community 177 - "KeyBindUI"
Cohesion: 0.17
Nodes (7): Coroutine, IEnumerator, TMP_Text, KeyBindErrorUI, Button, TMP_Text, KeyBindUI

### Community 178 - "Notification Channels — Design"
Cohesion: 0.22
Nodes (8): Data flow, Decisions, Edge cases, Files, Notification Channels — Design, Out of scope, Scene and prefabs, Testing

### Community 179 - "UIWindow"
Cohesion: 0.18
Nodes (9): UIType, Inventory, MainMenu, PauseMenu, Quest, SettingMenu, SettingPause, UIWindow (+1 more)

### Community 180 - "TrashBin"
Cohesion: 0.20
Nodes (8): PointerEventData, SellSlot, Image, PointerEventData, Sprite, Vector3, TrashBin, IDropHandler

### Community 181 - ".Notify"
Cohesion: 0.22
Nodes (5): Sprite, SetUp, TearDown, Test, NotificationTests

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
Nodes (11): `Debug.Log*` calls — 85 in 35 files, Dependency cycles (type level), Largest types, Layer violations — 0 type links, Metrics, Most depended-on (fan-in), Most dependencies (fan-out), Per-frame methods (`Update`/`FixedUpdate`/`LateUpdate`) — 32 in 30 files (+3 more)

### Community 189 - "Gameplay/QuestSystems/Quest"
Cohesion: 0.40
Nodes (5): Event subscriptions, Files, Gameplay/QuestSystems/Quest, part 1, part 2

### Community 192 - "IDialogueView"
Cohesion: 0.07
Nodes (21): GameObject, Sprite, IDialogueView, IsOpen, Panel, GameObject, Image, Sprite (+13 more)

### Community 193 - "NotificationView"
Cohesion: 0.16
Nodes (10): NotificationChannel, Center, General, Resource, System, NotificationView, Classes, Data (+2 more)

### Community 194 - "LoadingManager"
Cohesion: 0.18
Nodes (7): GameObject, HashSet, IEnumerator, Slider, LoadingManager, Instance, IsLoading

### Community 195 - ".HandleBulletHit"
Cohesion: 0.13
Nodes (6): IEnumerator, Collider2D, Collision2D, GameObject, Transform, Vector2

### Community 197 - "QuestSaveData"
Cohesion: 0.27
Nodes (6): QuestSaveData, Func, IEnumerable, List, Quest, Global Constraints

### Community 198 - "UML refresh & refactor roadmap: design"
Cohesion: 0.29
Nodes (6): Decisions (agreed with the user), Deliverables, Goal, Next, UML refresh & refactor roadmap: design, Verification

### Community 199 - "PlayerInteriorState"
Cohesion: 0.22
Nodes (8): PlayerInterior, House, None, Shop, UpgradeShop, PlayerInteriorState, Current, CurrentExitAction

### Community 200 - "UpgradeTooltip"
Cohesion: 0.20
Nodes (9): CanvasGroup, GameObject, List, RectTransform, TMP_Text, Transform, UpgradeTooltip, Canvas (+1 more)

### Community 201 - "GameManager"
Cohesion: 0.17
Nodes (8): GameState, Transform, GameManager, GameState, Instance, PlayerSpawnPoint, Task 3: `GameFlowEvents` — player code requests "return to land" instead of calling GameManager, Why this scope (data, 2026-09-29)

### Community 202 - ".PickClosest"
Cohesion: 0.24
Nodes (9): Func, IEnumerable, Vector2, InteractionTargeting, Test, Vector2, InteractionTargetingTests, Target (+1 more)

### Community 203 - "QuestCategory"
Cohesion: 0.18
Nodes (10): QuestCategory, Explore, FishL, FishM, FishS, Trash, QuestCategoryUIData, Action (+2 more)

### Community 204 - "SaveBridge"
Cohesion: 0.12
Nodes (15): ISaveParticipant, IEnumerator, LoadSceneMode, Scene, SaveBridge, CurrentSlot, Instance, File map (paths relative to `Assets/Project_Ocean/Scripts/`) (+7 more)

### Community 205 - ".LimitBreached"
Cohesion: 0.22
Nodes (3): IEnumerator, Test, GameFlowEventsTests

### Community 206 - "LandOpeningController"
Cohesion: 0.36
Nodes (4): Animator, GameObject, IEnumerator, LandOpeningController

### Community 207 - "Risks"
Cohesion: 0.20
Nodes (9): GameLog, Test, GameLogTests, ConditionalAttribute, Risks, Task 1: Logger and test, Task 2: Replace, Task 3: Prove the stripping (+1 more)

### Community 208 - "EnemyBaseProfile"
Cohesion: 0.17
Nodes (8): LayerMask, EnemyBaseProfile, RequiresNavMesh, FriendlyEnemyProfile, Vector2, EnemyFleeState, StateTag, ScaredEnemyProfile

### Community 209 - "VolumeSliderUI"
Cohesion: 0.40
Nodes (3): Slider, TMP_Text, VolumeSliderUI

### Community 210 - "Refactor Phase 3c-1: Move Gameplay Orchestrators Out of Core — Implementation Plan"
Cohesion: 0.40
Nodes (4): Global Constraints, Moves (paths relative to `Assets/Project_Ocean/Scripts/`), Refactor Phase 3c-1: Move Gameplay Orchestrators Out of Core — Implementation Plan, Task 1: Move the orchestrators and save UI

### Community 211 - "Cutscene"
Cohesion: 0.18
Nodes (8): Action, Component, List, Rigidbody2D, Cutscene, CutsceneId, CutsceneTrackBinding, PlayableDirector

### Community 212 - "guid_snapshot.py"
Cohesion: 0.20
Nodes (7): json, pathlib, re, sys, Snapshot and compare .meta GUIDs under Assets/Project_Ocean/Scripts. Moves must…, Self-test for gen_uml's parser. Run: python tools/uml/test_gen_uml.py, unittest

### Community 213 - "ReviceQuestListUI"
Cohesion: 0.22
Nodes (7): TextMeshProUGUI, Transform, ReviceQuestListUI, CanvasGroup, Image, TextMeshProUGUI, ReviceQuestSlot

### Community 214 - "Game.Tests"
Cohesion: 0.15
Nodes (5): Game.Tests, nunit_framework, system_linq, system_text_regularexpressions, unityengine_testtools

### Community 215 - "1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)"
Cohesion: 0.24
Nodes (6): NotificationManager, SoundKeys, CutsceneSkipHint, Bus — `NotificationManager` (existing static class, `UI/NotificationManager.cs`), Goal, 1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)

### Community 216 - "00-overview.md"
Cohesion: 0.10
Nodes (18): Layer dependencies, Module dependencies, Module table, Overview — layers & modules, Core/Inputs, Event subscriptions, Files, Core/Inputs/KeyBind (+10 more)

### Community 217 - ".MovePlayerTo"
Cohesion: 0.22
Nodes (5): LoadSceneMode, Rigidbody2D, Scene, Vector3, Task 2: `PlayerRegistry.Local` everywhere — delete `GameManager.CurPlayer`

### Community 218 - "PlayerCamera"
Cohesion: 0.33
Nodes (4): LoadSceneMode, Scene, PlayerCamera, CinemachineCamera

### Community 220 - ".AttachRope"
Cohesion: 0.32
Nodes (5): GameObject, GameObject, LineRenderer, Rigidbody2D, RopeSystemUtil

### Community 221 - "SaveData"
Cohesion: 0.11
Nodes (11): List, OneTimePickupTracker, Instance, List, SaveData, Test, SaveDataCompatTests, GameObject (+3 more)

### Community 222 - ".Shooting"
Cohesion: 0.29
Nodes (3): Collider2D, IEnumerator, Rigidbody2D

### Community 223 - "Package assembly GUIDs (resolved from Library/PackageCache)"
Cohesion: 0.29
Nodes (6): Global Constraints, Package assembly GUIDs (resolved from Library/PackageCache), Refactor Phase 4: Assemblies and Namespaces — Implementation Plan, Task 1: Split into assemblies (namespaces unchanged), Task 2: Rename namespaces per folder, Task 3: Verify live, measure, docs

### Community 224 - "QuestStatus"
Cohesion: 0.29
Nodes (6): QuestStatus, Finish, InProgress, New, ReadyToTurnIn, Reject

### Community 225 - "EnemyAttackState"
Cohesion: 0.24
Nodes (3): EnemyAttackState, IsAttacking, StateTag

### Community 226 - "NotificationLine"
Cohesion: 0.39
Nodes (5): CanvasGroup, IEnumerator, Image, TextMeshProUGUI, NotificationLine

### Community 227 - "Refactor Phase 5b: Split OxygenRope — Implementation Plan"
Cohesion: 0.22
Nodes (8): Collider2D, List, Vector2, RopeCornerFinder, Refactor Phase 5b: Split OxygenRope — Implementation Plan, Risks, Task 2: Extract, Task 3: Verify live, docs

### Community 228 - "EnemyWanderState"
Cohesion: 0.16
Nodes (8): EnemyVision, Vector3, EnemyWanderState, StateTag, LayerMask, Transform, Vector2, EnemyVisionHelper

### Community 229 - "InteractUI"
Cohesion: 0.14
Nodes (13): UnityEvent, InteractUI, Test, ViewContractTests, 8a: `Interactable` + `PlayerInteractor`; `InteractUI` becomes a subclass (no behaviour change besides closest-wins), 8b: Door, Chest, Trash derive from `Interactable`; shared popup view, 8c: Pickups join, 8d: Door split + camera (+5 more)

### Community 230 - "SceneLoadContext"
Cohesion: 0.40
Nodes (4): Vector3, SceneLoadContext, IsLoading, Position

### Community 231 - "Core.md"
Cohesion: 0.09
Nodes (22): Core, Files, Core/Save, Core/Save/DayReset, Event subscriptions, Files, Files, Files (+14 more)

### Community 232 - "unityeditor"
Cohesion: 0.08
Nodes (12): MenuItem, DocsInstaller, SceneAsset, ReadmeReader, DenysAlmaral, HierarchyPlus.Editor, Game.Editor, system_io (+4 more)

### Community 236 - "com.unity.collab-proxy"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.collab-proxy

### Community 237 - "Cleanup Phase 6e: Items Feature Folder, Checkpoint to Deprecated — Implementation Plan"
Cohesion: 0.33
Nodes (5): Checks, Cleanup Phase 6e: Items Feature Folder, Checkpoint to Deprecated — Implementation Plan, Comment updates (text only), Delete, Moves (Unity `AssetDatabase.MoveAsset`, so GUIDs and prefab/scene links are kept)

### Community 238 - ".RemoveAllWhere"
Cohesion: 0.33
Nodes (4): Dictionary, Predicate, Dictionary, Predicate

### Community 240 - "UI_Inventory.md"
Cohesion: 0.08
Nodes (24): Core/Datas/Items, Files, Files, Gameplay/Inventory, Files, Gameplay/Items, Event subscriptions, Files (+16 more)

### Community 248 - "GameSessionTests"
Cohesion: 0.29
Nodes (4): SetUp, TearDown, Test, GameSessionTests

### Community 249 - "UIType"
Cohesion: 0.25
Nodes (8): UIType, Inventory, MainMenu, None, PauseMenu, Quest, SettingMenu, SettingPause

### Community 251 - "NormalBullet"
Cohesion: 0.43
Nodes (3): Rigidbody2D, NormalBullet, MaxRange

### Community 252 - "Settings"
Cohesion: 0.67
Nodes (3): LayerMask, Settings, Global Constraints

### Community 268 - "UpgradeServiceEventTests"
Cohesion: 0.22
Nodes (4): SetUp, TearDown, Test, UpgradeServiceEventTests

### Community 270 - "PlayerMoney"
Cohesion: 0.17
Nodes (6): PlayerMoney, Current, CurrentMoney, Money, TMP_Text, MoneyUI

## Knowledge Gaps
- **1148 isolated node(s):** `IsVisible`, `IconStyle`, `BadgeStyle`, `WarningContent`, `None` (+1143 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1997 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **29 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Game.Core` connect `Game.Core` to `KeyBindManager`, `AdditiveSceneSetupWindow`, `system_collections_generic`, `Item`, `Game.UI`, `TutorialActionId`, `Phase 7a: decouple the GameManager hub and player-side upward links`, `Phase 3: cut remaining wrong-way links (medium risk)`, `NPCManager`, `Phase 2: put types in the right layer (medium risk, biggest win)`, `unityengine`, `GameFeature`, `QuestData.cs`, `NotificationView`, `unityengine_inputsystem`, `SaveBridge`, `Risks`, `Game.Tests`, `unityeditor`, `ConfirmationDialog`, `Cleanup Phase 6c: Find* Calls — Implementation Plan`?**
  _High betweenness centrality (0.072) - this node is a cross-community bridge._
- **Why does `Findings & refactor roadmap` connect `InputReader` to `README.md`, `3. Root causes`, `Phase 6: cleanup (low risk, any time after Phase 4)`, `1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)`?**
  _High betweenness centrality (0.070) - this node is a cross-community bridge._
- **Why does `3. Root causes` connect `3. Root causes` to `UIManager`, `OxygenRope`, `InputReader`, `Player`, `Item`, `PlayerMoney`, `ShopManager`, `TutorialActionId`, `QuestDetailPanel`, `Phase 2: put types in the right layer (medium risk, biggest win)`, `UpgradeService`, `EnemyFSM`, `IEnemyBrain`, `UpgradeData`, `DayManager`, `LoadingManager`, `GameManager`, `QuestCategory`, `SaveBridge`, `InventoryManager`, `QuestManager`, `QuestBoardUI`, `1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)`, `EnemyBrainContext`, `QuestSlot`, `PlayerHealth`, `NormalBullet`?**
  _High betweenness centrality (0.061) - this node is a cross-community bridge._
- **Are the 40 inferred relationships involving `InventoryManager` (e.g. with `Notification Channels Implementation Plan` and `Task 2: Money and item delta events + presenter`) actually correct?**
  _`InventoryManager` has 40 INFERRED edges - model-reasoned connections that need verification._
- **What connects `IsVisible`, `IconStyle`, `BadgeStyle` to the rest of the system?**
  _1148 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `project-ocean.slnx` be split into smaller, more focused modules?**
  _Cohesion score 0.02666666666666667 - nodes in this community are weakly interconnected._
- **Should `Game.Core` be split into smaller, more focused modules?**
  _Cohesion score 0.05163511187607573 - nodes in this community are weakly interconnected._