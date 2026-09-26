# Refactor Phase 2: Put Types in the Right Layer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move misplaced types into the layer they belong to (Core / Gameplay / UI), so wrong-way layer links drop from today's 111, without changing behaviour.

**Architecture:** Two kinds of change only. (1) Cut embedded types out of shared files into their own files: pure text moves, no logic edits. (2) Move script files between folders with `AssetDatabase.MoveAsset` so `.meta` GUIDs are kept. Namespaces stay `Game.Testing` and everything stays in the one `Game.Testing` assembly, so the compiler sees no difference. Progress is measured with the UML generator's "Layer violations" count.

**Tech Stack:** Unity 6000.3.6f1, Unity MCP (`execute_code`, `refresh_unity`, `run_tests`, `read_console`), NUnit EditMode tests, Python 3.12 (`tools/uml/gen_uml.py`, `tools/refactor/guid_snapshot.py`).

## Global Constraints

- Spec: `docs/uml/findings.md` §4 Phase 2 ("Only folder moves; no logic changes"), with the deviations listed below.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore` or `git mv`. Each "Hand-off" step leaves changes in the working tree for the user.
- Existing files move only via `UnityEditor.AssetDatabase.MoveAsset` / `CreateFolder` / `DeleteAsset` (Unity MCP `execute_code`), never a raw file-system mv/rm. If `execute_code` safety checks block `DeleteAsset` or `MoveAsset`, re-run the identical code with `safety_checks=false` and disclose it in the report.
- New files (extracted types) are created with the Write tool. Unity generates their `.meta` on refresh. Plain C# types are linked by name, not GUID, so a new GUID is safe for them.
- No namespace, asmdef, or logic changes. Every extracted block is copied character for character, comments included.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111` before any Unity call. A transient bridge timeout right after it is known; retry once.
- After any Play-mode run: `git status -- "*.unity" "*.prefab"` must show no changes. If Unity re-saved a scene or prefab, report it and do not restore it yourself.
- Scene/prefab edits and the Bootstrap scene's managers are out of scope.

**Deviations from findings.md §4 Phase 2 (found while planning, because the move would create new wrong-way links):**
- `LoadingManager` stays in `UI/Loading`: it holds a `LoadingVisualController` (UI). It moves in Phase 3, once it talks to the view through an event. Only `LoadingType` moves now.
- `TutorialGate` and `TutorialStep` stay in Gameplay: they call `TutorialPromptUI` and `QuestManager`. Only `TutorialActionId` + `TutorialEvents` move to Core.
- `PlayerFeedbackVFX` stays in UI: it fades screen-space `Image` vignettes. Only `PlayerSprintVFX` (a `ParticleSystem`) moves to Gameplay.

## File map

All script paths are relative to `Assets/Project_Ocean/Scripts/`.

