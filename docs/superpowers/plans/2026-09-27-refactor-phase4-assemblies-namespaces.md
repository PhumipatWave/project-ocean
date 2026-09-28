# Refactor Phase 4: Assemblies and Namespaces — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The compiler enforces the layering reached in Phase 3: `Game.Core` ← `Game.Gameplay` ← `Game.UI`, plus editor-only `Game.Editor` and `Game.Deprecated`. The misleading `namespace Game.Testing` becomes `Game.Core` / `Game.Gameplay` / `Game.UI` / `Game.Editor` / `Game.Deprecated`, matching each folder.

**Architecture:**
- **Step A, assemblies first, namespaces unchanged.** One namespace may span several assemblies, so this isolates "what really crosses layers". The static UML parser reported 0 violations, but the compiler is the real judge.
  - Existing asmdef files are *moved/renamed* with `AssetDatabase` to keep their GUIDs: `Scripts/Core/Inputs/Game.Core.asmdef` → `Scripts/Core/Game.Core.asmdef`, and `Scripts/Game.Testing.asmdef` → `Scripts/Gameplay/Game.Gameplay.asmdef` (renamed inside).
  - New asmdefs are created for UI, Editor and Deprecated.
- **Step B, namespaces:** mechanical rename per folder, plus the `using` lines each layer needs.
- **Checks:** script GUIDs are untouched, so scenes, prefabs and ScriptableObject assets keep their links. A player-script compile (`PlayerBuildInterface.CompilePlayerScripts`) proves editor-only code is out of the build.

**Tech Stack:** Unity 6000.3.6f1 + Unity MCP, asmdef JSON, Python (UML tools).

## Global Constraints

