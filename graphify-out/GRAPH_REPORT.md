# Graph Report - project-ocean  (2026-10-20)

## Corpus Check
- 423 files · ~379,293 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 2685 file(s) not represented in the graph (top: .meta 1835, .asset 510, .prefab 137)

## Summary
- 4936 nodes · 10073 edges · 255 communities (228 shown, 27 thin omitted)
- Extraction: 91% EXTRACTED · 9% INFERRED · 0% AMBIGUOUS · INFERRED: 923 edges (avg confidence: 0.9)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `eb5f0dc1`
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
- PlayerMoney
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
- TutorialGate
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
- QuestManager
- Player
- NPC
- What You Must Do When Invoked
- com.unity.render-pipelines.core
- HierarchyCategory
- Core (51 files)
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
- PlayerCamera
- SpawnCatalog
- PlayerSplitTests
- com.unity.modules.physics
- EnemyFacingController
- Gameplay_Entities_Enemies.md
- Gameplay_QuestSystems_Quest.md
- Entity
- PlayerHealth
- .FromSave
- SaveData
- DayManager
- ComponentInspectorPopup
- ComponentMinimap
- .DrawCategory
- FishCollectionUI
- PlayerSelfDialogueUI
- com.unity.editorcoroutines
- ItemSpawner
- unityengine_scenemanagement
- UISelectOnOpen
- ObjectPoolManager
- ReachPoint
- PlayerActions
- .Build
- RopeBreakCountdownUI
- com.unity.2d.tilemap
- com.unity.dt.app-ui
- InventorySlot
- QuestType
- EnemyNavPathfinder
- QuestBoardUI
- Gameplay (131 files)
- Boid
- com.unity.modules.uielements
- QuestStepDataDrawer
- Interactable
- com.unity.modules.subsystems
- project-ocean-worktree2.slnx
- EnemyFollowState
- EnemyBrainBase
- Avoidance
- AmmoType
- UIActions
- InventoryModel
- QuestTrackerUI
- unityengine
- gen_drawio.py
- HierarchyDataCache
- ShellPickup
- DialogueState
- NPCManager
- com.unity.modules.jsonserialize
- FishCollectionManager
- com.unity.nuget.newtonsoft-json
- com.unity.ext.nunit
- CollectableItemBase
- .ResolveCompassTarget
- TimePeriodUI
- Sprite
- Gameplay_Entities.md
- NextDayAnimationUI
- .Pick
- ConfirmationDialog
- LandUI
- UNITY PROJECT AI GUIDE
- UpgradeSlot
- com.unity.modules.ai
- .Info
- HierarchyPlus — Installation Guide
- MotionBendTracking
- .Close
- FishAlertUI
- ScriptNamingTests
- EnemyStateBase
- VerletRope
- UISelectionCursor
- EnemyAgent
- LoadingUI
- GrapplingHookBullet
- CategoryColorMap
- EntityDeathHandler
- AudioManager
- EnemyHitbox
- Bird
- Item
- graphify reference: extra exports and benchmark
- HierarchyPlusCore
- MissingScriptDetector
- Parallax
- QuestMarkerBase
- DamagePopup
- parse_types
- UIWindow
- MonoBehaviour
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
- .UpdateAim
- .Awake
- .BeginReach
- SwimState
- PlayerInteriorState
- EventInteractable
- MoneyUI
- 1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)
- MoveDirection
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
- .claude/CLAUDE.md
- extraction-spec.md
- IDialogueView
- EntityAnimation
- PlayerSprintVFX
- DevPanel
- SaveBridge
- KeyBindManager
- QuestSplitTests
- UpgradeTooltip
- .Pick
- AcceptedQuestListUI
- PlayerInteractor
- RopeSystemUtil
- VolumeSliderUI
- ControlScheme
- ChestInteract
- GameState
- DocsInstaller
- Cutscene
- guid_snapshot.py
- .LimitBreached
- Game.Tests
- ItemDrop
- 00-overview.md
- EnemyAttackState
- IntroPlayer
- LoadingButton
- LinearPatrolProfile
- UIFadeBackground
- PunchScale
- .HasLineOfSightRay
- NotificationLine
- RopeSplitTests
- EnemyWanderState
- .GetAction
- LandOpeningController
- Core.md
- DoorQuestMarker
- NormalBullet
- DependencyFalsePositiveTest
- ParserTest
- GameLogTests
- .OnSceneLoaded
- TipPanelUI
- UIType
- Gameplay_Entities_Players.md
- WaterReflection
- Player
- Page
- DrawioTest
- .Get
- UpgradeServiceEventTests
- Mode
- QuestCategory
- TutorialUIMarker
- unityengine_inputsystem
- FishSize
- tarjan
- AsmdefInstaller
- .Shoot
- RewardEvents
- .FindBestCorner
- com.h8man.2d.navmeshplus
- KeyBindResetAllButton

## God Nodes (most connected - your core abstractions)
1. `Game.Core` - 264 edges
2. `Game.Gameplay` - 221 edges
3. `Gameplay (131 files)` - 167 edges
4. `Item` - 91 edges
5. `Game.UI` - 87 edges
6. `InventoryManager` - 78 edges
7. `FishMotion` - 68 edges
8. `Quest` - 62 edges
9. `UI (58 files)` - 60 edges
10. `Core (51 files)` - 59 edges

## Surprising Connections (you probably didn't know these)
- `5. Risks` --references--> `SaveData`  [INFERRED]
  docs/uml/findings.md → Assets/Project_Ocean/Scripts/Core/Save/SaveData.cs
- `Core (51 files)` --references--> `AmmoType`  [INFERRED]
  docs/uml/03-all-scripts.md → Assets/Project_Ocean/Scripts/Core/Data/BulletData.cs
- `Core (51 files)` --references--> `BulletData`  [INFERRED]
  docs/uml/03-all-scripts.md → Assets/Project_Ocean/Scripts/Core/Data/BulletData.cs
