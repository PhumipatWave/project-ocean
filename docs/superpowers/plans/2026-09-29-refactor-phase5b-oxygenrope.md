# Refactor Phase 5b: Split OxygenRope — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `OxygenRope` (785 lines) keeps the rope's *game rules*: pins (start, wrap corners, checkpoints, hub tie-in), wrap/unwrap decisions, the player length clamp, the breach countdown and drawing. The Verlet physics and the collider-corner geometry move out. Behaviour is identical and the public API is unchanged, so no caller changes.

**Architecture:** new plain C# files next to `OxygenRope.cs` in `Assets/Project_Ocean/Scripts/Gameplay/Entities/Players/OxygenTube/`, namespace `Game.Gameplay`. No MonoBehaviour is added, so no scene or prefab changes.

| New class | Moved from OxygenRope (verbatim bodies) | Size |
|---|---|---|
| `VerletRope` (instance) | `Seg`, the `segs` list, `SimStart`, `GrowSegs`, `ShrinkSegs`, `Integrate`, `PinEnds`, `Solve`, `SolvePair`, `PushOutColliders`, `RebuildSegs`, and the segment half of `MeasureLength` | ~190 |
| `RopeCornerFinder` (static) | `FindBestCorner`, `GetColliderCorners` (with the `ObjectWrapPoint` path) | ~90 |

`OxygenRope` keeps (~470 lines): the Inspector fields (unchanged names, so prefab/scene data stays), all public API and events, `Awake`/`FixedUpdate`/`LateUpdate`, pins and the setup methods, `EnforceRopeLimit`/`SnapPlayer`, `TryWrap`/`TryUnwrap`/`IsValidAnchor`, `DrawRope`, the pin half of `MeasureLength`, `CheckLimit`/`LimitBreached`.

**Why not split the breach countdown too (roadmap item):** it's ~50 lines and runs on a coroutine. Moving it means either a new MonoBehaviour (a scene/prefab edit) or swapping the coroutine for a hand-rolled timer (a timing change). Neither is worth it for 50 lines. Skipped; revisit if it grows.

