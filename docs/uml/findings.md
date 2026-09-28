# Findings & refactor roadmap

[← index](README.md) · Hand-written 2026-09-26 on branch `develop-26sep26-refactor`. Numbers come from the generated [metrics](metrics.md) unless noted.

## 1. What changed since the old UML (2026-09-19, `develop-18sep-hotfix`)

| | Old PDF | Now |
|---|---|---|
| Script files (live, excl. `Deprecated/`) | 186 | 200 (after Phase 1) |
| Types | 229 | 253 |
| `static Instance` singletons | 22 | 23 (+ `LoadingManager`, `OneTimePickupTracker`; − `FlockManager`) |
| Largest dependency cycle | 51 types / 21 modules | 82 types / 32 modules |
| Wrong-way layer links | 64 | 0 (after Phase 3; the metric no longer counts member names) |

**New systems:** Loading (`LoadingManager`, `BootstrapLoader`, `LoadingButton`, loading visuals), notification channels (`NotificationChannel`, `NotificationMessage`, `NotificationView` → `StackNotificationView` / `ToastNotificationView`, `ResourceNotificationPresenter`), audio channels (`SoundChannel`, `SoundKeys`, `AudioSliderBinder`), characters (`CharacterData`, `CharacterDatabase`, `DialogueSpeaker`, `DialogueState`), explore quests (`ExploreAreaTrigger`, `ExploreEvents`), `TutorialUIMarker`, `OneTimePickupTracker`, `RevealAfterTotalDives`, `CutsceneSkipHint`, VFX (`PlayerFeedbackVFX`, `PlayerSprintVFX`), `PunchScale`, `UIFadeBackground`.

**Gone:** `FlockLeader`, `FlockManager`, `NotificationTextUI` (replaced by notification views), `UIDimBackground` (replaced by `UIFadeBackground`).

**Grew (raw file lines, old → now):** `QuestManager` 717 → 956, `Player` 494 → 557, `InventoryManager` 547 → 596, `UIManager` 547 → 570, `OxygenRope` 747 → 784. `Quest.cs` is now 545 lines.

**Why the cycle and violations roughly doubled:** the new Loading and Notification code sits in `UI/` but is called from Core and Gameplay (`GameManager`, `SaveBridge`, `QuestManager`, `TutorialGate`, `NPC` …). `LoadingManager` (9 wrong-way links), `NotificationManager` (7) and `NotificationChannel` (4) are now among the top offenders.

## 2. Unused scripts moved to `Scripts/Deprecated/`

Criteria: script GUID not referenced by any scene, prefab or asset (text serialization), **and** none of its types named in any other script. Moved together with their `.meta` files (GUIDs kept). Unity recompiled with no errors.

| File | Was in | Note |
|---|---|---|
| `EnemySpawnManager.cs` | Core/Save Game/DayAndResetEnemys | Empty stub ("Superseded by GameObjectSpawnManager") |
| `WorldItemPickup.cs` | Gameplay/InteractObjects | Empty stub ("handled by ItemDrop.cs") |
| `FishData.cs` (+ `EnemyType`) | Core/Datas | SO with no assets |
| `KeyBind.cs` | Core/Inputs/KeyBind | Class never used |
| `IHookable.cs` | Gameplay/RopeSystems | Interface with no implementers |
| `EnemyHitbox.cs`, `EnemyWoundSystem.cs` | Gameplay/Entitys/Enemys/Partbraked | Part-break prototype; only use each other |
| `MiniHub.cs` | Gameplay/Entitys/Players/OxygenTube | Not in any scene/prefab |
| `SellSlot.cs` | UI/Inventory | Not in any scene/prefab |
| `ItemUI.cs` | UI/Items | Not in any scene/prefab |

**Also unreferenced, moved to `Deprecated/` in Phase 1 by user decision:** `InteractableNextDay.cs` (was `Interactable NextDay.cs`), `GameBootstrap.cs`, `LandOpeningController.cs`, `LoadingUI.cs` (all three were in `UI/LoadingSecne`).

**Bugs fixed 2026-09-27:** Mini Hub building slot now saved (SaveData.buildingItems); NPC quest offer uses its own LandUI/NpcQuestOfferPanel (was hidden inside the closed QuestBoardPanel); Ocean_Map explore trigger "Area" now has a dialogue panel (MQ_1002 step 13 line was skipped).

