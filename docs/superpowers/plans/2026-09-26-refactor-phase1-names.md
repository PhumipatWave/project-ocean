# Refactor Phase 1: Names Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix typo folder names and file/class name mismatches in `Assets/Project_Ocean/Scripts` without breaking any scene, prefab or asset link.

**Architecture:** All moves and renames run inside the Unity Editor through `AssetDatabase` (via Unity MCP `execute_code`), so every `.meta` GUID is kept. An EditMode test guards the naming rules. A GUID snapshot script proves no GUID changed. Pure renames only: no logic changes.

**Tech Stack:** Unity 6000.3.6f1, NUnit EditMode tests (`Game.Tests.EditMode`), Unity MCP, Python 3.12 (stdlib).

## Global Constraints

- Spec: `docs/uml/findings.md` §4 Phase 1, and `docs/superpowers/specs/2026-09-26-uml-refresh-and-refactor-design.md`.
- Claude does not run `git commit` or `git push`. Every "Hand-off" step leaves changes in the working tree for the user.
- Every move or rename goes through `AssetDatabase.RenameAsset` / `AssetDatabase.MoveAsset`, never a raw file-system `mv`.
- Unity MCP: call `set_active_instance` with `project-ocean@7db8e111` (or the current id from `mcpforunity://instances`) before any Unity call.
- Managers live in the AS_PlayerHUD scene. Do not edit the Bootstrap scene's manager hierarchy.
- Out of scope: namespaces, asmdefs, moving types between layers (Phase 2+), data-asset folders such as `ScriptableDatas/Items/Collectables/Fishs`, and GameObject names in scenes (e.g. `Enemys` in `Ocean_Map.unity`).

## File map

| File | Change |
|---|---|
| `tools/refactor/guid_snapshot.py` | Create. Dumps GUID→path for Scripts, compares two snapshots. Reused by Phase 2. |
| `Assets/Project_Ocean/Tests/EditMode/ScriptNamingTests.cs` | Create. Guard: no typo folder/file names; each script file name matches a declared type. |
| `Core/Datas/RopeData.cs` | Rename to `RopeStructureData.cs` |
| `Core/Save Game/DayAndResetEnemys/Interactable NextDay.cs` | Rename to `InteractableNextDay.cs` |
| `UI/Items/Itemdrop.cs` | Rename to `ItemDrop.cs` (case-only, two steps) |
| `UI/Inventory/InventoryPlayer/Draggableltem.cs` | Rename file + class to `DraggableItem` |
| 11 scripts referencing `Draggableltem` | Replace identifier `Draggableltem` → `DraggableItem` |
| 9 folders | Renamed (Task 3 table) |
| `UI/Upgrads/Not User/` | Delete (contains only empty folder `UpgradsSlot`) |

All script paths are relative to `Assets/Project_Ocean/Scripts/`.

---

### Task 1: Safety net (GUID snapshot + naming test)

**Files:**
- Create: `tools/refactor/guid_snapshot.py`
- Create: `Assets/Project_Ocean/Tests/EditMode/ScriptNamingTests.cs`

**Interfaces:**
- Produces: `python tools/refactor/guid_snapshot.py snapshot <out.json>` and `python tools/refactor/guid_snapshot.py compare <before.json> <after.json>`. Exit code 0 means every GUID from before still exists after. The test class `Game.Tests.ScriptNamingTests` has tests `NoTypoFolderOrFileNames` and `ScriptFileNameMatchesADeclaredType`.

- [ ] **Step 1: Create the GUID snapshot script**

`tools/refactor/guid_snapshot.py`:

```python
"""Snapshot and compare .meta GUIDs under Assets/Project_Ocean/Scripts.

Moves must keep GUIDs, or scene/prefab links break. Usage:
  python tools/refactor/guid_snapshot.py snapshot before.json
  python tools/refactor/guid_snapshot.py compare before.json after.json
"""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SCRIPTS = ROOT / "Assets" / "Project_Ocean" / "Scripts"


def snapshot():
    out = {}
    for meta in SCRIPTS.rglob("*.meta"):
        m = re.search(r"^guid: (\w+)", meta.read_text(encoding="utf-8", errors="replace"), re.M)
        if m:
            out[m.group(1)] = meta.relative_to(ROOT).as_posix()[:-5]
    return out


def compare(before, after):
    missing = {g: p for g, p in before.items() if g not in after}
    moved = {g: (p, after[g]) for g, p in before.items() if g in after and after[g] != p}
    for g, (a, b) in sorted(moved.items(), key=lambda x: x[1][0]):
        print(f"moved   {a} -> {b}")
    for g, p in sorted(missing.items(), key=lambda x: x[1]):
        print(f"MISSING {p} ({g})")
    print(f"{len(moved)} moved, {len(missing)} missing, {len(after) - len(before) + len(missing)} new")
    return 1 if missing else 0


if __name__ == "__main__":
    if sys.argv[1] == "snapshot":
        Path(sys.argv[2]).write_text(json.dumps(snapshot(), indent=1, sort_keys=True), encoding="utf-8")
        print(f"wrote {sys.argv[2]}")
    else:
        load = lambda p: json.loads(Path(p).read_text(encoding="utf-8"))
        sys.exit(compare(load(sys.argv[2]), load(sys.argv[3])))
```