- `Core (51 files)` --references--> `Item`  [INFERRED]
  docs/uml/03-all-scripts.md → Assets/Project_Ocean/Scripts/Core/Data/Items/Item.cs
- `3. Root causes` --references--> `Item`  [INFERRED]
  docs/uml/findings.md → Assets/Project_Ocean/Scripts/Core/Data/Items/Item.cs

## Import Cycles
- None detected.

## Communities (255 total, 27 thin omitted)

### Community 0 - "project-ocean.slnx"
Cohesion: 0.03
Nodes (74): Assembly-CSharp, Assembly-CSharp-Editor, Game.Deprecated, Game.Editor, Game.Gameplay, Game.Tests.EditMode, Game.UI, glTFast (+66 more)

### Community 1 - "Game.Core"
Cohesion: 0.05
Nodes (5): Game.Core, Game.Gameplay, system_collections, Unity.Cinemachine, unityengine_ai

### Community 2 - "UIManager"
Cohesion: 0.10
Nodes (8): Dictionary, GameObject, UIManager, CurrentUI, IsUIOpen, Persistent, Phase 6: cleanup (low risk, any time after Phase 4), UIType

### Community 3 - "README.md"
Cohesion: 0.10
Nodes (14): Inheritance & interfaces, Declared events with no `+=` subscriber found, Event catalogue, Event flow, Files, Gameplay/Entities/Players/OxygenTube, Event subscriptions, Files (+6 more)

### Community 4 - "OxygenRope"
Cohesion: 0.09
Nodes (21): Coroutine, GameObject, LayerMask, LineRenderer, List, Rigidbody2D, Transform, Vector2 (+13 more)

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
Cohesion: 0.10
Nodes (12): CallbackContext, Func, InputSystem_Actions, Vector2, InputReader, AimStick, InputActions, MousePos (+4 more)

### Community 9 - "AdditiveSceneSetupWindow"
Cohesion: 0.07
Nodes (23): AdditiveSceneEntry, Component, GUIStyle, Vector2, ComponentTooltipWindow, IsVisible, AdditiveSceneConfig, AdditiveSceneEntry (+15 more)

### Community 10 - "PlayerMoney"
Cohesion: 0.09
Nodes (12): PlayerMoney, Current, CurrentMoney, Money, Dictionary, Predicate, Dictionary, Predicate (+4 more)

### Community 11 - "ExploreAreaTrigger"
Cohesion: 0.14
Nodes (7): List, Transform, DialogueLine, ExploreAreaTrigger, View, DialogueLine, Mode

### Community 12 - "UI.md"
Cohesion: 0.10
Nodes (15): Core/Data, Files, Files, UI/Dialogue, Event subscriptions, Files, Event subscriptions, Files (+7 more)

### Community 13 - "system_collections_generic"
Cohesion: 0.04
Nodes (11): Vector3, FishConstant, FishAlive, system, system_collections_generic, system_security_cryptography, system_text, unityengine_inputsystem_utilities (+3 more)

### Community 14 - "ObjectWrapPoint"
Cohesion: 0.18
Nodes (10): BoxCollider2D, Collider2D, IEnumerable, List, Vector2, ObjectWrapPoint, WrapPoint, CompositeCollider2D (+2 more)

### Community 15 - "Enemy"
Cohesion: 0.07
Nodes (15): EnemyEvents, Collision2D, IEnumerator, Enemy, Brain, EnemyId, IsAttacking, Player (+7 more)

### Community 16 - "packages-lock.json"
Cohesion: 0.05
Nodes (41): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.audio, com.unity.modules.director, com.unity.modules.imageconversion (+33 more)

### Community 17 - "QuestProgressTracker"
Cohesion: 0.17
Nodes (4): Action, Func, List, QuestProgressTracker

### Community 18 - "com.unity.mathematics"
Cohesion: 0.07
Nodes (35): dependencies, dependencies, depth, source, url, version, dependencies, depth (+27 more)

### Community 19 - "EnemyBrain"
Cohesion: 0.08
Nodes (19): AggressiveEnemyProfile, detectDistance, LayerMask, EnemyBaseProfile, RequiresNavMesh, Transform, Vector3, EnemyBrain (+11 more)

### Community 20 - "ShopManager"
Cohesion: 0.08
Nodes (9): TutorialStep, Button, GameObject, Image, TMP_Text, Transform, ShopManager, Instance (+1 more)

### Community 21 - "Game.UI"
Cohesion: 0.05
Nodes (8): IHookable, Game.UI, Game.Deprecated, tmpro, unityengine_eventsystems, unityengine_inputsystem_controls, unityengine_inputsystem_ui, unityengine_ui

### Community 22 - "Quest"
Cohesion: 0.06
Nodes (31): IEnumerable, List, Quest, Category, Classification, CurrentCount, CurrentStep, CurrentStepIndex (+23 more)

### Community 23 - "TutorialGate"
Cohesion: 0.15
Nodes (5): TutorialGate, CurrentPrompt, IsRestricting, RequiredAction, TutorialPromptUI

### Community 24 - "InventoryManager"
Cohesion: 0.07
Nodes (17): GameObject, IReadOnlyList, List, TextMeshProUGUI, Transform, InventoryManager, ActiveBuildingSlotIndex, AllItems (+9 more)

### Community 25 - "QuestStep"
Cohesion: 0.07
Nodes (29): DialogueType, Accept, Finish, InProgress, NotFinish, Offer, Reject, characterId (+21 more)

### Community 26 - "com.unity.modules.imgui"
Cohesion: 0.09
Nodes (23): dependencies, depth, source, url, version, dependencies, depth, source (+15 more)