- Spec: `docs/uml/findings.md` §4 Phase 4; the user asked for it on 2026-09-27 after the three bug fixes.
- Behaviour identical. No scene/prefab edits (`m_EditorClassIdentifier` strings in scenes are editor metadata and may go stale; do not touch them).
- `Assets/Project_Ocean/Scripts/Core/Inputs/InputSystem_Actions.cs` is generated. Do not edit it; it stays in the global namespace, inside `Game.Core`.
- asmdef moves via `AssetDatabase.MoveAsset` (keeps GUIDs); new asmdefs via the Write tool.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`; retry once on a transient timeout.
- If a compile error shows a real Core→Gameplay/UI or Gameplay→UI dependency the UML parser missed, fix it the Phase 3 way: move the type to the right layer, or add an interface/event in the lower layer. Use the smallest change, record it in the report, and don't add unrelated changes.

## Package assembly GUIDs (resolved from Library/PackageCache)

| Assembly | GUID |
|---|---|
| Unity.InputSystem | `75469ad4d38634e559750d17036d5f7c` |
| Unity.TextMeshPro | `6055be8ebefd69e48b49212b09b47b2f` |
| UnityEngine.UI | `2bafac87e7f4b9b418d9448d219b01ab` |
| Unity.Cinemachine | `4307f53044263cf4b835bd812fc161a4` |
| Unity.Timeline | `f06555f75b070af458a003d92f9efb00` |
| Game.Core (existing file, moved) | `066058f87c9e21a4f9b6f2271994db58` |

Use name references (`"Game.Core"`, `"Game.Gameplay"`) for the project assemblies in the new asmdefs; that's readable and what the test asmdef already does. Keep package references by GUID, as today.

---

### Task 1: Split into assemblies (namespaces unchanged)

**Files:**
- Move: `Assets/Project_Ocean/Scripts/Core/Inputs/Game.Core.asmdef` → `Assets/Project_Ocean/Scripts/Core/Game.Core.asmdef`
- Move: `Assets/Project_Ocean/Scripts/Game.Testing.asmdef` → `Assets/Project_Ocean/Scripts/Gameplay/Game.Gameplay.asmdef`
- Create: `Assets/Project_Ocean/Scripts/UI/Game.UI.asmdef`, `Assets/Project_Ocean/Scripts/Editor/Game.Editor.asmdef`, `Assets/Project_Ocean/Scripts/Deprecated/Game.Deprecated.asmdef`
- Modify: `Assets/Project_Ocean/Tests/EditMode/Game.Tests.EditMode.asmdef`

- [ ] **Step 1: Baseline**

Unity MCP: `refresh_unity`, `read_console` `error CS` = 0, `run_tests` EditMode, and record the count (23 after the bug fixes).
Run: `python tools/refactor/guid_snapshot.py snapshot "$TEMP/p4_before.json"`.

- [ ] **Step 2: Move the two existing asmdefs**

`execute_code`:
```csharp
string a = UnityEditor.AssetDatabase.MoveAsset("Assets/Project_Ocean/Scripts/Core/Inputs/Game.Core.asmdef", "Assets/Project_Ocean/Scripts/Core/Game.Core.asmdef");
string b = UnityEditor.AssetDatabase.MoveAsset("Assets/Project_Ocean/Scripts/Game.Testing.asmdef", "Assets/Project_Ocean/Scripts/Gameplay/Game.Gameplay.asmdef");
return "core=" + (string.IsNullOrEmpty(a) ? "ok" : a) + " gameplay=" + (string.IsNullOrEmpty(b) ? "ok" : b);
```
Expected: `core=ok gameplay=ok`. Don't compile yet (Step 3 rewrites the contents first). If Unity auto-compiles and errors appear, that's expected until Step 4.

- [ ] **Step 3: Write the asmdef contents**

`Assets/Project_Ocean/Scripts/Core/Game.Core.asmdef` (overwrite):
```json
{
    "name": "Game.Core",
    "rootNamespace": "Game.Core",
    "references": [
        "GUID:75469ad4d38634e559750d17036d5f7c",
        "GUID:6055be8ebefd69e48b49212b09b47b2f",
        "GUID:2bafac87e7f4b9b418d9448d219b01ab"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```
`Assets/Project_Ocean/Scripts/Gameplay/Game.Gameplay.asmdef` (overwrite):
```json
{
    "name": "Game.Gameplay",
    "rootNamespace": "Game.Gameplay",
    "references": [
        "Game.Core",
        "GUID:75469ad4d38634e559750d17036d5f7c",
        "GUID:6055be8ebefd69e48b49212b09b47b2f",
        "GUID:2bafac87e7f4b9b418d9448d219b01ab",
        "GUID:4307f53044263cf4b835bd812fc161a4",
        "GUID:f06555f75b070af458a003d92f9efb00"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```
`Assets/Project_Ocean/Scripts/UI/Game.UI.asmdef`:
```json
{
    "name": "Game.UI",
    "rootNamespace": "Game.UI",
    "references": [
        "Game.Core",
        "Game.Gameplay",
        "GUID:75469ad4d38634e559750d17036d5f7c",
        "GUID:6055be8ebefd69e48b49212b09b47b2f",
        "GUID:2bafac87e7f4b9b418d9448d219b01ab",
        "GUID:4307f53044263cf4b835bd812fc161a4",
        "GUID:f06555f75b070af458a003d92f9efb00"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```
`Assets/Project_Ocean/Scripts/Editor/Game.Editor.asmdef`:
```json
{
    "name": "Game.Editor",
    "rootNamespace": "Game.Editor",
    "references": [
        "Game.Core",
        "Game.Gameplay",
        "Game.UI"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```
`Assets/Project_Ocean/Scripts/Deprecated/Game.Deprecated.asmdef`:
```json
{
    "name": "Game.Deprecated",
    "rootNamespace": "Game.Deprecated",
    "references": [
        "Game.Core",
        "Game.Gameplay",
        "Game.UI",
        "GUID:75469ad4d38634e559750d17036d5f7c",
        "GUID:6055be8ebefd69e48b49212b09b47b2f",
        "GUID:2bafac87e7f4b9b418d9448d219b01ab"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```
In `Assets/Project_Ocean/Tests/EditMode/Game.Tests.EditMode.asmdef`, replace the reference `"Game.Testing",` with `"Game.Core",`, `"Game.Gameplay",` and `"Game.UI",` (three lines). Keep everything else.

- [ ] **Step 4: Compile and fix real cross-layer references**

`refresh_unity` (force, compile request, wait), then `read_console` `error CS` (page through all of them). Group the errors by missing type and by referencing file/assembly:
- **"type or namespace not found" for a package type** (e.g. `NavMeshSurface`, `Light2D`, `UnityEngine.Rendering.Universal`): add that package's asmdef GUID to the referencing asmdef. Find the GUID in `Library/PackageCache/**/<Name>.asmdef.meta` and list it in the report.
- **A Core file uses a Gameplay/UI type, or a Gameplay file uses a UI type:** a real hidden violation. Fix it the Phase 3 way, with the smallest change (move the misplaced type, or add an interface/event in the lower layer). Report each one: file:line, type, fix.
- **A type in the global namespace inside an assembly that others can't see:** same treatment (reference or move).
Repeat until there are 0 `error CS`. Then `run_tests` EditMode: same count as the baseline, all passing.

- [ ] **Step 5: Player-script compile (editor code excluded from builds)**

`execute_code`:
```csharp
var settings = new UnityEditor.Build.Player.ScriptCompilationSettings
{
    group = UnityEditor.BuildTargetGroup.Standalone,
    target = UnityEditor.BuildTarget.StandaloneWindows64,
    options = UnityEditor.Build.Player.ScriptCompilationOptions.None,
};
var outDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "p4_playercompile");
var result = UnityEditor.Build.Player.PlayerBuildInterface.CompilePlayerScripts(settings, outDir);
return "assemblies=" + string.Join(",", result.assemblies);
```
Expected: the list contains `Game.Core.dll`, `Game.Gameplay.dll` and `Game.UI.dll`, and does NOT contain `Game.Editor.dll`, `Game.Deprecated.dll` or `Game.Tests.EditMode.dll`. Then `read_console` for errors from this compile: 0. If this API is blocked or unavailable, report UNVERIFIED with the reason.

- [ ] **Step 6: GUIDs unchanged**

```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/p4_after1.json"
python tools/refactor/guid_snapshot.py compare "$TEMP/p4_before.json" "$TEMP/p4_after1.json"
```
Expected: 2 moved (the two asmdefs), 0 missing, new = the 3 new asmdefs plus metas (plus any files moved in Step 4, which must be listed).

---

### Task 2: Rename namespaces per folder

**Files:** every `.cs` under `Assets/Project_Ocean/Scripts` except `Core/Inputs/InputSystem_Actions.cs`, plus `Assets/Project_Ocean/Tests/EditMode/*.cs`.

- [ ] **Step 1: Mechanical rename script**

Save as `$TEMP/p4_ns.py` and run from the project root:
```python
import re
from pathlib import Path

S = Path("Assets/Project_Ocean/Scripts")
LAYER_NS = {"Core": "Game.Core", "Gameplay": "Game.Gameplay", "UI": "Game.UI", "Editor": "Game.Editor", "Deprecated": "Game.Deprecated"}
NEEDS = {
    "Game.Core": [],
    "Game.Gameplay": ["Game.Core"],
    "Game.UI": ["Game.Core", "Game.Gameplay"],
    "Game.Editor": ["Game.Core", "Game.Gameplay", "Game.UI"],
    "Game.Deprecated": ["Game.Core", "Game.Gameplay", "Game.UI"],
}
changed = []
for f in S.rglob("*.cs"):
    if f.name == "InputSystem_Actions.cs":
        continue
    layer = f.relative_to(S).parts[0]
    ns = LAYER_NS.get(layer)
    if ns is None:
        continue
    raw = f.read_bytes()
    bom = raw.startswith(b"\xef\xbb\xbf")
    text = raw.decode("utf-8-sig")
    nl = "\r\n" if "\r\n" in text else "\n"
    new = re.sub(r"^(\s*)namespace Game\.(Testing|Core)\b", lambda m: m.group(1) + "namespace " + ns, text, flags=re.M)
    new = re.sub(r"^using Game\.Testing;\s*$\r?\n?", "", new, flags=re.M)
    has_ns = re.search(r"^\s*namespace ", new, flags=re.M) is not None
    wanted = [u for u in NEEDS[ns] if not re.search(rf"^using {re.escape(u)};", new, flags=re.M)]
    if wanted:
        block = "".join(f"using {u};{nl}" for u in wanted)
        m = list(re.finditer(r"^using [^\n]+;\r?$", new, flags=re.M))
        if m:
            pos = m[-1].end() + len(nl)
            new = new[:pos] + block + new[pos:]
        else:
            new = block + (nl if has_ns else "") + new
    if new != text:
        f.write_bytes((b"\xef\xbb\xbf" if bom else b"") + new.encode("utf-8"))
        changed.append(str(f))
print(len(changed), "files changed")
```
Files with no namespace (global, e.g. `AudioManager.cs`, `IntroPlayer.cs`, `MoveKeyBindUI.cs`, `ButtonHoverScale.cs`) get only the `using` lines here. Step 2 wraps them.

- [ ] **Step 2: Put the global-namespace scripts into their layer namespace**

For each file under `Assets/Project_Ocean/Scripts` (except `InputSystem_Actions.cs`) that still has no `namespace` line (`grep -rLE "^\s*namespace " --include=*.cs Assets/Project_Ocean/Scripts`), wrap everything after the `using` block in `namespace <LayerNs>` `{ … }` and indent the body by 4 spaces. Do it with the Edit tool per file, so formatting stays exact. Expected list: `Core/Inputs/KeyBind/UIKeyBind/MoveKeyBindUI.cs`, `Core/VideoManager/IntroPlayer.cs`, `Gameplay/AudioSetting/AudioManager.cs`, `AudioSliderBinder.cs`, `SoundChannel.cs`, `SoundKeys.cs`, `VolumeSliderUI.cs`, `Gameplay/Entities/Players/PlayerSprintVFX.cs`, `Gameplay/InteractObjects/ObjectWrapPoint.cs`, `UI/ButtonHoverScale.cs`, `UI/UIMenuSetting/SettingTabManager.cs`. Unity links MonoBehaviours by script GUID, so this is scene-safe.

- [ ] **Step 3: Tests**

In each `Assets/Project_Ocean/Tests/EditMode/*.cs`, replace `using Game.Testing;` with the three lines `using Game.Core;`, `using Game.Gameplay;` and `using Game.UI;`. Keep `namespace Game.Tests`.

- [ ] **Step 4: Compile and fix**

`refresh_unity` (force, compile request, wait) and `read_console` `error CS`. Typical leftovers to fix:
- a file needs `using Game.UI;`: an Editor or Deprecated file, or a `UnityEngine.UI` ambiguity;
- an `Image`/`Text` name conflict in `Game.UI` files. Qualify the name (`UnityEngine.UI.Image`), or change `using UnityEngine.UI;` to an alias only in that file;
- a cref or string that mentions `Game.Testing`. Update it.
List every manual fix. Then `run_tests` EditMode: all pass, same count.
```bash
grep -rn "Game\.Testing" --include=*.cs --include=*.asmdef Assets/Project_Ocean | head
```
Expected: no output.

- [ ] **Step 5: Player-script compile again**

Re-run the Task 1 Step 5 snippet. Expected: the same three runtime assemblies and 0 errors.

---

### Task 3: Verify live, measure, docs

- [ ] **Step 1: Missing-script scan**

Run the prefab missing-script scan snippet (in `docs/superpowers/plans/2026-09-26-refactor-phase2-layers.md`, Task 1 Step 3). Also scan every scene under `Assets/Project_Ocean/Levels/Scenes`: open each additively, count `GameObjectUtility.GetMonoBehavioursWithMissingScriptCount` over all objects, close without saving, then restore the original scene setup. Expected: 0 missing (as the Phase 2/3 baselines).

- [ ] **Step 2: Play smoke run**

Back up the save folder `C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean` first. Play from Bootstrap: MainMenu → New Game → Land_Map (talk to an NPC, open the quest board) → Ocean_Map (enemies spawn with runtime unlocks, pick up an item) → back to Land → save → load. `read_console`: no `MissingReferenceException`, no "The referenced script (Unknown) on this Behaviour is missing", and no errors from moved types. Exit Play, restore saves byte-identical (RESTORED), and confirm `git status --short -- "*.unity" "*.prefab"` shows only the files from earlier fixes.

- [ ] **Step 3: Test that a wrong-way reference now fails to compile**

Temporarily create `Assets/Project_Ocean/Scripts/Core/__LayerProbe.cs` with:
```csharp
namespace Game.Core { internal static class LayerProbe { static object X => typeof(Game.Gameplay.GameManager); } }
```
`refresh_unity` + `read_console`. Expected: `CS0234` or `CS0246` (Gameplay not visible from Core). Then delete the probe file and its `.meta` via `AssetDatabase.DeleteAsset("Assets/Project_Ocean/Scripts/Core/__LayerProbe.cs")`, recompile, and check for 0 errors.

- [ ] **Step 4: Docs and graph**

```bash
python tools/uml/gen_uml.py && python tools/uml/test_gen_uml.py
graphify update .
```
In `docs/uml/findings.md`, under `### Phase 4`, add below the heading: `**Status:** done <date>. Assemblies Game.Core ← Game.Gameplay ← Game.UI (+ editor-only Game.Editor, Game.Deprecated); namespaces match folders; a Core→Gameplay reference fails to compile (probe verified).` Also add any hidden violations fixed in Task 1 Step 4 as one line.

- [ ] **Step 5: Hand-off**

Suggested commits: "build: split scripts into Game.Core/Gameplay/UI assemblies" (asmdefs + any hidden-violation fixes) and "refactor: namespaces match layers" (the rename). Do not commit.