- [ ] **Step 2: Take the "before" snapshot outside the repo**

Run (project root, Git Bash):
```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/phase1_before.json"
python tools/refactor/guid_snapshot.py compare "$TEMP/phase1_before.json" "$TEMP/phase1_before.json"
```
Expected: `wrote …`, then `0 moved, 0 missing, 0 new`, exit code 0.

- [ ] **Step 2b: Record the baseline missing-script count**

Run the Unity MCP `execute_code` snippet from Task 5 Step 2 now, before any rename. Note the count and the hit list in your task report as the baseline. (Missing scripts that already exist are not Phase 1's problem, but they must not grow.)

- [ ] **Step 3: Write the failing naming test**

`Assets/Project_Ocean/Tests/EditMode/ScriptNamingTests.cs`:

```csharp
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Game.Tests
{
    public class ScriptNamingTests
    {
        private const string Root = "Assets/Project_Ocean/Scripts";

        // Unity links a MonoBehaviour/ScriptableObject to its asset by file name == class name.
        private static readonly string[] Typos =
        {
            "Entitys", "Enemys", "Beharviours", "Partbraked", "Upgrads", "LoadingSecne",
            "MovePlayerDirsction", "DayAndResetEnemys", "Save Game", "Not User", "Draggableltem"
        };

        private static bool IsLive(string path) =>
            !path.Replace('\\', '/').Contains("/Deprecated/") && !path.EndsWith("InputSystem_Actions.cs");

        [Test]
        public void NoTypoFolderOrFileNames()
        {
            var entries = Directory.GetDirectories(Root, "*", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(Root, "*.cs", SearchOption.AllDirectories))
                .Where(IsLive);
            var bad = entries
                .Where(p => Typos.Contains(Path.GetFileNameWithoutExtension(p)))
                .ToList();
            Assert.IsEmpty(bad, string.Join("\n", bad));
        }

        [Test]
        public void ScriptFileNameMatchesADeclaredType()
        {
            var bad = Directory.GetFiles(Root, "*.cs", SearchOption.AllDirectories)
                .Where(IsLive)
                .Where(p => !Regex.IsMatch(File.ReadAllText(p),
                    $@"\b(class|struct|interface|enum|record)\s+{Regex.Escape(Path.GetFileNameWithoutExtension(p))}\b"))
                .ToList();
            Assert.IsEmpty(bad, string.Join("\n", bad));
        }
    }
}
```

- [ ] **Step 4: Run the test and confirm it fails for the right reasons**

Unity MCP: `refresh_unity` (compile: request, wait_for_ready: true), then `run_tests` with mode `EditMode`, filter `ScriptNamingTests`; poll `get_test_job` until done.
Expected: both tests FAIL.
- `NoTypoFolderOrFileNames` lists the 9 typo folders, `Not User`, and `Draggableltem.cs`.
- `ScriptFileNameMatchesADeclaredType` lists exactly `RopeData.cs`, `Interactable NextDay.cs`, `Itemdrop.cs`.
`NotificationTests` must still pass: run the full EditMode suite once and confirm.

- [ ] **Step 5: Hand-off**

Tell the user the new files (`tools/refactor/guid_snapshot.py`, `ScriptNamingTests.cs` + `.meta`) are ready to commit as "test: add script naming guard". Do not commit.

---

### Task 2: Fix file/class name mismatches

**Files:**
- Rename: `Core/Datas/RopeData.cs` → `RopeStructureData.cs`
- Rename: `Core/Save Game/DayAndResetEnemys/Interactable NextDay.cs` → `InteractableNextDay.cs`
- Rename: `UI/Items/Itemdrop.cs` → `ItemDrop.cs`
- Rename: `UI/Inventory/InventoryPlayer/Draggableltem.cs` → `DraggableItem.cs`
- Modify (identifier only): `Core/Save Game/SaveBridge.cs`, `UI/Inventory/InventoryBin/TrashBin.cs`, `UI/Inventory/InventoryBuy/ShopBuySlot.cs`, `UI/Inventory/InventoryBuy/ShopInventoryBuy.cs`, `UI/Inventory/InventoryManager.cs`, `UI/Inventory/InventoryPlayer/InventorySlot.cs`, `UI/Inventory/InventoryQuestService.cs`, `UI/Shop/ShopManager.cs`, `UI/Shop/ShopTransactionService.cs`, `Deprecated/SellSlot.cs`, and the renamed `DraggableItem.cs`

**Interfaces:**
- Consumes: Task 1 test.
- Produces: class `DraggableItem` (was `Draggableltem`), same members. Later phases refer to `DraggableItem`.

- [ ] **Step 1: Rename the three mismatched files in Unity**

Unity MCP `execute_code`:

```csharp
const string R = "Assets/Project_Ocean/Scripts/";
string Log(string what, string err) => what + ": " + (string.IsNullOrEmpty(err) ? "ok" : err);
var results = new System.Collections.Generic.List<string>
{
    Log("RopeData", UnityEditor.AssetDatabase.RenameAsset(R + "Core/Datas/RopeData.cs", "RopeStructureData")),
    Log("InteractableNextDay", UnityEditor.AssetDatabase.RenameAsset(R + "Core/Save Game/DayAndResetEnemys/Interactable NextDay.cs", "InteractableNextDay")),
    // Case-only rename needs a temporary name on Windows' case-insensitive file system.
    Log("Itemdrop->tmp", UnityEditor.AssetDatabase.RenameAsset(R + "UI/Items/Itemdrop.cs", "ItemDrop_tmp")),
    Log("tmp->ItemDrop", UnityEditor.AssetDatabase.RenameAsset(R + "UI/Items/ItemDrop_tmp.cs", "ItemDrop")),
};
return string.Join("\n", results);
```
Expected: four lines ending in `ok`.

- [ ] **Step 2: Rename `Draggableltem` → `DraggableItem` in code**

Run (project root, Git Bash). This whole-word replacement only touches C# identifiers and the trailing comment in the class file:
```bash
cd Assets/Project_Ocean/Scripts
grep -rlw "Draggableltem" --include=*.cs . | while IFS= read -r f; do sed -i 's/\bDraggableltem\b/DraggableItem/g' "$f"; done
grep -rnw "Draggableltem" --include=*.cs . | wc -l
```
Expected: last line prints `0`.

- [ ] **Step 3: Rename the class file in Unity**

Unity MCP `execute_code`:
```csharp
return UnityEditor.AssetDatabase.RenameAsset(
    "Assets/Project_Ocean/Scripts/UI/Inventory/InventoryPlayer/Draggableltem.cs", "DraggableItem");
```
Expected: empty string (success).

- [ ] **Step 4: Compile and check the console**

Unity MCP: `refresh_unity` (mode force, compile request, wait_for_ready true), then `read_console` types `["error"]`, filter `error CS`.
Expected: 0 entries.

- [ ] **Step 5: Run the naming test**

Unity MCP `run_tests` EditMode, filter `ScriptNamingTests`.
Expected: `ScriptFileNameMatchesADeclaredType` PASSES. `NoTypoFolderOrFileNames` still FAILS, now listing only folders (no `Draggableltem.cs`).

- [ ] **Step 6: Check the prefab link**

Run:
```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/phase1_after2.json"
python tools/refactor/guid_snapshot.py compare "$TEMP/phase1_before.json" "$TEMP/phase1_after2.json"
```
Expected: 4 `moved` lines (the 4 renamed scripts), `0 missing`, exit code 0.
Then Unity MCP `manage_prefabs` / `manage_asset` to inspect `Assets/Project_Ocean/Prefabs/Buttons/Inventory/InventoryItem.prefab`: its root must still have a `DraggableItem` component (not "Missing script").

- [ ] **Step 7: Hand-off**

Tell the user the changes are ready to commit as "refactor: match script file names to class names". Do not commit.

---

### Task 3: Rename typo folders

**Files:** folders only (no code changes).

| Old path | New name |
|---|---|
| `Gameplay/Entitys` | `Entities` |
| `Gameplay/Entities/Enemys` | `Enemies` |
| `Gameplay/Entities/Enemies/Beharviours` | `Behaviours` |
| `Gameplay/Entities/Enemies/Partbraked` | `PartBreak` |
| `UI/Upgrads` | `Upgrades` |
| `UI/LoadingSecne` | `Loading` |
| `Core/Inputs/KeyBind/MovePlayerDirsction` | `MoveDirection` |
| `Core/Save Game/DayAndResetEnemys` | `DayReset` |
| `Core/Save Game` | `Save` |
| `UI/Upgrades/Not User` | delete (only holds the empty folder `UpgradsSlot`) |

**Interfaces:**
- Consumes: Task 2 (file names already fixed).
- Produces: the folder layout that Phase 2 plans use (`Gameplay/Entities/...`, `Core/Save/...`, `UI/Upgrades/...`, `UI/Loading/...`).

- [ ] **Step 1: Confirm `Not User` is empty of files**

Run:
```bash
find "Assets/Project_Ocean/Scripts/UI/Upgrads/Not User" -type f ! -name "*.meta"
```
Expected: no output. If any file is listed, stop and ask the user.

- [ ] **Step 2: Rename folders in Unity (order matters: each row uses the previous row's result)**

Unity MCP `execute_code`:
```csharp
const string R = "Assets/Project_Ocean/Scripts/";
var steps = new (string path, string newName)[]
{
    ("Gameplay/Entitys", "Entities"),
    ("Gameplay/Entities/Enemys", "Enemies"),
    ("Gameplay/Entities/Enemies/Beharviours", "Behaviours"),
    ("Gameplay/Entities/Enemies/Partbraked", "PartBreak"),
    ("UI/Upgrads", "Upgrades"),
    ("UI/LoadingSecne", "Loading"),
    ("Core/Inputs/KeyBind/MovePlayerDirsction", "MoveDirection"),
    ("Core/Save Game/DayAndResetEnemys", "DayReset"),
    ("Core/Save Game", "Save"),
};
var log = new System.Collections.Generic.List<string>();
foreach (var (path, newName) in steps)
{
    var err = UnityEditor.AssetDatabase.RenameAsset(R + path, newName);
    log.Add(path + " -> " + newName + ": " + (string.IsNullOrEmpty(err) ? "ok" : err));
    if (!string.IsNullOrEmpty(err)) break;
}
bool deleted = UnityEditor.AssetDatabase.DeleteAsset(R + "UI/Upgrades/Not User");
log.Add("delete UI/Upgrades/Not User: " + deleted);
UnityEditor.AssetDatabase.Refresh();
return string.Join("\n", log);
```
Expected: 9 lines ending in `ok`, then `delete UI/Upgrades/Not User: True`.

- [ ] **Step 3: Compile, console, tests**

Unity MCP: `refresh_unity` (force, compile request, wait), `read_console` errors filtered `error CS` → expected 0. `run_tests` EditMode (whole suite) → expected: all pass, including both `ScriptNamingTests`.

- [ ] **Step 4: GUID check**

Run:
```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/phase1_after3.json"
python tools/refactor/guid_snapshot.py compare "$TEMP/phase1_before.json" "$TEMP/phase1_after3.json"
```
Expected: `0 missing`, exit code 0. The only GUID that disappears legitimately is the deleted `Not User` folder and its `UpgradsSlot` subfolder. If they show as `MISSING`, that is expected and the only allowed exception. Any `.cs` in the MISSING list = stop and investigate.

- [ ] **Step 5: Hand-off**

Tell the user the changes are ready to commit as "refactor: fix typo folder names in Scripts". Do not commit.

---

### Task 4: Decide the four unreferenced scripts

**Files:** `Core/Save/DayReset/InteractableNextDay.cs`, `UI/Loading/GameBootstrap.cs`, `UI/Loading/LandOpeningController.cs`, `UI/Loading/LoadingUI.cs` (paths after Task 3).

**Interfaces:**
- Consumes: Task 3 paths.
- Produces: nothing other phases depend on.

- [ ] **Step 1: Re-check they are still unreferenced**

Run from project root (reuses the scan from the UML session):
```bash
for f in "Core/Save/DayReset/InteractableNextDay.cs" "UI/Loading/GameBootstrap.cs" "UI/Loading/LandOpeningController.cs" "UI/Loading/LoadingUI.cs"; do
  g=$(grep -oE "^guid: \w+" "Assets/Project_Ocean/Scripts/$f.meta" | cut -c7-)
  n=$(grep -rl "$g" Assets --include=*.unity --include=*.prefab --include=*.asset | wc -l)
  echo "$f: $n asset refs"
done
```
Expected: `0 asset refs` for each. `InteractableNextDay` can now be attached, because Task 2 fixed its file name. If any count is above 0, that script is in use: drop it from this task.

- [ ] **Step 2: Ask the user (AskUserQuestion, multiSelect)**

Question: "Which of these unreferenced scripts should move to `Scripts/Deprecated/`?" Options: `InteractableNextDay` (sleep/next-day trigger; `DayManager` already advances days), `GameBootstrap` (loads Land_Map on Start; overlaps `BootstrapLoader`), `LandOpeningController`, `LoadingUI` (overlaps `LoadingManager` + loading visuals).

- [ ] **Step 3: Move the chosen scripts in Unity**

Unity MCP `execute_code`. Put only the chosen paths in `chosen`:
```csharp
const string R = "Assets/Project_Ocean/Scripts/";
var chosen = new[] { "UI/Loading/GameBootstrap.cs" }; // replace with the user's picks
var log = new System.Collections.Generic.List<string>();
foreach (var p in chosen)
{
    var err = UnityEditor.AssetDatabase.MoveAsset(R + p, R + "Deprecated/" + System.IO.Path.GetFileName(p));
    log.Add(p + ": " + (string.IsNullOrEmpty(err) ? "ok" : err));
}
return string.Join("\n", log);
```
Expected: one `ok` per chosen file. If the user picked none, skip this step.

- [ ] **Step 4: Compile and console**

Unity MCP `refresh_unity` + `read_console` (`error CS`) → expected 0.

---

### Task 5: Verify live and refresh docs

**Files:**
- Regenerate: `docs/uml/**` (generated pages only; `findings.md` is edited by hand)
- Modify: `docs/uml/findings.md` §2 and §4 Phase 1 (mark done, update paths)

**Interfaces:**
- Consumes: Tasks 1–4.

- [ ] **Step 1: Play-mode smoke run**

Unity MCP: open `Assets/Project_Ocean/Levels/Scenes/Loading/Bootstrap.unity`, enter Play mode, then set `Application.runInBackground = true` via `execute_code`. Load MainMenuScene → New Game → Land_Map → dive to Ocean_Map (route: `reference_live_enemy_test_route` memory), then save and load. Afterwards run `read_console` for errors.
Expected: no `Missing script`, `NullReferenceException` or `MissingReferenceException`. Inventory item drag works (it uses `DraggableItem`). Exit Play mode.

- [ ] **Step 2: Scan scenes/prefabs for missing scripts**

Unity MCP `execute_code`:
```csharp
int missing = 0;
var hits = new System.Collections.Generic.List<string>();
foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Project_Ocean" }))
{
    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
    var go = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path);
    foreach (var t in go.GetComponentsInChildren<UnityEngine.Transform>(true))
    {
        int n = UnityEditor.GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
        if (n > 0) { missing += n; hits.Add(path + " / " + t.name); }
    }
}
return missing + " missing\n" + string.Join("\n", hits);
```
Expected: count and hit list identical to the Task 1 Step 2b baseline. Any new hit = stop and investigate.

- [ ] **Step 3: Regenerate UML and graph**

Run (project root):
```bash
python tools/uml/gen_uml.py
python tools/uml/test_gen_uml.py
graphify update .
```
Expected: generator prints `… files, … types, … modules`. Self-test `OK`. Module names now show `Gameplay/Entities`, `Core/Save`, `UI/Upgrades`, `UI/Loading`.

- [ ] **Step 4: Update `docs/uml/findings.md`**

- In §2, replace the "Also unreferenced but kept" paragraph with the Task 4 outcome.
- In the "File name ≠ class name" line, append `— fixed in Phase 1.`
- In §4 Phase 1, add `**Status:** done 2026-09-26 (or actual date).` under the heading, and change later-phase paths: `Gameplay/Entitys` → `Gameplay/Entities`, `UI/Upgrads` → `UI/Upgrades`, `UI/LoadingSecne` → `UI/Loading`, `Core/Save Game` → `Core/Save`.

- [ ] **Step 5: Hand-off**

Summarize for the user: the files changed, the test results, the GUID compare output, the smoke-run result. Suggested commit: "docs: refresh UML after Phase 1 renames". Do not commit or push.
