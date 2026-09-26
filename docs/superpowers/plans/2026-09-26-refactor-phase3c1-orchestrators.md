# Refactor Phase 3c-1: Move Gameplay Orchestrators Out of Core — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Put the classes that orchestrate gameplay (spawn the player, apply upgrades, save player and quest state) in the Gameplay layer, and the save-menu UI in the UI layer. Core keeps only infrastructure. Wrong-way links go from 37 to about 25, with no logic change.

**Architecture:** File moves only, via `AssetDatabase.MoveAsset` (GUIDs kept, so scene and prefab links survive). Namespaces stay `Game.Testing`. This is step 1 of the agreed hybrid. Step 2 (`SaveBridge` → per-system `ISaveParticipant`, `UpgradeService` → `OnUpgradeApplied`) gets its own plan and a full Play-mode save/load run.

**Tech Stack:** Unity 6000.3.6f1 + Unity MCP, NUnit EditMode, Python 3.12 (`tools/uml/gen_uml.py`, `tools/refactor/guid_snapshot.py`).

## Global Constraints

- The user approved the hybrid approach on 2026-09-26 (move orchestrators first, then clean the SaveBridge/UpgradeService internals).
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Moves, folder creation and deletion only via `UnityEditor.AssetDatabase` (Unity MCP `execute_code`). If safety checks block the call, re-run the identical code with `safety_checks=false` and disclose it. No code edits.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`; retry once on a transient timeout.
- The working tree already holds uncommitted Phase 3a/3b work and the `EntityAnimation.cs` bug fix. Leave them alone.

## Moves (paths relative to `Assets/Project_Ocean/Scripts/`)

| From | To |
|---|---|
| `Core/GameManager.cs` | `Gameplay/Game/GameManager.cs` |
| `Core/PlayerRegistry.cs` | `Gameplay/Entities/Players/PlayerRegistry.cs` |
| `Core/Save/SaveBridge.cs` | `Gameplay/Save/SaveBridge.cs` |
| `Core/Services/UpgradeService.cs` | `Gameplay/Services/UpgradeService.cs` |
| `Core/Services/FeatureUnlockService.cs` | `Gameplay/Services/FeatureUnlockService.cs` |
| `Core/Save/SaveMenuUI.cs` | `UI/Save/SaveMenuUI.cs` |
| `Core/Save/SaveSlotUI.cs` | `UI/Save/SaveSlotUI.cs` |
| delete `Core/Services/` (empty after the moves) | — |

---

### Task 1: Move the orchestrators and save UI

**Files:** moves only (table above).

**Interfaces:**
- Produces: folders `Gameplay/Game/`, `Gameplay/Save/`, `Gameplay/Services/`, `UI/Save/`. Step 2's plan edits `Gameplay/Save/SaveBridge.cs` and `Gameplay/Services/UpgradeService.cs` at these paths.

- [ ] **Step 1: Baseline**

Run (project root, Git Bash):
```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/phase3c1_before.json"
python tools/uml/gen_uml.py
grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
```
Expected: `wrote …`; `Layer violations — 37`.

- [ ] **Step 2: Move in Unity**

Unity MCP `execute_code`:
```csharp
const string R = "Assets/Project_Ocean/Scripts/";
var log = new System.Collections.Generic.List<string>();
foreach (var f in new[] { new[] { "Gameplay", "Game" }, new[] { "Gameplay", "Save" }, new[] { "Gameplay", "Services" }, new[] { "UI", "Save" } })
    log.Add("mkdir " + f[0] + "/" + f[1] + ": " + UnityEditor.AssetDatabase.CreateFolder(R + f[0], f[1]));
var moves = new[]
{
    new[] { "Core/GameManager.cs",                   "Gameplay/Game/GameManager.cs" },
    new[] { "Core/PlayerRegistry.cs",                "Gameplay/Entities/Players/PlayerRegistry.cs" },
    new[] { "Core/Save/SaveBridge.cs",               "Gameplay/Save/SaveBridge.cs" },
    new[] { "Core/Services/UpgradeService.cs",       "Gameplay/Services/UpgradeService.cs" },
    new[] { "Core/Services/FeatureUnlockService.cs", "Gameplay/Services/FeatureUnlockService.cs" },
    new[] { "Core/Save/SaveMenuUI.cs",               "UI/Save/SaveMenuUI.cs" },
    new[] { "Core/Save/SaveSlotUI.cs",               "UI/Save/SaveSlotUI.cs" },
};
foreach (var m in moves)
{
    string err = UnityEditor.AssetDatabase.MoveAsset(R + m[0], R + m[1]);
    log.Add(m[0] + " -> " + m[1] + ": " + (string.IsNullOrEmpty(err) ? "ok" : err));
    if (!string.IsNullOrEmpty(err)) return string.Join("\n", log);
}
bool empty = UnityEditor.AssetDatabase.FindAssets("", new[] { R + "Core/Services" }).Length == 0;
log.Add("delete Core/Services: " + (empty ? UnityEditor.AssetDatabase.DeleteAsset(R + "Core/Services").ToString() : "SKIPPED, not empty"));
return string.Join("\n", log);
```
Expected: 4 folder GUIDs, 7 lines ending in `ok`, `delete Core/Services: True`.

- [ ] **Step 3: Compile, test, GUID check, metric, prefab scan**

Unity MCP: `refresh_unity` (force, compile request, wait). `read_console` `error CS`: 0. `run_tests` EditMode: 9/9.
```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/phase3c1_after.json"
python tools/refactor/guid_snapshot.py compare "$TEMP/phase3c1_before.json" "$TEMP/phase3c1_after.json"
python tools/uml/gen_uml.py
python tools/uml/test_gen_uml.py
grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
```
Expected: 7 `moved`; the only `MISSING` entry is the folder `Assets/Project_Ocean/Scripts/Core/Services`; `Layer violations` about 25 (simulated 26 with GameManager moved; record the exact number and list any row whose From or To type was moved here).
Then run the prefab missing-script scan snippet (in `docs/superpowers/plans/2026-09-26-refactor-phase2-layers.md`, Task 1 Step 3). Expected: `0 missing`.

- [ ] **Step 4: Update `docs/uml/findings.md`**

Under `### Phase 3`, append to the existing `**Progress:**` line:
```markdown
 3c-1 done: `GameManager` → `Gameplay/Game`, `SaveBridge` → `Gameplay/Save`, `UpgradeService`/`FeatureUnlockService` → `Gameplay/Services`, `PlayerRegistry` → `Gameplay/Entities/Players`, save menu UI → `UI/Save` (37 → <new count>).
```
Then run `graphify update .` (project root).

- [ ] **Step 5: Hand-off**

Suggested commit: "refactor: move gameplay orchestrators out of Core". Do not commit. The full Play-mode save/load run happens at the end of step 2 (3c-2), which changes save logic.