### Community 27 - "LoadingManager"
Cohesion: 0.11
Nodes (13): ILoadingVisual, GameObject, HashSet, IEnumerator, MonoBehaviour, Slider, LoadingManager, IsLoading (+5 more)

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
Nodes (9): Action, Dictionary, GameObject, IEnumerator, List, LoadSceneMode, Scene, CutsceneManager (+1 more)

### Community 35 - "PlayerGunHandler"
Cohesion: 0.11
Nodes (16): LayerMask, LineRenderer, ParticleSystem, SpriteRenderer, Transform, PlayerGunHandler, AimDir, CanAttack (+8 more)

### Community 36 - "QuestManager"
Cohesion: 0.09
Nodes (6): Coroutine, IEnumerator, List, QuestManager, Persistent, QuestStepEffects

### Community 37 - "Player"
Cohesion: 0.15
Nodes (10): GameObject, Rigidbody2D, Player, DashSpeed, FaceDir, GunHandler, Movement, OxygenTube (+2 more)

### Community 38 - "NPC"
Cohesion: 0.09
Nodes (14): Collider2D, GameObject, NPC, HasQuest, NpcId, NpcName, NpcPortrait, QuestOfferView (+6 more)

### Community 39 - "What You Must Do When Invoked"
Cohesion: 0.08
Nodes (24): For /graphify add and --watch, For /graphify query, For the commit hook and native CLAUDE.md integration, For --update and --cluster-only, /graphify, Honesty Rules, Interpreter guard for subcommands, Part A - Structural extraction for code files (+16 more)

### Community 40 - "com.unity.render-pipelines.core"
Cohesion: 0.09
Nodes (25): depth, source, version, dependencies, depth, source, version, dependencies (+17 more)

### Community 41 - "HierarchyCategory"
Cohesion: 0.09
Nodes (20): DevelopmentStatus, Complete, Deprecated, New, None, Problem, Review, WorkInProgress (+12 more)

### Community 42 - "Core (51 files)"
Cohesion: 0.07
Nodes (29): InventoryItemDatabase, ItemType, Ammo, Consumable, KeyItem, Normal, List, UpgradeData (+21 more)

### Community 43 - "GameFeature"
Cohesion: 0.08
Nodes (21): GameFeature, BackToLand, Combat, EnemySpawn, EnemySpawnLevel1, EnemySpawnLevel2, EnemySpawnLevel3, Interact (+13 more)

### Community 44 - "EnemyBrainContext"
Cohesion: 0.09
Nodes (22): MonoBehaviour, NavMeshAgent, Transform, EnemyBrainContext, IsDead, PlayerTransform, Dictionary, EnemyFSM (+14 more)

### Community 45 - "TrashInteract"
Cohesion: 0.18
Nodes (8): Coroutine, GameObject, IEnumerator, MonoBehaviour, Rigidbody2D, Vector3, TrashInteract, Popup

### Community 46 - "dependencies"
Cohesion: 0.08
Nodes (24): dependencies, depth, source, url, version, dependencies, depth, source (+16 more)

### Community 47 - "com.unity.test-framework"
Cohesion: 0.09
Nodes (23): dependencies, dependencies, depth, source, url, version, dependencies, depth (+15 more)

### Community 48 - "LinearPatrolBrain"
Cohesion: 0.11
Nodes (11): IBrainHitReaction, Collider2D, Component, Rigidbody2D, Transform, Vector2, LinearPatrolBrain, IsAttacking (+3 more)

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
Cohesion: 0.14
Nodes (10): Collider2D, GameObject, IEnumerator, Rigidbody2D, SpriteRenderer, Vector2, Bullet, MaxRange (+2 more)

### Community 53 - "CallbackContext"
Cohesion: 0.18
Nodes (3): CallbackContext, IPlayerActions, IUIActions

### Community 54 - "TutorialActionId"
Cohesion: 0.09
Nodes (22): TutorialActionId, AcceptQuest, BackToLand, BreakRopeLimit, BuyItem, Dive, HitEnemy, Interact (+14 more)

### Community 55 - "PlayerCamera"
Cohesion: 0.33
Nodes (4): LoadSceneMode, Scene, PlayerCamera, CinemachineCamera

### Community 56 - "SpawnCatalog"
Cohesion: 0.15
Nodes (6): Dictionary, GameObject, List, EnemySpawnEntry, ItemSpawnEntry, SpawnCatalog

### Community 57 - "PlayerSplitTests"
Cohesion: 0.09
Nodes (15): Inventory, Current, PlayerItemUse, TearDown, Test, InventoryAccessTests, GameObject, List (+7 more)

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
Cohesion: 0.11
Nodes (7): PlayerHealth, IsInvincible, GameObject, SetUp, TearDown, Test, PlayerInvincibilityTests

### Community 64 - ".FromSave"
Cohesion: 0.13
Nodes (13): ItemSaveData, Func, IReadOnlyList, List, InventorySaveMapper, SlotBuilder, TearDown, TearDown (+5 more)

### Community 65 - "SaveData"
Cohesion: 0.08
Nodes (15): HashSet, EnemyKillTracker, DestroyGameObjectOnDuplicate, List, OneTimePickupTracker, DestroyGameObjectOnDuplicate, ISaveParticipant, List (+7 more)

### Community 66 - "DayManager"
Cohesion: 0.12
Nodes (10): UnityEvent, DayManager, CanDive, CurrentDay, CurrentPeriod, CurrentPeriodDisplayName, DivesRemaining, DivesToday (+2 more)

### Community 67 - "ComponentInspectorPopup"
Cohesion: 0.17
Nodes (7): Component, Editor, GUIStyle, Rect, Vector2, ComponentInspectorPopup, PopupWindowContent

### Community 68 - "ComponentMinimap"
Cohesion: 0.22
Nodes (10): Component, Dictionary, GameObject, HashSet, List, Rect, Texture2D, Type (+2 more)

### Community 69 - ".DrawCategory"
Cohesion: 0.18
Nodes (11): GameObject, GUIStyle, Rect, HierarchyPlusDrawer, BadgeStyle, IconStyle, Color, DevelopmentStatus (+3 more)

