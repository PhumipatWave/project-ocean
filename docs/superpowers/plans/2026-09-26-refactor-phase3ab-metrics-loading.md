# Refactor Phase 3a+3b: Accurate Metrics, Spawner Moves, LoadingManager to Core — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove the 11 false-positive rows from the UML layer-violation metric, then cut the real count from 50 to about 37 by moving spawners and `QuestTrackerUI` to their layer and moving `LoadingManager` to Core behind a small interface.

**Architecture:** (1) Fix `tools/uml/gen_uml.py` so member-access names (`SceneNames.MainMenu`), enum values and a type's own member names no longer count as type references (TDD in `tools/uml/test_gen_uml.py`). (2) Move files with `AssetDatabase.MoveAsset` (GUIDs kept). (3) Add `ILoadingVisual` in Core; `LoadingVisualController` implements it; `LoadingManager` keeps its serialized field under the same name, retyped to `MonoBehaviour`, so the scene reference survives. Then `LoadingManager.cs` moves to `Core/Scenes`.

**Tech Stack:** Python 3.12 (stdlib `unittest`), Unity 6000.3.6f1 + Unity MCP, NUnit EditMode tests.

## Global Constraints

- Spec: `docs/uml/findings.md` §4 Phase 3 (sub-projects 3a + 3b, agreed with the user 2026-09-26).
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`. Each "Hand-off" step leaves changes in the working tree.
- Existing files move only via `UnityEditor.AssetDatabase.MoveAsset` / `CreateFolder` (Unity MCP `execute_code`), never raw mv. If safety checks block the call, re-run the identical code with `safety_checks=false` and disclose it.
- No namespace or asmdef changes (`namespace Game.Testing` everywhere).
- Behaviour must not change: the loading screen must still show the right icon profile per `LoadingType`.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`. A transient bridge timeout right after it is known; retry once.
- After Play or test runs: `git status -- "*.unity" "*.prefab"`. Report any change. Before calling one harmful, check in Play whether the visible result is broken (memory `feedback_check_scene_dirty_after_play.md`). Do not restore scene files.
- Comment rules (CLAUDE.md): 1–2 line WHY comments, no history, XML `<summary>` only on public API.

---

### Task 1: Stop counting member names as type references

**Files:**
- Modify: `tools/uml/gen_uml.py` (function `resolve`, the lines computing `used` / `t.deps`)
- Test: `tools/uml/test_gen_uml.py` (add class `DependencyFalsePositiveTest`)

**Interfaces:**
- Produces: `TypeInfo.deps` without member-access identifiers, enum values or the type's own member names. Metrics pages regenerate with the corrected counts.

- [ ] **Step 1: Write the failing test**

Append to `tools/uml/test_gen_uml.py`, above `if __name__ == "__main__":`:
```python
SAMPLE_NAMES = '''
namespace Game.Testing
{
    public enum Kind { Quest, Other }
    public static class Names { public const string Quest = "q"; }
    public class Quest { }
    public class User { void F() { var s = Names.Quest; var k = Kind.Quest; } }
    public class Real { void F() { var q = new Quest(); } }
}
'''


class DependencyFalsePositiveTest(unittest.TestCase):
    def setUp(self):
        code = g.strip_code(SAMPLE_NAMES)
        types = g.parse_types(code, g.SRC / "Test" / "Names.cs")
        for t in types:
            g.parse_members(t)
        self.by_name, _ = g.resolve(types)

    def test_enum_values_are_not_dependencies(self):
        self.assertEqual(set(), self.by_name["Kind"].deps)

    def test_own_member_name_is_not_a_dependency(self):
        self.assertNotIn("Quest", self.by_name["Names"].deps)

    def test_member_access_is_not_a_dependency(self):
        self.assertEqual({"Names", "Kind"}, self.by_name["User"].deps)

    def test_real_type_use_still_counts(self):
        self.assertEqual({"Quest"}, self.by_name["Real"].deps)
```

- [ ] **Step 2: Run it and confirm it fails**