| Change | From | To |
|---|---|---|
| Extract `TutorialGate` + `TutorialStep` | `Gameplay/QuestSystems/Quest/TutorialEvents.cs` | new `Gameplay/QuestSystems/Quest/TutorialGate.cs` |
| Extract `EnemyEvents` | `Gameplay/Entities/Enemies/Enemy.cs` | new `Core/Events/EnemyEvents.cs` |
| Extract `NpcEvents` | `Gameplay/Entities/NPCs/NPC.cs` | new `Core/Events/NpcEvents.cs` |
| Extract `LoadingType` | `UI/Loading/LoadingVisualController/LoadingVisualController.cs` | new `Core/Scenes/LoadingType.cs` |
| Move | `UI/Items/Item.cs` (`Item`, `ItemType`) | `Core/Datas/Items/Item.cs` |
| Move | `UI/Inventory/InventoryItemDatabase.cs` | `Core/Datas/Items/InventoryItemDatabase.cs` |
| Move | `UI/Upgrades/UpgradeData.cs` (`UpgradeData`, `UpgradeType`, `UpgradeUnlock`) | `Core/Datas/Upgrades/UpgradeData.cs` |
| Move | `UI/NotificationManager.cs` | `Core/Events/NotificationManager.cs` |
| Move | `UI/Notifications/NotificationMessage.cs` | `Core/Events/NotificationMessage.cs` |
| Move | `UI/Notifications/NotificationChannel.cs` | `Core/Events/NotificationChannel.cs` |
| Move | `Gameplay/QuestSystems/Quest/TutorialEvents.cs` (after extraction) | `Core/Events/TutorialEvents.cs` |
| Move | `Gameplay/Cutscenes/CutsceneEvents.cs` | `Core/Events/CutsceneEvents.cs` |
| Move | `Gameplay/QuestSystems/Quest/ExploreEvents.cs` | `Core/Events/ExploreEvents.cs` |
| Move | `Core/Save/SceneLoadContext.cs` | `Core/Scenes/SceneLoadContext.cs` |
| Move | `Core/Interfaces/IEnemyBrain.cs`, `IEnemyState.cs`, `IBrainAttack.cs`, `IBrainVision.cs`, `IBrainHitReaction.cs` | `Gameplay/Entities/Enemies/Brain/Interfaces/` |
| Move | `Core/Datas/ScriptableDatas/Quests/QuestBoard/QuestBoardUI.cs`, `QuestSlot.cs`, `QuestDetailPanel.cs`, `QuestCategorySlot.cs` (the `QB_*.asset` files stay) | `UI/Quest/` |
| Move | `UI/Items/ItemDrop.cs` | `Gameplay/InteractObjects/ItemDrop.cs` |
| Move | `UI/VFX/PlayerSprintVFX.cs` | `Gameplay/Entities/Players/PlayerSprintVFX.cs` |
| Delete (empty after moves) | `Core/Interfaces/`, `UI/Items/` | — |

---

### Task 1: Baseline

**Files:** none changed.

**Interfaces:**
- Produces: `$TEMP/phase2_before.json` (GUID snapshot), baseline numbers in the report: layer violations, EditMode pass count, prefab missing-script count.

- [ ] **Step 1: Confirm a clean start**

Run (project root, Git Bash):
```bash
git status --short -- Assets/Project_Ocean | grep -v "^??" | head
```
Expected: no output (Phase 1 is committed). If output appears, list it in the report and continue. Those changes are the user's, so do not touch them.

- [ ] **Step 2: Snapshot GUIDs and metrics**

Run:
```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/phase2_before.json"
python tools/uml/gen_uml.py
grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
```
Expected: `wrote …`, generator line `200 files, 253 types, 56 modules …`, then `Layer violations — 111`.

- [ ] **Step 3: Test and missing-script baseline**

Unity MCP: `refresh_unity` (compile request, wait), `run_tests` EditMode (whole suite), poll `get_test_job`. Expected: 9/9 pass.
Then `execute_code`:
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
Expected: `0 missing`. Record the result as the baseline.

---

### Task 2: Extract embedded types into their own files

**Files:**
- Create: `Gameplay/QuestSystems/Quest/TutorialGate.cs`, `Core/Events/EnemyEvents.cs`, `Core/Events/NpcEvents.cs`, `Core/Scenes/LoadingType.cs`
- Modify: `Gameplay/QuestSystems/Quest/TutorialEvents.cs`, `Gameplay/Entities/Enemies/Enemy.cs`, `Gameplay/Entities/NPCs/NPC.cs`, `UI/Loading/LoadingVisualController/LoadingVisualController.cs`

**Interfaces:**
- Consumes: Task 1 baseline.
- Produces: same type names, same members, new file locations. `Core/Events/` and `Core/Scenes/` folders exist after this task (Task 3 moves more files into them).

- [ ] **Step 1: Create the two new folders in Unity**

Unity MCP `execute_code`:
```csharp
string a = UnityEditor.AssetDatabase.CreateFolder("Assets/Project_Ocean/Scripts/Core", "Events");
string b = UnityEditor.AssetDatabase.CreateFolder("Assets/Project_Ocean/Scripts/Core", "Scenes");
return "Events guid=" + a + "\nScenes guid=" + b;
```
Expected: two non-empty GUIDs.

- [ ] **Step 2: Create `Core/Events/EnemyEvents.cs` and remove the block from `Enemy.cs`**

