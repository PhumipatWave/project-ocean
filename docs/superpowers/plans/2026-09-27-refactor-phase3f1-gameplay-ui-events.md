# Refactor Phase 3f-1: Gameplay Stops Calling UI Directly — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove 10 wrong-way links without scene or prefab edits. The low-risk half of 3f. The other half (NPC/ExploreAreaTrigger dialogue views, `NPC → QuestDetailPanel`, `Cutscene` skip dialog) needs prefab rewiring and gets its own plan (3f-2).

**Architecture:**
- **Data in the wrong file:** the `GameState` enum moves out of `GameManager.cs` into `Core/GameState.cs`, because `SaveData` (Core) uses it.
- **Component in the wrong layer:** `InteractUI` is the E-press interaction trigger and depends only on Core and Gameplay, so it moves to `Gameplay/Interaction`.
- **Gameplay → UI calls become events or an interface:** `QuestManager.OnQuestCompleted` (`QuestCompleteUI` listens), `GameManager.OnReturningToMenu` (`UIManager` listens), `Player.OnPlayerDamaged` (`PlayerFeedbackVFX` listens; this also removes a `FindFirstObjectByType`), and `IDamagePopup` (Gameplay) implemented by `DamagePoint` (UI).
- Listeners subscribe where their old lookup would have found them: in `Awake` for the singletons, which set `Instance` in `Awake`, and in `OnEnable` for `PlayerFeedbackVFX`, which was found by `FindFirstObjectByType` and so only while active. That keeps behaviour identical.

**Tech Stack:** Unity 6000.3.6f1 + Unity MCP, NUnit EditMode, Python UML tools.

## Global Constraints

- Part of the approved clean-code direction (hybrid: correct layers plus decoupling), agreed on 2026-09-26/27.
- Behaviour identical: the quest-complete popup, pause-menu close on return to menu, damage vignette, damage and death numbers all still appear at the same moments.
- No scene/prefab edits. Moves via `AssetDatabase.MoveAsset` (Unity MCP `execute_code`); new files via the Write tool; code via Edit.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Back up the save folder `C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean` before Play and restore it byte-identical after. After Play: `git status --short -- "*.unity" "*.prefab"` → report.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`; retry once on a transient timeout.
- Comment rules (CLAUDE.md): short WHY comments, no history; XML `<summary>` one sentence on public API.

---

### Task 1: GameState to Core, InteractUI to Gameplay

**Files (paths relative to `Assets/Project_Ocean/Scripts/`):**
- Create: `Core/GameState.cs`
- Modify: `Gameplay/Game/GameManager.cs` (delete the enum block, lines 6–11)
- Move: `UI/Interactions/InteractUI.cs` → `Gameplay/Interaction/InteractUI.cs`

- [ ] **Step 1: Baseline**

```bash
python tools/refactor/guid_snapshot.py snapshot "$TEMP/p3f1_before.json"
python tools/uml/gen_uml.py && grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
```
Record the count. Expect 26, or one less if Phase 3d's regeneration already ran.

- [ ] **Step 2: Extract `GameState`**

Create `Assets/Project_Ocean/Scripts/Core/GameState.cs`:
```csharp
namespace Game.Testing
{
    public enum GameState
    {
        Land,
        Ocean,
        Menu
    }
}
```
In `Gameplay/Game/GameManager.cs` delete exactly:
```csharp
    public enum GameState
    {
        Land,
        Ocean,
        Menu
    }

