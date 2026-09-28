# Refactor Phase 5d: ObjectWrapPoint Dead Code + Phase 5 Close-out — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Finish Phase 5. `ObjectWrapPoint` (613 lines) doesn't need a split; it has ~370 lines of unused API. Remove it (or disable it; the user decides, see Task 1). The remaining roadmap rows are reviewed and closed.

**Findings (2026-09-29):**

| Class | Now | Verdict |
|---|---|---|
| `ObjectWrapPoint` | 613 lines, 32 methods, 1 dep | One job (wrap corners for a collider). Only `GetActivePoints` is called from outside (by `RopeCornerFinder`). `GetAllWrapPoints`, `IsNearEnabledWrapPoint`, `GetClosestPoint`, `GetWrapPointForContact`, `TryBuildBoundaryPath`, `PathPenetratesCollider`, `GetCollider` have 0 code and 0 scene/prefab callers, and the private helpers only they use are dead too (`Cross`, `PruneBoundaryPath`, `FindCandidateIndices`, `BuildOrderedPathGroups`, `FindClosestIndex`, `BuildArc`, `ChooseExteriorArc`, `PathSideRelativeTo`, `ComputeCentroid`, `PolylineLength`). It's an older wrap approach; the rope now uses plain corners. |
| `SaveBridge` | 307 lines (was 435) | Done by Phase 3c (save participants). No action. |
| `ShopManager` | 407 lines, 37 methods | One job (the buy/sell window). No split. CLAUDE.md's "separate UI view and logic" applies (buy/sell rules live in the UI script), but that's a design change, not a size split. Listed for later. |
| `Quest.cs` | 546 lines, 13 methods | Enums + two data classes (`QuestStep`, `Quest`) with 144 comment lines. Not a god class. Move `QuestStep` (~245 lines) to its own `QuestStep.cs` (one class per file; plain `[Serializable]` class, so no GUID or asset risk). |

**Kept in ObjectWrapPoint:** `WrapPoint`, the Inspector fields, `Awake`, `OnValidate`, `OnDrawGizmos`, `GetActivePoints`, `GeneratePoints` and its generators/filters (`GenerateFrom*`, `AddFilteredPath`, `MergeClosePoints`, `IsCollinear`, `IsConcave`, `IsAngleTooSharp`, `Add`). Expected ≈ 245 lines. Serialized fields are untouched, so the scene's generated wrap points stay.

## Global Constraints

- No behaviour change. The deterministic rope harness from Phase 5b (`$TEMP/p5b_harness.cs`, baseline `$TEMP/p5b_baseline.txt`) runs on the Ocean tilemap, which has an `ObjectWrapPoint`, so it's the live check here too.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`. Back up saves before Play and restore after.

---

### Task 1: ObjectWrapPoint dead API

- [ ] **Step 1:** Re-check 0 callers (code, scenes, prefabs, assets) for every member above right before editing.
- [ ] **Step 2:** Remove the members (user choice: delete, git keeps history; or disable with `#if ENABLE_WRAP_BOUNDARY_PATH`).
- [ ] **Step 3:** Compile: 0 `error CS`. `run_tests` EditMode: 34/34.

### Task 2: QuestStep to its own file

- [ ] **Step 1:** Create `Gameplay/QuestSystems/Quest/QuestStep.cs` with the `QuestStep` class moved verbatim (same namespace, same `using`s it needs). Remove it from `Quest.cs`.
- [ ] **Step 2:** Compile: 0 errors. Tests: 34/34. Check that a QuestData asset still shows its steps (load `MQ_1002`, count `steps` = 15).

### Task 3: Verify live, docs

- [ ] **Step 1:** Run the Phase 5b rope harness in Ocean. Expected: identical to `$TEMP/p5b_baseline.txt` (the clamp line may differ by float rounding, as in 5b).
- [ ] **Step 2:** Missing-script scan: 0. Restore saves (RESTORED), `git status --short -- "*.unity" "*.prefab"` empty.
- [ ] **Step 3:** `findings.md` Phase 5: mark `ObjectWrapPoint`, `SaveBridge`, `ShopManager`, `Quest.cs` with the verdicts above, and mark Phase 5 done. UML regen + tests, `graphify update .`.
- [ ] **Step 4:** Hand-off. Suggested commit: "refactor: drop unused ObjectWrapPoint boundary-path API; QuestStep in its own file". Do not commit.
