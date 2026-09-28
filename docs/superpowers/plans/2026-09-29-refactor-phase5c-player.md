# Refactor Phase 5c: Split Player — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

> **Revised 2026-09-29 (user):** the checkpoint item is not used in the game; only the MiniHub item is. So instead of extracting `PlayerCheckpoints`, all checkpoint-item code is disabled with `#if ENABLE_CHECKPOINT_ITEM` (re-enable by adding that define in Player Settings → Scripting Define Symbols). `PlayerItemUse` is still extracted: the MiniHub item goes through it (plain `Building` item, no `IItemEffect`, spawned by `ExecuteItem`'s fallback, then `MiniHubItem.Start` calls `AttachToHub`). Checkpoint tests are replaced by item-use tests. Disabled: `CheckPointItem.cs` (whole class), Player's checkpoint list/API/`StartPull`/`HandleTeleport` (+ its `OnTeleport` subscription)/land reset block, `InventoryManager`'s "T" hint, `OxygenRope.AttachToCheckpoint`/`DetachFromCheckpoint`/`HasCheckpoint`. Kept: `IItemEffect` (generic seam, used by `UseItem`/`InventoryManager` hint), `ItemType.CheckpointItem` (enum values are serialized ints), the `isCheckpoint` pin flag read by `TryUnwrap` (always false now). Expected side effect: the unused `CheckPoint.prefab` shows 1 missing script while the define is off.

**Goal:** `Player` (551 lines) keeps what a player *is*: input wiring, movement, aim/attack, scene enter (land/ocean), hit feedback and animation. Checkpoint bookkeeping and item use move out. Behaviour is identical and the public API is unchanged, so no caller changes.

**UIManager was reviewed and skipped:** one job (window stack + input lock/timescale + Esc), padded by formatting and 16 one-line wrappers. A split gains nothing. Its only cut, 4 unused wrappers (`ToggleSettingMenu`, `ToggleSettingPause`, `ToggleInventory`, `ToggleQuest`: 0 code and 0 scene/prefab callers), goes to Phase 6.

**Architecture:** plain C# next to `Player.cs` in `Assets/Project_Ocean/Scripts/Gameplay/Entities/Players/`, namespace `Game.Gameplay`. No component is added, so the Player prefab is untouched.

| New class | Moved from Player (verbatim bodies) | Size |
|---|---|---|
| `PlayerCheckpoints` (instance, owned by Player) | the `checkpoints` list, `_activePullCoroutine`, `CheckpointCount`, `CanPlaceCheckpoint`, `LastCheckpoint`, `Register`, `Unregister`, `ClearAll`, `CanPlaceFromSlot`, `StartPull`, and the checkpoint half of the Land branch of `OnSceneLoaded` → `ResetForLand()` | ~90 |
| `PlayerItemUse` (static) | `UseItem(ItemType)` → `Use(Player, ItemType)`, `ExecuteItem(Item)` → `Execute(Player, Item)` | ~60 |

Player keeps one-line forwards for its public checkpoint API (`CheckpointCount`, `CanPlaceCheckpoint`, `LastCheckpoint`, `RegisterCheckpoint`, `UnregisterCheckpoint`, `ClearAllCheckpoints`, `CanPlaceCheckpointFromSlot`, `StartPull`), so `CheckPointItem` and `InventoryManager` compile unchanged. Expected `Player.cs` ≈ 440 lines.

**Wiring:**
```csharp
// PlayerCheckpoints: coroutines and Destroy need a MonoBehaviour host.
public PlayerCheckpoints(MonoBehaviour host, Func<OxygenRope> rope)
```
```csharp
// Player
private PlayerCheckpoints checkpoints;
protected override void Awake() { oxygenTube = ...; checkpoints = new PlayerCheckpoints(this, () => oxygenTube); base.Awake(); ... }
public void RegisterCheckpoint(CheckPointItem cp) => checkpoints.Register(cp);
// ...same for the other forwards
// OnSceneLoaded, Land branch: the pull-cancel + slot-clear + list-clear + RefreshBuildingHints block → checkpoints.ResetForLand();
// HandleUseItem: UseItem(ItemType.Building) → PlayerItemUse.Use(this, ItemType.Building);
```
`checkpoints` is created at the top of `Awake`, before `base.Awake()`, so nothing in the base class can touch it while it's null.

**Dead code found (not removed here; listed for Phase 6):** `Aim()` (its `input.OnAimed` subscription is commented out), `Interact()` (only logs "Interact"), `CanPlaceCheckpoint` and `ClearAllCheckpoints` (0 callers), the `Alpha1` "Test Input" in `Update`, the empty `[Header("Player Feedback VFX")]` on a private non-serialized field.

**Tech Stack:** Unity 6000.3.6f1 + Unity MCP, NUnit EditMode.

## Global Constraints

- Roadmap: `docs/uml/findings.md` §4 Phase 5 (one class per PR; this is the Player PR).
- No behaviour change, public API unchanged, serialized field names unchanged (Player prefab data stays).
- Move bodies verbatim, comments included (renames only as listed). Comment and dead-code clean-up is Phase 6.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`.
- Back up the save folder before Play and restore it byte-identical after.

## Risks

- **Coroutine owner:** the pull coroutine must still run on the Player (so it stops with the Player, and `ResetForLand` can stop it). `PlayerCheckpoints` calls `host.StartCoroutine`/`host.StopCoroutine`.
- **Order inside the Land branch:** the checkpoint reset runs at the same point it does today (after the rope reset and timeScale/velocity reset).
- **`Destroy`:** becomes `Object.Destroy`, same call.

---

### Task 1: Tests first

**Files:** Create `Assets/Project_Ocean/Tests/EditMode/PlayerSplitTests.cs`

- [ ] **Step 1:** Tests against the new API (red until Task 2). Use a host `GameObject` with an empty MonoBehaviour, and `CheckPointItem` components on plain GameObjects (set `SourceSlotIndex` if it's settable; if it isn't, test only the count rules and say so in the report). Destroy all objects in `TearDown`.
  - `CanPlaceFromSlot_FalseWhenSlotAlreadyUsed_OrThreePlaced`.
  - `Unregister_RemovesAndClearsBuildingSlot` (fake `IInventory` records `ClearBuildingSlot` calls).
  - `ResetForLand_ClearsListAndSlots`.
- [ ] **Step 2:** `refresh_unity`, `read_console`: red for the missing types only.

### Task 2: Extract

- [ ] **Step 1:** Create `PlayerCheckpoints.cs` and `PlayerItemUse.cs`.
- [ ] **Step 2:** Edit `Player.cs`: remove the moved members, add the field, the forwards and the two call sites.
- [ ] **Step 3:** Compile: 0 `error CS`. `run_tests` EditMode: 32 + new, all pass.

### Task 3: Verify live, docs

- [ ] **Step 1:** Missing-script scan (prefabs and scenes): 0.
- [ ] **Step 2:** Play smoke (save backup first). Load slot 0 → Land: player moves, sprint VFX. → Ocean: aim/shoot fires a bullet. Use a checkpoint item from the building slot: it places, `CheckpointCount` = 1, and the rope attaches. Teleport pulls the player to it. Take damage: the hit flash shows. → back to Land: `CheckpointCount` = 0, the building slot is cleared, timeScale = 1. Console: no new errors. Exit Play, restore saves (RESTORED), `git status --short -- "*.unity" "*.prefab"` empty.
- [ ] **Step 3:** `python tools/uml/gen_uml.py && python tools/uml/test_gen_uml.py`, `graphify update .`. In `docs/uml/findings.md`: mark the `Player` row done (with line count), mark `UIManager` as reviewed and skipped, and add the dead code above to Phase 6.
- [ ] **Step 4:** Hand-off. Suggested commit: "refactor(player): extract checkpoint bookkeeping and item use from Player". Do not commit.