New file `Assets/Project_Ocean/Scripts/Core/Events/EnemyEvents.cs`:
```csharp
namespace Game.Testing
{
    public static class EnemyEvents
    {
        /// <summary>Raised once when an enemy dies. Payload = the enemy's id (Enemy.EnemyId).</summary>
        public static event System.Action<string> OnEnemyKilled;

        public static void RaiseEnemyKilled(string enemyId)
        {
            OnEnemyKilled?.Invoke(enemyId);
        }
    }
}
```
In `Gameplay/Entities/Enemies/Enemy.cs`, delete exactly these lines (currently lines 6–16: the `EnemyEvents` class and the blank line after it), so `public class Enemy : Entity` directly follows `{`:
```csharp
    public static class EnemyEvents
    {
        /// <summary>Raised once when an enemy dies. Payload = the enemy's id (Enemy.EnemyId).</summary>
        public static event System.Action<string> OnEnemyKilled;

        public static void RaiseEnemyKilled(string enemyId)
        {
            OnEnemyKilled?.Invoke(enemyId);
        }
    }

```

- [ ] **Step 3: Create `Core/Events/NpcEvents.cs` and remove the block from `NPC.cs`**

New file `Assets/Project_Ocean/Scripts/Core/Events/NpcEvents.cs`:
```csharp
namespace Game.Testing
{
    /// <summary>Raised whenever the player interacts with an NPC. Payload = that NPC's npcId.</summary>
    public static class NpcEvents
    {
        public static event System.Action<string> OnNpcTalkedTo;

        public static void RaiseNpcTalkedTo(string npcId)
        {
            OnNpcTalkedTo?.Invoke(npcId);
        }
    }
}
```
In `Gameplay/Entities/NPCs/NPC.cs`, delete exactly these lines (currently lines 7–17) so `public class NPC : MonoBehaviour` directly follows `{`:
```csharp
    /// <summary>Raised whenever the player interacts with an NPC. Payload = that NPC's npcId.</summary>
    public static class NpcEvents
    {
        public static event System.Action<string> OnNpcTalkedTo;

        public static void RaiseNpcTalkedTo(string npcId)
        {
            OnNpcTalkedTo?.Invoke(npcId);
        }
    }

```

- [ ] **Step 4: Create `Core/Scenes/LoadingType.cs` and remove the enum from `LoadingVisualController.cs`**

New file `Assets/Project_Ocean/Scripts/Core/Scenes/LoadingType.cs`:
```csharp
namespace Game.Testing
{
    public enum LoadingType
    {
        Menu,
        Land,
        Ocean
    }
}
```
In `UI/Loading/LoadingVisualController/LoadingVisualController.cs`, delete exactly these lines so `public class LoadingVisualController : MonoBehaviour` directly follows `{`:
```csharp
    public enum LoadingType
    {
        Menu,
        Land,
        Ocean
    }

```

- [ ] **Step 5: Split `TutorialEvents.cs`**

Create `Assets/Project_Ocean/Scripts/Gameplay/QuestSystems/Quest/TutorialGate.cs` with this header, then the two classes copied verbatim from `TutorialEvents.cs`, then the closing brace:
```csharp
namespace Game.Testing
{
```
- Copy block 1 verbatim: from the line `    /// <summary>` directly above `    /// While a Tutorial/Talk checkpoint is active, restricts the player to only the` through the closing `    }` of `public static class TutorialGate`.
- Add one blank line.
- Copy block 2 verbatim: from the line `    /// <summary>` directly above `    /// Reads the quest state directly, for UI inside blocking panels (shop, upgrade)` through the closing `    }` of `public static class TutorialStep`.
- End the file with:
```csharp
}
```
Then delete both blocks (and the blank line before each) from `TutorialEvents.cs`. It must end up containing only `using System;`, the namespace, `enum TutorialActionId` (with all its comments) and `static class TutorialEvents`.

- [ ] **Step 6: Verify the text move lost nothing**