### Community 70 - "FishCollectionUI"
Cohesion: 0.08
Nodes (16): Image, Sprite, TMP_Text, FishCollectionSlot, Button, GameObject, List, TextMeshProUGUI (+8 more)

### Community 71 - "PlayerSelfDialogueUI"
Cohesion: 0.21
Nodes (6): Line, IEnumerator, List, Line, PlayerSelfDialogueUI, View

### Community 72 - "com.unity.editorcoroutines"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, url, version, dependencies, depth, source (+10 more)

### Community 73 - "ItemSpawner"
Cohesion: 0.17
Nodes (6): GameObject, List, Transform, ItemSpawner, ZoneItemEntry, Weight

### Community 75 - "UISelectOnOpen"
Cohesion: 0.19
Nodes (10): Button, Canvas, GameObject, IEnumerator, List, Vector2, UISelectOnOpen, EventSystem (+2 more)

### Community 76 - "ObjectPoolManager"
Cohesion: 0.21
Nodes (8): IPoolable, Dictionary, GameObject, Quaternion, Queue, Vector3, ObjectPoolManager, DestroyGameObjectOnDuplicate

### Community 77 - "ReachPoint"
Cohesion: 0.16
Nodes (13): ReachMode, Position, PositionToBite, Transform, Wander, Quaternion, Vector3, ReachPoint (+5 more)

### Community 78 - "PlayerActions"
Cohesion: 0.12
Nodes (14): IPlayerActions, PlayerActions, @Aim, @AimStick, @Attack, enabled, @Interact, @Inventory (+6 more)

### Community 79 - ".Build"
Cohesion: 0.20
Nodes (6): Transform, GameObject, SetUp, TearDown, Test, SlotBuilderTests

### Community 80 - "RopeBreakCountdownUI"
Cohesion: 0.18
Nodes (7): CanvasGroup, Coroutine, IEnumerator, Image, LoadSceneMode, Scene, RopeBreakCountdownUI

### Community 81 - "com.unity.2d.tilemap"
Cohesion: 0.08
Nodes (27): dependencies, depth, source, url, version, dependencies, depth, dependencies (+19 more)

### Community 82 - "com.unity.dt.app-ui"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, url, version, dependencies, depth, source (+8 more)

### Community 83 - "InventorySlot"
Cohesion: 0.05
Nodes (27): PointerEventData, SellSlot, Color, Image, PointerEventData, ShopBuySlot, GameObject, ShopInventoryBuy (+19 more)

### Community 84 - "QuestType"
Cohesion: 0.29
Nodes (7): QuestType, Cutscene, Delivery, ExploreArea, Talk, Tutorial, WaitingQuest

### Community 85 - "EnemyNavPathfinder"
Cohesion: 0.18
Nodes (11): Action, Func, Vector3, EnemySearchState, SearchOrigin, StateTag, LayerMask, NavMeshAgent (+3 more)

### Community 86 - "QuestBoardUI"
Cohesion: 0.15
Nodes (8): Button, GameObject, List, Quest, TextMeshProUGUI, QuestBoardUI, QuestsPerPage, QuestCategoryUIData

### Community 87 - "Gameplay (131 files)"
Cohesion: 0.08
Nodes (35): SoundKeys, CutsceneGate, IsPlaying, AnimHash, IBrainAttack, AttackRange, RevealAfterTotalDives, MiniHubItem (+27 more)

### Community 88 - "Boid"
Cohesion: 0.15
Nodes (7): List, Vector2, Vector3, Boid, HasLeader, Leader, Phase 10: break the 36-type gameplay cycle

### Community 89 - "com.unity.modules.uielements"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, url, version, depth, source, version (+7 more)

### Community 90 - "QuestStepDataDrawer"
Cohesion: 0.11
Nodes (20): GUIStyle, SceneAsset, ReadmeReaderEditor, SerializedProperty, QuestDataEditor, characterId, Dictionary, GUIContent (+12 more)

### Community 91 - "Interactable"
Cohesion: 0.16
Nodes (9): Collider2D, Collider2D, GameObject, Interactable, CanInteract, HoldsLock, IsFeatureUnlocked, IsReady (+1 more)

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

### Community 97 - "AmmoType"
Cohesion: 0.40
Nodes (5): AmmoType, Granade, GrapplingHook, Normal, Tranquilizer

### Community 98 - "UIActions"
Cohesion: 0.22
Nodes (9): IUIActions, UIActions, @Cancel, @Click, enabled, @MiddleClick, @RightClick, @ScrollWheel (+1 more)

### Community 99 - "InventoryModel"
Cohesion: 0.16
Nodes (9): InventoryModel, BuildingSlotCount, PlayerSlotCount, SlotCount, ItemStack, IsEmpty, Test, InventoryModelTests (+1 more)

### Community 100 - "QuestTrackerUI"
Cohesion: 0.17
Nodes (6): Image, QuestVisuals, GameObject, Image, TextMeshProUGUI, QuestTrackerUI

### Community 101 - "unityengine"
Cohesion: 0.06
Nodes (11): DenysAlmaral, HierarchyPlus.Editor, Game.Editor, system_io, unityeditor, unityeditor_scenemanagement, unityeditorinternal, unityengine (+3 more)

### Community 102 - "gen_drawio.py"
Cohesion: 0.17
Nodes (19): collections, html, math, build(), class_box(), colors_of(), feature_page(), measure_container() (+11 more)

### Community 103 - "HierarchyDataCache"
Cohesion: 0.27
Nodes (7): DevelopmentStatusTag, Dictionary, GameObject, Cache, HierarchyDataCache, Cache, HierarchyCategoryTag

### Community 104 - "ShellPickup"
Cohesion: 0.23
Nodes (4): Animator, Coroutine, IEnumerator, ShellPickup