**File name ≠ class name** (Unity cannot attach or load these reliably): `Interactable NextDay.cs` → `InteractableNextDay` (likely why it is in no scene), `Core/Datas/RopeData.cs` → `RopeStructureData` (SO), `UI/Items/Itemdrop.cs` → `ItemDrop` (case only) — fixed in Phase 1.

## 3. Root causes

1. **Domain code lives in UI folders.** `Item`, `ItemType`, `UpgradeData`, `UpgradeType`, `InventoryManager` (data + rules + view in one class), `ShopManager`, `NotificationManager` (a static event bus), `LoadingManager` (a global manager). Core and Gameplay must reach "up" to use them.
2. **UI lives in Core.** `QuestBoardUI`, `QuestSlot`, `QuestDetailPanel`, `QuestCategorySlot` sit in `Core/Datas/ScriptableDatas/Quests/QuestBoard`.
3. **Shared event types live in Gameplay.** `TutorialActionId` and `TutorialEvents` (Gameplay/QuestSystems) are raised by Core types (`GameManager`, `DayManager`, `UpgradeService`).
4. **Enemy-only interfaces live in Core.** `IEnemyBrain`, `IEnemyState`, `IBrainAttack`, `IBrainVision`, `IBrainHitReaction` sit in `Core/Interfaces` but reference `EnemyBrainContext` and `EnemyFSM` in Gameplay.
5. **Hubs pull concrete types instead of using existing seams.** `SaveBridge` (23 deps) calls `PlayerHealth`, `PlayerMoney`, `InventoryManager`, `QuestManager` directly, although `ISaveParticipant` already exists and 6 classes implement it. `UpgradeService` pokes `OxygenRope`, `NormalBullet`, `PlayerHealth`, `PlayerMoney`. `GameManager` calls `UIManager`, `Player`, `NotificationManager`.
6. **Everything compiles in one assembly named `Game.Testing`** (192 files use `namespace Game.Testing`), so nothing stops a new wrong-way reference.

## 4. Refactor roadmap (structure first)

One phase = one branch = one PR. Each phase keeps behaviour unchanged, ends with a clean compile, EditMode tests, a Play-mode smoke run (MainMenu → Land_Map → Ocean_Map → save/load), and `python tools/uml/gen_uml.py` to show the effect in [metrics](metrics.md).

**Rules for all phases:** move and rename only inside the Unity Editor or with the `.meta` file (GUIDs keep scene/prefab links). Managers stay in AS_PlayerHUD; do not edit the Bootstrap duplicates. No new managers.

### Phase 1: names (low risk)
**Status:** done 2026-09-26.
- Rename folders: `Entitys`→`Entities`, `Enemys`→`Enemies`, `Beharviours`→`Behaviours`, `Partbraked`→`PartBreak`, `Upgrads`→`Upgrades`, `LoadingSecne`→`Loading`, `MovePlayerDirsction`→`MoveDirection`, `DayAndResetEnemys`→`DayReset`, `Save Game`→`Save`.
- Fix file/class mismatches listed in §2 (`Draggableltem` → `DraggableItem` too).
- Decide the four "kept" unreferenced scripts.
- **Done when:** no typo folders, every MB/SO file name matches its class.

### Phase 2: put types in the right layer (medium risk, biggest win)
**Status:** done 2026-09-26. Wrong-way links 111 → 61 (target < 55 missed; the rest is Phase 3, and 2 rows are enum-name false positives: `ItemType.Quest`, `UpgradeType.NormalBullet`).

**Deviations:** `LoadingManager` stays in UI until Phase 3 (it holds `LoadingVisualController`). `TutorialGate`/`TutorialStep` stay in Gameplay (they call `TutorialPromptUI`/`QuestManager`). `PlayerFeedbackVFX` stays in UI (it is a screen-space vignette). Only `LoadingType`, `TutorialActionId`/`TutorialEvents` and `PlayerSprintVFX` moved from those groups.