Run (project root). This compares the non-blank lines of the four original files at `HEAD` with the non-blank lines of old + new files now:
```bash
S=Assets/Project_Ocean/Scripts
before=$(for f in Gameplay/QuestSystems/Quest/TutorialEvents.cs Gameplay/Entities/Enemies/Enemy.cs Gameplay/Entities/NPCs/NPC.cs UI/Loading/LoadingVisualController/LoadingVisualController.cs; do git show "HEAD:$S/$f"; done | tr -d '\r' | grep -v '^\s*$' | grep -vE '^\s*(namespace Game.Testing|\{|\})\s*$' | sort)
after=$(cat $S/Gameplay/QuestSystems/Quest/TutorialEvents.cs $S/Gameplay/QuestSystems/Quest/TutorialGate.cs $S/Gameplay/Entities/Enemies/Enemy.cs $S/Core/Events/EnemyEvents.cs $S/Gameplay/Entities/NPCs/NPC.cs $S/Core/Events/NpcEvents.cs $S/UI/Loading/LoadingVisualController/LoadingVisualController.cs $S/Core/Scenes/LoadingType.cs | tr -d '\r' | grep -v '^\s*$' | grep -vE '^\s*(namespace Game.Testing|\{|\})\s*$' | sort)
diff <(echo "$before") <(echo "$after") && echo "IDENTICAL CONTENT"
```
Expected: `IDENTICAL CONTENT`. Namespace lines and lone braces are ignored because each new file adds its own wrapper.

- [ ] **Step 7: Compile and test**

Unity MCP: `refresh_unity` (force, compile request, wait). `read_console` with types `["error"]` and filter `error CS`: expect 0 entries. `run_tests` EditMode: expect 9/9 pass. `ScriptFileNameMatchesADeclaredType` covers the four new files.

- [ ] **Step 8: Hand-off**

Report the files created and modified. Suggested commit: "refactor: extract embedded event/enum types into own files". Do not commit.

---

### Task 3: Move shared data and event types down to Core

**Files:** moves only (see File map rows "→ Core/…").

**Interfaces:**
- Consumes: Task 2 (`Core/Events`, `Core/Scenes` exist; `TutorialEvents.cs` holds only the enum + event class).
- Produces: `Core/Datas/Items/`, `Core/Datas/Upgrades/`, `Core/Events/` (8 files), `Core/Scenes/` (2 files).

- [ ] **Step 1: Move files in Unity**

Unity MCP `execute_code`:
```csharp
const string R = "Assets/Project_Ocean/Scripts/";
var log = new System.Collections.Generic.List<string>();
log.Add("mkdir Items: " + UnityEditor.AssetDatabase.CreateFolder(R + "Core/Datas", "Items"));
log.Add("mkdir Upgrades: " + UnityEditor.AssetDatabase.CreateFolder(R + "Core/Datas", "Upgrades"));
var moves = new string[,]
{
    { "UI/Items/Item.cs",                                "Core/Datas/Items/Item.cs" },
    { "UI/Inventory/InventoryItemDatabase.cs",           "Core/Datas/Items/InventoryItemDatabase.cs" },
    { "UI/Upgrades/UpgradeData.cs",                      "Core/Datas/Upgrades/UpgradeData.cs" },
    { "UI/NotificationManager.cs",                       "Core/Events/NotificationManager.cs" },
    { "UI/Notifications/NotificationMessage.cs",         "Core/Events/NotificationMessage.cs" },
    { "UI/Notifications/NotificationChannel.cs",         "Core/Events/NotificationChannel.cs" },
    { "Gameplay/QuestSystems/Quest/TutorialEvents.cs",   "Core/Events/TutorialEvents.cs" },
    { "Gameplay/Cutscenes/CutsceneEvents.cs",            "Core/Events/CutsceneEvents.cs" },
    { "Gameplay/QuestSystems/Quest/ExploreEvents.cs",    "Core/Events/ExploreEvents.cs" },
    { "Core/Save/SceneLoadContext.cs",                   "Core/Scenes/SceneLoadContext.cs" },
};
for (int i = 0; i < moves.GetLength(0); i++)
{
    string err = UnityEditor.AssetDatabase.MoveAsset(R + moves[i, 0], R + moves[i, 1]);
    log.Add(moves[i, 0] + " -> " + moves[i, 1] + ": " + (string.IsNullOrEmpty(err) ? "ok" : err));
    if (!string.IsNullOrEmpty(err)) break;
}
return string.Join("\n", log);
```
Expected: two folder GUIDs, then 10 lines ending in `ok`. On the first error, stop and report BLOCKED with the message.

- [ ] **Step 2: Compile, test, GUID check**

