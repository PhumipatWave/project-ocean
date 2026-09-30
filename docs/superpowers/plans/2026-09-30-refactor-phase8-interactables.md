# Refactor Phase 8: One Interactable Hierarchy for Doors, Chests, Trash, NPC Zones and Pickups — Implementation Plan

**Goal:** Every E-press target (InteractUI zones, `DoorInteract`, `ChestInteract`, `TrashInteract`, `CollectableItemBase` pickups) derives from one abstract `Interactable`. One `PlayerInteractor` on the player owns the E press, picks the closest ready target and shows only its icon. Door/Chest/Trash stop needing a sibling `InteractUI` + Inspector `UnityEvent` wiring, and stop driving UI widgets directly.

**Status:** done 2026-09-30 (8a–8d; the door split was done together with 8b). `EnemyLoot` was a third pickup subclass not listed below; it keeps its old "no InteractItem unlock" rule.

**User decisions (2026-09-30):** do all phases; closest target wins; **chest keeps today's behaviour** (opened state is a static list, not saved).

## Facts this plan relies on (checked 2026-09-30)

- `Land_Map` InteractUI zones: 3 shop/upgrade/home counters, 11 house boxes + 3 NPC `Talk` zones (call `NPC.Interact`), `PlayerShip`, `QuestBoardProb`, 3 house doors + 3 warp points (call `DoorInteract.Interact`), 12 trash cans (call `TrashInteract.SearchTrash`, from `TrashCan.prefab`), 1 chest (no persistent call; `ChestInteract` adds a runtime listener). `Area.prefab` (`ExploreAreaTrigger`), `BaseNPC.prefab`, `House.prefab`, `NPC_House.prefab` also carry InteractUI.
- Doors use `requireSpecificFeature` (Sleep/Shop/Upgrade); trash uses `Trash`; chest none.
- Pickups: `ItemDrop` in `BaseCollectableItem.prefab`, `Shelle.prefab`, `Shelle 2.prefab`; each subscribes to a serialized `InputReader` and picks the closest pickup itself.
- `InteractUI` public API used elsewhere: `onInteract` (ExploreAreaTrigger, Chest), `ReleasePlayerLock` (NPC, ExploreAreaTrigger, Chest), `PlayerExited` (NPC). Must stay.
- Trash popups use `LandUI/InteractDialoguePanel`; chest uses world `Environments/ChestPanel/InteractDialoguePanel4`.
- Door statics `FindByTutorialAction` / `FindNearestExitAction` are used by `QuestNavigation`.
- `CinemachineCore.OnTargetObjectWarped(Transform, Vector3)` exists (static, notifies every vcam), so the door needs no camera lookup.

## Behaviour changes (accepted)

1. Overlapping zones: the closest ready target gets the press (was: whichever zone the player entered first). Only the focused target shows its icon.
2. Pickups are also blocked during cutscenes and loading (they were not).

Everything else stays: lock-holder semantics (the zone holding `PlayerUIState` gets repeat presses to advance/close), tutorial Interact gate and event for zone-type targets only, "Locked — progress the main quest" toast, chest static list, trash daily reset.

## Target design

```
Interactable : MonoBehaviour (abstract)                 Gameplay/Interaction
  [SerializeField] interactIcon, requireSpecificFeature, specificFeature
  protected virtual bool TakesPlayerLock => true        pickups: false (no lock, no tutorial gate/event, no toast)
  protected virtual bool IsFeatureUnlocked              Interact (+ specific feature)
  public virtual bool CanInteract => true               silent readiness (e.g. shell closed)
  protected abstract void Interact(Player player)
  public void ReleasePlayerLock(), HideIcon(); event PlayerExited
  trigger enter/exit → PlayerInteractor.Add/Remove

PlayerInteractor : MonoBehaviour (on Player.prefab)
  subscribes Player.input.OnInteracted; global gates (cutscene, loading, self-dialogue)
  target = lock holder in range, else InteractionTargeting.PickClosest(ready)
  Update(): icon on for focus only

InteractUI : Interactable        → onInteract.Invoke()           (unchanged Inspector wiring)
DoorInteract : Interactable      → fade + warp; event Warped
DoorQuestMarker : MonoBehaviour  → registry, marker UI, raises tutorial action on Warped
ChestInteract : Interactable     → money once (static list) + popup
TrashInteract : Interactable     → random loot, daily reset + popup
CollectableItemBase : Interactable → TryCollect()

IInteractPopupView (Gameplay/Contracts) ← InteractPopupView (UI) on both popup panels
```