Run (project root): `python tools/uml/test_gen_uml.py`
Expected: FAIL. `test_enum_values_are_not_dependencies`, `test_own_member_name_is_not_a_dependency` and `test_member_access_is_not_a_dependency` fail, because each finds `Quest` in deps. The 5 old tests and `test_real_type_use_still_counts` pass.

- [ ] **Step 3: Implement the fix**

In `tools/uml/gen_uml.py`, function `resolve`, replace these two lines:
```python
        used = set(IDENT.findall(t.body)) & names
        t.deps = used - {t.name} - set(t.assoc) - {t.base_class} - set(t.interfaces)
```
with:
```python
        # `X.Member` names, enum values and own member names are not type uses (e.g. SceneNames.MainMenu).
        own = {mb.name for mb in t.members}
        code = re.sub(r"\.\s*[A-Z]\w*", ".", t.body)
        used = set() if t.kind == "enum" else set(IDENT.findall(code)) & names
        t.deps = used - {t.name} - set(t.assoc) - {t.base_class} - set(t.interfaces) - own
```

- [ ] **Step 4: Run tests and regenerate**

Run:
```bash
python tools/uml/test_gen_uml.py
python tools/uml/gen_uml.py
grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
```
Expected: `Ran 9 tests … OK`; generator line printed; `Layer violations — 50`. If the number is not 50, list the rows that changed against the old `docs/uml/metrics.md` (`git diff docs/uml/metrics.md`) in the report, and check that each removed row is a false positive (a `.Member` access, an enum value, or a same-named member).

- [ ] **Step 5: Hand-off**

Suggested commit: "fix(uml): ignore member-access and enum names in dependency count". Do not commit.

---

### Task 2: Move spawners and QuestTrackerUI to their layer

**Files (moves only, paths relative to `Assets/Project_Ocean/Scripts/`):**
| From | To |
|---|---|
| `Core/Save/DayReset/EnemySpawner.cs` | `Gameplay/Spawning/EnemySpawner.cs` |
| `Core/Save/DayReset/ItemSpawner.cs` | `Gameplay/Spawning/ItemSpawner.cs` |
| `Core/Save/DayReset/GameObjectSpawnManager.cs` | `Gameplay/Spawning/GameObjectSpawnManager.cs` |
| `Gameplay/QuestSystems/Quest/QuestTrackerUI.cs` | `UI/Quest/QuestTrackerUI.cs` |

**Interfaces:**
- Consumes: Task 1 (accurate metric).
- Produces: folder `Gameplay/Spawning/`.

- [ ] **Step 1: Snapshot GUIDs**

Run: `python tools/refactor/guid_snapshot.py snapshot "$TEMP/phase3_before.json"`
Expected: `wrote …`.

- [ ] **Step 2: Move in Unity**

Unity MCP `execute_code`:
```csharp
const string R = "Assets/Project_Ocean/Scripts/";
var log = new System.Collections.Generic.List<string>();
log.Add("mkdir Gameplay/Spawning: " + UnityEditor.AssetDatabase.CreateFolder(R + "Gameplay", "Spawning"));
var moves = new string[,]
{
    { "Core/Save/DayReset/EnemySpawner.cs",           "Gameplay/Spawning/EnemySpawner.cs" },
    { "Core/Save/DayReset/ItemSpawner.cs",            "Gameplay/Spawning/ItemSpawner.cs" },
    { "Core/Save/DayReset/GameObjectSpawnManager.cs", "Gameplay/Spawning/GameObjectSpawnManager.cs" },
    { "Gameplay/QuestSystems/Quest/QuestTrackerUI.cs", "UI/Quest/QuestTrackerUI.cs" },
};
for (int i = 0; i < moves.GetLength(0); i++)
{
    string err = UnityEditor.AssetDatabase.MoveAsset(R + moves[i, 0], R + moves[i, 1]);
    log.Add(moves[i, 0] + " -> " + moves[i, 1] + ": " + (string.IsNullOrEmpty(err) ? "ok" : err));
    if (!string.IsNullOrEmpty(err)) break;
}
return string.Join("\n", log);
```
Expected: one folder GUID, then 4 lines ending in `ok`.

- [ ] **Step 3: Compile, test, GUID check, metric**