Unity MCP: `refresh_unity` (force, compile request, wait). `read_console` `error CS`: 0 entries. `run_tests` EditMode: 9/9.
Then:
```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/phase2_after3.json"
python tools/refactor/guid_snapshot.py compare "$TEMP/phase2_before.json" "$TEMP/phase2_after3.json"
```
Expected: 10 `moved` lines matching Step 1, `0 missing`, exit 0. (The files and folders created in Task 2 and here count as "new".)

- [ ] **Step 3: SO assets still resolve**

Unity MCP `execute_code`:
```csharp
int items = UnityEditor.AssetDatabase.FindAssets("t:Item").Length;
int upgrades = UnityEditor.AssetDatabase.FindAssets("t:UpgradeData").Length;
return "Item assets: " + items + ", UpgradeData assets: " + upgrades;
```
Expected: both counts above 0 (the `Item` and `UpgradeData` ScriptableObject assets still load their moved scripts). Record the numbers.

- [ ] **Step 4: Hand-off**

Suggested commit: "refactor: move shared data and event types to Core". Do not commit.

---

### Task 4: Move enemy interfaces, quest board UI and world objects to their layer

**Files:** moves and two folder deletions (File map rows → Gameplay / UI, plus the Delete row).

**Interfaces:**
- Consumes: Task 3 (UI/Items now holds only `ItemDrop.cs`).
- Produces: `Gameplay/Entities/Enemies/Brain/Interfaces/` (5 files), `UI/Quest/` (4 files); `Core/Interfaces/` and `UI/Items/` removed.

- [ ] **Step 1: Move files and delete emptied folders in Unity**

Unity MCP `execute_code`:
```csharp
const string R = "Assets/Project_Ocean/Scripts/";
var log = new System.Collections.Generic.List<string>();
log.Add("mkdir Brain/Interfaces: " + UnityEditor.AssetDatabase.CreateFolder(R + "Gameplay/Entities/Enemies/Brain", "Interfaces"));
log.Add("mkdir UI/Quest: " + UnityEditor.AssetDatabase.CreateFolder(R + "UI", "Quest"));
var moves = new string[,]
{
    { "Core/Interfaces/IEnemyBrain.cs",       "Gameplay/Entities/Enemies/Brain/Interfaces/IEnemyBrain.cs" },
    { "Core/Interfaces/IEnemyState.cs",       "Gameplay/Entities/Enemies/Brain/Interfaces/IEnemyState.cs" },
    { "Core/Interfaces/IBrainAttack.cs",      "Gameplay/Entities/Enemies/Brain/Interfaces/IBrainAttack.cs" },
    { "Core/Interfaces/IBrainVision.cs",      "Gameplay/Entities/Enemies/Brain/Interfaces/IBrainVision.cs" },
    { "Core/Interfaces/IBrainHitReaction.cs", "Gameplay/Entities/Enemies/Brain/Interfaces/IBrainHitReaction.cs" },
    { "Core/Datas/ScriptableDatas/Quests/QuestBoard/QuestBoardUI.cs",      "UI/Quest/QuestBoardUI.cs" },
    { "Core/Datas/ScriptableDatas/Quests/QuestBoard/QuestSlot.cs",         "UI/Quest/QuestSlot.cs" },
    { "Core/Datas/ScriptableDatas/Quests/QuestBoard/QuestDetailPanel.cs",  "UI/Quest/QuestDetailPanel.cs" },
    { "Core/Datas/ScriptableDatas/Quests/QuestBoard/QuestCategorySlot.cs", "UI/Quest/QuestCategorySlot.cs" },
    { "UI/Items/ItemDrop.cs",        "Gameplay/InteractObjects/ItemDrop.cs" },
    { "UI/VFX/PlayerSprintVFX.cs",   "Gameplay/Entities/Players/PlayerSprintVFX.cs" },
};
for (int i = 0; i < moves.GetLength(0); i++)
{
    string err = UnityEditor.AssetDatabase.MoveAsset(R + moves[i, 0], R + moves[i, 1]);
    log.Add(moves[i, 0] + " -> " + moves[i, 1] + ": " + (string.IsNullOrEmpty(err) ? "ok" : err));
    if (!string.IsNullOrEmpty(err)) return string.Join("\n", log);
}
foreach (var dir in new[] { "Core/Interfaces", "UI/Items" })
{
    bool empty = UnityEditor.AssetDatabase.FindAssets("", new[] { R + dir }).Length == 0;
    log.Add("delete " + dir + ": " + (empty ? UnityEditor.AssetDatabase.DeleteAsset(R + dir).ToString() : "SKIPPED, not empty"));
}
return string.Join("\n", log);
```
Expected: two folder GUIDs, 11 lines ending in `ok`, `delete Core/Interfaces: True`, `delete UI/Items: True`.