| Move | From → To |
|---|---|
| `Item`, `ItemType`, `UpgradeData`, `UpgradeType`, `InventoryItemDatabase` | UI → `Core/Datas` |
| `NotificationManager`, `NotificationMessage`, `NotificationChannel` (bus only; views stay) | UI → `Core/Events` |
| `TutorialActionId`, `TutorialEvents`, `CutsceneEvents`, `EnemyEvents`, `NpcEvents` (extract from `NPC.cs`), `ExploreEvents` | Gameplay → `Core/Events` |
| `LoadingType` (UI), `SceneLoadContext` (Core/Save); `LoadingManager` waits for Phase 3 | → `Core/Scenes` |
| `IEnemyBrain`, `IEnemyState`, `IBrain*` | Core/Interfaces → `Gameplay/Entities/Enemies/Brain` |
| Quest board UI (`QuestBoardUI`, `QuestSlot`, `QuestDetailPanel`, `QuestCategorySlot`, `QuestCategoryUIData`) | Core/Datas → `UI/Quest` |
| `ItemDrop`, `PlayerSprintVFX` (world objects; `PlayerFeedbackVFX` stays UI) | UI → Gameplay |
- **Done when:** wrong-way links roughly halved (target < 55). Only folder moves; no logic changes.

### Phase 3: cut remaining wrong-way links (medium risk)
**Progress:** 3a + 3b done 2026-09-26. Metric fixed (61 → 50 real), then 50 → 37: spawners → `Gameplay/Spawning`, `QuestTrackerUI` → `UI/Quest`, `LoadingManager` → `Core/Scenes` via `ILoadingVisual`. 3c-1 done: `GameManager` → `Gameplay/Game`, `SaveBridge` → `Gameplay/Save`, `UpgradeService`/`FeatureUnlockService` → `Gameplay/Services`, `PlayerRegistry` → `Gameplay/Entities/Players`, save menu UI → `UI/Save` (37 → 27). 3c-2 done: PlayerHealth, PlayerMoney, InventoryManager, DayManager save their own slice via ISaveParticipant (save content verified identical). 3d done: UpgradeService raises OnLevelGranted/OnLevelsLoaded; PlayerHealth and InventoryManager apply their own effects (26 links). 3f-1 done: GameState → Core, InteractUI → Gameplay/Interaction; QuestCompleteUI, UIManager, PlayerFeedbackVFX listen to gameplay events; DamagePoint via IDamagePopup (16 links). 3e done: gameplay uses IInventory/Inventory.Current instead of InventoryManager (6 links). 3f-2 done: NPC/ExploreAreaTrigger/PlayerSelfDialogueUI use IDialogueView (DialoguePanelView), NPC → IQuestOfferView, Cutscene → IModalPrompt (0 links).
- `SaveBridge`: every saved system implements `ISaveParticipant` (add `PlayerHealth`, `PlayerMoney`, `InventoryManager`, `CutsceneManager`); `SaveBridge` loses the concrete calls. — done in 3c-2
- `InventoryManager`: split into `Inventory` (slots, add/remove, save; Gameplay) + `InventoryView` (UI). `QuestManager`, `Player`, `EnemyLoot`, `ItemDrop`, `UpgradeService`, `CheckPointItem` talk to `Inventory` only. — done in 3e as an interface (IInventory); a full data/view split is still possible later if inventory grows.
- `UpgradeService`: raise `OnUpgradeApplied(UpgradeType, level)`. `OxygenRope`, `PlayerHealth`, `NormalBullet` apply their own effect. — done in 3d (DevGrantNextLevel's InventoryManager check waits for 3e)
- `GameManager` → UI: replace `UIManager` / `NotificationManager` calls with events (`OnGameStateChanged`) the UI listens to — done in 3f-1 (`OnReturningToMenu`).
- Gameplay → UI helpers: `DamagePoint` popup driven by `EntityHealth.OnDamaged`; `DialogueSpeaker` / `InteractUI` / `QuestDetailPanel` called through events from `NPC`, `ExploreAreaTrigger`, `ChestInteract`. — done in 3f-1/3f-2
- `FeatureUnlockService` → `QuestManager`: subscribe to `OnQuestUpdated` instead of querying it.
- **Done when:** [metrics](metrics.md) shows 0 wrong-way links, and the largest cycle has none of Core in it.

### Phase 4: assemblies and namespaces (compile-time enforcement)
**Status:** done 2026-09-29. Assemblies Game.Core ← Game.Gameplay ← Game.UI (+ editor-only Game.Editor, Game.Deprecated); namespaces match folders; a Core→Gameplay reference fails to compile (probe verified). No hidden layer violations surfaced; only fix was `QuestDataEditor : UnityEditor.Editor` (the `Game.Editor` namespace shadowed `Editor`).
- asmdefs `Game.Core` ← `Game.Gameplay` ← `Game.UI`, plus `Game.Editor` (Editor-only). Merge today's `Core/Inputs/Game.Core.asmdef` into the new Core one first (`InputReader` → `Player` and `KeyBindManager` → `Player` must be gone by then). `Deprecated/` gets its own asmdef or is deleted.
- Rename `namespace Game.Testing` → `Game.Core` / `Game.Gameplay` / `Game.UI`. Safe for scenes and prefabs because Unity links scripts by GUID. Checked: no `[SerializeReference]` or type-by-string lookups in live code.
- Update `Tests/EditMode/Game.Tests.EditMode.asmdef` references.
- **Done when:** the project compiles with the three assemblies, and a wrong-way `using` fails to compile.

### Phase 5: split large classes (one class per PR)
| Class | Size now | Split into |
|---|---|---|
| `QuestManager` | **done 2026-09-29:** 955 → 586 lines | `QuestProgressTracker` (step evaluation, 9 event subscriptions) · `QuestStepEffects` (money check, feature changes, item consumption) · `QuestSaveMapper` (save adapter); registry, lifecycle and tracking stay in `QuestManager` |
| `OxygenRope` | **done 2026-09-29:** 785 → 558 lines | `VerletRope` (segment physics) · `RopeCornerFinder` (wrap-corner geometry). Breach countdown (~50 lines, coroutine) and drawing (16 lines) stay: splitting them needs a new component or a timer rewrite for no real gain |
| `Player` | **done 2026-09-29:** 551 → 506 lines (~70 of them disabled checkpoint code) | `PlayerItemUse` (item use; the MiniHub path). Checkpoint item is unused in the game, so its code is disabled with `#if ENABLE_CHECKPOINT_ITEM` (Player, `CheckPointItem`, `InventoryManager` "T" hint, `OxygenRope` checkpoint pins) instead of extracted |
| `UIManager` | **reviewed 2026-09-29, not split:** one job (window stack + input lock/timescale + Esc); size is formatting and 16 one-line wrappers | 4 unused wrappers → Phase 6 |
| `ObjectWrapPoint` | 608 lines, 27 methods | review first; only 1 dep, may be fine as is |
| `ShopManager`, `SaveBridge`, `Quest.cs` | 405 / 435 / 545 lines | after Phase 3; re-measure first |

### Phase 6: cleanup (low risk, any time after Phase 4)
- 137 `Debug.Log*` calls in 40 files → one `[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]` logger.
- 14 `Find*` calls in 8 files → a spawn-point registry (the `Player`/`GameManager` tag lookups) and serialized references.
- 23 singletons → keep true globals (GameManager, SaveBridge, AudioManager, LoadingManager, ObjectPoolManager); expose the rest through events or serialized references as Phase 3 removes callers.
- `public` fields on ScriptableObject profiles (`EnemyBaseProfile`, `Item`) → `[SerializeField] private` + read-only properties. Rename with `[FormerlySerializedAs]` to keep asset data.
- Route bullets, drops and inventory rows through the existing `ObjectPoolManager` where profiling shows spawn cost.
- Dead code found in Phase 5c: `UIManager.ToggleSettingMenu`/`ToggleSettingPause`/`ToggleInventory`/`ToggleQuest` (0 code and scene callers); `Player.Aim()` (its `OnAimed` subscription is commented out), `Player.Interact()` (only logs), the `Alpha1` "Test Input" in `Player.Update`.
- **Bug (pre-existing):** `Player.playerSpriteRenderer` is never assigned, so the player hit flash never shows (`OnDamagedVisual` returns early). Found in the Phase 5c live check.

## 5. Risks

- **Scene/prefab links:** folder moves without `.meta` break them. Move in the Editor, then check `git status` shows renames rather than delete + add.
- **Static-parser blind spots:** the metrics miss `GetComponent<T>`, inspector wiring and `SendMessage`, and name collisions overcount (e.g. a local variable named like a type still counts). Confirm each move with the Unity compiler, not only the graph.
- **Save compatibility:** Phases 3 and 5 touch save code. Keep `SaveData` fields and `saveVersion` unchanged unless a migration is added.
- **Live verification:** a green compile is not proof. Run the Play-mode smoke route after every phase.
