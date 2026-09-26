# Graph Report - project-ocean  (2026-09-26)

## Corpus Check
- 352 files · ~386,638 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 2581 file(s) not represented in the graph (top: .meta 1740, .asset 511, .prefab 134)

## Summary
- 4330 nodes · 8066 edges · 222 communities (201 shown, 21 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 459 edges (avg confidence: 0.9)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a3da2953`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- project-ocean-worktree2.slnx
- unityengine
- UIManager
- KeyBindManager
- OxygenRope
- manifest.json
- dependencies
- EnemyNavAgentBase
- InputReader
- InventoryManager
- Player
- ExploreAreaTrigger
- AdditiveSceneSetupWindow
- system_collections_generic
- ObjectWrapPoint
- Enemy
- packages-lock.json
- QuestManager
- com.unity.mathematics
- EnemyBrain
- ShopManager
- unityengine_ui
- Quest
- DraggableItem
- CollectableItemBase
- QuestStep
- com.unity.modules.imgui
- LoadingType
- com.unity.2d.common
- gen_uml.py
- com.unity.modules.animation
- FishMotion
- @InputSystem_Actions
- GameHUD
- Cutscene
- PlayerGunHandler
- Player
- unityeditor
- NPC
- What You Must Do When Invoked
- com.unity.render-pipelines.core
- HierarchyCategory
- SaveData
- GameFeature
- EnemyFSM
- TrashInteract
- dependencies
- com.unity.test-framework
- LinearPatrolBrain
- README.md
- QuestData.cs
- EnemySpawner
- ObjectPoolManager
- CallbackContext
- EnemyNavPathfinder
- MonoBehaviour
- UpgradeSlot
- LoadingManager
- com.unity.modules.physics
- EnemyFacingController
- Gameplay_Entities_Enemies.md
- Core.md
- Entity
- EntityHealth
- Vector2
- DayManager
- DoorInteract
- ComponentInspectorPopup
- ComponentMinimap
- .DrawCategory
- GameObjectSpawnManager
- PlayerSelfDialogueUI
- com.unity.editorcoroutines
- ItemSpawner
- Game.Core
- com.unity.searcher
- ScriptableObject
- ReachPoint
- PlayerActions
- ChestInteract
- RopeBreakCountdownUI
- com.unity.2d.tilemap
- dependencies
- PlayerHealth
- SaveMenuUI
- UpgradeService
- QuestBoardUI
- Bullet
- Boid
- com.unity.modules.uielements
- QuestStepDataDrawer
- ShelleController
- com.unity.modules.jsonserialize
- project-ocean.slnx
- EnemyFollowState
- EnemyFleeState
- Avoidance
- AmmoWheel
- UIActions
- ISaveParticipant
- EnemyWanderState
- PunchScale
- TutorialPromptUI
- HierarchyDataCache
- PlayerMoney
- NextDayAnimationUI
- SaveSystem
- DevPanel
- .InteractPosition
- TrashBin
- com.unity.nuget.newtonsoft-json
- com.unity.ext.nunit
- ReadmeReaderEditor
- NPCManager
- .UnregisterCallbacks
- TutorialActionId
- InteractUI
- ReviceQuestListUI
- LandOpeningController
- ConfirmationDialog
- LandUI
- UNITY PROJECT AI GUIDE
- SceneLoadContext
- com.unity.modules.ai
- UNITY PROJECT AI GUIDE
- HierarchyPlus — Installation Guide
- MotionBendTracking
- PlayerUIState
- FishAlertUI
- CameraShakeManager
- GameManager
- PlayerInterior
- TutorialActionMarker
- ShopBuySlot
- QuestCompleteUI
- GrapplingHookBullet
- CategoryColorMap
- IntroPlayer
- AudioManager
- EnemyHitbox
- Bird
- MoneyUI
- graphify reference: extra exports and benchmark
- HierarchyPlusCore
- MissingScriptDetector
- Parallax
- PlayerFeedbackVFX
- PlayerCamera
- VolumeSliderUI
- NotificationLine
- UIWindow
- TipPanelUI
- CategoryIconMap
- com.h8man.2d.navmeshplus
- KeyBindResetAllButton
- DialogueState
- MoveKeyBindUI
- .TryOpenScript
- EnemyStateBase
- EnemyKillTracker
- InteractableNextDay
- SimpleAnimLoop
- PlayerStabAttack
- graphify reference: query, path, explain
- LoadingUI
- .Awake
- .BeginReach
- SwimState
- .Notify
- 3. Root causes
- Findings & refactor roadmap
- EntityDeathHandler
- SettingTabManager
- UISettingManager
- com.unity.collections
- MiniHub
- com.unity.modules.physics2d
- KeyAction
- graphify reference: add a URL and watch a folder
- graphify reference: commit hook and native CLAUDE.md integration
- graphify reference: incremental update and cluster-only
- HierarchyPlus — Folder Structure
- graphify reference: GitHub clone and cross-repo merge
- graphify reference: transcribe video and audio
- Metrics
- MoveDirection
- .claude/CLAUDE.md
- extraction-spec.md
- EnemyBrainContext
- OneTimePickupTracker
- unityengine_scenemanagement
- EnemyAttackState
- TutorialUIMarker
- LoadingButton
- UML refresh & refactor roadmap: design
- 00-overview.md
- UpgradeTooltip
- QuestDetailPanel
- .RaiseAction
- 1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)
- TutorialGate
- Gameplay/QuestSystems/Quest
- PlayerSprintVFX
- Gameplay_Entities_Players_OxygenTube.md
- UIFadeBackground
- KeyBindUI
- Cutscene.cs
- Gameplay_InteractObjects.md
- EntityAnimation
- Phase 3: cut remaining wrong-way links (medium risk)
- ItemDrop
- Gameplay_Entities_Players.md
- SceneNames.cs
- Core_Services.md
- UI_Inventory.md
- UIType
- NormalBullet
- KeyBindSettings

## God Nodes (most connected - your core abstractions)
1. `Game.Testing` - 199 edges
2. `FishMotion` - 68 edges
3. `OxygenRope` - 68 edges
4. `InventoryManager` - 67 edges
5. `Player` - 62 edges
6. `QuestManager` - 62 edges
7. `EnemyNavAgentBase` - 59 edges
8. `Item` - 51 edges
9. `QuestStep` - 49 edges
10. `ShopManager` - 49 edges

## Surprising Connections (you probably didn't know these)
- `Task 3: LoadingManager to Core behind ILoadingVisual` --references--> `LoadingManager`  [INFERRED]
  docs/superpowers/plans/2026-09-26-refactor-phase3ab-metrics-loading.md → Assets/Project_Ocean/Scripts/Core/Scenes/LoadingManager.cs
- `Presenter` --references--> `ResourceNotificationPresenter`  [INFERRED]
  docs/superpowers/specs/2026-09-25-notification-channels-design.md → Assets/Project_Ocean/Scripts/UI/Notifications/ResourceNotificationPresenter.cs
- `Phase 2: put types in the right layer (medium risk, biggest win)` --references--> `InventoryItemDatabase`  [INFERRED]
  docs/uml/findings.md → Assets/Project_Ocean/Scripts/Core/Datas/Items/InventoryItemDatabase.cs
- `File map` --references--> `Item`  [INFERRED]
  docs/superpowers/plans/2026-09-26-refactor-phase2-layers.md → Assets/Project_Ocean/Scripts/Core/Datas/Items/Item.cs
- `Task 3: Move shared data and event types down to Core` --references--> `Item`  [INFERRED]
  docs/superpowers/plans/2026-09-26-refactor-phase2-layers.md → Assets/Project_Ocean/Scripts/Core/Datas/Items/Item.cs

## Import Cycles
- None detected.

## Communities (222 total, 21 thin omitted)

### Community 0 - "project-ocean-worktree2.slnx"
Cohesion: 0.03
Nodes (72): Assembly-CSharp, Assembly-CSharp-Editor, Game.Core, Game.Testing, glTFast.dots, glTFast.Export, HFDownloader, HierarchyPlus.Editor (+64 more)

### Community 1 - "unityengine"
Cohesion: 0.05
Nodes (6): IHookable, AnimHash, Game.Testing, system_collections, unityengine, unityengine_ai

### Community 2 - "UIManager"
Cohesion: 0.12
Nodes (6): Dictionary, UIManager, CurrentUI, Instance, IsUIOpen, UIType

### Community 3 - "KeyBindManager"
Cohesion: 0.24
Nodes (7): Action, InputAction, InputSystem_Actions, KeyBindManager, Actions, Instance, SavePath

### Community 4 - "OxygenRope"
Cohesion: 0.08
Nodes (20): Coroutine, GameObject, IEnumerator, LayerMask, LineRenderer, Rigidbody2D, Transform, OxygenRope (+12 more)

### Community 5 - "manifest.json"
Cohesion: 0.03
Nodes (60): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.director, com.unity.modules.imageconversion (+52 more)

### Community 6 - "dependencies"
Cohesion: 0.03
Nodes (61): dependencies, com.coplaydev.unity-mcp, com.h8man.2d.navmeshplus, com.unity.2d.animation, com.unity.2d.aseprite, com.unity.2d.psdimporter, com.unity.2d.sprite, com.unity.2d.spriteshape (+53 more)

### Community 7 - "EnemyNavAgentBase"
Cohesion: 0.06
Nodes (22): Transform, IEnemyBrain, IsAttacking, Collider2D, Collision2D, FacingTurnMode, GameObject, NavMeshAgent (+14 more)

### Community 8 - "InputReader"
Cohesion: 0.09
Nodes (10): CallbackContext, Func, InputSystem_Actions, Vector2, InputReader, InputActions, MousePos, MovePos (+2 more)

### Community 9 - "InventoryManager"
Cohesion: 0.09
Nodes (16): InventoryItemDatabase, GameObject, Sprite, Item, Action, GameObject, IReadOnlyList, List (+8 more)

### Community 10 - "Player"
Cohesion: 0.05
Nodes (23): Color, Coroutine, GameObject, IEnumerator, List, LoadSceneMode, Rigidbody2D, Scene (+15 more)

### Community 11 - "ExploreAreaTrigger"
Cohesion: 0.11
Nodes (11): ExploreEvents, Collider2D, GameObject, Image, List, TextMeshProUGUI, Transform, DialogueLine (+3 more)

### Community 12 - "AdditiveSceneSetupWindow"
Cohesion: 0.07
Nodes (23): AdditiveSceneEntry, Component, GUIStyle, Vector2, ComponentTooltipWindow, IsVisible, AdditiveSceneConfig, AdditiveSceneEntry (+15 more)

### Community 13 - "system_collections_generic"
Cohesion: 0.06
Nodes (10): Vector3, FishConstant, CutsceneGate, IsPlaying, FishAlive, system, system_collections_generic, system_io (+2 more)

### Community 14 - "ObjectWrapPoint"
Cohesion: 0.13
Nodes (12): Collider2D, Dictionary, IEnumerable, IReadOnlyList, List, Vector2, ObjectWrapPoint, WrapPoint (+4 more)

### Community 15 - "Enemy"
Cohesion: 0.08
Nodes (14): Animator, Collision2D, Color, IEnumerator, SpriteRenderer, Enemy, Brain, EnemyId (+6 more)

### Community 16 - "packages-lock.json"
Cohesion: 0.05
Nodes (41): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.director, com.unity.modules.imageconversion (+33 more)

### Community 17 - "QuestManager"
Cohesion: 0.09
Nodes (8): Quest, QuestSaveData, Coroutine, Func, IEnumerator, List, QuestManager, Instance

### Community 18 - "com.unity.mathematics"
Cohesion: 0.09
Nodes (25): dependencies, depth, source, url, version, dependencies, depth, source (+17 more)

### Community 19 - "EnemyBrain"
Cohesion: 0.08
Nodes (19): LayerMask, EnemyBaseProfile, RequiresNavMesh, Transform, Vector3, EnemyBrain, AttackRange, IsAttacking (+11 more)

### Community 20 - "ShopManager"
Cohesion: 0.08
Nodes (8): TutorialStep, Button, GameObject, IEnumerator, Image, TMP_Text, ShopManager, Instance

### Community 21 - "unityengine_ui"
Cohesion: 0.08
Nodes (3): tmpro, unityengine_eventsystems, unityengine_ui

### Community 22 - "Quest"
Cohesion: 0.06
Nodes (29): IEnumerable, Quest, Category, Classification, CurrentCount, CurrentStep, CurrentStepIndex, DisplayNpcId (+21 more)

### Community 23 - "DraggableItem"
Cohesion: 0.07
Nodes (25): PointerEventData, SellSlot, Image, PointerEventData, TMP_Text, Transform, DraggableItem, Color (+17 more)

### Community 24 - "CollectableItemBase"
Cohesion: 0.11
Nodes (9): EnemyLoot, CanCollect, Collider2D, GameObject, HashSet, Transform, CollectableItemBase, CanCollect (+1 more)

### Community 25 - "QuestStep"
Cohesion: 0.07
Nodes (30): characterId, Dictionary, Func, HashSet, IReadOnlyList, List, Sprite, type (+22 more)

### Community 26 - "com.unity.modules.imgui"
Cohesion: 0.09
Nodes (23): dependencies, depth, source, url, version, dependencies, depth, source (+15 more)

### Community 27 - "LoadingType"
Cohesion: 0.11
Nodes (19): ILoadingVisual, LoadingType, Land, Menu, Ocean, Mode, Interact, TriggerZone (+11 more)

### Community 28 - "com.unity.2d.common"
Cohesion: 0.07
Nodes (28): dependencies, depth, source, url, version, depth, source, url (+20 more)

### Community 29 - "gen_uml.py"
Cohesion: 0.05
Nodes (43): collections, Task 1: Stop counting member names as type references, json, pathlib, re, sys, Snapshot and compare .meta GUIDs under Assets/Project_Ocean/Scripts. Moves must…, class_block() (+35 more)

### Community 30 - "com.unity.modules.animation"
Cohesion: 0.07
Nodes (32): dependencies, depth, source, url, version, dependencies, depth, source (+24 more)

### Community 31 - "FishMotion"
Cohesion: 0.07
Nodes (18): AnimationClip, Action, Color, Dictionary, GameObject, IEnumerator, Quaternion, DrawDebug (+10 more)

### Community 32 - "@InputSystem_Actions"
Cohesion: 0.07
Nodes (26): IEnumerable, IEnumerator, InputAction, InputBinding, List, @InputSystem_Actions, asset, bindingMask (+18 more)

### Community 33 - "GameHUD"
Cohesion: 0.11
Nodes (10): Button, CanvasGroup, GameObject, IEnumerator, Image, LoadSceneMode, Scene, TextMeshProUGUI (+2 more)

### Community 34 - "Cutscene"
Cohesion: 0.08
Nodes (18): CutsceneEvents, Action, Component, List, Rigidbody2D, Cutscene, CutsceneId, CutsceneTrackBinding (+10 more)

### Community 35 - "PlayerGunHandler"
Cohesion: 0.08
Nodes (20): Collider2D, IEnumerator, LayerMask, LineRenderer, ParticleSystem, Rigidbody2D, SpriteRenderer, Transform (+12 more)

### Community 36 - "Player"
Cohesion: 0.10
Nodes (14): Vector2, PlayerRegistry, Local, Player, IEnumerator, LineRenderer, Rigidbody2D, CheckPointItem (+6 more)

### Community 37 - "unityeditor"
Cohesion: 0.11
Nodes (7): MenuItem, AsmdefInstaller, MenuItem, DocsInstaller, HierarchyPlus.Editor, unityeditor, unityeditorinternal

### Community 38 - "NPC"
Cohesion: 0.10
Nodes (11): NpcEvents, Collider2D, GameObject, Image, Sprite, TextMeshProUGUI, NPC, HasQuest (+3 more)

### Community 39 - "What You Must Do When Invoked"
Cohesion: 0.08
Nodes (24): For /graphify add and --watch, For /graphify query, For the commit hook and native CLAUDE.md integration, For --update and --cluster-only, /graphify, Honesty Rules, Interpreter guard for subcommands, Part A - Structural extraction for code files (+16 more)

### Community 40 - "com.unity.render-pipelines.core"
Cohesion: 0.15
Nodes (14): depth, source, version, dependencies, depth, source, version, dependencies (+6 more)

### Community 41 - "HierarchyCategory"
Cohesion: 0.09
Nodes (20): DevelopmentStatus, Complete, Deprecated, New, None, Problem, Review, WorkInProgress (+12 more)

### Community 42 - "SaveData"
Cohesion: 0.16
Nodes (9): IEnumerator, LoadSceneMode, Scene, SaveBridge, CurrentSlot, Instance, List, ItemSaveData (+1 more)

### Community 43 - "GameFeature"
Cohesion: 0.09
Nodes (21): GameFeature, BackToLand, Combat, EnemySpawn, EnemySpawnLevel1, EnemySpawnLevel2, EnemySpawnLevel3, Interact (+13 more)

### Community 44 - "EnemyFSM"
Cohesion: 0.12
Nodes (16): Dictionary, EnemyFSM, CurrentState, CurrentTag, Vector3, EnemyStateTag, Attack, Flee (+8 more)

### Community 45 - "TrashInteract"
Cohesion: 0.13
Nodes (10): Collider2D, Coroutine, GameObject, IEnumerator, Image, Rigidbody2D, Sprite, TextMeshProUGUI (+2 more)

### Community 46 - "dependencies"
Cohesion: 0.08
Nodes (24): dependencies, depth, source, url, version, dependencies, depth, source (+16 more)

### Community 47 - "com.unity.test-framework"
Cohesion: 0.09
Nodes (23): dependencies, dependencies, depth, source, url, version, dependencies, depth (+15 more)

### Community 48 - "LinearPatrolBrain"
Cohesion: 0.07
Nodes (21): IBrainHitReaction, Collider2D, Component, Rigidbody2D, Transform, Vector2, LinearPatrolBrain, IsAttacking (+13 more)

### Community 49 - "README.md"
Cohesion: 0.07
Nodes (22): Inheritance & interfaces, Declared events with no `+=` subscriber found, Event catalogue, Event flow, Core/Inputs/KeyBind, Files, Core/Inputs/KeyBind/MoveDirection, Files (+14 more)

### Community 50 - "QuestData.cs"
Cohesion: 0.09
Nodes (27): DeliverySource, EnemyKills, InventoryHeld, NextQuestMode, List, Sprite, DeliveryCountMode, CombinedTotal (+19 more)

### Community 51 - "EnemySpawner"
Cohesion: 0.14
Nodes (8): GameObject, IEnumerator, LayerMask, List, Rigidbody2D, Vector3, EnemySpawner, ZoneSpawnEntry

### Community 52 - "ObjectPoolManager"
Cohesion: 0.18
Nodes (9): IPoolable, Dictionary, GameObject, Quaternion, Queue, Vector3, ObjectPoolManager, Instance (+1 more)

### Community 53 - "CallbackContext"
Cohesion: 0.17
Nodes (3): CallbackContext, IPlayerActions, IUIActions

### Community 54 - "EnemyNavPathfinder"
Cohesion: 0.13
Nodes (13): Action, Func, Vector3, EnemySearchState, SearchOrigin, StateTag, LayerMask, NavMeshAgent (+5 more)

### Community 55 - "MonoBehaviour"
Cohesion: 0.09
Nodes (12): Vector3, WaterShadowWiggle, ItemUI, MiniHubItem, TMP_Text, LoadingTextAnimation, MainMenu, IEnumerator (+4 more)

### Community 56 - "UpgradeSlot"
Cohesion: 0.13
Nodes (8): UpgradeManager, Button, Color, Image, PointerEventData, RectTransform, TMP_Text, UpgradeSlot

### Community 57 - "LoadingManager"
Cohesion: 0.16
Nodes (7): GameObject, HashSet, IEnumerator, Slider, LoadingManager, Instance, IsLoading

### Community 58 - "com.unity.modules.physics"
Cohesion: 0.06
Nodes (37): dependencies, depth, source, version, dependencies, depth, source, version (+29 more)

### Community 59 - "EnemyFacingController"
Cohesion: 0.12
Nodes (12): SpriteRenderer, Transform, CharacterFacing2D, FacingTurnMode, Transform, Vector2, Vector3, EnemyFacingController (+4 more)

### Community 60 - "Gameplay_Entities_Enemies.md"
Cohesion: 0.08
Nodes (28): Core/Pool, Files, Files, Gameplay/Entities/Enemies/Behaviours, Files, Files, Gameplay/Entities/Enemies/Brain/FSM, Files (+20 more)

### Community 61 - "Core.md"
Cohesion: 0.08
Nodes (34): Core, Core/Events, Files, Files, Core/Save, Core/Save/DayReset, Event subscriptions, Files (+26 more)

### Community 62 - "Entity"
Cohesion: 0.14
Nodes (5): Rigidbody2D, Entity, CurrentHealth, IsDead, MaxHealth

### Community 63 - "EntityHealth"
Cohesion: 0.13
Nodes (9): Color, Coroutine, IEnumerator, SpriteRenderer, EntityHealth, CurrentHealth, HitColor, IsDead (+1 more)

### Community 64 - "Vector2"
Cohesion: 0.16
Nodes (6): Collider2D, List, Vector2, Vector3, Seg, WrapPin

### Community 65 - "DayManager"
Cohesion: 0.10
Nodes (12): UnityEvent, DayManager, CanDive, CurrentDay, DivesRemaining, DivesToday, HasDivedToday, Instance (+4 more)

### Community 66 - "DoorInteract"
Cohesion: 0.16
Nodes (8): CanvasGroup, CinemachineCamera, GameObject, IEnumerator, List, Transform, DoorInteract, TutorialAction

### Community 67 - "ComponentInspectorPopup"
Cohesion: 0.17
Nodes (7): Component, Editor, GUIStyle, Rect, Vector2, ComponentInspectorPopup, PopupWindowContent

### Community 68 - "ComponentMinimap"
Cohesion: 0.22
Nodes (10): Component, Dictionary, GameObject, HashSet, List, Rect, Texture2D, Type (+2 more)

### Community 69 - ".DrawCategory"
Cohesion: 0.18
Nodes (11): GameObject, GUIStyle, Rect, HierarchyPlusDrawer, BadgeStyle, IconStyle, Color, DevelopmentStatus (+3 more)

### Community 70 - "GameObjectSpawnManager"
Cohesion: 0.14
Nodes (7): Dictionary, GameObject, List, EnemySpawnEntry, GameObjectSpawnManager, Instance, ItemSpawnEntry

### Community 71 - "PlayerSelfDialogueUI"
Cohesion: 0.18
Nodes (8): GameObject, IEnumerator, Image, List, TextMeshProUGUI, Line, PlayerSelfDialogueUI, BlocksInteraction

### Community 72 - "com.unity.editorcoroutines"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, url, version, dependencies, depth, source (+10 more)

### Community 73 - "ItemSpawner"
Cohesion: 0.18
Nodes (5): GameObject, List, Transform, ItemSpawner, ZoneItemEntry

### Community 74 - "Game.Core"
Cohesion: 0.09
Nodes (4): Game.Core, unityengine_events, unityengine_inputsystem, unityengine_video

### Community 75 - "com.unity.searcher"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 76 - "ScriptableObject"
Cohesion: 0.12
Nodes (18): SceneAsset, ReadmeReader, SwimConfig, BulletData, RopeStructureData, Sprite, CharacterData, Dictionary (+10 more)

### Community 77 - "ReachPoint"
Cohesion: 0.16
Nodes (13): ReachMode, Position, PositionToBite, Transform, Wander, Quaternion, Vector3, ReachPoint (+5 more)

### Community 78 - "PlayerActions"
Cohesion: 0.12
Nodes (15): PlayerActions, @Aim, @Attack, enabled, @Interact, @Inventory, @ItemConsume, @Menu (+7 more)

### Community 79 - "ChestInteract"
Cohesion: 0.20
Nodes (7): Collider2D, GameObject, Image, List, Sprite, TextMeshProUGUI, ChestInteract

### Community 80 - "RopeBreakCountdownUI"
Cohesion: 0.18
Nodes (7): CanvasGroup, Coroutine, IEnumerator, Image, LoadSceneMode, Scene, RopeBreakCountdownUI

### Community 81 - "com.unity.2d.tilemap"
Cohesion: 0.13
Nodes (16): dependencies, depth, dependencies, depth, source, url, version, source (+8 more)

### Community 82 - "dependencies"
Cohesion: 0.07
Nodes (28): dependencies, dependencies, depth, source, url, version, dependencies, depth (+20 more)

### Community 83 - "PlayerHealth"
Cohesion: 0.15
Nodes (5): IEntityHealth, CurrentHealth, IsDead, MaxHealth, PlayerHealth

### Community 84 - "SaveMenuUI"
Cohesion: 0.20
Nodes (6): GameObject, SaveMenuUI, Button, GameObject, TMP_Text, SaveSlotUI

### Community 85 - "UpgradeService"
Cohesion: 0.27
Nodes (3): Dictionary, UpgradeService, Instance

### Community 86 - "QuestBoardUI"
Cohesion: 0.09
Nodes (18): QuestCategory, Explore, FishL, FishM, FishS, Trash, Button, GameObject (+10 more)

### Community 87 - "Bullet"
Cohesion: 0.12
Nodes (10): Collider2D, GameObject, IEnumerator, Rigidbody2D, SpriteRenderer, Vector2, Bullet, MaxRange (+2 more)

### Community 88 - "Boid"
Cohesion: 0.17
Nodes (6): List, Vector2, Vector3, Boid, HasLeader, Leader

### Community 89 - "com.unity.modules.uielements"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, url, version, depth, source, version (+7 more)

### Community 90 - "QuestStepDataDrawer"
Cohesion: 0.14
Nodes (20): characterId, Dictionary, GUIContent, List, Rect, SerializedProperty, FieldVisibility, QuestStepDataDrawer (+12 more)

### Community 91 - "ShelleController"
Cohesion: 0.23
Nodes (4): Animator, Coroutine, IEnumerator, ShelleController

### Community 92 - "com.unity.modules.jsonserialize"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, version, dependencies, depth, source, version (+13 more)

### Community 93 - "project-ocean.slnx"
Cohesion: 0.03
Nodes (72): Game.Tests.EditMode, glTFast, glTFast.Documentation.Examples, glTFast.Editor, glTFast.Newtonsoft, HierarchyPlus.Runtime, MCPForUnity.Runtime, NavMeshPlus (+64 more)

### Community 94 - "EnemyFollowState"
Cohesion: 0.19
Nodes (7): AggressiveEnemyProfile, detectDistance, Vector2, Vector3, EnemyFollowState, LastKnownPlayerPosition, StateTag

### Community 95 - "EnemyFleeState"
Cohesion: 0.21
Nodes (5): LayerMask, Vector2, Vector2, EnemyFleeState, StateTag

### Community 96 - "Avoidance"
Cohesion: 0.21
Nodes (7): Avoidance, Avoiding, Parent, found, numTries, RaycastHit, wayOut

### Community 97 - "AmmoWheel"
Cohesion: 0.17
Nodes (9): AmmoType, Granade, GrapplingHook, Normal, Tranquilizer, GameObject, RectTransform, AmmoWheel (+1 more)

### Community 98 - "UIActions"
Cohesion: 0.21
Nodes (8): UIActions, @Cancel, @Click, enabled, @MiddleClick, @RightClick, @ScrollWheel, @Submit

### Community 99 - "ISaveParticipant"
Cohesion: 0.22
Nodes (4): GameState, ISaveParticipant, List, SaveSlotMeta

### Community 100 - "EnemyWanderState"
Cohesion: 0.16
Nodes (8): EnemyVision, Vector3, EnemyWanderState, StateTag, LayerMask, Transform, Vector2, EnemyVisionHelper

### Community 101 - "PunchScale"
Cohesion: 0.15
Nodes (8): Coroutine, IEnumerator, Vector3, PunchScale, GameObject, Image, TextMeshProUGUI, QuestTrackerUI

### Community 102 - "TutorialPromptUI"
Cohesion: 0.27
Nodes (3): Sprite, TutorialPromptUI, CurrentPrompt

### Community 103 - "HierarchyDataCache"
Cohesion: 0.27
Nodes (7): DevelopmentStatusTag, Dictionary, GameObject, Cache, HierarchyDataCache, Cache, HierarchyCategoryTag

### Community 104 - "PlayerMoney"
Cohesion: 0.15
Nodes (9): PlayerMoney, CurrentMoney, Money, ShopTransactionService, File map, Spec corrections found while planning, Task 1: Channel bus and toast view, Task 5: Live verification (+1 more)

### Community 105 - "NextDayAnimationUI"
Cohesion: 0.17
Nodes (6): Animator, CanvasGroup, Coroutine, IEnumerator, NextDayAnimationUI, IsPlaying

### Community 106 - "SaveSystem"
Cohesion: 0.38
Nodes (3): List, SaveSystem, LegacyPath

### Community 107 - "DevPanel"
Cohesion: 0.16
Nodes (8): Button, GameObject, List, DevItemButton, DevPanel, DevItemButton, TMP_Dropdown, Toggle

### Community 108 - ".InteractPosition"
Cohesion: 0.23
Nodes (7): GameObject, QuestCompass, Vector3, Collider2D, Transform, Vector3, QuestNavigation

### Community 109 - "TrashBin"
Cohesion: 0.10
Nodes (15): ButtonHoverScale, PointerEventData, TMP_Text, Vector3, Image, PointerEventData, Sprite, Vector3 (+7 more)

### Community 110 - "com.unity.nuget.newtonsoft-json"
Cohesion: 0.17
Nodes (12): dependencies, depth, hash, source, version, dependencies, depth, source (+4 more)

### Community 111 - "com.unity.ext.nunit"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, version, dependencies, depth, source, url (+4 more)

### Community 112 - "ReadmeReaderEditor"
Cohesion: 0.22
Nodes (6): GUIStyle, SceneAsset, ReadmeReaderEditor, SerializedProperty, QuestDataEditor, Editor

### Community 113 - "NPCManager"
Cohesion: 0.25
Nodes (4): Dictionary, Sprite, NPCManager, Instance

### Community 115 - "TutorialActionId"
Cohesion: 0.10
Nodes (21): TutorialActionId, AcceptQuest, BackToLand, BreakRopeLimit, BuyItem, Dive, HitEnemy, Interact (+13 more)

### Community 116 - "InteractUI"
Cohesion: 0.22
Nodes (4): Collider2D, GameObject, UnityEvent, InteractUI

### Community 117 - "ReviceQuestListUI"
Cohesion: 0.22
Nodes (7): TextMeshProUGUI, Transform, ReviceQuestListUI, CanvasGroup, Image, TextMeshProUGUI, ReviceQuestSlot

### Community 118 - "LandOpeningController"
Cohesion: 0.36
Nodes (4): Animator, GameObject, IEnumerator, LandOpeningController

### Community 119 - "ConfirmationDialog"
Cohesion: 0.24
Nodes (6): Action, Button, GameObject, TextMeshProUGUI, ConfirmationDialog, Instance

### Community 120 - "LandUI"
Cohesion: 0.12
Nodes (10): alpha, Transform, Button, CanvasGroup, Dictionary, TMP_Text, LandUI, blocksRaycasts (+2 more)

### Community 121 - "UNITY PROJECT AI GUIDE"
Cohesion: 0.17
Nodes (11): AI Workflow, Architecture Rules, Comment Rules, Existing Managers, Git Rules, graphify, Project Overview, Save System Rules (+3 more)

### Community 122 - "SceneLoadContext"
Cohesion: 0.40
Nodes (4): Vector3, SceneLoadContext, IsLoading, Position

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

### Community 127 - "PlayerUIState"
Cohesion: 0.18
Nodes (6): Action, PlayerUIState, IsLocked, ResetCount, EscClosablePanel, Stack

### Community 128 - "FishAlertUI"
Cohesion: 0.23
Nodes (4): Coroutine, GameObject, IEnumerator, FishAlertUI

### Community 130 - "GameManager"
Cohesion: 0.11
Nodes (11): LoadSceneMode, Rigidbody2D, Scene, Transform, Vector3, GameManager, CurPlayer, Instance (+3 more)

### Community 131 - "PlayerInterior"
Cohesion: 0.22
Nodes (8): PlayerInterior, House, None, Shop, UpgradeShop, PlayerInteriorState, Current, CurrentExitAction

### Community 132 - "TutorialActionMarker"
Cohesion: 0.24
Nodes (4): GameObject, List, Transform, TutorialActionMarker

### Community 133 - "ShopBuySlot"
Cohesion: 0.16
Nodes (8): Color, Image, PointerEventData, ShopBuySlot, GameObject, ShopInventoryBuy, Instance, IPointerClickHandler

### Community 134 - "QuestCompleteUI"
Cohesion: 0.24
Nodes (6): Coroutine, GameObject, IEnumerator, TextMeshProUGUI, QuestCompleteUI, Instance

### Community 135 - "GrapplingHookBullet"
Cohesion: 0.16
Nodes (11): GameObject, Collider2D, IEnumerator, Transform, Vector2, GrapplingHookBullet, AttachedToObject, GameObject (+3 more)

### Community 136 - "CategoryColorMap"
Cohesion: 0.42
Nodes (5): Color, Dictionary, HierarchyCategory, Texture2D, CategoryColorMap

### Community 138 - "IntroPlayer"
Cohesion: 0.31
Nodes (3): Button, IntroPlayer, VideoPlayer

### Community 139 - "AudioManager"
Cohesion: 0.07
Nodes (18): AudioChannel, BGM, Master, SFX, AudioManager, Instance, AudioSource, LoadSceneMode (+10 more)

### Community 140 - "EnemyHitbox"
Cohesion: 0.18
Nodes (10): Collider2D, EnemyHitbox, HitPart, Body, Head, Tail, GameObject, Vector2 (+2 more)

### Community 141 - "Bird"
Cohesion: 0.43
Nodes (3): SpriteRenderer, Transform, Bird

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

### Community 147 - "PlayerFeedbackVFX"
Cohesion: 0.36
Nodes (4): Coroutine, IEnumerator, Image, PlayerFeedbackVFX

### Community 148 - "PlayerCamera"
Cohesion: 0.33
Nodes (4): CinemachineCamera, LoadSceneMode, Scene, PlayerCamera

### Community 149 - "VolumeSliderUI"
Cohesion: 0.40
Nodes (3): Slider, TMP_Text, VolumeSliderUI

### Community 150 - "NotificationLine"
Cohesion: 0.11
Nodes (14): CutsceneSkipHint, CanvasGroup, IEnumerator, Image, TextMeshProUGUI, NotificationLine, Data flow, Decisions (+6 more)

### Community 151 - "UIWindow"
Cohesion: 0.18
Nodes (9): UIType, Inventory, MainMenu, PauseMenu, Quest, SettingMenu, SettingPause, UIWindow (+1 more)

### Community 152 - "TipPanelUI"
Cohesion: 0.43
Nodes (3): Button, GameObject, TipPanelUI

### Community 153 - "CategoryIconMap"
Cohesion: 0.48
Nodes (4): Dictionary, HierarchyCategory, Texture2D, CategoryIconMap

### Community 155 - "com.h8man.2d.navmeshplus"
Cohesion: 0.33
Nodes (6): dependencies, depth, hash, source, version, com.h8man.2d.navmeshplus

### Community 157 - "DialogueState"
Cohesion: 0.31
Nodes (5): GameObject, HashSet, DialogueState, IsShowing, RuntimeInitializeOnLoadMethod

### Community 158 - "MoveKeyBindUI"
Cohesion: 0.16
Nodes (7): Coroutine, IEnumerator, TMP_Text, KeyBindErrorUI, Button, TMP_Text, MoveKeyBindUI

### Community 159 - ".TryOpenScript"
Cohesion: 0.60
Nodes (3): Component, ScriptNavigator, MonoScript

### Community 160 - "EnemyStateBase"
Cohesion: 0.14
Nodes (6): EnemyStateBase, enemyBrainContext, enemyFSM, StateTag, EnemyPrepareAttackState, StateTag

### Community 161 - "EnemyKillTracker"
Cohesion: 0.18
Nodes (3): HashSet, EnemyKillTracker, Instance

### Community 162 - "InteractableNextDay"
Cohesion: 0.22
Nodes (4): Button, Collider2D, GameObject, InteractableNextDay

### Community 163 - "SimpleAnimLoop"
Cohesion: 0.47
Nodes (3): Animator, IEnumerator, SimpleAnimLoop

### Community 164 - "PlayerStabAttack"
Cohesion: 0.50
Nodes (3): GameObject, IEnumerator, PlayerStabAttack

### Community 165 - "graphify reference: query, path, explain"
Cohesion: 0.33
Nodes (5): For /graphify explain, For /graphify path, graphify reference: query, path, explain, Step 0 — Constrained query expansion (REQUIRED before traversal), Step 1 — Traversal

### Community 166 - "LoadingUI"
Cohesion: 0.24
Nodes (5): Animator, CanvasGroup, Slider, TMP_Text, LoadingUI

### Community 169 - "SwimState"
Cohesion: 0.40
Nodes (5): SwimState, Biting, Reaching, Stopped, Swimming

### Community 170 - ".Notify"
Cohesion: 0.11
Nodes (9): Sprite, Sprite, ResourceNotificationPresenter, SaveNotificationUI, Test, NotificationTests, Task 4: Prefabs and scene setup, SetUp (+1 more)

### Community 171 - "3. Root causes"
Cohesion: 0.09
Nodes (28): ItemType, Ammo, Building, CheckpointItem, Consumable, KeyItem, Normal, List (+20 more)

### Community 172 - "Findings & refactor roadmap"
Cohesion: 0.47
Nodes (3): EncryptionHelper, 5. Risks, Findings & refactor roadmap

### Community 174 - "EntityDeathHandler"
Cohesion: 0.27
Nodes (5): Color, NavMeshAgent, Rigidbody2D, SpriteRenderer, EntityDeathHandler

### Community 175 - "SettingTabManager"
Cohesion: 0.39
Nodes (4): Button, Color, GameObject, SettingTabManager

### Community 176 - "UISettingManager"
Cohesion: 0.39
Nodes (3): GameObject, UISettingManager, IsOpen

### Community 177 - "com.unity.collections"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, depth, source, url (+3 more)

### Community 178 - "MiniHub"
Cohesion: 0.33
Nodes (7): GameObject, Vector2, HubEntry, MiniHub, HasHub, HubPosition, HubEntry

### Community 179 - "com.unity.modules.physics2d"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 180 - "KeyAction"
Cohesion: 0.18
Nodes (11): KeyAction, Aim, Attack, Interact, Inventory, Move, Sprint, SwitchItem (+3 more)

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
Nodes (11): `Debug.Log*` calls — 137 in 40 files, Dependency cycles (type level), Largest types, Layer violations — 37 type links, Metrics, Most depended-on (fan-in), Most dependencies (fan-out), Per-frame methods (`Update`/`FixedUpdate`/`LateUpdate`) — 33 in 31 files (+3 more)

### Community 189 - "MoveDirection"
Cohesion: 0.20
Nodes (9): MoveDirection, Down, Left, Right, Up, 4. Refactor roadmap (structure first), Phase 1: names (low risk), Phase 4: assemblies and namespaces (compile-time enforcement) (+1 more)

### Community 192 - "EnemyBrainContext"
Cohesion: 0.14
Nodes (10): Transform, EnemyBrainBase, enemyFSM, IsAttacking, MonoBehaviour, NavMeshAgent, Transform, EnemyBrainContext (+2 more)

### Community 193 - "OneTimePickupTracker"
Cohesion: 0.22
Nodes (3): List, OneTimePickupTracker, Instance

### Community 194 - "unityengine_scenemanagement"
Cohesion: 0.10
Nodes (6): GameState, Land, Menu, Ocean, Unity.Cinemachine, unityengine_scenemanagement

### Community 195 - "EnemyAttackState"
Cohesion: 0.24
Nodes (3): EnemyAttackState, IsAttacking, StateTag

### Community 198 - "UML refresh & refactor roadmap: design"
Cohesion: 0.29
Nodes (6): Decisions (agreed with the user), Deliverables, Goal, Next, UML refresh & refactor roadmap: design, Verification

### Community 199 - "00-overview.md"
Cohesion: 0.10
Nodes (14): Layer dependencies, Module dependencies, Module table, Overview — layers & modules, Core/Datas/ScriptableDatas/Characters, Files, Core/VideoManager, Files (+6 more)

### Community 200 - "UpgradeTooltip"
Cohesion: 0.22
Nodes (8): CanvasGroup, GameObject, List, RectTransform, TMP_Text, Transform, UpgradeTooltip, Canvas

### Community 201 - "QuestDetailPanel"
Cohesion: 0.09
Nodes (15): Image, Sprite, QuestVisuals, Action, Button, Image, TextMeshProUGUI, QuestDetailPanel (+7 more)

### Community 204 - ".RaiseAction"
Cohesion: 0.20
Nodes (3): GameObject, InventoryUIController, IsOpen

### Community 205 - "1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)"
Cohesion: 0.06
Nodes (32): NotificationChannel, Center, General, Resource, System, NotificationManager, Sprite, NotificationMessage (+24 more)

### Community 207 - "Gameplay/QuestSystems/Quest"
Cohesion: 0.40
Nodes (5): Event subscriptions, Files, Gameplay/QuestSystems/Quest, part 1, part 2

### Community 208 - "PlayerSprintVFX"
Cohesion: 0.27
Nodes (5): ParticleSystem, Quaternion, Vector2, Vector3, PlayerSprintVFX

### Community 210 - "Gameplay_Entities_Players_OxygenTube.md"
Cohesion: 0.29
Nodes (5): Files, Gameplay/Entities/Players/OxygenTube, Event subscriptions, Files, UI/Ocean

### Community 213 - "KeyBindUI"
Cohesion: 0.32
Nodes (3): Button, TMP_Text, KeyBindUI

### Community 214 - "Cutscene.cs"
Cohesion: 0.12
Nodes (10): DenysAlmaral, Game.Tests, nunit_framework, system_linq, system_security_cryptography, system_text, system_text_regularexpressions, unityeditor_scenemanagement (+2 more)

### Community 216 - "Gameplay_InteractObjects.md"
Cohesion: 0.18
Nodes (8): Core/Inputs, Event subscriptions, Files, Files, Gameplay/AudioSetting, Event subscriptions, Files, Gameplay/InteractObjects

### Community 218 - "Phase 3: cut remaining wrong-way links (medium risk)"
Cohesion: 0.16
Nodes (8): Image, Sprite, TMP_Text, DialogueSpeaker, IEnumerator, TextMeshProUGUI, DamagePoint, Phase 3: cut remaining wrong-way links (medium risk)

### Community 227 - "ItemDrop"
Cohesion: 0.29
Nodes (3): IEnumerator, ItemDrop, ActiveItem

### Community 232 - "Gameplay_Entities_Players.md"
Cohesion: 0.12
Nodes (11): Core/Datas, Files, Event subscriptions, Files, Gameplay/Entities/Players, Files, Gameplay/MainCameras, Files (+3 more)

### Community 237 - "Core_Services.md"
Cohesion: 0.15
Nodes (11): Core/Datas/Upgrades, Files, Core/Services, Files, Files, Gameplay/Projectiles, Files, Gameplay/RopeSystems (+3 more)

### Community 240 - "UI_Inventory.md"
Cohesion: 0.11
Nodes (17): Core/Datas/Items, Files, Event subscriptions, Files, Files, UI/Inventory/InventoryBin, Files, UI/Inventory/InventoryBuy (+9 more)

### Community 241 - "UIType"
Cohesion: 0.25
Nodes (8): UIType, Inventory, MainMenu, None, PauseMenu, Quest, SettingMenu, SettingPause

### Community 242 - "NormalBullet"
Cohesion: 0.43
Nodes (3): Rigidbody2D, NormalBullet, MaxRange

### Community 249 - "KeyBindSettings"
Cohesion: 0.50
Nodes (3): List, KeyBindingData, KeyBindSettings

## Knowledge Gaps
- **1058 isolated node(s):** `IsVisible`, `IconStyle`, `BadgeStyle`, `WarningContent`, `None` (+1053 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1789 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **21 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `3. Root causes` connect `3. Root causes` to `GameManager`, `UIManager`, `OxygenRope`, `EnemyNavAgentBase`, `InventoryManager`, `Player`, `QuestManager`, `EnemyBrain`, `ShopManager`, `SaveData`, `EnemyFSM`, `Findings & refactor roadmap`, `LinearPatrolBrain`, `LoadingManager`, `EnemyBrainContext`, `DayManager`, `QuestDetailPanel`, `1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)`, `PlayerHealth`, `UpgradeService`, `QuestBoardUI`, `ISaveParticipant`, `PlayerMoney`, `NormalBullet`, `TutorialActionId`?**
  _High betweenness centrality (0.083) - this node is a cross-community bridge._
- **Why does `Findings & refactor roadmap` connect `Findings & refactor roadmap` to `3. Root causes`, `ScriptableObject`, `1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)`, `README.md`, `MoveDirection`?**
  _High betweenness centrality (0.079) - this node is a cross-community bridge._
- **Why does `Unity.Cinemachine` connect `unityengine_scenemanagement` to `project-ocean-worktree2.slnx`, `project-ocean.slnx`?**
  _High betweenness centrality (0.070) - this node is a cross-community bridge._
- **What connects `IsVisible`, `IconStyle`, `BadgeStyle` to the rest of the system?**
  _1058 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `project-ocean-worktree2.slnx` be split into smaller, more focused modules?**
  _Cohesion score 0.0273972602739726 - nodes in this community are weakly interconnected._
- **Should `unityengine` be split into smaller, more focused modules?**
  _Cohesion score 0.046984572230014024 - nodes in this community are weakly interconnected._
- **Should `UIManager` be split into smaller, more focused modules?**
  _Cohesion score 0.12012012012012012 - nodes in this community are weakly interconnected._