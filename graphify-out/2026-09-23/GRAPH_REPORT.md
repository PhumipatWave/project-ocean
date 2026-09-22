# Graph Report - project-ocean  (2026-09-23)

## Corpus Check
- 250 files · ~321,235 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 2452 file(s) not represented in the graph (top: .meta 1645, .asset 506, .prefab 123)

## Summary
- 3716 nodes · 6446 edges · 197 communities (173 shown, 24 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 168 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `f7965e64`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- project-ocean.slnx
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
- InventorySlot
- Boid
- QuestStep
- com.unity.modules.imgui
- QuestBoardUI
- com.unity.2d.common
- ItemSpawner
- com.unity.modules.animation
- FishMotion
- @InputSystem_Actions
- GameHUD
- CutsceneManager
- PlayerGunHandler
- Player
- unityeditor
- SaveData
- What You Must Do When Invoked
- com.unity.render-pipelines.core
- HierarchyCategory
- EnemyStateBase
- GameFeature
- EnemyBrainContext
- TrashInteract
- dependencies
- com.unity.test-framework
- LinearPatrolBrain
- PlayerMoney
- QuestData.cs
- TutorialActionId
- unityengine_scenemanagement
- CallbackContext
- EnemyNavPathfinder
- MonoBehaviour
- EnemyFacingController
- EntityHealth
- com.unity.modules.physics
- GameManager
- InventoryUIController
- Game.Core
- EnemySpawner
- EnemyWanderState
- Entity
- NPC
- DoorInteract
- ComponentInspectorPopup
- ComponentMinimap
- .DrawCategory
- GameObjectSpawnManager
- PlayerSelfDialogueUI
- com.unity.editorcoroutines
- ObjectPoolManager
- com.unity.collections
- PlayerHealth
- QuestDetailPanel
- ReachPoint
- PlayerActions
- ChestInteract
- RopeBreakCountdownUI
- com.unity.2d.tilemap
- com.unity.dt.app-ui
- EnemyKillTracker
- SaveMenuUI
- UpgradeData
- EnemyFollowState
- Bullet
- UpgradeSlot
- com.unity.modules.uielements
- QuestStepDataDrawer
- ShelleController
- QuestStatus
- NotificationTextUI
- UpgradeTooltip
- com.unity.modules.subsystems
- Avoidance
- AmmoWheel
- UIActions
- SaveSystem
- QuestCategory
- EntityDeathHandler
- TutorialPromptUI
- HierarchyDataCache
- EnemyFleeState
- NextDayAnimationUI
- InteractableNextDay
- Cutscene
- .InteractPosition
- TrashBin
- com.unity.nuget.newtonsoft-json
- com.unity.ext.nunit
- ReadmeReaderEditor
- NPCManager
- .UnregisterCallbacks
- DevPanel
- InteractUI
- ReviceQuestListUI
- ButtonHoverScale
- ConfirmationDialog
- LandUI
- UNITY PROJECT AI GUIDE
- .Apply
- com.unity.modules.ai
- UNITY PROJECT AI GUIDE
- HierarchyPlus — Installation Guide
- MotionBendTracking
- PlayerUIState
- DayManager
- EnemyBrainBase
- Draggableltem
- PlayerInterior
- TutorialActionMarker
- Item
- QuestCompleteUI
- com.unity.modules.physics2d
- CategoryColorMap
- IntroPlayer
- AudioManager
- EnemyHitbox
- Bird
- GrapplingHookBullet
- graphify reference: extra exports and benchmark
- HierarchyPlusCore
- MissingScriptDetector
- Parallax
- .RaiseAction
- PlayerCamera
- VolumeSliderUI
- UISlotHoverHighlight
- CutsceneSkipHint
- ReviceQuestListUI.cs
- CategoryIconMap
- DamagePoint
- .HandleBulletHit
- com.unity.collab-proxy
- TipPanelUI
- .TryOpenScript
- ScriptableObject
- SceneLoadContext
- FishAlertUI
- SimpleAnimLoop
- CameraShakeManager
- graphify reference: query, path, explain
- ShopBuySlot
- .Awake
- .BeginReach
- SwimState
- EnemyAgent
- .StartDive
- EntityAnimation
- EncryptionHelper
- .Apply
- QuestSlot
- WaterShadowWiggle
- ItemDrop
- MiniHub
- KeyBindResetAllButton
- DialogueState
- SettingTabManager
- graphify reference: add a URL and watch a folder
- graphify reference: commit hook and native CLAUDE.md integration
- graphify reference: incremental update and cluster-only
- HierarchyPlus — Folder Structure
- graphify reference: GitHub clone and cross-repo merge
- graphify reference: transcribe video and audio
- SceneNames.cs
- PlayerStabAttack
- .claude/CLAUDE.md
- extraction-spec.md
- QuestTrackerUI
- NotificationUI
- TutorialUIMarker

## God Nodes (most connected - your core abstractions)
1. `Game.Testing` - 178 edges
2. `FishMotion` - 68 edges
3. `Quest` - 67 edges
4. `EnemyNavAgentBase` - 63 edges
5. `OxygenRope` - 61 edges
6. `InventoryManager` - 60 edges
7. `Player` - 56 edges
8. `QuestManager` - 55 edges
9. `QuestStep` - 48 edges
10. `UIManager` - 44 edges

## Surprising Connections (you probably didn't know these)
- `FishMotion` --references--> `ReachPoint`  [EXTRACTED]
  Assets/Project_Ocean/Resources/DenysAlmaral/FishAlive/Scripts/FishMotion.cs → Assets/Project_Ocean/Resources/DenysAlmaral/FishAlive/Scripts/ReachPoint.cs
- `FishMotion` --references--> `SwimConfig`  [EXTRACTED]
  Assets/Project_Ocean/Resources/DenysAlmaral/FishAlive/Scripts/FishMotion.cs → Assets/Project_Ocean/Resources/DenysAlmaral/FishAlive/Scripts/SwimConfig.cs
- `PlayerGunHandler` --references--> `AmmoType`  [EXTRACTED]
  Assets/Project_Ocean/Scripts/Gameplay/Entitys/Players/PlayerGunHandler.cs → Assets/Project_Ocean/Scripts/Core/Datas/BulletData.cs
- `PlayerGunHandler` --references--> `RopeStructureData`  [EXTRACTED]
  Assets/Project_Ocean/Scripts/Gameplay/Entitys/Players/PlayerGunHandler.cs → Assets/Project_Ocean/Scripts/Core/Datas/RopeData.cs
- `NPCManager` --references--> `CharacterDatabase`  [EXTRACTED]
  Assets/Project_Ocean/Scripts/Gameplay/Environments/NPCManager.cs → Assets/Project_Ocean/Scripts/Core/Datas/ScriptableDatas/Characters/CharacterDatabase.cs

## Import Cycles
- None detected.

## Communities (197 total, 24 thin omitted)

### Community 0 - "project-ocean.slnx"
Cohesion: 0.01
Nodes (143): Assembly-CSharp, Assembly-CSharp-Editor, Game.Core, Game.Testing, glTFast, glTFast.Documentation.Examples, glTFast.dots, glTFast.Editor (+135 more)

### Community 1 - "unityengine"
Cohesion: 0.05
Nodes (6): AnimHash, IHookable, Game.Testing, system_collections, unityengine, unityengine_ai

### Community 2 - "UIManager"
Cohesion: 0.06
Nodes (25): CanvasGroup, UIFadeBackground, Dictionary, UIManager, CurrentUI, Instance, IsUIOpen, UIType (+17 more)

### Community 3 - "KeyBindManager"
Cohesion: 0.05
Nodes (36): KeyAction, Aim, Attack, Interact, Inventory, Move, Sprint, SwitchItem (+28 more)

### Community 4 - "OxygenRope"
Cohesion: 0.06
Nodes (27): Collider2D, Coroutine, GameObject, IEnumerator, LayerMask, LineRenderer, List, Rigidbody2D (+19 more)

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
Cohesion: 0.06
Nodes (17): CallbackContext, Func, InputSystem_Actions, Vector2, InputReader, InputActions, MousePos, MovePos (+9 more)

### Community 9 - "InventoryManager"
Cohesion: 0.10
Nodes (9): Action, AudioClip, GameObject, List, TextMeshProUGUI, InventoryManager, ActiveBuildingSlotIndex, Instance (+1 more)

### Community 10 - "Player"
Cohesion: 0.05
Nodes (24): AudioClip, AudioSource, Color, Coroutine, GameObject, IEnumerator, List, Rigidbody2D (+16 more)

### Community 11 - "ExploreAreaTrigger"
Cohesion: 0.09
Nodes (14): Collider2D, GameObject, Image, List, TextMeshProUGUI, Transform, DialogueLine, ExploreAreaTrigger (+6 more)

### Community 12 - "AdditiveSceneSetupWindow"
Cohesion: 0.07
Nodes (23): AdditiveSceneEntry, Component, GUIStyle, Vector2, ComponentTooltipWindow, IsVisible, AdditiveSceneConfig, AdditiveSceneEntry (+15 more)

### Community 13 - "system_collections_generic"
Cohesion: 0.06
Nodes (12): Vector3, FishConstant, CutsceneGate, IsPlaying, FishAlive, system, system_collections_generic, system_io (+4 more)

### Community 14 - "ObjectWrapPoint"
Cohesion: 0.13
Nodes (12): Collider2D, Dictionary, IEnumerable, IReadOnlyList, List, Vector2, ObjectWrapPoint, WrapPoint (+4 more)

### Community 15 - "Enemy"
Cohesion: 0.06
Nodes (19): Animator, Collision2D, Color, IEnumerator, SpriteRenderer, Enemy, Brain, EnemyId (+11 more)

### Community 16 - "packages-lock.json"
Cohesion: 0.05
Nodes (41): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.director, com.unity.modules.imageconversion (+33 more)

### Community 17 - "QuestManager"
Cohesion: 0.09
Nodes (5): Coroutine, Func, IEnumerator, QuestManager, Instance

### Community 18 - "com.unity.mathematics"
Cohesion: 0.06
Nodes (40): dependencies, depth, source, url, version, dependencies, depth, source (+32 more)

### Community 19 - "EnemyBrain"
Cohesion: 0.08
Nodes (20): IBrainAttack, AttackRange, IBrainHitReaction, IBrainVision, ViewAngle, ViewDistance, AggressiveEnemyProfile, detectDistance (+12 more)

### Community 20 - "ShopManager"
Cohesion: 0.10
Nodes (6): GameObject, IEnumerator, Image, TMP_Text, ShopManager, Instance

### Community 21 - "unityengine_ui"
Cohesion: 0.09
Nodes (3): tmpro, unityengine_eventsystems, unityengine_ui

### Community 22 - "Quest"
Cohesion: 0.08
Nodes (23): Quest, Category, Classification, CurrentCount, CurrentStep, CurrentStepIndex, DisplayNpcId, IsGuidedSequence (+15 more)

### Community 23 - "InventorySlot"
Cohesion: 0.16
Nodes (5): Color, Image, PointerEventData, InventorySlot, InventoryQuestService

### Community 24 - "Boid"
Cohesion: 0.09
Nodes (13): List, NavMeshAgent, Vector2, Boid, IsPaused, List, FlockLeader, GroupFlock (+5 more)

### Community 25 - "QuestStep"
Cohesion: 0.06
Nodes (31): characterId, Dictionary, Func, HashSet, IEnumerable, IReadOnlyList, List, Sprite (+23 more)

### Community 26 - "com.unity.modules.imgui"
Cohesion: 0.06
Nodes (35): dependencies, depth, source, url, version, dependencies, depth, source (+27 more)

### Community 27 - "QuestBoardUI"
Cohesion: 0.14
Nodes (8): Button, GameObject, List, Sprite, TextMeshProUGUI, QuestBoardUI, QuestsPerPage, QuestCategoryUIData

### Community 28 - "com.unity.2d.common"
Cohesion: 0.06
Nodes (34): dependencies, depth, source, url, version, dependencies, depth, source (+26 more)

### Community 29 - "ItemSpawner"
Cohesion: 0.18
Nodes (5): GameObject, List, Transform, ItemSpawner, ZoneItemEntry

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
Cohesion: 0.09
Nodes (13): Button, CanvasGroup, GameObject, IEnumerator, Image, LoadSceneMode, Scene, TextMeshProUGUI (+5 more)

### Community 34 - "CutsceneManager"
Cohesion: 0.15
Nodes (8): CutsceneEvents, Action, Dictionary, GameObject, IEnumerator, List, CutsceneManager, Instance

### Community 35 - "PlayerGunHandler"
Cohesion: 0.08
Nodes (20): AudioClip, Collider2D, IEnumerator, LayerMask, LineRenderer, Rigidbody2D, SpriteRenderer, Transform (+12 more)

### Community 36 - "Player"
Cohesion: 0.11
Nodes (14): Vector2, PlayerRegistry, Local, Player, IEnumerator, LineRenderer, Rigidbody2D, CheckPointItem (+6 more)

### Community 37 - "unityeditor"
Cohesion: 0.10
Nodes (8): MenuItem, AsmdefInstaller, MenuItem, DocsInstaller, DenysAlmaral, HierarchyPlus.Editor, unityeditor, unityeditorinternal

### Community 38 - "SaveData"
Cohesion: 0.13
Nodes (11): IEnumerator, List, LoadSceneMode, Scene, SaveBridge, CurrentSlot, Instance, List (+3 more)

### Community 39 - "What You Must Do When Invoked"
Cohesion: 0.08
Nodes (24): For /graphify add and --watch, For /graphify query, For the commit hook and native CLAUDE.md integration, For --update and --cluster-only, /graphify, Honesty Rules, Interpreter guard for subcommands, Part A - Structural extraction for code files (+16 more)

### Community 40 - "com.unity.render-pipelines.core"
Cohesion: 0.09
Nodes (25): depth, source, version, dependencies, depth, source, version, dependencies (+17 more)

### Community 41 - "HierarchyCategory"
Cohesion: 0.09
Nodes (20): DevelopmentStatus, Complete, Deprecated, New, None, Problem, Review, WorkInProgress (+12 more)

### Community 42 - "EnemyStateBase"
Cohesion: 0.08
Nodes (18): IEnemyState, StateTag, Vector3, EnemyStateBase, enemyBrainContext, enemyFSM, StateTag, EnemyStateTag (+10 more)

### Community 43 - "GameFeature"
Cohesion: 0.09
Nodes (21): GameFeature, BackToLand, Combat, EnemySpawn, EnemySpawnLevel1, EnemySpawnLevel2, EnemySpawnLevel3, Interact (+13 more)

### Community 44 - "EnemyBrainContext"
Cohesion: 0.16
Nodes (10): MonoBehaviour, NavMeshAgent, Transform, EnemyBrainContext, IsDead, PlayerTransform, Dictionary, EnemyFSM (+2 more)

### Community 45 - "TrashInteract"
Cohesion: 0.12
Nodes (11): AudioSource, Collider2D, Coroutine, GameObject, IEnumerator, Image, Rigidbody2D, Sprite (+3 more)

### Community 46 - "dependencies"
Cohesion: 0.08
Nodes (24): dependencies, depth, hash, source, version, dependencies, depth, source (+16 more)

### Community 47 - "com.unity.test-framework"
Cohesion: 0.09
Nodes (23): dependencies, dependencies, depth, source, url, version, dependencies, depth (+15 more)

### Community 48 - "LinearPatrolBrain"
Cohesion: 0.08
Nodes (18): Rigidbody2D, Transform, Vector2, LinearPatrolBrain, IsAttacking, State, Moving, Pausing (+10 more)

### Community 49 - "PlayerMoney"
Cohesion: 0.14
Nodes (6): PlayerMoney, CurrentMoney, Money, TMP_Text, MoneyUI, ShopTransactionService

### Community 50 - "QuestData.cs"
Cohesion: 0.09
Nodes (27): DeliverySource, EnemyKills, InventoryHeld, NextQuestMode, List, Sprite, DeliveryCountMode, CombinedTotal (+19 more)

### Community 51 - "TutorialActionId"
Cohesion: 0.10
Nodes (21): TutorialActionId, AcceptQuest, BackToLand, BreakRopeLimit, BuyItem, Dive, HitEnemy, Interact (+13 more)

### Community 52 - "unityengine_scenemanagement"
Cohesion: 0.09
Nodes (9): GameState, Land, Menu, Ocean, system_linq, Unity.Cinemachine, unityeditor_scenemanagement, unityengine_scenemanagement (+1 more)

### Community 53 - "CallbackContext"
Cohesion: 0.17
Nodes (3): CallbackContext, IPlayerActions, IUIActions

### Community 54 - "EnemyNavPathfinder"
Cohesion: 0.18
Nodes (11): Action, Func, Vector3, EnemySearchState, SearchOrigin, StateTag, LayerMask, NavMeshAgent (+3 more)

### Community 55 - "MonoBehaviour"
Cohesion: 0.12
Nodes (8): RevealAfterTotalDives, MiniHubItem, ItemUI, MainMenu, SaveNotificationUI, UILock, SettingMenuButton, MonoBehaviour

### Community 56 - "EnemyFacingController"
Cohesion: 0.14
Nodes (12): SpriteRenderer, Transform, CharacterFacing2D, FacingTurnMode, Transform, Vector2, Vector3, EnemyFacingController (+4 more)

### Community 57 - "EntityHealth"
Cohesion: 0.13
Nodes (9): Color, Coroutine, IEnumerator, SpriteRenderer, EntityHealth, CurrentHealth, HitColor, IsDead (+1 more)

### Community 58 - "com.unity.modules.physics"
Cohesion: 0.10
Nodes (20): dependencies, depth, source, version, dependencies, depth, source, version (+12 more)

### Community 59 - "GameManager"
Cohesion: 0.12
Nodes (11): LoadSceneMode, Rigidbody2D, Scene, Transform, Vector3, GameManager, CurPlayer, Instance (+3 more)

### Community 60 - "InventoryUIController"
Cohesion: 0.31
Nodes (4): AudioClip, GameObject, InventoryUIController, IsOpen

### Community 61 - "Game.Core"
Cohesion: 0.10
Nodes (3): Game.Core, unityengine_events, unityengine_inputsystem

### Community 62 - "EnemySpawner"
Cohesion: 0.13
Nodes (8): GameObject, IEnumerator, LayerMask, List, Rigidbody2D, Vector3, EnemySpawner, ZoneSpawnEntry

### Community 63 - "EnemyWanderState"
Cohesion: 0.16
Nodes (8): EnemyVision, Vector3, EnemyWanderState, StateTag, LayerMask, Transform, Vector2, EnemyVisionHelper

### Community 64 - "Entity"
Cohesion: 0.15
Nodes (5): Rigidbody2D, Entity, CurrentHealth, IsDead, MaxHealth

### Community 65 - "NPC"
Cohesion: 0.10
Nodes (11): Collider2D, GameObject, Image, Sprite, TextMeshProUGUI, NPC, HasQuest, NpcId (+3 more)

### Community 66 - "DoorInteract"
Cohesion: 0.13
Nodes (10): AudioClip, AudioSource, CanvasGroup, CinemachineCamera, GameObject, IEnumerator, List, Transform (+2 more)

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
Cohesion: 0.17
Nodes (9): Line, GameObject, IEnumerator, Image, List, TextMeshProUGUI, Line, PlayerSelfDialogueUI (+1 more)

### Community 72 - "com.unity.editorcoroutines"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, url, version, dependencies, depth, source (+10 more)

### Community 73 - "ObjectPoolManager"
Cohesion: 0.19
Nodes (8): IPoolable, Dictionary, GameObject, Quaternion, Queue, Vector3, ObjectPoolManager, Instance

### Community 74 - "com.unity.collections"
Cohesion: 0.12
Nodes (17): dependencies, dependencies, depth, source, url, version, depth, source (+9 more)

### Community 75 - "PlayerHealth"
Cohesion: 0.15
Nodes (5): IEntityHealth, CurrentHealth, IsDead, MaxHealth, PlayerHealth

### Community 76 - "QuestDetailPanel"
Cohesion: 0.23
Nodes (5): Action, Button, Image, TextMeshProUGUI, QuestDetailPanel

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

### Community 82 - "com.unity.dt.app-ui"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, url, version, dependencies, depth, source (+8 more)

### Community 83 - "EnemyKillTracker"
Cohesion: 0.13
Nodes (4): HashSet, EnemyKillTracker, Instance, ISaveParticipant

### Community 84 - "SaveMenuUI"
Cohesion: 0.21
Nodes (6): GameObject, SaveMenuUI, Button, GameObject, TMP_Text, SaveSlotUI

### Community 85 - "UpgradeData"
Cohesion: 0.18
Nodes (10): Dictionary, UpgradeService, Instance, List, UpgradeData, UpgradeType, Inventory, NormalBullet (+2 more)

### Community 86 - "EnemyFollowState"
Cohesion: 0.14
Nodes (8): EnemyAttackState, IsAttacking, StateTag, Vector2, Vector3, EnemyFollowState, LastKnownPlayerPosition, StateTag

### Community 87 - "Bullet"
Cohesion: 0.13
Nodes (9): GameObject, IEnumerator, Rigidbody2D, Vector2, Bullet, GranadeBullet, Rigidbody2D, NormalBullet (+1 more)

### Community 88 - "UpgradeSlot"
Cohesion: 0.17
Nodes (7): UpgradeManager, Color, Image, PointerEventData, RectTransform, TMP_Text, UpgradeSlot

### Community 89 - "com.unity.modules.uielements"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, url, version, depth, source, version (+7 more)

### Community 90 - "QuestStepDataDrawer"
Cohesion: 0.14
Nodes (20): characterId, Dictionary, GUIContent, List, Rect, SerializedProperty, FieldVisibility, QuestStepDataDrawer (+12 more)

### Community 91 - "ShelleController"
Cohesion: 0.23
Nodes (4): Animator, Coroutine, IEnumerator, ShelleController

### Community 92 - "QuestStatus"
Cohesion: 0.29
Nodes (6): QuestStatus, Finish, InProgress, New, ReadyToTurnIn, Reject

### Community 93 - "NotificationTextUI"
Cohesion: 0.17
Nodes (7): CanvasGroup, Coroutine, IEnumerator, Sprite, TextMeshProUGUI, NotificationTextUI, IsShowing

### Community 94 - "UpgradeTooltip"
Cohesion: 0.22
Nodes (8): CanvasGroup, GameObject, List, RectTransform, TMP_Text, Transform, UpgradeTooltip, Canvas

### Community 95 - "com.unity.modules.subsystems"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 96 - "Avoidance"
Cohesion: 0.21
Nodes (7): Avoidance, Avoiding, Parent, found, numTries, RaycastHit, wayOut

### Community 97 - "AmmoWheel"
Cohesion: 0.17
Nodes (9): AmmoType, Granade, GrapplingHook, Normal, Tranquilizer, GameObject, RectTransform, AmmoWheel (+1 more)

### Community 98 - "UIActions"
Cohesion: 0.21
Nodes (8): UIActions, @Cancel, @Click, enabled, @MiddleClick, @RightClick, @ScrollWheel, @Submit

### Community 99 - "SaveSystem"
Cohesion: 0.26
Nodes (5): GameState, List, SaveSlotMeta, SaveSystem, LegacyPath

### Community 100 - "QuestCategory"
Cohesion: 0.18
Nodes (10): QuestCategoryUIData, Action, Button, QuestCategorySlot, QuestCategory, Explore, FishL, FishM (+2 more)

### Community 101 - "EntityDeathHandler"
Cohesion: 0.27
Nodes (5): Color, NavMeshAgent, Rigidbody2D, SpriteRenderer, EntityDeathHandler

### Community 102 - "TutorialPromptUI"
Cohesion: 0.21
Nodes (4): Sprite, TutorialPromptUI, Sprite, NotificationManager

### Community 103 - "HierarchyDataCache"
Cohesion: 0.27
Nodes (7): DevelopmentStatusTag, Dictionary, GameObject, Cache, HierarchyDataCache, Cache, HierarchyCategoryTag

### Community 104 - "EnemyFleeState"
Cohesion: 0.22
Nodes (5): LayerMask, Vector2, Vector2, EnemyFleeState, StateTag

### Community 105 - "NextDayAnimationUI"
Cohesion: 0.20
Nodes (6): Animator, CanvasGroup, Coroutine, IEnumerator, NextDayAnimationUI, IsPlaying

### Community 106 - "InteractableNextDay"
Cohesion: 0.22
Nodes (4): Button, Collider2D, GameObject, InteractableNextDay

### Community 107 - "Cutscene"
Cohesion: 0.19
Nodes (7): Action, Component, List, Cutscene, CutsceneId, CutsceneTrackBinding, PlayableDirector

### Community 108 - ".InteractPosition"
Cohesion: 0.23
Nodes (7): GameObject, QuestCompass, Vector3, Collider2D, Transform, Vector3, QuestNavigation

### Community 109 - "TrashBin"
Cohesion: 0.20
Nodes (8): Image, PointerEventData, Sprite, Vector3, TrashBin, PointerEventData, SellSlot, IDropHandler

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

### Community 115 - "DevPanel"
Cohesion: 0.21
Nodes (6): Button, GameObject, DevItemButton, DevPanel, DevItemButton, Toggle

### Community 116 - "InteractUI"
Cohesion: 0.22
Nodes (4): Collider2D, GameObject, UnityEvent, InteractUI

### Community 117 - "ReviceQuestListUI"
Cohesion: 0.22
Nodes (7): TextMeshProUGUI, Transform, ReviceQuestListUI, CanvasGroup, Image, TextMeshProUGUI, ReviceQuestSlot

### Community 118 - "ButtonHoverScale"
Cohesion: 0.26
Nodes (5): ButtonHoverScale, PointerEventData, TMP_Text, Vector3, IPointerExitHandler

### Community 119 - "ConfirmationDialog"
Cohesion: 0.24
Nodes (6): Action, Button, GameObject, TextMeshProUGUI, ConfirmationDialog, Instance

### Community 120 - "LandUI"
Cohesion: 0.15
Nodes (9): alpha, Button, CanvasGroup, Dictionary, TMP_Text, LandUI, blocksRaycasts, interactable (+1 more)

### Community 121 - "UNITY PROJECT AI GUIDE"
Cohesion: 0.18
Nodes (10): AI Workflow, Architecture Rules, Existing Managers, Git Rules, graphify, Project Overview, Save System Rules, Scene Structure (+2 more)

### Community 122 - ".Apply"
Cohesion: 0.27
Nodes (3): Image, Sprite, QuestVisuals

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
Cohesion: 0.20
Nodes (5): Action, PlayerUIState, IsLocked, EscClosablePanel, Stack

### Community 128 - "DayManager"
Cohesion: 0.14
Nodes (9): UnityEvent, DayManager, CanDive, CurrentDay, DivesRemaining, DivesToday, HasDivedToday, Instance (+1 more)

### Community 129 - "EnemyBrainBase"
Cohesion: 0.24
Nodes (4): Transform, EnemyBrainBase, enemyFSM, IsAttacking

### Community 130 - "Draggableltem"
Cohesion: 0.11
Nodes (16): Image, PointerEventData, TMP_Text, Transform, Draggableltem, ItemType, Ammo, Building (+8 more)

### Community 131 - "PlayerInterior"
Cohesion: 0.22
Nodes (8): PlayerInterior, House, None, Shop, UpgradeShop, PlayerInteriorState, Current, CurrentExitAction

### Community 132 - "TutorialActionMarker"
Cohesion: 0.24
Nodes (4): GameObject, List, Transform, TutorialActionMarker

### Community 133 - "Item"
Cohesion: 0.24
Nodes (4): InventoryItemDatabase, GameObject, Sprite, Item

### Community 134 - "QuestCompleteUI"
Cohesion: 0.24
Nodes (6): Coroutine, GameObject, IEnumerator, TextMeshProUGUI, QuestCompleteUI, Instance

### Community 135 - "com.unity.modules.physics2d"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 136 - "CategoryColorMap"
Cohesion: 0.42
Nodes (5): Color, Dictionary, HierarchyCategory, Texture2D, CategoryColorMap

### Community 138 - "IntroPlayer"
Cohesion: 0.33
Nodes (3): Button, IntroPlayer, VideoPlayer

### Community 140 - "EnemyHitbox"
Cohesion: 0.18
Nodes (10): Collider2D, EnemyHitbox, HitPart, Body, Head, Tail, GameObject, Vector2 (+2 more)

### Community 141 - "Bird"
Cohesion: 0.43
Nodes (3): SpriteRenderer, Transform, Bird

### Community 142 - "GrapplingHookBullet"
Cohesion: 0.16
Nodes (11): GameObject, Collider2D, IEnumerator, Transform, Vector2, GrapplingHookBullet, AttachedToObject, GameObject (+3 more)

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

### Community 147 - ".RaiseAction"
Cohesion: 0.22
Nodes (3): Collider2D, SpriteRenderer, TutorialEvents

### Community 148 - "PlayerCamera"
Cohesion: 0.33
Nodes (4): CinemachineCamera, LoadSceneMode, Scene, PlayerCamera

### Community 149 - "VolumeSliderUI"
Cohesion: 0.40
Nodes (3): TMP_Text, VolumeSliderUI, Slider

### Community 150 - "UISlotHoverHighlight"
Cohesion: 0.29
Nodes (5): Color, Image, PointerEventData, UISlotHoverHighlight, IPointerEnterHandler

### Community 153 - "CategoryIconMap"
Cohesion: 0.48
Nodes (4): Dictionary, HierarchyCategory, Texture2D, CategoryIconMap

### Community 155 - "DamagePoint"
Cohesion: 0.32
Nodes (3): IEnumerator, TextMeshProUGUI, DamagePoint

### Community 156 - ".HandleBulletHit"
Cohesion: 0.22
Nodes (4): Collider2D, Collision2D, GameObject, Transform

### Community 157 - "com.unity.collab-proxy"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.collab-proxy

### Community 158 - "TipPanelUI"
Cohesion: 0.43
Nodes (3): Button, GameObject, TipPanelUI

### Community 159 - ".TryOpenScript"
Cohesion: 0.60
Nodes (3): Component, ScriptNavigator, MonoScript

### Community 160 - "ScriptableObject"
Cohesion: 0.12
Nodes (17): SceneAsset, ReadmeReader, SwimConfig, BulletData, EnemyType, Boss, Normal, Special (+9 more)

### Community 161 - "SceneLoadContext"
Cohesion: 0.40
Nodes (4): Vector3, SceneLoadContext, IsLoading, Position

### Community 162 - "FishAlertUI"
Cohesion: 0.23
Nodes (4): Coroutine, GameObject, IEnumerator, FishAlertUI

### Community 163 - "SimpleAnimLoop"
Cohesion: 0.47
Nodes (3): Animator, IEnumerator, SimpleAnimLoop

### Community 165 - "graphify reference: query, path, explain"
Cohesion: 0.33
Nodes (5): For /graphify explain, For /graphify path, graphify reference: query, path, explain, Step 0 — Constrained query expansion (REQUIRED before traversal), Step 1 — Traversal

### Community 166 - "ShopBuySlot"
Cohesion: 0.15
Nodes (8): Color, Image, PointerEventData, ShopBuySlot, GameObject, ShopInventoryBuy, Instance, IPointerClickHandler

### Community 169 - "SwimState"
Cohesion: 0.40
Nodes (5): SwimState, Biting, Reaching, Stopped, Swimming

### Community 170 - "EnemyAgent"
Cohesion: 0.25
Nodes (4): EnemyAgent, AutoReturnDelay, BaseProfile, requiresNavMesh

### Community 174 - ".Apply"
Cohesion: 0.40
Nodes (4): Image, Sprite, TMP_Text, DialogueSpeaker

### Community 175 - "QuestSlot"
Cohesion: 0.24
Nodes (7): Action, Button, Image, Sprite, TextMeshProUGUI, TMP_Text, QuestSlot

### Community 177 - "ItemDrop"
Cohesion: 0.22
Nodes (5): AudioClip, AudioSource, IEnumerator, ItemDrop, ActiveItem

### Community 178 - "MiniHub"
Cohesion: 0.33
Nodes (7): GameObject, Vector2, HubEntry, MiniHub, HasHub, HubPosition, HubEntry

### Community 180 - "DialogueState"
Cohesion: 0.24
Nodes (6): GameObject, HashSet, Transform, DialogueState, IsShowing, RuntimeInitializeOnLoadMethod

### Community 181 - "SettingTabManager"
Cohesion: 0.39
Nodes (4): Button, Color, GameObject, SettingTabManager

### Community 182 - "graphify reference: add a URL and watch a folder"
Cohesion: 0.50
Nodes (3): For /graphify add, For --watch, graphify reference: add a URL and watch a folder

### Community 183 - "graphify reference: commit hook and native CLAUDE.md integration"
Cohesion: 0.50
Nodes (3): For git commit hook, For native CLAUDE.md integration, graphify reference: commit hook and native CLAUDE.md integration

### Community 184 - "graphify reference: incremental update and cluster-only"
Cohesion: 0.50
Nodes (3): For --cluster-only, For --update (incremental re-extraction), graphify reference: incremental update and cluster-only

### Community 189 - "PlayerStabAttack"
Cohesion: 0.50
Nodes (3): GameObject, IEnumerator, PlayerStabAttack

### Community 194 - "QuestTrackerUI"
Cohesion: 0.33
Nodes (4): GameObject, Image, TextMeshProUGUI, QuestTrackerUI

## Knowledge Gaps
- **1062 isolated node(s):** `IsVisible`, `IconStyle`, `BadgeStyle`, `WarningContent`, `None` (+1057 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1713 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **24 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Game.Testing` connect `unityengine` to `PlayerInterior`, `Item`, `system_collections_generic`, `EnemyBrain`, `unityengine_ui`, `ReviceQuestListUI.cs`, `ScriptableObject`, `Player`, `unityeditor`, `SaveData`, `EnemyStateBase`, `GameFeature`, `QuestData.cs`, `unityengine_scenemanagement`, `SceneNames.cs`, `Game.Core`, `ObjectPoolManager`, `EnemyKillTracker`, `Bullet`?**
  _High betweenness centrality (0.089) - this node is a cross-community bridge._
- **Why does `Unity.Cinemachine` connect `unityengine_scenemanagement` to `project-ocean.slnx`?**
  _High betweenness centrality (0.070) - this node is a cross-community bridge._
- **Why does `OxygenRope` connect `OxygenRope` to `unityengine`, `GameHUD`, `Player`, `PlayerHealth`, `UpgradeData`, `MonoBehaviour`?**
  _High betweenness centrality (0.031) - this node is a cross-community bridge._
- **What connects `IsVisible`, `IconStyle`, `BadgeStyle` to the rest of the system?**
  _1062 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `project-ocean.slnx` be split into smaller, more focused modules?**
  _Cohesion score 0.013888888888888888 - nodes in this community are weakly interconnected._
- **Should `unityengine` be split into smaller, more focused modules?**
  _Cohesion score 0.0493730407523511 - nodes in this community are weakly interconnected._
- **Should `UIManager` be split into smaller, more focused modules?**
  _Cohesion score 0.055178652193577565 - nodes in this community are weakly interconnected._