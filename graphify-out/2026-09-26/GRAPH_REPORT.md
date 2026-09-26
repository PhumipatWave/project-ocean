# Graph Report - project-ocean  (2026-09-25)

## Corpus Check
- 273 files · ~360,269 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 2571 file(s) not represented in the graph (top: .meta 1730, .asset 511, .prefab 134)

## Summary
- 3977 nodes · 7173 edges · 206 communities (179 shown, 27 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 243 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `0ffec56a`
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
- Item
- CollectableItemBase
- QuestStep
- com.unity.modules.imgui
- QuestBoardUI
- com.unity.2d.common
- UIWindow
- com.unity.modules.animation
- FishMotion
- @InputSystem_Actions
- GameHUD
- Cutscene
- PlayerGunHandler
- Player
- unityeditor
- SaveBridge
- What You Must Do When Invoked
- com.unity.render-pipelines.core
- HierarchyCategory
- system_collections
- GameFeature
- EnemyFSM
- TrashInteract
- dependencies
- com.unity.test-framework
- LinearPatrolBrain
- com.unity.collections
- QuestData.cs
- TutorialActionId
- Boid
- CallbackContext
- EnemyNavPathfinder
- MonoBehaviour
- EnemyFacingController
- EntityHealth
- com.unity.modules.physics
- EnemyBaseProfile
- com.unity.modules.physics2d
- GameManager
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
- com.unity.modules.subsystems
- Game.Core
- ItemSpawner
- ScriptableObject
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
- .Open
- project-ocean.slnx
- UpgradeTooltip
- Vector2
- Avoidance
- AmmoWheel
- UIActions
- SaveSystem
- FishAlertUI
- PunchScale
- TutorialPromptUI
- HierarchyDataCache
- PlayerMoney
- NextDayAnimationUI
- InteractableNextDay
- DevPanel
- .InteractPosition
- TrashBin
- com.unity.nuget.newtonsoft-json
- com.unity.ext.nunit
- ReadmeReaderEditor
- NPCManager
- .UnregisterCallbacks
- UIType
- InteractUI
- ReviceQuestListUI
- ButtonHoverScale
- ConfirmationDialog
- LandUI
- UNITY PROJECT AI GUIDE
- QuestDetailPanel
- com.unity.modules.ai
- UNITY PROJECT AI GUIDE
- HierarchyPlus — Installation Guide
- MotionBendTracking
- PlayerUIState
- DayManager
- EnemyBrainContext
- LoadingManager
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
- ItemDrop
- graphify reference: extra exports and benchmark
- HierarchyPlusCore
- MissingScriptDetector
- Parallax
- PlayerFeedbackVFX
- PlayerCamera
- VolumeSliderUI
- PlayerSprintVFX
- TipPanelUI
- CategoryIconMap
- LoadingUI
- KeyBindResetAllButton
- DialogueState
- AsmdefInstaller
- .TryOpenScript
- DocsInstaller
- SceneLoadContext
- TutorialGate
- SimpleAnimLoop
- CameraShakeManager
- graphify reference: query, path, explain
- SaveSlotMeta
- .Awake
- .BeginReach
- SwimState
- .Notify
- SaveData
- EncryptionHelper
- PlayerStabAttack
- .Apply
- SettingTabManager
- UISettingManager
- LandOpeningController
- MiniHub
- UISlotHoverHighlight
- PlayerHealth
- graphify reference: add a URL and watch a folder
- graphify reference: commit hook and native CLAUDE.md integration
- graphify reference: incremental update and cluster-only
- HierarchyPlus — Folder Structure
- graphify reference: GitHub clone and cross-repo merge
- graphify reference: transcribe video and audio
- .HandleBulletHit
- .claude/CLAUDE.md
- extraction-spec.md
- unityengine_scenemanagement
- MoneyUI
- TutorialUIMarker
- LoadingButton
- NotificationLine
- SoundKeys.cs
- NormalBullet
- QuestSlot
- EnemyAgent
- NotificationMessage
- EnemyAttackState
- com.unity.collab-proxy
- .SetupAggressiveStates
- EntityAnimation
- NotificationTests.cs

## God Nodes (most connected - your core abstractions)
1. `Game.Testing` - 194 edges
2. `FishMotion` - 68 edges
3. `Quest` - 66 edges
4. `OxygenRope` - 64 edges
5. `InventoryManager` - 64 edges
6. `EnemyNavAgentBase` - 59 edges
7. `Player` - 56 edges
8. `QuestManager` - 56 edges
9. `QuestStep` - 49 edges
10. `ShopManager` - 47 edges

## Surprising Connections (you probably didn't know these)
- `Goal` --references--> `CutsceneSkipHint`  [INFERRED]
  docs/superpowers/specs/2026-09-25-notification-channels-design.md → Assets/Project_Ocean/Scripts/Gameplay/Cutscenes/CutsceneSkipHint.cs
- `Gameplay changes` --references--> `PlayerMoney`  [INFERRED]
  docs/superpowers/specs/2026-09-25-notification-channels-design.md → Assets/Project_Ocean/Scripts/Gameplay/Entitys/Players/PlayerMoney.cs
- `Gameplay changes` --references--> `QuestManager`  [INFERRED]
  docs/superpowers/specs/2026-09-25-notification-channels-design.md → Assets/Project_Ocean/Scripts/Gameplay/QuestSystems/Quest/QuestManager.cs
- `Gameplay changes` --references--> `TutorialPromptUI`  [INFERRED]
  docs/superpowers/specs/2026-09-25-notification-channels-design.md → Assets/Project_Ocean/Scripts/Gameplay/QuestSystems/Quest/TutorialPromptUI.cs
- `Scene and prefabs` --references--> `GameHUD`  [INFERRED]
  docs/superpowers/specs/2026-09-25-notification-channels-design.md → Assets/Project_Ocean/Scripts/UI/GameHUD.cs

## Import Cycles
- None detected.

## Communities (206 total, 27 thin omitted)

### Community 0 - "project-ocean-worktree2.slnx"
Cohesion: 0.03
Nodes (72): Assembly-CSharp, Assembly-CSharp-Editor, Game.Core, Game.Testing, glTFast.dots, glTFast.Export, HFDownloader, HierarchyPlus.Editor (+64 more)

### Community 1 - "unityengine"
Cohesion: 0.05
Nodes (7): SceneNames, AnimHash, IHookable, Game.Testing, unityengine, unityengine_ai, unityengine_events

### Community 2 - "UIManager"
Cohesion: 0.12
Nodes (6): Dictionary, UIManager, CurrentUI, Instance, IsUIOpen, UIType

### Community 3 - "KeyBindManager"
Cohesion: 0.05
Nodes (36): KeyAction, Aim, Attack, Interact, Inventory, Move, Sprint, SwitchItem (+28 more)

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
Cohesion: 0.07
Nodes (18): Transform, IEnemyBrain, IsAttacking, FacingTurnMode, NavMeshAgent, EnemyNavAgentBase, ActiveBrain, AutoReturnDelay (+10 more)

### Community 8 - "InputReader"
Cohesion: 0.09
Nodes (10): CallbackContext, Func, InputSystem_Actions, Vector2, InputReader, InputActions, MousePos, MovePos (+2 more)

### Community 9 - "InventoryManager"
Cohesion: 0.08
Nodes (14): InventoryItemDatabase, Action, GameObject, IReadOnlyList, List, TextMeshProUGUI, InventoryManager, ActiveBuildingSlotIndex (+6 more)

### Community 10 - "Player"
Cohesion: 0.05
Nodes (23): Color, Coroutine, GameObject, IEnumerator, List, LoadSceneMode, Rigidbody2D, Scene (+15 more)

### Community 11 - "ExploreAreaTrigger"
Cohesion: 0.09
Nodes (14): Collider2D, GameObject, Image, List, TextMeshProUGUI, Transform, DialogueLine, ExploreAreaTrigger (+6 more)

### Community 12 - "AdditiveSceneSetupWindow"
Cohesion: 0.07
Nodes (23): AdditiveSceneEntry, Component, GUIStyle, Vector2, ComponentTooltipWindow, IsVisible, AdditiveSceneConfig, AdditiveSceneEntry (+15 more)

### Community 13 - "system_collections_generic"
Cohesion: 0.05
Nodes (13): Vector3, FishConstant, CutsceneGate, IsPlaying, FishAlive, system, system_collections_generic, system_security_cryptography (+5 more)

### Community 14 - "ObjectWrapPoint"
Cohesion: 0.13
Nodes (12): Collider2D, Dictionary, IEnumerable, IReadOnlyList, List, Vector2, ObjectWrapPoint, WrapPoint (+4 more)

### Community 15 - "Enemy"
Cohesion: 0.08
Nodes (15): Animator, Collision2D, Color, IEnumerator, SpriteRenderer, Enemy, Brain, EnemyId (+7 more)

### Community 16 - "packages-lock.json"
Cohesion: 0.05
Nodes (41): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.director, com.unity.modules.imageconversion (+33 more)

### Community 17 - "QuestManager"
Cohesion: 0.09
Nodes (6): Coroutine, Func, IEnumerator, QuestManager, Instance, Task 1: Channel bus and toast view

### Community 18 - "com.unity.mathematics"
Cohesion: 0.06
Nodes (40): dependencies, depth, source, url, version, dependencies, depth, source (+32 more)

### Community 19 - "EnemyBrain"
Cohesion: 0.10
Nodes (13): IBrainAttack, AttackRange, IBrainHitReaction, IBrainVision, ViewAngle, ViewDistance, Transform, Vector3 (+5 more)

### Community 20 - "ShopManager"
Cohesion: 0.08
Nodes (8): TutorialStep, Button, GameObject, IEnumerator, Image, TMP_Text, ShopManager, Instance

### Community 21 - "unityengine_ui"
Cohesion: 0.08
Nodes (3): tmpro, unityengine_eventsystems, unityengine_ui

### Community 22 - "Quest"
Cohesion: 0.07
Nodes (29): Quest, Category, Classification, CurrentCount, CurrentStep, CurrentStepIndex, DisplayNpcId, IsGuidedSequence (+21 more)

### Community 23 - "Item"
Cohesion: 0.06
Nodes (27): Image, PointerEventData, TMP_Text, Transform, Draggableltem, Color, Image, PointerEventData (+19 more)

### Community 24 - "CollectableItemBase"
Cohesion: 0.11
Nodes (9): EnemyLoot, CanCollect, Collider2D, GameObject, HashSet, Transform, CollectableItemBase, CanCollect (+1 more)

### Community 25 - "QuestStep"
Cohesion: 0.06
Nodes (31): characterId, Dictionary, Func, HashSet, IEnumerable, IReadOnlyList, List, Sprite (+23 more)

### Community 26 - "com.unity.modules.imgui"
Cohesion: 0.06
Nodes (35): dependencies, depth, source, url, version, dependencies, depth, source (+27 more)

### Community 27 - "QuestBoardUI"
Cohesion: 0.08
Nodes (18): Button, GameObject, List, Sprite, TextMeshProUGUI, QuestBoardUI, QuestsPerPage, QuestCategoryUIData (+10 more)

### Community 28 - "com.unity.2d.common"
Cohesion: 0.06
Nodes (34): dependencies, depth, source, url, version, dependencies, depth, source (+26 more)

### Community 29 - "UIWindow"
Cohesion: 0.18
Nodes (9): UIType, Inventory, MainMenu, PauseMenu, Quest, SettingMenu, SettingPause, UIWindow (+1 more)

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

### Community 34 - "Cutscene"
Cohesion: 0.08
Nodes (18): Action, Component, List, Rigidbody2D, Cutscene, CutsceneId, CutsceneTrackBinding, CutsceneEvents (+10 more)

### Community 35 - "PlayerGunHandler"
Cohesion: 0.09
Nodes (19): Collider2D, IEnumerator, LayerMask, LineRenderer, ParticleSystem, Rigidbody2D, SpriteRenderer, Transform (+11 more)

### Community 36 - "Player"
Cohesion: 0.09
Nodes (14): Vector2, PlayerRegistry, Local, Player, IEnumerator, LineRenderer, Rigidbody2D, CheckPointItem (+6 more)

### Community 37 - "unityeditor"
Cohesion: 0.13
Nodes (6): DenysAlmaral, HierarchyPlus.Editor, system_linq, unityeditor, unityeditor_scenemanagement, unityeditorinternal

### Community 38 - "SaveBridge"
Cohesion: 0.15
Nodes (6): IEnumerator, LoadSceneMode, Scene, SaveBridge, CurrentSlot, Instance

### Community 39 - "What You Must Do When Invoked"
Cohesion: 0.08
Nodes (24): For /graphify add and --watch, For /graphify query, For the commit hook and native CLAUDE.md integration, For --update and --cluster-only, /graphify, Honesty Rules, Interpreter guard for subcommands, Part A - Structural extraction for code files (+16 more)

### Community 40 - "com.unity.render-pipelines.core"
Cohesion: 0.09
Nodes (25): depth, source, version, dependencies, depth, source, version, dependencies (+17 more)

### Community 41 - "HierarchyCategory"
Cohesion: 0.09
Nodes (20): DevelopmentStatus, Complete, Deprecated, New, None, Problem, Review, WorkInProgress (+12 more)

### Community 43 - "GameFeature"
Cohesion: 0.09
Nodes (21): GameFeature, BackToLand, Combat, EnemySpawn, EnemySpawnLevel1, EnemySpawnLevel2, EnemySpawnLevel3, Interact (+13 more)

### Community 44 - "EnemyFSM"
Cohesion: 0.08
Nodes (22): IEnemyState, StateTag, Dictionary, EnemyFSM, CurrentState, CurrentTag, Vector3, EnemyStateBase (+14 more)

### Community 45 - "TrashInteract"
Cohesion: 0.14
Nodes (10): Collider2D, Coroutine, GameObject, IEnumerator, Image, Rigidbody2D, Sprite, TextMeshProUGUI (+2 more)

### Community 46 - "dependencies"
Cohesion: 0.08
Nodes (24): dependencies, depth, hash, source, version, dependencies, depth, source (+16 more)

### Community 47 - "com.unity.test-framework"
Cohesion: 0.09
Nodes (23): dependencies, dependencies, depth, source, url, version, dependencies, depth (+15 more)

### Community 48 - "LinearPatrolBrain"
Cohesion: 0.08
Nodes (20): Collider2D, Component, Rigidbody2D, Transform, Vector2, LinearPatrolBrain, IsAttacking, State (+12 more)

### Community 49 - "com.unity.collections"
Cohesion: 0.12
Nodes (17): dependencies, dependencies, depth, source, url, version, depth, source (+9 more)

### Community 50 - "QuestData.cs"
Cohesion: 0.09
Nodes (27): DeliverySource, EnemyKills, InventoryHeld, NextQuestMode, List, Sprite, DeliveryCountMode, CombinedTotal (+19 more)

### Community 51 - "TutorialActionId"
Cohesion: 0.10
Nodes (21): TutorialActionId, AcceptQuest, BackToLand, BreakRopeLimit, BuyItem, Dive, HitEnemy, Interact (+13 more)

### Community 52 - "Boid"
Cohesion: 0.06
Nodes (22): IPoolable, Dictionary, GameObject, Quaternion, Queue, Vector3, ObjectPoolManager, Instance (+14 more)

### Community 53 - "CallbackContext"
Cohesion: 0.17
Nodes (3): CallbackContext, IPlayerActions, IUIActions

### Community 54 - "EnemyNavPathfinder"
Cohesion: 0.18
Nodes (11): Action, Func, Vector3, EnemySearchState, SearchOrigin, StateTag, LayerMask, NavMeshAgent (+3 more)

### Community 55 - "MonoBehaviour"
Cohesion: 0.08
Nodes (13): Vector3, WaterShadowWiggle, RevealAfterTotalDives, MiniHubItem, ItemUI, BootstrapLoader, GameBootstrap, TMP_Text (+5 more)

### Community 56 - "EnemyFacingController"
Cohesion: 0.14
Nodes (12): SpriteRenderer, Transform, CharacterFacing2D, FacingTurnMode, Transform, Vector2, Vector3, EnemyFacingController (+4 more)

### Community 57 - "EntityHealth"
Cohesion: 0.13
Nodes (9): Color, Coroutine, IEnumerator, SpriteRenderer, EntityHealth, CurrentHealth, HitColor, IsDead (+1 more)

### Community 58 - "com.unity.modules.physics"
Cohesion: 0.10
Nodes (20): dependencies, depth, source, version, dependencies, depth, source, version (+12 more)

### Community 59 - "EnemyBaseProfile"
Cohesion: 0.15
Nodes (8): LayerMask, EnemyBaseProfile, RequiresNavMesh, FriendlyEnemyProfile, Vector2, EnemyFleeState, StateTag, ScaredEnemyProfile

### Community 60 - "com.unity.modules.physics2d"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 61 - "GameManager"
Cohesion: 0.11
Nodes (12): LoadSceneMode, Rigidbody2D, Scene, Transform, Vector3, GameManager, CurPlayer, Instance (+4 more)

### Community 62 - "EnemySpawner"
Cohesion: 0.14
Nodes (8): GameObject, IEnumerator, LayerMask, List, Rigidbody2D, Vector3, EnemySpawner, ZoneSpawnEntry

### Community 63 - "EnemyWanderState"
Cohesion: 0.15
Nodes (8): EnemyVision, Vector3, EnemyWanderState, StateTag, LayerMask, Transform, Vector2, EnemyVisionHelper

### Community 64 - "Entity"
Cohesion: 0.14
Nodes (5): Rigidbody2D, Entity, CurrentHealth, IsDead, MaxHealth

### Community 65 - "NPC"
Cohesion: 0.10
Nodes (11): Collider2D, GameObject, Image, Sprite, TextMeshProUGUI, NPC, HasQuest, NpcId (+3 more)

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
Cohesion: 0.17
Nodes (9): Line, GameObject, IEnumerator, Image, List, TextMeshProUGUI, Line, PlayerSelfDialogueUI (+1 more)

### Community 72 - "com.unity.editorcoroutines"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, url, version, dependencies, depth, source (+10 more)

### Community 73 - "com.unity.modules.subsystems"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 74 - "Game.Core"
Cohesion: 0.11
Nodes (3): Game.Core, system_io, unityengine_inputsystem

### Community 75 - "ItemSpawner"
Cohesion: 0.18
Nodes (5): GameObject, List, Transform, ItemSpawner, ZoneItemEntry

### Community 76 - "ScriptableObject"
Cohesion: 0.17
Nodes (12): SceneAsset, ReadmeReader, SwimConfig, BulletData, EnemyType, Boss, Normal, Special (+4 more)

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
Cohesion: 0.18
Nodes (3): HashSet, EnemyKillTracker, Instance

### Community 84 - "SaveMenuUI"
Cohesion: 0.20
Nodes (6): GameObject, SaveMenuUI, Button, GameObject, TMP_Text, SaveSlotUI

### Community 85 - "UpgradeData"
Cohesion: 0.19
Nodes (10): Dictionary, UpgradeService, Instance, List, UpgradeData, UpgradeType, Inventory, NormalBullet (+2 more)

### Community 86 - "EnemyFollowState"
Cohesion: 0.14
Nodes (9): AggressiveEnemyProfile, detectDistance, LayerMask, Vector2, Vector2, Vector3, EnemyFollowState, LastKnownPlayerPosition (+1 more)

### Community 87 - "Bullet"
Cohesion: 0.12
Nodes (10): Collider2D, GameObject, IEnumerator, Rigidbody2D, SpriteRenderer, Vector2, Bullet, MaxRange (+2 more)

### Community 88 - "UpgradeSlot"
Cohesion: 0.13
Nodes (8): UpgradeManager, Button, Color, Image, PointerEventData, RectTransform, TMP_Text, UpgradeSlot

### Community 89 - "com.unity.modules.uielements"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, url, version, depth, source, version (+7 more)

### Community 90 - "QuestStepDataDrawer"
Cohesion: 0.14
Nodes (20): characterId, Dictionary, GUIContent, List, Rect, SerializedProperty, FieldVisibility, QuestStepDataDrawer (+12 more)

### Community 91 - "ShelleController"
Cohesion: 0.23
Nodes (4): Animator, Coroutine, IEnumerator, ShelleController

### Community 93 - "project-ocean.slnx"
Cohesion: 0.03
Nodes (72): Game.Tests.EditMode, glTFast, glTFast.Documentation.Examples, glTFast.Editor, glTFast.Newtonsoft, HierarchyPlus.Runtime, MCPForUnity.Runtime, NavMeshPlus (+64 more)

### Community 94 - "UpgradeTooltip"
Cohesion: 0.23
Nodes (8): CanvasGroup, GameObject, List, RectTransform, TMP_Text, Transform, UpgradeTooltip, Canvas

### Community 95 - "Vector2"
Cohesion: 0.16
Nodes (6): Collider2D, List, Vector2, Vector3, Seg, WrapPin

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
Cohesion: 0.38
Nodes (3): List, SaveSystem, LegacyPath

### Community 100 - "FishAlertUI"
Cohesion: 0.23
Nodes (4): Coroutine, GameObject, IEnumerator, FishAlertUI

### Community 101 - "PunchScale"
Cohesion: 0.28
Nodes (4): Coroutine, IEnumerator, Vector3, PunchScale

### Community 102 - "TutorialPromptUI"
Cohesion: 0.27
Nodes (3): Sprite, TutorialPromptUI, CurrentPrompt

### Community 103 - "HierarchyDataCache"
Cohesion: 0.27
Nodes (7): DevelopmentStatusTag, Dictionary, GameObject, Cache, HierarchyDataCache, Cache, HierarchyCategoryTag

### Community 104 - "PlayerMoney"
Cohesion: 0.14
Nodes (7): PlayerMoney, CurrentMoney, Money, ShopTransactionService, Global Constraints, Notification Channels Implementation Plan, Spec corrections found while planning

### Community 105 - "NextDayAnimationUI"
Cohesion: 0.20
Nodes (6): Animator, CanvasGroup, Coroutine, IEnumerator, NextDayAnimationUI, IsPlaying

### Community 106 - "InteractableNextDay"
Cohesion: 0.22
Nodes (4): Button, Collider2D, GameObject, InteractableNextDay

### Community 107 - "DevPanel"
Cohesion: 0.16
Nodes (8): Button, GameObject, List, DevItemButton, DevPanel, DevItemButton, TMP_Dropdown, Toggle

### Community 108 - ".InteractPosition"
Cohesion: 0.23
Nodes (7): GameObject, QuestCompass, Vector3, Collider2D, Transform, Vector3, QuestNavigation

### Community 109 - "TrashBin"
Cohesion: 0.31
Nodes (5): Image, PointerEventData, Sprite, Vector3, TrashBin

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
Cohesion: 0.16
Nodes (9): Sprite, CharacterData, Dictionary, List, CharacterDatabase, Dictionary, Sprite, NPCManager (+1 more)

### Community 115 - "UIType"
Cohesion: 0.25
Nodes (8): UIType, Inventory, MainMenu, None, PauseMenu, Quest, SettingMenu, SettingPause

### Community 116 - "InteractUI"
Cohesion: 0.22
Nodes (4): Collider2D, GameObject, UnityEvent, InteractUI

### Community 117 - "ReviceQuestListUI"
Cohesion: 0.22
Nodes (7): TextMeshProUGUI, Transform, ReviceQuestListUI, CanvasGroup, Image, TextMeshProUGUI, ReviceQuestSlot

### Community 118 - "ButtonHoverScale"
Cohesion: 0.29
Nodes (4): ButtonHoverScale, PointerEventData, TMP_Text, Vector3

### Community 119 - "ConfirmationDialog"
Cohesion: 0.24
Nodes (6): Action, Button, GameObject, TextMeshProUGUI, ConfirmationDialog, Instance

### Community 120 - "LandUI"
Cohesion: 0.12
Nodes (10): alpha, Transform, Button, CanvasGroup, Dictionary, TMP_Text, LandUI, blocksRaycasts (+2 more)

### Community 121 - "UNITY PROJECT AI GUIDE"
Cohesion: 0.17
Nodes (11): AI Workflow, Architecture Rules, Comment Rules, Existing Managers, Git Rules, graphify, Project Overview, Save System Rules (+3 more)

### Community 122 - "QuestDetailPanel"
Cohesion: 0.10
Nodes (12): Action, Button, Image, TextMeshProUGUI, QuestDetailPanel, GameObject, Image, TextMeshProUGUI (+4 more)

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

### Community 128 - "DayManager"
Cohesion: 0.14
Nodes (9): UnityEvent, DayManager, CanDive, CurrentDay, DivesRemaining, DivesToday, HasDivedToday, Instance (+1 more)

### Community 129 - "EnemyBrainContext"
Cohesion: 0.13
Nodes (10): Transform, EnemyBrainBase, enemyFSM, IsAttacking, MonoBehaviour, NavMeshAgent, Transform, EnemyBrainContext (+2 more)

### Community 130 - "LoadingManager"
Cohesion: 0.10
Nodes (17): GameObject, HashSet, IEnumerator, Slider, LoadingManager, Instance, IsLoading, Animator (+9 more)

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

### Community 142 - "ItemDrop"
Cohesion: 0.29
Nodes (3): IEnumerator, ItemDrop, ActiveItem

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
Cohesion: 0.22
Nodes (5): Slider, TMP_Text, VolumeSliderUI, IEnumerator, NotificationUI

### Community 150 - "PlayerSprintVFX"
Cohesion: 0.27
Nodes (5): ParticleSystem, Quaternion, Vector2, Vector3, PlayerSprintVFX

### Community 152 - "TipPanelUI"
Cohesion: 0.43
Nodes (3): Button, GameObject, TipPanelUI

### Community 153 - "CategoryIconMap"
Cohesion: 0.48
Nodes (4): Dictionary, HierarchyCategory, Texture2D, CategoryIconMap

### Community 155 - "LoadingUI"
Cohesion: 0.24
Nodes (5): Animator, CanvasGroup, Slider, TMP_Text, LoadingUI

### Community 157 - "DialogueState"
Cohesion: 0.31
Nodes (5): GameObject, HashSet, DialogueState, IsShowing, RuntimeInitializeOnLoadMethod

### Community 159 - ".TryOpenScript"
Cohesion: 0.60
Nodes (3): Component, ScriptNavigator, MonoScript

### Community 161 - "SceneLoadContext"
Cohesion: 0.40
Nodes (4): Vector3, SceneLoadContext, IsLoading, Position

### Community 163 - "SimpleAnimLoop"
Cohesion: 0.47
Nodes (3): Animator, IEnumerator, SimpleAnimLoop

### Community 165 - "graphify reference: query, path, explain"
Cohesion: 0.33
Nodes (5): For /graphify explain, For /graphify path, graphify reference: query, path, explain, Step 0 — Constrained query expansion (REQUIRED before traversal), Step 1 — Traversal

### Community 166 - "SaveSlotMeta"
Cohesion: 0.50
Nodes (3): GameState, List, SaveSlotMeta

### Community 169 - "SwimState"
Cohesion: 0.40
Nodes (5): SwimState, Biting, Reaching, Stopped, Swimming

### Community 170 - ".Notify"
Cohesion: 0.09
Nodes (14): CutsceneSkipHint, Sprite, NotificationManager, Sprite, ResourceNotificationPresenter, NotificationTests, Task 2: Money and item delta events + presenter, Bus — `NotificationManager` (existing static class, `UI/NotificationManager.cs`) (+6 more)

### Community 171 - "SaveData"
Cohesion: 0.14
Nodes (8): List, OneTimePickupTracker, Instance, ISaveParticipant, List, ItemSaveData, QuestSaveData, SaveData

### Community 173 - "PlayerStabAttack"
Cohesion: 0.50
Nodes (3): GameObject, IEnumerator, PlayerStabAttack

### Community 174 - ".Apply"
Cohesion: 0.40
Nodes (4): Image, Sprite, TMP_Text, DialogueSpeaker

### Community 175 - "SettingTabManager"
Cohesion: 0.39
Nodes (4): Button, Color, GameObject, SettingTabManager

### Community 176 - "UISettingManager"
Cohesion: 0.39
Nodes (3): GameObject, UISettingManager, IsOpen

### Community 177 - "LandOpeningController"
Cohesion: 0.36
Nodes (4): Animator, GameObject, IEnumerator, LandOpeningController

### Community 178 - "MiniHub"
Cohesion: 0.33
Nodes (7): GameObject, Vector2, HubEntry, MiniHub, HasHub, HubPosition, HubEntry

### Community 179 - "UISlotHoverHighlight"
Cohesion: 0.25
Nodes (6): Color, Image, PointerEventData, UISlotHoverHighlight, IPointerEnterHandler, IPointerExitHandler

### Community 181 - "PlayerHealth"
Cohesion: 0.15
Nodes (5): IEntityHealth, CurrentHealth, IsDead, MaxHealth, PlayerHealth

### Community 182 - "graphify reference: add a URL and watch a folder"
Cohesion: 0.50
Nodes (3): For /graphify add, For --watch, graphify reference: add a URL and watch a folder

### Community 183 - "graphify reference: commit hook and native CLAUDE.md integration"
Cohesion: 0.50
Nodes (3): For git commit hook, For native CLAUDE.md integration, graphify reference: commit hook and native CLAUDE.md integration

### Community 184 - "graphify reference: incremental update and cluster-only"
Cohesion: 0.50
Nodes (3): For --cluster-only, For --update (incremental re-extraction), graphify reference: incremental update and cluster-only

### Community 189 - ".HandleBulletHit"
Cohesion: 0.22
Nodes (4): Collider2D, Collision2D, GameObject, Transform

### Community 194 - "unityengine_scenemanagement"
Cohesion: 0.09
Nodes (7): GameState, Land, Menu, Ocean, Unity.Cinemachine, unityengine_scenemanagement, unityengine_video

### Community 198 - "NotificationLine"
Cohesion: 0.15
Nodes (13): CanvasGroup, IEnumerator, Image, TextMeshProUGUI, NotificationLine, Data flow, Decisions, Edge cases (+5 more)

### Community 200 - "NormalBullet"
Cohesion: 0.39
Nodes (3): Rigidbody2D, NormalBullet, MaxRange

### Community 201 - "QuestSlot"
Cohesion: 0.24
Nodes (7): Action, Button, Image, Sprite, TextMeshProUGUI, TMP_Text, QuestSlot

### Community 203 - "EnemyAgent"
Cohesion: 0.25
Nodes (4): EnemyAgent, AutoReturnDelay, BaseProfile, requiresNavMesh

### Community 205 - "NotificationMessage"
Cohesion: 0.06
Nodes (28): NotificationChannel, Center, General, Resource, System, Sprite, NotificationMessage, Channel (+20 more)

### Community 206 - "EnemyAttackState"
Cohesion: 0.24
Nodes (3): EnemyAttackState, IsAttacking, StateTag

### Community 208 - "com.unity.collab-proxy"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.collab-proxy

## Knowledge Gaps
- **950 isolated node(s):** `IsVisible`, `IconStyle`, `BadgeStyle`, `WarningContent`, `None` (+945 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1651 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **27 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Game.Testing` connect `unityengine` to `PlayerInterior`, `InventoryManager`, `system_collections_generic`, `EnemyBrain`, `unityengine_ui`, `Player`, `unityeditor`, `system_collections`, `GameFeature`, `SaveData`, `EnemyFSM`, `QuestData.cs`, `Boid`, `unityengine_scenemanagement`, `Game.Core`, `ScriptableObject`, `NotificationMessage`, `NotificationTests.cs`, `Bullet`?**
  _High betweenness centrality (0.081) - this node is a cross-community bridge._
- **Why does `Unity.Cinemachine` connect `unityengine_scenemanagement` to `project-ocean-worktree2.slnx`, `project-ocean.slnx`?**
  _High betweenness centrality (0.039) - this node is a cross-community bridge._
- **Why does `EnemyNavAgentBase` connect `EnemyNavAgentBase` to `unityengine`, `Player`, `FishAlertUI`, `EnemyAgent`, `Enemy`, `Boid`, `MonoBehaviour`, `CollectableItemBase`, `EnemyFacingController`, `.HandleBulletHit`, `EnemySpawner`, `EnemyWanderState`?**
  _High betweenness centrality (0.038) - this node is a cross-community bridge._
- **What connects `IsVisible`, `IconStyle`, `BadgeStyle` to the rest of the system?**
  _950 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `project-ocean-worktree2.slnx` be split into smaller, more focused modules?**
  _Cohesion score 0.0273972602739726 - nodes in this community are weakly interconnected._
- **Should `unityengine` be split into smaller, more focused modules?**
  _Cohesion score 0.04968354430379747 - nodes in this community are weakly interconnected._
- **Should `UIManager` be split into smaller, more focused modules?**
  _Cohesion score 0.12012012012012012 - nodes in this community are weakly interconnected._