### Community 105 - "DialogueState"
Cohesion: 0.21
Nodes (7): GameObject, HashSet, Transform, DialogueState, BlocksInteraction, IsShowing, RuntimeInitializeOnLoadMethod

### Community 106 - "NPCManager"
Cohesion: 0.29
Nodes (3): Dictionary, NPCManager, Persistent

### Community 107 - "com.unity.modules.jsonserialize"
Cohesion: 0.08
Nodes (27): dependencies, depth, source, version, dependencies, depth, source, version (+19 more)

### Community 108 - "FishCollectionManager"
Cohesion: 0.14
Nodes (8): HashSet, IReadOnlyList, List, FishCollectionManager, CollectedCount, CollectedItemIds, TotalFishCount, IReadOnlyCollection

### Community 110 - "com.unity.nuget.newtonsoft-json"
Cohesion: 0.11
Nodes (18): dependencies, depth, hash, source, version, dependencies, depth, source (+10 more)

### Community 111 - "com.unity.ext.nunit"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, version, dependencies, depth, source, url (+4 more)

### Community 112 - "CollectableItemBase"
Cohesion: 0.33
Nodes (5): CollectableItemBase, CanInteract, IsCollectable, IsFeatureUnlocked, TakesPlayerLock

### Community 113 - ".ResolveCompassTarget"
Cohesion: 0.19
Nodes (8): GameObject, QuestCompass, Transform, Vector3, Collider2D, Transform, Vector3, QuestNavigation

### Community 114 - "TimePeriodUI"
Cohesion: 0.21
Nodes (8): TimePeriod, Afternoon, Evening, Morning, Image, RectTransform, Sprite, TimePeriodUI

### Community 115 - "Sprite"
Cohesion: 0.22
Nodes (9): IInteractPopupView, IsOpen, Sprite, GameObject, Image, TMP_Text, InteractPopupView, IsOpen (+1 more)

### Community 116 - "Gameplay_Entities.md"
Cohesion: 0.29
Nodes (5): Event subscriptions, Files, Gameplay/Entities, Files, UI/Interactions

### Community 117 - "NextDayAnimationUI"
Cohesion: 0.17
Nodes (6): Animator, CanvasGroup, Coroutine, IEnumerator, NextDayAnimationUI, IsPlaying

### Community 118 - ".Pick"
Cohesion: 0.25
Nodes (9): IReadOnlyList, IWeightedEntry, Weight, WeightedPicker, Test, Entry, Weight, WeightedPickerTests (+1 more)

### Community 119 - "ConfirmationDialog"
Cohesion: 0.10
Nodes (15): Action, IModalPrompt, CanShow, ModalPrompt, Current, Action, Button, GameObject (+7 more)

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

### Community 124 - ".Info"
Cohesion: 0.10
Nodes (16): Object, List, SaveSlotMeta, SaveSystem, LegacyPath, List, ContinueButtonGate, Button (+8 more)

### Community 125 - "HierarchyPlus — Installation Guide"
Cohesion: 0.20
Nodes (9): 1. Copy the Package, 2. Install Assembly Definitions, 3. Verify, 4. Tag Your GameObjects, 5. Component Minimap, 6. Unity Version Notes, Category Tag, Development Status Tag (+1 more)

### Community 126 - "MotionBendTracking"
Cohesion: 0.27
Nodes (7): Queue, Vector3, MotionBendTracking, Capacity, Count, MotionData, MotionData

### Community 127 - ".Close"
Cohesion: 0.18
Nodes (6): Action, PlayerUIState, IsLocked, ResetCount, EscClosablePanel, Stack

### Community 128 - "FishAlertUI"
Cohesion: 0.23
Nodes (4): Coroutine, GameObject, IEnumerator, FishAlertUI

### Community 130 - "EnemyStateBase"
Cohesion: 0.12
Nodes (9): LayerMask, Vector2, EnemyStateBase, enemyBrainContext, enemyFSM, StateTag, Vector2, EnemyFleeState (+1 more)

### Community 131 - "VerletRope"
Cohesion: 0.21
Nodes (9): Settings, LayerMask, List, Vector2, Seg, Settings, VerletRope, Count (+1 more)

### Community 132 - "UISelectionCursor"
Cohesion: 0.19
Nodes (9): Canvas, Color, GameObject, Image, Rect, RectTransform, Sprite, Vector3 (+1 more)

### Community 133 - "EnemyAgent"
Cohesion: 0.33
Nodes (4): EnemyAgent, AutoReturnDelay, BaseProfile, requiresNavMesh

### Community 134 - "LoadingUI"
Cohesion: 0.24
Nodes (5): Animator, CanvasGroup, Slider, TMP_Text, LoadingUI

### Community 135 - "GrapplingHookBullet"
Cohesion: 0.31
Nodes (6): Collider2D, IEnumerator, Transform, Vector2, GrapplingHookBullet, AttachedToObject

### Community 136 - "CategoryColorMap"
Cohesion: 0.42
Nodes (5): Color, Dictionary, HierarchyCategory, Texture2D, CategoryColorMap

### Community 138 - "EntityDeathHandler"
Cohesion: 0.16
Nodes (8): Color, Rigidbody2D, SpriteRenderer, EntityDeathHandler, GameObject, IEnumerator, PlayerStabAttack, Phase 9: Entity cleanup (small scope)

### Community 139 - "AudioManager"
Cohesion: 0.08
Nodes (16): AudioSource, LoadSceneMode, Scene, AudioChannel, BGM, Master, SFX, AudioManager (+8 more)

### Community 140 - "EnemyHitbox"
Cohesion: 0.16
Nodes (10): Collider2D, EnemyHitbox, HitPart, Body, Head, Tail, GameObject, Vector2 (+2 more)

### Community 141 - "Bird"
Cohesion: 0.43
Nodes (3): SpriteRenderer, Transform, Bird