- [ ] **Step 2: Compile, test, GUID check**

Unity MCP: `refresh_unity` (force, compile request, wait). `read_console` `error CS`: 0. `run_tests` EditMode: 9/9.
```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/phase2_after4.json"
python tools/refactor/guid_snapshot.py compare "$TEMP/phase2_before.json" "$TEMP/phase2_after4.json"
```
Expected: 21 `moved` lines (10 from Task 3 + 11 here). The only `MISSING` entries allowed are the two deleted folders `Assets/Project_Ocean/Scripts/Core/Interfaces` and `Assets/Project_Ocean/Scripts/UI/Items`. Any `.cs` listed as MISSING means stop and investigate.

- [ ] **Step 3: Hand-off**

Suggested commit: "refactor: move enemy interfaces, quest board UI and world objects to their layer". Do not commit.

---

### Task 5: Verify live, measure, update docs

**Files:**
- Regenerate: `docs/uml/**` (generated pages)
- Modify: `docs/uml/findings.md` (§4 Phase 2 status + deviations)

**Interfaces:**
- Consumes: Tasks 1–4.

- [ ] **Step 1: Missing-script scan**

Run the Task 1 Step 3 `execute_code` snippet again. Expected: identical to the baseline (`0 missing`).

- [ ] **Step 2: Play-mode smoke run**

Use the routes in memory files `reference_live_enemy_test_route.md` and `feedback_unity_play_mode_unfocused.md` (set `Application.runInBackground = true` after entering Play). Route: Bootstrap scene → MainMenu → New Game → Land_Map, open the quest board (QuestBoardUI moved), pick up or check an item (Item/ItemDrop moved), → Ocean_Map (LoadingType moved) → save → load. Back up the save folder before the run and restore it after, as in Phase 1 Task 5. Afterwards run `read_console` for errors. Classify every error: caused by Phase 2 (a moved or extracted type in its stack) or pre-existing (compare with the Phase 1 run: the URP "Missing types" message and the "DontDestroyOnLoad only works for root GameObjects" messages were already there). Exit Play mode.
Expected: no errors caused by Phase 2.

- [ ] **Step 3: Scene/prefab dirty check**

Run:
```bash
git status --short -- "*.unity" "*.prefab"
```
Expected: no output. If a file is listed, report it with `git diff --stat` for that file. Do not restore it.

- [ ] **Step 4: Regenerate and measure**

Run:
```bash
python tools/uml/gen_uml.py
python tools/uml/test_gen_uml.py
grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
graphify update .
```
Expected: the self-test prints `OK`. The layer-violation count is below the baseline 111. Record the new number. If it is not lower, report DONE_WITH_CONCERNS with the top remaining "To type" entries from metrics.md.

- [ ] **Step 5: Update `docs/uml/findings.md`**

Under `### Phase 2: put types in the right layer`, add these lines directly below the heading (fill in the measured number):
```markdown
**Status:** done 2026-09-26. Wrong-way links 111 → <new count>.
**Deviations:** `LoadingManager` stays in UI until Phase 3 (it holds `LoadingVisualController`). `TutorialGate`/`TutorialStep` stay in Gameplay (they call `TutorialPromptUI`/`QuestManager`). `PlayerFeedbackVFX` stays in UI (it is a screen-space vignette). Only `LoadingType`, `TutorialActionId`/`TutorialEvents` and `PlayerSprintVFX` moved from those groups.
```
In `### Phase 3`, add one bullet at the end of its list:
```markdown
- `LoadingManager` → Core: talk to `LoadingVisualController` through an event or interface, then move it to `Core/Scenes`.
```

- [ ] **Step 6: Hand-off**

Report: every task's result, the violation count before and after, the smoke-run result, the dirty-check result. Suggested commit: "docs: refresh UML after Phase 2 layer moves". Do not commit or push.