Unity MCP: `refresh_unity` (force, compile request, wait). `read_console` types `["error"]` filter `error CS`: 0 entries. `run_tests` EditMode: 9/9.
```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/phase3_after2.json"
python tools/refactor/guid_snapshot.py compare "$TEMP/phase3_before.json" "$TEMP/phase3_after2.json"
python tools/uml/gen_uml.py
grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
```
Expected: 4 `moved`, `0 missing`, exit 0; `Layer violations — 45`. If the count differs, list the added and removed rows in the report.

- [ ] **Step 4: Hand-off**

Suggested commit: "refactor: move spawners to Gameplay and QuestTrackerUI to UI". Do not commit.

---

### Task 3: LoadingManager to Core behind ILoadingVisual

**Files:**
- Create: `Assets/Project_Ocean/Scripts/Core/Scenes/ILoadingVisual.cs`
- Modify: `Assets/Project_Ocean/Scripts/UI/Loading/LoadingVisualController/LoadingVisualController.cs` (class declaration line)
- Modify: `Assets/Project_Ocean/Scripts/UI/Loading/LoadingManager.cs` (field at lines 45–46, call at lines 216–219)
- Move: `UI/Loading/LoadingManager.cs` → `Core/Scenes/LoadingManager.cs`

**Interfaces:**
- Produces: `public interface ILoadingVisual { void ApplyProfile(LoadingType type); }` (Core). `LoadingVisualController : MonoBehaviour, ILoadingVisual`. `LoadingManager`'s public API is unchanged.

- [ ] **Step 1: Record the current scene reference (baseline for Step 5)**

Enter Play mode from `Assets/Project_Ocean/Levels/Scenes/Loading/Bootstrap.unity` (set `Application.runInBackground = true`, see memory `feedback_unity_play_mode_unfocused.md`). Wait until `LoadingManager.Instance` exists, then `execute_code`:
```csharp
var lm = Game.Testing.LoadingManager.Instance;
if (lm == null) return "LoadingManager.Instance is null";
var f = typeof(Game.Testing.LoadingManager).GetField("loadingVisualController",
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
var v = f.GetValue(lm) as UnityEngine.Object;
return "scene=" + lm.gameObject.scene.name + " go=" + lm.gameObject.name + " visual=" + (v == null ? "NULL" : v.name + " (" + v.GetType().Name + ")");
```
Record the output, then exit Play mode. Expected: `visual=<name> (LoadingVisualController)`. If it says `NULL`, the field was never wired: note it and continue, because the refactor cannot lose a reference that doesn't exist.

- [ ] **Step 2: Create `Core/Scenes/ILoadingVisual.cs`**

```csharp
namespace Game.Testing
{
    /// <summary>Loading-screen visual that LoadingManager (Core) drives without referencing UI types.</summary>
    public interface ILoadingVisual
    {
        void ApplyProfile(LoadingType type);
    }
}
```

- [ ] **Step 3: Implement the interface in `LoadingVisualController.cs`**

Replace:
```csharp
    public class LoadingVisualController : MonoBehaviour
```
with:
```csharp
    public class LoadingVisualController : MonoBehaviour, ILoadingVisual
```

- [ ] **Step 4: Retype the field in `LoadingManager.cs`, keeping its name**

Replace:
```csharp
        [SerializeField]
        private LoadingVisualController loadingVisualController;
```
with:
```csharp
        // Typed as MonoBehaviour so Core doesn't reference the UI class; same name keeps the scene reference.
        [SerializeField]
        private MonoBehaviour loadingVisualController;
```
Replace:
```csharp
            if (loadingVisualController != null)
            {
                loadingVisualController.ApplyProfile(loadingType);
            }
```
with:
```csharp
            if (loadingVisualController is ILoadingVisual visual)
            {
                visual.ApplyProfile(loadingType);
            }
```

- [ ] **Step 5: Compile, then check the reference survived**

Unity MCP: `refresh_unity` (force, compile request, wait). `read_console` `error CS`: 0.
Repeat the Step 1 Play-mode check. Expected: exactly the same `scene`, `go` and `visual` output as the Step 1 baseline. Exit Play mode.

- [ ] **Step 6: Move `LoadingManager.cs` to Core**