### Community 142 - "Item"
Cohesion: 0.04
Nodes (35): GameObject, Sprite, Item, CanBuy, CanSell, FishSize, ItemDescription, ItemID (+27 more)

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

### Community 148 - "QuestMarkerBase"
Cohesion: 0.17
Nodes (5): GameObject, QuestMarkerBase, PollEveryFrame, List, TutorialActionMarker

### Community 149 - "DamagePopup"
Cohesion: 0.20
Nodes (6): IDamagePopup, IEnumerator, TextMeshProUGUI, DamagePopup, Test, GameplayUiEventTests

### Community 150 - "parse_types"
Cohesion: 0.17
Nodes (9): match_brace(), Member, member_headers(), parse_members(), parse_types(), Find type declarations in code; returns list of TypeInfo (nested types…, Yield top-level member declaration headers of a type body., split_top() (+1 more)

### Community 151 - "UIWindow"
Cohesion: 0.14
Nodes (12): UIType, FishCollection, Inventory, MainMenu, None, PauseMenu, Quest, SettingMenu (+4 more)

### Community 152 - "MonoBehaviour"
Cohesion: 0.06
Nodes (23): Vector3, WaterShadowWiggle, GameBootstrap, FishCollectionData, Image, EnemyHealthDisplay, KeepUpright, BootstrapLoader (+15 more)

### Community 153 - "CategoryIconMap"
Cohesion: 0.48
Nodes (4): Dictionary, HierarchyCategory, Texture2D, CategoryIconMap

### Community 155 - "QuestDetailPanel"
Cohesion: 0.17
Nodes (8): Action, IQuestOfferView, Action, Button, Image, Quest, TextMeshProUGUI, QuestDetailPanel

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
Cohesion: 0.20
Nodes (10): KeyAction, Aim, Attack, Interact, Inventory, Move, Sprint, UseItem (+2 more)

### Community 161 - "PlayerMovement"
Cohesion: 0.10
Nodes (11): Rigidbody2D, Vector2, PlayerMovement, DashSpeed, FaceDir, IsLand, MoveSpeed, SprintSpeed (+3 more)

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

### Community 166 - ".UpdateAim"
Cohesion: 0.22
Nodes (3): Vector2, Test, StickAimTests

### Community 169 - "SwimState"
Cohesion: 0.40
Nodes (5): SwimState, Biting, Reaching, Stopped, Swimming

### Community 170 - "PlayerInteriorState"
Cohesion: 0.25
Nodes (8): PlayerInterior, House, None, Shop, UpgradeShop, PlayerInteriorState, Current, CurrentExitAction

### Community 171 - "EventInteractable"
Cohesion: 0.33
Nodes (3): UnityEvent, EventInteractable, Collider2D

### Community 173 - "1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)"
Cohesion: 0.09
Nodes (25): SceneAsset, ReadmeReader, SwimConfig, BulletData, RopeStructureData, Sprite, CharacterData, Dictionary (+17 more)

### Community 174 - "MoveDirection"
Cohesion: 0.18
Nodes (8): MoveDirection, Down, Left, Right, Up, Action, MoveKeyBindUI, Phase 1: names (low risk)

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
Cohesion: 0.06
Nodes (19): Singleton, DestroyGameObjectOnDuplicate, Instance, IsDuplicate, Persistent, GameState, Transform, GameManager (+11 more)

### Community 179 - "QuestSlot"
Cohesion: 0.27
Nodes (7): Action, Button, Image, Quest, TextMeshProUGUI, TMP_Text, QuestSlot

### Community 180 - "TrashBin"
Cohesion: 0.16
Nodes (10): Image, PointerEventData, Vector3, TrashBin, Color, Image, PointerEventData, UISlotHoverHighlight (+2 more)

### Community 181 - ".Notify"
Cohesion: 0.12
Nodes (7): Sprite, CutsceneSkipHint, ResourceNotificationPresenter, SetUp, TearDown, Test, NotificationTests

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

### Community 192 - "IDialogueView"
Cohesion: 0.13
Nodes (10): GameObject, IDialogueView, IsOpen, Panel, GameObject, Image, TMP_Text, DialoguePanelView (+2 more)

### Community 193 - "EntityAnimation"
Cohesion: 0.38
Nodes (4): Animator, HashSet, EntityAnimation, RuntimeAnimatorController

### Community 195 - "PlayerSprintVFX"
Cohesion: 0.31
Nodes (5): ParticleSystem, Quaternion, Vector2, Vector3, PlayerSprintVFX

### Community 196 - "DevPanel"
Cohesion: 0.16
Nodes (8): Button, GameObject, List, DevItemButton, DevPanel, DevItemButton, TMP_Dropdown, Toggle

### Community 197 - "SaveBridge"
Cohesion: 0.13
Nodes (10): Vector3, SceneLoadContext, IsLoading, Position, IEnumerator, LoadSceneMode, Scene, SaveBridge (+2 more)

### Community 198 - "KeyBindManager"
Cohesion: 0.20
Nodes (8): InputSystem_Actions, KeyBindManager, Actions, IsRebinding, LastRebindFrame, Persistent, SavePath, Phase 4: assemblies and namespaces (compile-time enforcement)

### Community 199 - "QuestSplitTests"
Cohesion: 0.15
Nodes (9): QuestSaveData, Func, IEnumerable, List, QuestSaveMapper, TearDown, Test, QuestSplitTests (+1 more)

### Community 200 - "UpgradeTooltip"
Cohesion: 0.20
Nodes (9): Canvas, CanvasGroup, GameObject, List, RectTransform, TMP_Text, Transform, UpgradeTooltip (+1 more)

### Community 202 - ".Pick"
Cohesion: 0.47
Nodes (5): Test, Vector2, InteractionTargetingTests, Target, Target

### Community 203 - "AcceptedQuestListUI"
Cohesion: 0.22
Nodes (7): TextMeshProUGUI, Transform, AcceptedQuestListUI, CanvasGroup, Image, TextMeshProUGUI, AcceptedQuestSlot

### Community 204 - "PlayerInteractor"
Cohesion: 0.15
Nodes (7): Func, IEnumerable, Vector2, InteractionTargeting, List, PlayerInteractor, Blocked

### Community 205 - "RopeSystemUtil"
Cohesion: 0.32
Nodes (5): GameObject, GameObject, LineRenderer, Rigidbody2D, RopeSystemUtil

### Community 206 - "VolumeSliderUI"
Cohesion: 0.40
Nodes (3): Slider, TMP_Text, VolumeSliderUI

### Community 207 - "ControlScheme"
Cohesion: 0.16
Nodes (8): ControlScheme, Gamepad, KeyboardMouse, ControlSchemeExtensions, Action, KeyBindUI, Test, KeyBindSchemeTests

### Community 208 - "ChestInteract"
Cohesion: 0.27
Nodes (5): Collider2D, HashSet, MonoBehaviour, ChestInteract, Popup

### Community 209 - "GameState"
Cohesion: 0.13
Nodes (10): GameSession, State, GameState, Land, Menu, Ocean, SetUp, TearDown (+2 more)

### Community 211 - "Cutscene"
Cohesion: 0.18
Nodes (8): Action, Component, List, Rigidbody2D, Cutscene, CutsceneId, CutsceneTrackBinding, PlayableDirector

### Community 212 - "guid_snapshot.py"
Cohesion: 0.20
Nodes (7): json, pathlib, re, sys, Snapshot and compare .meta GUIDs under Assets/Project_Ocean/Scripts. Moves must…, Self-test for gen_uml's parser. Run: python tools/uml/test_gen_uml.py, unittest

### Community 213 - ".LimitBreached"
Cohesion: 0.18
Nodes (4): GameFlowEvents, IEnumerator, Test, GameFlowEventsTests

### Community 214 - "Game.Tests"
Cohesion: 0.14
Nodes (5): Game.Tests, nunit_framework, system_linq, system_text_regularexpressions, unityengine_testtools

### Community 215 - "ItemDrop"
Cohesion: 0.29
Nodes (3): IEnumerator, ItemDrop, ActiveItem

### Community 216 - "00-overview.md"
Cohesion: 0.11
Nodes (15): Layer dependencies, Module dependencies, Module table, Overview — layers & modules, Core/Inputs/KeyBind, Files, Core/Inputs/KeyBind/MoveDirection, Files (+7 more)

### Community 217 - "EnemyAttackState"
Cohesion: 0.33
Nodes (3): EnemyAttackState, IsAttacking, StateTag

### Community 218 - "IntroPlayer"
Cohesion: 0.31
Nodes (3): Button, IntroPlayer, VideoPlayer

### Community 220 - "LinearPatrolProfile"
Cohesion: 0.20
Nodes (9): LayerMask, LinearPatrolProfile, RequiresNavMesh, PatrolAxis, Horizontal, Vertical, PatrolMovementMode, FixedLength (+1 more)

### Community 222 - "UIFadeBackground"
Cohesion: 0.29
Nodes (3): CanvasGroup, Graphic, UIFadeBackground

### Community 223 - "PunchScale"
Cohesion: 0.28
Nodes (4): Coroutine, IEnumerator, Vector3, PunchScale

### Community 225 - ".HasLineOfSightRay"
Cohesion: 0.40
Nodes (4): LayerMask, Transform, Vector2, EnemyVisionHelper

### Community 226 - "NotificationLine"
Cohesion: 0.39
Nodes (5): CanvasGroup, IEnumerator, Image, TextMeshProUGUI, NotificationLine

### Community 227 - "RopeSplitTests"
Cohesion: 0.21
Nodes (7): BoxCollider2D, GameObject, Settings, TearDown, Test, Vector2, RopeSplitTests

### Community 228 - "EnemyWanderState"
Cohesion: 0.27
Nodes (3): Vector3, EnemyWanderState, StateTag

### Community 230 - "LandOpeningController"
Cohesion: 0.36
Nodes (4): Animator, GameObject, IEnumerator, LandOpeningController

### Community 231 - "Core.md"
Cohesion: 0.11
Nodes (19): Core, Files, Core/Save, Core/Save/DayReset, Event subscriptions, Files, Files, Core/Scenes (+11 more)

### Community 232 - "DoorQuestMarker"
Cohesion: 0.27
Nodes (7): CanvasGroup, IEnumerator, Transform, DoorInteract, List, DoorQuestMarker, TutorialAction

### Community 233 - "NormalBullet"
Cohesion: 0.43
Nodes (3): Rigidbody2D, NormalBullet, MaxRange

### Community 236 - "GameLogTests"
Cohesion: 0.50
Nodes (3): Test, GameLogTests, ConditionalAttribute

### Community 237 - ".OnSceneLoaded"
Cohesion: 0.33
Nodes (4): LoadSceneMode, Rigidbody2D, Scene, Vector3

### Community 238 - "TipPanelUI"
Cohesion: 0.36
Nodes (3): Button, GameObject, TipPanelUI

### Community 239 - "UIType"
Cohesion: 0.20
Nodes (10): UIType, FishCollection, Inventory, MainMenu, None, PauseMenu, Quest, SettingMenu (+2 more)

### Community 240 - "Gameplay_Entities_Players.md"
Cohesion: 0.07
Nodes (29): Core/Data/Items, Files, Files, Gameplay/AudioSetting, Event subscriptions, Files, Gameplay/Entities/Players, Files (+21 more)

### Community 241 - "WaterReflection"
Cohesion: 0.08
Nodes (23): AnimatedTile, RandomStartAnimatedTile, Color, GameObject, SpriteRenderer, Transform, Vector2, MirrorDirection (+15 more)

### Community 242 - "Player"
Cohesion: 0.17
Nodes (6): Player, PlayerRegistry, Local, IItemEffect, ActionHint, KeepInInventory

### Community 249 - "UpgradeServiceEventTests"
Cohesion: 0.22
Nodes (4): SetUp, TearDown, Test, UpgradeServiceEventTests

### Community 250 - "Mode"
Cohesion: 0.67
Nodes (3): Mode, Interact, TriggerZone

### Community 252 - "QuestCategory"
Cohesion: 0.18
Nodes (10): QuestCategory, Explore, FishL, FishM, FishS, Trash, QuestCategoryUIData, Action (+2 more)

### Community 256 - "TutorialUIMarker"
Cohesion: 0.40
Nodes (3): Graphic, TutorialUIMarker, PollEveryFrame

### Community 258 - "FishSize"
Cohesion: 0.40
Nodes (5): FishSize, Large, Medium, None, Small

### Community 261 - ".Shoot"
Cohesion: 0.29
Nodes (3): Collider2D, IEnumerator, Rigidbody2D

### Community 263 - "RewardEvents"
Cohesion: 0.33
Nodes (3): RewardEvents, Test, RewardEventsTests

### Community 266 - ".FindBestCorner"
Cohesion: 0.60
Nodes (4): Collider2D, List, Vector2, RopeCornerFinder

### Community 267 - "com.h8man.2d.navmeshplus"
Cohesion: 0.33
Nodes (6): dependencies, depth, hash, source, version, com.h8man.2d.navmeshplus

## Knowledge Gaps
- **1127 isolated node(s):** `IsVisible`, `IconStyle`, `BadgeStyle`, `WarningContent`, `None` (+1122 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1964 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **27 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Gameplay (131 files)` connect `Gameplay (131 files)` to `OxygenRope`, `EnemyNavAgentBase`, `PlayerMoney`, `ExploreAreaTrigger`, `ObjectWrapPoint`, `Enemy`, `QuestProgressTracker`, `EnemyBrain`, `ShopManager`, `Quest`, `TutorialGate`, `QuestStep`, `CutsceneManager`, `PlayerGunHandler`, `QuestManager`, `Player`, `NPC`, `Core (51 files)`, `GameFeature`, `EnemyBrainContext`, `TrashInteract`, `LinearPatrolBrain`, `EntityHealth`, `EnemySpawner`, `Bullet`, `PlayerCamera`, `SpawnCatalog`, `PlayerSplitTests`, `EnemyFacingController`, `Entity`, `PlayerHealth`, `.FromSave`, `PlayerSelfDialogueUI`, `ItemSpawner`, `QuestType`, `EnemyNavPathfinder`, `Boid`, `Interactable`, `EnemyFollowState`, `EnemyBrainBase`, `InventoryModel`, `QuestTrackerUI`, `ShellPickup`, `NPCManager`, `CollectableItemBase`, `.ResolveCompassTarget`, `Sprite`, `.Pick`, `ConfirmationDialog`, `FishAlertUI`, `EnemyStateBase`, `VerletRope`, `EnemyAgent`, `GrapplingHookBullet`, `EntityDeathHandler`, `AudioManager`, `Bird`, `Item`, `QuestMarkerBase`, `DamagePopup`, `MonoBehaviour`, `QuestDetailPanel`, `PlayerMovement`, `SimpleAnimLoop`, `PlayerInteriorState`, `EventInteractable`, `Singleton`, `.Notify`, `IDialogueView`, `EntityAnimation`, `PlayerSprintVFX`, `DevPanel`, `SaveBridge`, `QuestSplitTests`, `AcceptedQuestListUI`, `PlayerInteractor`, `RopeSystemUtil`, `VolumeSliderUI`, `ChestInteract`, `Cutscene`, `ItemDrop`, `EnemyAttackState`, `LinearPatrolProfile`, `.HasLineOfSightRay`, `EnemyWanderState`, `DoorQuestMarker`, `NormalBullet`, `Player`, `Mode`, `QuestCategory`, `TutorialUIMarker`, `.FindBestCorner`?**
  _High betweenness centrality (0.196) - this node is a cross-community bridge._
- **Why does `All scripts` connect `MonoBehaviour` to `03-all-scripts.md`, `Core (51 files)`, `QuestStepDataDrawer`, `Gameplay (131 files)`?**
  _High betweenness centrality (0.078) - this node is a cross-community bridge._
- **Why does `Core (51 files)` connect `Core (51 files)` to `RewardEvents`, `InputReader`, `KeyBindResetAllButton`, `Item`, `Enemy`, `TutorialGate`, `MonoBehaviour`, `LoadingManager`, `KeyAction`, `NotificationMessage`, `GameFeature`, `1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)`, `MoveDirection`, `RebindButtonBase`, `Singleton`, `TutorialActionId`, `.FromSave`, `SaveData`, `DayManager`, `SaveBridge`, `KeyBindManager`, `QuestSplitTests`, `ObjectPoolManager`, `ControlScheme`, `GameState`, `.LimitBreached`, `IntroPlayer`, `AmmoType`, `DialogueState`, `.Info`, `.Close`?**
  _High betweenness centrality (0.052) - this node is a cross-community bridge._
- **Are the 166 inferred relationships involving `Gameplay (131 files)` (e.g. with `AudioChannel` and `AudioManager`) actually correct?**
  _`Gameplay (131 files)` has 166 INFERRED edges - model-reasoned connections that need verification._
- **What connects `IsVisible`, `IconStyle`, `BadgeStyle` to the rest of the system?**
  _1127 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `project-ocean.slnx` be split into smaller, more focused modules?**
  _Cohesion score 0.02666666666666667 - nodes in this community are weakly interconnected._
- **Should `Game.Core` be split into smaller, more focused modules?**
  _Cohesion score 0.05185779203421545 - nodes in this community are weakly interconnected._