## Global constraints

- Scene/prefab edits only through `execute_code` + `EditorSceneManager`/`PrefabUtility`; `git status --short -- "*.unity" "*.prefab"` after each save (memory: manage_scene save bug).
- Files keep encoding/line endings. CLAUDE.md comment rules. The user commits.
- Compile = `refresh_unity(force, request)` + 0 `error CS`. Tests = EditMode all pass (baseline 58).
- Back up `LocalLow/DefaultCompany/project-ocean` before Play; verify byte-identical after.

---

### 8a: `Interactable` + `PlayerInteractor`; `InteractUI` becomes a subclass (no behaviour change besides closest-wins)
- Create `Scripts/Gameplay/Interaction/Interactable.cs`, `PlayerInteractor.cs`, `InteractionTargeting.cs` (pure `PickClosest`).
- Rewrite `InteractUI.cs`: fields move to the base with the same names (serialized data survives), `Interact` invokes `onInteract`.
- Add `PlayerInteractor` to `Prefabs/Entitys/Players/Player.prefab`.
- Test: `Tests/EditMode/InteractionTargetingTests.cs` (closest wins, not-ready skipped, empty → null).

### 8b: Door, Chest, Trash derive from `Interactable`; shared popup view
- Create `Gameplay/Contracts/IInteractPopupView.cs`, `UI/InteractPopupView.cs`.
- Rewrite the three scripts: no own trigger code, money via `player.GetComponent<PlayerMoney>()`, popup through the view, lock release through `ReleasePlayerLock`.
- Scene/prefab script: add `InteractPopupView` to both popup panels; per door/chest/trash object copy icon + feature from its `InteractUI` into the new base fields, set the popup view, remove the `InteractUI` (`TrashCan.prefab` once, then Land_Map instances). Door `Interact` calls vanish with the component.
- Test: `ViewContractTests` asserts `InteractPopupView : IInteractPopupView`.

### 8c: Pickups join
- `CollectableItemBase : Interactable`: drop `inputReader`, `_itemsInRange`, closest logic; `collectButton` → base `interactIcon` via `FormerlySerializedAs`; `TakesPlayerLock => false`; `IsFeatureUnlocked => InteractItem`; `CanInteract => IsCollectable`.
- `ItemDrop` unchanged except `playerInRange` reset.

### 8d: Door split + camera
- Create `Gameplay/Environments/DoorQuestMarker.cs` (registry statics, marker, tutorial action raised on `DoorInteract.Warped`). Callers (`QuestNavigation`) switch to it.
- Scene script: add `DoorQuestMarker` to the 6 door objects, copy `raiseTutorialAction`/`tutorialAction`/`markerUI`.
- Door uses `CinemachineCore.OnTargetObjectWarped`; drop `snapCamera`/`FindFirstObjectByType`.

### Verify (after each phase compile + tests; live once at the end)
Land_Map in Play: NPC talk (advance + close), shop/upgrade counters, house door in/out + tutorial marker, warp points, trash (junk + money popup, second search same day does nothing), chest (first reward popup, second popup, E closes), quest board, ship. Ocean_Map: shell pickup (only when open), item drop pickup. Two overlapping zones → closest acts. Console clean. Then `gen_uml.py`, `findings.md` Phase 8 entry, `graphify update .`.