Unity MCP `execute_code`:
```csharp
return UnityEditor.AssetDatabase.MoveAsset(
    "Assets/Project_Ocean/Scripts/UI/Loading/LoadingManager.cs",
    "Assets/Project_Ocean/Scripts/Core/Scenes/LoadingManager.cs");
```
Expected: empty string (success).

- [ ] **Step 7: Compile, test, GUID check, metric**

Unity MCP: `refresh_unity` (force, compile request, wait). `read_console` `error CS`: 0. `run_tests` EditMode: 9/9.
```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/phase3_after3.json"
python tools/refactor/guid_snapshot.py compare "$TEMP/phase3_before.json" "$TEMP/phase3_after3.json"
python tools/uml/gen_uml.py
grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
grep -E "\| LoadingManager \|" docs/uml/metrics.md | head
```
Expected: 5 `moved` (Task 2's 4 + LoadingManager), `0 missing`; `Layer violations` about 37 (the simulation predicted 38 − 1). No violation row has `LoadingManager` as the From type.

- [ ] **Step 8: Hand-off**

Suggested commit: "refactor: move LoadingManager to Core behind ILoadingVisual". Do not commit.

---

### Task 4: Verify live and update docs

**Files:**
- Regenerate: `docs/uml/**`
- Modify: `docs/uml/findings.md`

- [ ] **Step 1: Play-mode smoke run**

Back up the save folder first, and restore it and delete any test slot afterwards (same approach as Phase 1/2 Task 5). Route: Bootstrap → MainMenu → New Game → Land_Map → Ocean_Map (`LoadingManager.Instance.LoadScene(<ocean scene>, LoadingType.Ocean)`) → back to Land (`LoadingType.Land`) → save → load.
During each load, run `execute_code` to read the loading icon: find the active `LoadingVisualController` and return its `loadingImage.sprite?.name` via reflection. Expected: the sprite name changes to match the profile for each `LoadingType`, and the console has no `No Loading Profile found` warning.
Also confirm enemies and items still spawn in Ocean_Map, since the spawners moved: `Object.FindObjectsByType<Game.Testing.EnemySpawner>(FindObjectsSortMode.None).Length > 0`, plus at least one active `Enemy` or `ItemDrop` after the zone check has run.
Afterwards run `read_console` and classify each error: caused by Phase 3 (a moved or edited type in its stack) or pre-existing (the URP "Missing types" message and the "DontDestroyOnLoad only works for root GameObjects" messages were already there). Exit Play mode.

- [ ] **Step 2: Missing-script and dirty checks**

Run the prefab missing-script scan snippet (in `docs/superpowers/plans/2026-09-26-refactor-phase2-layers.md`, Task 1 Step 3). Expected: `0 missing`. Run `git status --short -- "*.unity" "*.prefab"` and report the output.

- [ ] **Step 3: Regenerate everything**

Run:
```bash
python tools/uml/gen_uml.py
python tools/uml/test_gen_uml.py
graphify update .
```

- [ ] **Step 4: Update `docs/uml/findings.md`**

- In §1's table, change the "Wrong-way layer links" row's "Now" cell to the new count and append ` (after Phase 3a/3b; the metric no longer counts member names)`.
- Under `### Phase 3`, add these lines directly below the heading (fill in the measured numbers):
```markdown
**Progress:** 3a + 3b done 2026-09-26. Metric fixed (61 → 50 real), then 50 → <new count>: spawners → `Gameplay/Spawning`, `QuestTrackerUI` → `UI/Quest`, `LoadingManager` → `Core/Scenes` via `ILoadingVisual`.
```
- Delete the Phase 3 bullet that starts with "`LoadingManager` → Core:" (it is done).
- In §5 Risks, change the "Static-parser blind spots" example so it no longer cites `SceneNames.MainMenu` / `Mode` (fixed). Replace it with `(e.g. a local variable named like a type still counts)`.

- [ ] **Step 5: Hand-off**

Report: the count after each task, the smoke-run results (sprite names per `LoadingType`, spawn check), the dirty check. Suggested commit: "docs: refresh UML after Phase 3a/3b". Do not commit.