**Settings stay on OxygenRope.** Moving the Inspector fields into a nested settings class would lose their serialized values (`[FormerlySerializedAs]` can't cross into a nested object). So `VerletRope` gets them per call, which also keeps live Inspector tweaking working:
```csharp
// VerletRope
public struct Settings { public float segmentLength, damping, segRadius; public int solverIterations, maxSimulatedSegs; public Vector2 gravity; public LayerMask collisionMask; }
public int Count => segs.Count;
public void Rebuild(Vector2 from, Vector2 to, float segmentLength);
public void Clear();
public void Step(Vector2 anchor, Vector2 player, float dt, in Settings s);   // Grow, Shrink, Integrate, PinEnds, Solve, PushOutColliders: same order as today
public float MeasureSegments(int maxSimulatedSegs);                         // frozen straight part + simulated part
public void AppendDrawPoints(List<Vector2> buf);                            // segs[1..], as DrawRope does today
```
```csharp
// OxygenRope.FixedUpdate, same order as today
EnforceRopeLimit();
rope.Step(pins[^1].pos, playerTransform.position, Time.fixedDeltaTime, SimSettings);
if (wrapCooldown == 0) TryWrap();
if (unwrapCooldown == 0) TryUnwrap();
CurrentLength = MeasurePins() + rope.MeasureSegments(maxSimulatedSegs);
CheckLimit();
```
`segs.Count < 2` guards → `rope.Count < 2`; `RebuildSegs()` → `rope.Rebuild(pins[^1].pos, playerPos-or-anchor, segmentLength)`; `segs.Clear()` in `ResetRope` → `rope.Clear()`; `FindBestCorner(...)` → `RopeCornerFinder.FindBestCorner(col, hit, dir, cornerBuf, compositeBuf)` (OxygenRope keeps owning the reuse buffers, so no new allocations).

**Tech Stack:** Unity 6000.3.6f1 + Unity MCP, NUnit EditMode.

## Global Constraints

- Roadmap: `docs/uml/findings.md` §4 Phase 5 (one class per PR; this is the OxygenRope PR).
- No behaviour change, public API unchanged, Inspector field names unchanged.
- Move bodies verbatim, comments included (renames only as listed above). Comment clean-up is Phase 6.
- Per-frame cost unchanged: no new allocations in `FixedUpdate`/`LateUpdate` (the `Settings` struct is passed by `in`).
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`.
- Back up the save folder before Play and restore it byte-identical after.

## Risks

- **Step order:** the physics order (Grow → Shrink → Integrate → PinEnds → Solve → PushOut) and its position between `EnforceRopeLimit` and `TryWrap` must not change. `Step` keeps it in one place.
- **Frozen-segment length:** `MeasureLength` depends on `SimStart`, which depends on `maxSimulatedSegs`. The pin half and the segment half must add up to the same number as today. Covered by a test and a live compare.
- **Frame timing in Play:** checked live by comparing `CurrentLength` before and after on the same dive route (below).

---

### Task 1: Tests and a baseline

**Files:** Create `Assets/Project_Ocean/Tests/EditMode/RopeSplitTests.cs`

- [ ] **Step 1: Live baseline before any change.** Play → load slot 0 → Ocean_Map (route from memory `reference_live_enemy_test_route.md`). Move the player along a fixed script (teleport to 3 fixed offsets from the start point, 30 FixedUpdates each) and record `CurrentLength`, pin count and segment count after each. Save the numbers in `$TEMP/p5b_baseline.txt`. Restore saves after.
- [ ] **Step 2: Write tests against the new API** (red until Task 2):
  - `Rebuild_StraightLine_SegmentCountMatchesLength`: from (0,0) to (3,0), segment 1 → 4 segments, `MeasureSegments` ≈ 3.
  - `Step_NoGravity_KeepsLengthWithinOneSegment`: settings with gravity 0, no collision mask; after 50 steps the length stays within `segmentLength` of the straight distance.
  - `Step_PlayerMovesCloser_ShrinksOneSegmentPerStep`.
  - `CornerFinder_BoxCollider_PicksNearestCorner`: a `BoxCollider2D` at the origin (size 2), hit point (0.9, 1), rope dir (1, 0) → corner (1, 1). Call `Physics2D.SyncTransforms()` first; clean up the GameObject in `TearDown`.
- [ ] **Step 3:** `refresh_unity`, `read_console`: expected red for the missing types only.

### Task 2: Extract

- [ ] **Step 1:** Create `RopeCornerFinder.cs`.
- [ ] **Step 2:** Create `VerletRope.cs`.
- [ ] **Step 3:** Edit `OxygenRope.cs`: remove the moved members, add the `VerletRope rope = new()` field and a `SimSettings` property built from the Inspector fields, point the calls at the new classes.
- [ ] **Step 4:** Compile: 0 `error CS`. `run_tests` EditMode: 28 + 4 new, all pass.

### Task 3: Verify live, docs

- [ ] **Step 1:** Repeat the Task 1 Step 1 route. Expected: pin count and segment count identical, `CurrentLength` within 0.01 m of the baseline at each stop.
- [ ] **Step 2:** Play smoke in Ocean: the rope draws, wraps a corner, unwraps, a checkpoint attaches and detaches, the length clamp holds the player at `MaxRopeLength`, Infinite Oxygen (DevPanel) skips the clamp. Console: no new errors. Exit Play, restore saves (RESTORED), `git status --short -- "*.unity" "*.prefab"` empty.
- [ ] **Step 3:** Missing-script scan (prefabs and scenes): 0.
- [ ] **Step 4:** `python tools/uml/gen_uml.py && python tools/uml/test_gen_uml.py`, `graphify update .`. In `docs/uml/findings.md` Phase 5, mark the `OxygenRope` row done with the new line count and the breach-countdown decision.
- [ ] **Step 5:** Hand-off. Suggested commit: "refactor(rope): extract Verlet simulation and corner finder from OxygenRope". Do not commit.