```
so that `public class GameManager : MonoBehaviour` directly follows `{`.

- [ ] **Step 3: Move `InteractUI`**

Unity MCP `execute_code`:
```csharp
const string R = "Assets/Project_Ocean/Scripts/";
string g = UnityEditor.AssetDatabase.CreateFolder(R + "Gameplay", "Interaction");
string e = UnityEditor.AssetDatabase.MoveAsset(R + "UI/Interactions/InteractUI.cs", R + "Gameplay/Interaction/InteractUI.cs");
return "folder=" + g + " move=" + (string.IsNullOrEmpty(e) ? "ok" : e);
```
Expected: a GUID and `move=ok`.

- [ ] **Step 4: Compile and test**

`refresh_unity` (force, compile request, wait); `read_console` `error CS`: 0; `run_tests` EditMode: all pass (15).

---

### Task 2: Events and interface replace direct UI calls

**Files:**
- Create: `Gameplay/Entities/IDamagePopup.cs`
- Modify: `UI/Interactions/DamagePoint.cs`, `Gameplay/Projectiles/Bullet.cs`, `Gameplay/Entities/EntityDeathHandler.cs`, `Gameplay/QuestSystems/Quest/QuestManager.cs`, `UI/QuestCompleteUI/QuestCompleteUI.cs`, `Gameplay/Game/GameManager.cs`, `UI/UIManager/UIManager.cs`, `Gameplay/Entities/Players/Player.cs`, `UI/VFX/PlayerFeedbackVFX.cs`
- Test: `Assets/Project_Ocean/Tests/EditMode/GameplayUiEventTests.cs`

**Interfaces:**
- Produces: `public interface IDamagePopup { void ShowDamagePoint(string damageText); void ShowDeath(); }`; `QuestManager.OnQuestCompleted` (`Action<Quest>`); `GameManager.OnReturningToMenu` (`Action`); `Player.OnPlayerDamaged` (`Action`).

- [ ] **Step 1: Write the failing test**

`Assets/Project_Ocean/Tests/EditMode/GameplayUiEventTests.cs`:
```csharp
using Game.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class GameplayUiEventTests
    {
        [Test]
        public void DamagePoint_IsFoundThroughIDamagePopup()
        {
            var root = new GameObject("enemy");
            var child = new GameObject("popup");
            child.transform.SetParent(root.transform);
            child.AddComponent<DamagePoint>();
            try
            {
                Assert.IsNotNull(root.GetComponentInChildren<IDamagePopup>());
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GameplayEvents_Exist()
        {
            System.Action<Quest> q = _ => { };
            System.Action a = () => { };
            QuestManager.OnQuestCompleted += q;   QuestManager.OnQuestCompleted -= q;
            GameManager.OnReturningToMenu += a;   GameManager.OnReturningToMenu -= a;
            Player.OnPlayerDamaged += a;          Player.OnPlayerDamaged -= a;
            Assert.Pass();
        }
    }
}
```
`refresh_unity` + `read_console` `error CS`. Expected red: `CS0246` (IDamagePopup not found) and `CS0117` for the three events.

- [ ] **Step 2: `IDamagePopup` + `DamagePoint`**

Create `Assets/Project_Ocean/Scripts/Gameplay/Entities/IDamagePopup.cs`:
```csharp
namespace Game.Testing
{
    /// <summary>Floating damage/death number on an entity; implemented by the UI DamagePoint.</summary>
    public interface IDamagePopup
    {
        void ShowDamagePoint(string damageText);
        void ShowDeath();
    }
}
```
In `UI/Interactions/DamagePoint.cs` change `public class DamagePoint : MonoBehaviour` to `public class DamagePoint : MonoBehaviour, IDamagePopup`. Its existing `public void ShowDeath()` and `public void ShowDamagePoint(string damageText)` already match.
In `Gameplay/Projectiles/Bullet.cs` replace:
```csharp
                    var darmagePoint = collision.GetComponentInChildren<DamagePoint>();
```
with:
```csharp
                    var darmagePoint = collision.GetComponentInChildren<IDamagePopup>();
```
In `Gameplay/Entities/EntityDeathHandler.cs` replace:
```csharp
            DamagePoint damagePoint = GetComponentInChildren<DamagePoint>();
```
with:
```csharp
            IDamagePopup damagePoint = GetComponentInChildren<IDamagePopup>();
```
Note: `if (damagePoint != null)` on an interface reference skips Unity's destroyed-object check. That's fine, because `GetComponentInChildren` never returns destroyed components.

- [ ] **Step 3: `QuestManager.OnQuestCompleted` → `QuestCompleteUI`**

In `QuestManager.cs`, directly below `public static event Action OnQuestUpdated;` add:
```csharp
        /// <summary>Raised when a quest (or an auto-complete step) finishes and the complete popup should show.</summary>
        public static event Action<Quest> OnQuestCompleted;
```
Replace:
```csharp
                    if (QuestCompleteUI.Instance != null)
                        QuestCompleteUI.Instance.Show(q);
```
with:
```csharp
                    OnQuestCompleted?.Invoke(q);
```
Replace:
```csharp
            if (QuestCompleteUI.Instance != null)
            {
                QuestCompleteUI.Instance.Show(quest);
            }
```
with:
```csharp
            OnQuestCompleted?.Invoke(quest);
```
In `UI/QuestCompleteUI/QuestCompleteUI.cs`, at the end of `Awake()` directly after `Instance = this;` (and any lines that follow it inside `Awake`), add:
```csharp
            QuestManager.OnQuestCompleted += Show;
```
Add this method to the class (if an `OnDestroy` already exists, add the `if` block to it instead):
```csharp
        private void OnDestroy()
        {
            if (Instance == this)
                QuestManager.OnQuestCompleted -= Show;
        }
```
Check that the duplicate path (`if (Instance != null && Instance != this) { ... return; }`) returns before the subscription line.

- [ ] **Step 4: `GameManager.OnReturningToMenu` → `UIManager`**

In `GameManager.cs` add below `public class GameManager : MonoBehaviour` `{`:
```csharp
        /// <summary>Raised just before returning to the main menu, so persistent UI can close.</summary>
        public static event System.Action OnReturningToMenu;
```
In `ToMainMenu()`, replace `            UIManager.Instance?.CloseAll();` with `            OnReturningToMenu?.Invoke();`. Keep the comment above it, but change its first words `// UIManager (pause menu, etc.) is DontDestroyOnLoad — without this,` to `// UIManager (pause menu, etc.) is DontDestroyOnLoad and closes on this event — without it,`.
In `UI/UIManager/UIManager.cs`, at the end of `Awake()` after `Instance = this;` (and anything following it in `Awake`), add:
```csharp
            GameManager.OnReturningToMenu += CloseAll;
```
Add (or extend an existing `OnDestroy`):
```csharp
        private void OnDestroy()
        {
            if (Instance == this)
                GameManager.OnReturningToMenu -= CloseAll;
        }
```
Check that `CloseAll` is a parameterless `void` method. If its signature differs, subscribe a lambda `() => CloseAll()` stored in a field.

- [ ] **Step 5: `Player.OnPlayerDamaged` → `PlayerFeedbackVFX`**

In `Player.cs`:
- Add at the top of the class body:
```csharp
        /// <summary>Raised when the player takes damage (visual feedback moment).</summary>
        public static event System.Action OnPlayerDamaged;
```
- In `OnDamagedVisual()` replace `            playerFeedbackVFX?.PlayDamageVFX();` with `            OnPlayerDamaged?.Invoke();`.
- Delete the field `        private PlayerFeedbackVFX playerFeedbackVFX;`, the method `FindPlayerFeedbackVFX()` (3 lines plus braces), and its call `FindPlayerFeedbackVFX();` in `Start()`.

In `UI/VFX/PlayerFeedbackVFX.cs`, add:
```csharp
        private void OnEnable() => Player.OnPlayerDamaged += PlayDamageVFX;
        private void OnDisable() => Player.OnPlayerDamaged -= PlayDamageVFX;
```
If `PlayerFeedbackVFX` already has `OnEnable`/`OnDisable`, add the lines inside them instead. Check that `PlayDamageVFX` is a parameterless public `void` method.

- [ ] **Step 6: Compile and test (green)**

`refresh_unity` (force, compile request, wait); `read_console` `error CS`: 0; `run_tests` EditMode: 17/17 (15 + 2).
Run (project root):
```bash
grep -nE "QuestCompleteUI|UIManager|PlayerFeedbackVFX|FindFirstObjectByType|\bDamagePoint\b" Assets/Project_Ocean/Scripts/Gameplay/QuestSystems/Quest/QuestManager.cs Assets/Project_Ocean/Scripts/Gameplay/Game/GameManager.cs Assets/Project_Ocean/Scripts/Gameplay/Entities/Players/Player.cs Assets/Project_Ocean/Scripts/Gameplay/Projectiles/Bullet.cs Assets/Project_Ocean/Scripts/Gameplay/Entities/EntityDeathHandler.cs
```
Expected: only comment lines (e.g. the `UIManager` comment in `GameManager`); no code references.

---

### Task 3: Verify live, measure, docs

- [ ] **Step 1: Back up saves**

```bash
rm -rf "$TEMP/p3f1_savebackup" && cp -r "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" "$TEMP/p3f1_savebackup" && echo backed-up
```

- [ ] **Step 2: Play checks**

Play from Bootstrap (`Application.runInBackground = true`; see memory `feedback_unity_play_mode_unfocused.md` and `reference_live_enemy_test_route.md`). Verify each item with `execute_code`:
1. **Damage vignette:** in Land_Map or Ocean_Map call `GameManager.CurPlayer.TakeDamage(1f)`. Read `PlayerFeedbackVFX`'s `damageVignette` color alpha (reflection) right after: it must be > 0 or rising (the fade coroutine started). No console error.
2. **Damage and death numbers:** in Ocean_Map (runtime-unlock spawns as in earlier runs), hit an enemy by calling the bullet's damage path, or `enemy.TakeDamage` plus `GetComponentInChildren<IDamagePopup>().ShowDamagePoint("5")`. Better: fire a real `NormalBullet` at an enemy if practical. Check that the DamagePoint text object activates; kill an enemy and check `ShowDeath` ran (text or animation state). Leave Ocean_Map within 3 minutes.
3. **Quest complete popup:** trigger a quest completion (e.g. call the `QuestManager` completion path for an active quest via reflection, or raise `QuestManager`'s event path by completing the tutorial's first step). Check that `QuestCompleteUI`'s panel becomes active.
4. **Return to menu closes UI:** open the pause menu (`UIManager` open method, or toggle via `InputReader`), then `GameManager.Instance.ToMainMenu()`. After MainMenu loads: `UIManager.Instance.IsUIOpen == false` and `Time.timeScale == 1`.
5. `InteractUI` still works: press-E interaction on an NPC or door (call `InteractUI`'s interact method or raise `InputReader`'s interact) and confirm the dialogue or door opens.
Exit Play. Record per-item pass/fail. Mark any item not verifiable as UNVERIFIED with the reason; don't claim it.

- [ ] **Step 3: Restore, dirty check, metric, docs**

```bash
rm -rf "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && cp -r "$TEMP/p3f1_savebackup" "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && diff -rq "$TEMP/p3f1_savebackup" "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && echo RESTORED
git status --short -- "*.unity" "*.prefab"
python tools/refactor/guid_snapshot.py snapshot "$TEMP/p3f1_after.json" && python tools/refactor/guid_snapshot.py compare "$TEMP/p3f1_before.json" "$TEMP/p3f1_after.json"
python tools/uml/gen_uml.py && python tools/uml/test_gen_uml.py && grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
graphify update .
```
Expected: RESTORED; empty git status; 1 moved (`InteractUI`), 0 missing; the count drops by about 10 from Step 1.
Append to the `**Progress:**` line under `### Phase 3` in `docs/uml/findings.md`: ` 3f-1 done: GameState → Core, InteractUI → Gameplay/Interaction; QuestCompleteUI, UIManager, PlayerFeedbackVFX listen to gameplay events; DamagePoint via IDamagePopup (<new count> links).`

- [ ] **Step 4: Hand-off**

Suggested commit: "refactor: gameplay raises events instead of calling UI". Do not commit.
