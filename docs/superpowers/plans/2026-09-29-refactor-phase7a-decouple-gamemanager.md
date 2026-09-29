# Refactor Phase 7a: Decouple the GameManager Hub and Player-Side Upward Links — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Gameplay code stops reaching into `GameManager` for "who is the player" and "what state are we in", and low-level player code stops calling high-level systems (`GameManager.ReturnToLand`, `QuestManager` rewards). `GameManager` becomes a scene-flow controller used by 5 types instead of 30.

**Architecture:** Three tiny static seams in `Game.Core` (same style as the existing `Core/Events/*Events.cs`): `GameSession` (current `GameState`), `GameFlowEvents` (request: return to land), `RewardEvents` (money granted). `GameManager` and `QuestManager` write/raise them; readers and the player code use them. Every call to `GameManager.CurPlayer` becomes `PlayerRegistry.Local` (the property already just forwards there), and the property is deleted.

**Tech Stack:** Unity 6000.3.6f1, C#, NUnit EditMode tests, Unity MCP (`refresh_unity`, `read_console`, `run_tests`, `execute_code`), Python 3 (`tools/uml`).

## Why this scope (data, 2026-09-29)

- `GameManager` is used by **30 types**. By call: `CurPlayer` 43×, `GameState` 8×, `ReturnToLand` 3×, others ≤ 2×. Only UI (`GameHUD`, `LandUI`, `UIManager`) and the MiniHub fields (`MiniHubItem`, `SaveBridge`) need anything else.
- Upward links from player code into higher systems: `PlayerHealth`/`OxygenRope` → `GameManager.ReturnToLand`, `PlayerMoney` → `QuestManager.OnMoneyRewarded`, `Player`/`OxygenRope` → `GameManager.GameState`.
- The 36-type dependency loop in `docs/uml/metrics.md` is **reported, not targeted**. Simulation predicts about 36 → 31. The static parser can't see member-access dependencies (e.g. `CurPlayer.OxygenTube` really depends on `Player`), so the loop number overstates what any single cut achieves. The real targets are the grep-able criteria below.
- **Out of scope (separate plans or deliberately left):** `InventoryManager` view/logic split (Phase 7b plan), `Player` → `TutorialGate`/`FeatureUnlockService` queries (`TutorialGate` calls into `TutorialPromptUI`, so it can't move to Core without an API split), `PlayerGunHandler` → `UpgradeService.GetLevel` query, the unused MiniHub fields (waiting on the user's design decision), `QuestManager` as a hub (simulation: cutting it gains nothing).

## Success criteria (all must hold at the end)

1. `grep -rn "GameManager\." Assets/Project_Ocean/Scripts/Gameplay/Entities` → no output.
2. `grep -rn "QuestManager" Assets/Project_Ocean/Scripts/Gameplay/Entities/Players/PlayerMoney.cs` → no output.
3. `grep -rn "CurPlayer" Assets/Project_Ocean/Scripts --include=*.cs` → no output.
4. `docs/uml/metrics.md` "Most depended-on": `GameManager` used by ≤ 6 types (was 28).
5. EditMode tests: all pass (36 before + 4 new = 40).
6. Live: dive → rope/health failure returns to land; a quest money reward reaches `PlayerMoney`; state-gated code (F item use, rope simulation, HUD return button) behaves as before.

## Global Constraints

- No behaviour change. Every replaced read gives the same result at runtime: `GameManager` always exists in play (persistent in `AS_PlayerHUD`), and `GameSession.State` defaults to `Land`, the same value `GameManager`'s old `gameState` field had before `Awake`.
- Removed public API: `GameManager.CurPlayer`, `QuestManager.OnMoneyRewarded`. All callers are migrated in the same task, and the compile proves none are left. `GameManager.GameState` stays, forwarding to `GameSession`, so the UI keeps working.
- Files keep their encoding, BOM and line endings. No scene/prefab edits.
- CLAUDE.md comment rules: 1–2 lines, why not what, XML `<summary>` on public API only.
- The user commits. Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`. The "Commit" steps below are hand-off suggestions.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`. "Compile" means `refresh_unity(mode=force, compile=request)` then `read_console(filter_text="error CS")` = 0 entries and `EditorUtility.scriptCompilationFailed == false`. "Run tests" means `run_tests(mode=EditMode)` + `get_test_job(wait_timeout=60)`.
- Back up `C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean` before any Play session and restore it byte-identical after (RESTORED).

---

### Task 1: `GameSession` — the game state leaves GameManager

**Files:**
- Create: `Assets/Project_Ocean/Scripts/Core/GameSession.cs`
- Create: `Assets/Project_Ocean/Tests/EditMode/GameSessionTests.cs`
- Modify: `Assets/Project_Ocean/Scripts/Gameplay/Game/GameManager.cs:22-23, 48-50, 70, 78, 80, 146, 173, 183`
- Modify (readers): `Gameplay/Entities/Players/OxygenTube/OxygenRope.cs:504-505`, `Gameplay/Entities/Players/Player.cs:355, 373, 380`, `Gameplay/QuestSystems/Quest/TutorialPromptUI.cs:54`, `UI/GameHUD.cs:153, 219`, `UI/Ocean/RopeBreakCountdownUI.cs:69` (all under `Assets/Project_Ocean/Scripts/`)

**Interfaces:**
- Produces: `Game.Core.GameSession.State` (`GameState`, get/set, default `GameState.Land`).

- [ ] **Step 1: Write the failing test**

`Assets/Project_Ocean/Tests/EditMode/GameSessionTests.cs`:
```csharp
using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    public class GameSessionTests
    {
        private GameState saved;

        [SetUp] public void SetUp() => saved = GameSession.State;
        [TearDown] public void TearDown() => GameSession.State = saved;

        [Test]
        public void State_RoundTrips()
        {
            GameSession.State = GameState.Ocean;
            Assert.AreEqual(GameState.Ocean, GameSession.State);
            GameSession.State = GameState.Menu;
            Assert.AreEqual(GameState.Menu, GameSession.State);
        }
    }
}
```

- [ ] **Step 2: Compile — expect red**

Expected: `error CS0103: The name 'GameSession' does not exist in the current context`.

- [ ] **Step 3: Create `GameSession`**

`Assets/Project_Ocean/Scripts/Core/GameSession.cs`:
```csharp
namespace Game.Core
{
    /// <summary>Current game state (Menu / Land / Ocean); GameManager writes it, anyone may read it.</summary>
    public static class GameSession
    {
        // Land = default(GameState), the value GameManager's old field had before its Awake.
        public static GameState State { get; set; } = GameState.Land;
    }
}
```

- [ ] **Step 4: GameManager writes through `GameSession`**

In `GameManager.cs`, replace lines 22-23:
```csharp
        private GameState gameState;
        public GameState GameState { get { return gameState; } set { gameState = value; } }
```
with:
```csharp
        public GameState GameState { get => GameSession.State; set => GameSession.State = value; }
```
Then replace every `gameState = ` with `GameSession.State = ` (7 places: Awake line 48, OnSceneLoaded 70/78/80, ToMainMenu 146, StartDive 173, ReturnToLand 183). After the edit, `grep -n "gameState" GameManager.cs` returns nothing.

- [ ] **Step 5: Readers use `GameSession.State`** (exact replacements)

| File:line | Before | After |
|---|---|---|
| `OxygenRope.cs:505` | `GameManager.Instance != null && GameManager.Instance.GameState == GameState.Ocean;` | `GameSession.State == GameState.Ocean;` |
| `Player.cs:355`, `:373`, `:380` | `GameManager.Instance.GameState == GameState.Ocean` | `GameSession.State == GameState.Ocean` |
| `TutorialPromptUI.cs:54` | `&& GameManager.Instance != null && GameManager.Instance.GameState == GameState.Ocean` | `&& GameSession.State == GameState.Ocean` |
| `GameHUD.cs:153` | `(GameManager.Instance != null && GameManager.Instance.GameState == GameState.Menu)` | `GameSession.State == GameState.Menu` |
| `GameHUD.cs:219` | `if (GameManager.Instance == null \|\| GameManager.Instance.GameState != GameState.Ocean)` | `if (GameSession.State != GameState.Ocean)` |
| `RopeBreakCountdownUI.cs:69` | `if (GameManager.Instance == null \|\| GameManager.Instance.GameState != GameState.Ocean)` | `if (GameSession.State != GameState.Ocean)` |

Why these are equivalent when `GameManager.Instance` is null (editor only): the old code gave `false` for "== Ocean/Menu" and `true` for "!= Ocean". `GameSession.State` is `Land` then, so the new code gives the same results.

- [ ] **Step 6: Compile + run tests**

Expected: 0 `error CS`; tests 37/37 (36 + 1). `grep -rn "\.GameState ==\|\.GameState !=" Assets/Project_Ocean/Scripts --include=*.cs` → no output.

- [ ] **Step 7: Commit (user)** — suggested: `refactor: GameSession holds the game state (GameManager writes, readers read)`.

---

### Task 2: `PlayerRegistry.Local` everywhere — delete `GameManager.CurPlayer`

**Files:**
- Modify (mechanical `GameManager.CurPlayer` → `PlayerRegistry.Local`), all under `Assets/Project_Ocean/Scripts/`: `Deprecated/CheckpointItem/CheckPointItem.cs`, `Gameplay/Cutscenes/Cutscene.cs`, `Gameplay/Entities/Enemies/EnemyLoot.cs`, `Gameplay/Entities/NPCs/NPC.cs`, `Gameplay/Entities/Players/DevPanel.cs`, `Gameplay/Environments/ChestInteract.cs`, `Gameplay/Environments/DoorInteract.cs`, `Gameplay/Interaction/InteractUI.cs`, `Gameplay/Items/Placeables/MiniHubItem.cs`, `Gameplay/MainCameras/PlayerCamera.cs`, `Gameplay/Projectiles/Bullet.cs`, `Gameplay/Projectiles/NormalBullet.cs`, `Gameplay/QuestSystems/Quest/PlayerSelfDialogueUI.cs`, `Gameplay/QuestSystems/Quest/QuestStepEffects.cs`, `Gameplay/QuestSystems/Quest/QuestVisuals.cs`, `Gameplay/QuestSystems/Quest/TutorialActionMarker.cs`, `Gameplay/Save/SaveBridge.cs`, `Gameplay/Services/UpgradeService.cs`, `Gameplay/Spawning/EnemySpawner.cs`, `UI/GameHUD.cs`, `UI/Inventory/InventoryManager.cs`, `UI/Money/MoneyUI.cs`, `UI/Shop/ShopManager.cs`
- Modify: `Gameplay/Game/GameManager.cs:16-20, 84, 113-134, 180`

**Interfaces:**
- Consumes: `Game.Gameplay.PlayerRegistry.Local` (`Player`, existing; `GameManager.CurPlayer` was `=> PlayerRegistry.Local`, so this is a pure rename).
- Produces: `GameManager.CurPlayer` no longer exists.

- [ ] **Step 1: Mechanical rename in the 23 caller files**

Run from the project root:
```bash
python - <<'EOF'
import re
from pathlib import Path
S = Path("Assets/Project_Ocean/Scripts")
n = 0
for f in S.rglob("*.cs"):
    if f.name == "GameManager.cs":
        continue
    b = f.read_bytes(); bom = b.startswith(b"\xef\xbb\xbf")
    try: t, enc = b.decode("utf-8-sig"), "utf-8"
    except UnicodeDecodeError: t, enc = b.decode("cp874"), "cp874"
    new, k = re.subn(r"\bGameManager\.CurPlayer\b", "PlayerRegistry.Local", t)
    if k:
        f.write_bytes((b"\xef\xbb\xbf" if bom else b"") + new.encode(enc)); n += k
        print(k, f)
print(n, "replacements")
EOF
```
Expected: 23 files, 43 replacements.

- [ ] **Step 2: GameManager uses `PlayerRegistry.Local` itself**

Delete lines 16-20 (the `CurPlayer` property and its summary). Line 84: `&& CurPlayer == null)` → `&& PlayerRegistry.Local == null)`. Replace `MovePlayerTo` (lines 112-134) with:
```csharp
        /// <summary>Teleports the player to a position and resets their velocity.</summary>
        private void MovePlayerTo(Vector3 position)
        {
            var player = PlayerRegistry.Local;
            if (player == null)
            {
                Debug.LogWarning("[GameManager] MovePlayerTo: no local player");
                return;
            }

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity  = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType        = RigidbodyType2D.Kinematic;
                player.transform.position = position;
                rb.bodyType        = RigidbodyType2D.Dynamic;
            }
            else
            {
                player.transform.position = position;
            }
        }
```
Line 180: `CurPlayer.OxygenTube.ResetRope();` → `PlayerRegistry.Local.OxygenTube.ResetRope();`

- [ ] **Step 3: Compile + run tests**

Expected: 0 `error CS` (any caller the script missed shows here as `CS0117: 'GameManager' does not contain a definition for 'CurPlayer'`); tests 37/37. `grep -rn "CurPlayer" Assets/Project_Ocean/Scripts --include=*.cs` → no output.

- [ ] **Step 4: Commit (user)** — suggested: `refactor: read the player from PlayerRegistry, drop GameManager.CurPlayer`.

---

### Task 3: `GameFlowEvents` — player code requests "return to land" instead of calling GameManager

**Files:**
- Create: `Assets/Project_Ocean/Scripts/Core/Events/GameFlowEvents.cs`
- Create: `Assets/Project_Ocean/Tests/EditMode/GameFlowEventsTests.cs`
- Modify: `Gameplay/Game/GameManager.cs` (Awake + new OnDestroy)
- Modify: `Gameplay/Entities/Players/PlayerHealth.cs:86`, `Gameplay/Entities/Players/OxygenTube/OxygenRope.cs:560`

**Interfaces:**
- Produces: `Game.Core.GameFlowEvents.OnReturnToLandRequested` (`event Action`), `Game.Core.GameFlowEvents.RequestReturnToLand()`.

- [ ] **Step 1: Write the failing test**

`Assets/Project_Ocean/Tests/EditMode/GameFlowEventsTests.cs`:
```csharp
using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    public class GameFlowEventsTests
    {
        [Test]
        public void RequestReturnToLand_RaisesOnce_AndStopsAfterUnsubscribe()
        {
            int calls = 0;
            System.Action handler = () => calls++;
            GameFlowEvents.OnReturnToLandRequested += handler;
            GameFlowEvents.RequestReturnToLand();
            GameFlowEvents.OnReturnToLandRequested -= handler;
            GameFlowEvents.RequestReturnToLand();
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void RequestReturnToLand_WithNoListener_DoesNotThrow()
        {
            Assert.DoesNotThrow(GameFlowEvents.RequestReturnToLand);
        }
    }
}
```

- [ ] **Step 2: Compile — expect red** (`CS0103 'GameFlowEvents'`).

- [ ] **Step 3: Create `GameFlowEvents`**

`Assets/Project_Ocean/Scripts/Core/Events/GameFlowEvents.cs`:
```csharp
namespace Game.Core
{
    /// <summary>Scene-flow requests from gameplay code; GameManager performs them.</summary>
    public static class GameFlowEvents
    {
        public static event System.Action OnReturnToLandRequested;

        public static void RequestReturnToLand() => OnReturnToLandRequested?.Invoke();
    }
}
```

- [ ] **Step 4: GameManager listens (live instance only)**

In `GameManager.Awake`, directly after `DontDestroyOnLoad(gameObject);` add:
```csharp
            GameFlowEvents.OnReturnToLandRequested += ReturnToLand;
```
Add below `Awake`:
```csharp
        // A duplicate destroyed in Awake never subscribed, so only the live instance unsubscribes.
        private void OnDestroy()
        {
            if (Instance == this)
                GameFlowEvents.OnReturnToLandRequested -= ReturnToLand;
        }
```
(Subscribing in `Awake` rather than `OnEnable` keeps a duplicate `GameManager`, which is destroyed at the end of its first frame, from also answering a request, the same pattern `UIManager` uses for `OnReturningToMenu`.)

- [ ] **Step 5: Player code raises the request**

`PlayerHealth.cs:86`: `GameManager.Instance.ReturnToLand();` → `GameFlowEvents.RequestReturnToLand();`
`OxygenRope.cs:560`: `GameManager.Instance.ReturnToLand();` → `GameFlowEvents.RequestReturnToLand();`
(`GameHUD`'s button keeps calling `GameManager.Instance?.ReturnToLand()`: UI → Gameplay is the allowed direction.)

- [ ] **Step 6: Compile + run tests** — expected 0 errors, 39/39. `grep -rn "GameManager\." Assets/Project_Ocean/Scripts/Gameplay/Entities` → no output (criterion 1).

- [ ] **Step 7: Commit (user)** — suggested: `refactor: player code requests return-to-land via GameFlowEvents`.

---

### Task 4: `RewardEvents` — PlayerMoney stops listening to QuestManager

**Files:**
- Create: `Assets/Project_Ocean/Scripts/Core/Events/RewardEvents.cs`
- Create: `Assets/Project_Ocean/Tests/EditMode/RewardEventsTests.cs`
- Modify: `Gameplay/QuestSystems/Quest/QuestManager.cs:173, 181, 487, 516`
- Modify: `Gameplay/Entities/Players/PlayerMoney.cs:19-20`

**Interfaces:**
- Produces: `Game.Core.RewardEvents.OnMoneyRewarded` (`event Action<int>`), `Game.Core.RewardEvents.RaiseMoneyRewarded(int amount)`.
- Removes: `QuestManager.OnMoneyRewarded` (its only subscriber, `PlayerMoney`, moves in this task; checked with `grep -rn "OnMoneyRewarded"`).

- [ ] **Step 1: Write the failing test**

`Assets/Project_Ocean/Tests/EditMode/RewardEventsTests.cs`:
```csharp
using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    public class RewardEventsTests
    {
        [Test]
        public void RaiseMoneyRewarded_PassesAmount()
        {
            int got = 0;
            System.Action<int> handler = a => got += a;
            RewardEvents.OnMoneyRewarded += handler;
            try { RewardEvents.RaiseMoneyRewarded(500); }
            finally { RewardEvents.OnMoneyRewarded -= handler; }
            Assert.AreEqual(500, got);
        }
    }
}
```

- [ ] **Step 2: Compile — expect red** (`CS0103 'RewardEvents'`).

- [ ] **Step 3: Create `RewardEvents`**

`Assets/Project_Ocean/Scripts/Core/Events/RewardEvents.cs`:
```csharp
namespace Game.Core
{
    /// <summary>Money granted by gameplay (quest rewards); PlayerMoney adds it to the balance.</summary>
    public static class RewardEvents
    {
        public static event System.Action<int> OnMoneyRewarded;

        public static void RaiseMoneyRewarded(int amount) => OnMoneyRewarded?.Invoke(amount);
    }
}
```

- [ ] **Step 4: QuestManager raises, PlayerMoney listens**

`QuestManager.cs`: lines 173 and 181, `OnMoneyRewarded?.Invoke(completedStep.RewardMoney);` → `RewardEvents.RaiseMoneyRewarded(completedStep.RewardMoney);`. Line 516, `OnMoneyRewarded?.Invoke(finalStepReward);` → `RewardEvents.RaiseMoneyRewarded(finalStepReward);`. Delete line 487, `public static event Action<int> OnMoneyRewarded;`.
`PlayerMoney.cs:19-20`:
```csharp
        private void OnEnable() => RewardEvents.OnMoneyRewarded += AddMoney;
        private void OnDisable() => RewardEvents.OnMoneyRewarded -= AddMoney;
```

- [ ] **Step 5: Compile + run tests** — expected 0 errors, 40/40. `grep -rn "OnMoneyRewarded" Assets/Project_Ocean/Scripts --include=*.cs` shows only `RewardEvents.cs`, `QuestManager.cs` (raise calls) and `PlayerMoney.cs`. Criterion 2 holds.

- [ ] **Step 6: Commit (user)** — suggested: `refactor: quest money rewards via RewardEvents (PlayerMoney no longer knows QuestManager)`.

---

### Task 5: Verify live, measure, docs

**Files:** `docs/uml/findings.md` (new `### Phase 7a` section under §4), regenerated `docs/uml/*`, `graphify-out/*`.

- [ ] **Step 1: Player-script compile** (`execute_code`, same snippet as Phase 4 Task 1 Step 5): `Game.Core.dll`, `Game.Gameplay.dll`, `Game.UI.dll` built, 0 errors.

- [ ] **Step 2: Live smoke** (back up saves first; load route in memory `reference_live_enemy_test_route.md`: `LoadScene("MainMenuScene")` → `SaveBridge.Instance.LoadGame(0)`, and call it again if it's dropped)
  1. On Land: `GameSession.State == Land` and `GameManager.Instance.GameState == Land`.
  2. `GameManager.Instance.StartDive()` (unlock `GameFeature.Ocean` first if the save hasn't) → after loading, `GameSession.State == Ocean`, and the rope is running (`OxygenRope.CurrentLength > 0` after moving the player 5 m).
  3. Lethal damage: `PlayerRegistry.Local.GetComponent<PlayerHealth>().TakeDamage(99999f)` → loads Land, and `GameSession.State == Land` (goes through `GameFlowEvents`).
  4. Money reward: pick a tracked quest step with `RewardMoney > 0` (as in Phase 5a) and call `QuestManager.Instance.DevSkipCurrentStep()` → `PlayerMoney.CurrentMoney` rises by exactly `RewardMoney`.
  5. `Player.HandleUseItem` in the Ocean with a Mini Hub (feature `UseItem` unlocked) still spawns the hub (state gate works).
  6. Console: no new errors. Exit Play, restore saves (RESTORED), `git status --short -- "*.unity" "*.prefab"` → only changes that were already there.

- [ ] **Step 3: Measure**

```bash
python tools/uml/gen_uml.py && python tools/uml/test_gen_uml.py
grep -A1 "^| GameManager |" docs/uml/metrics.md
grep -m1 -A4 "Dependency cycles" docs/uml/metrics.md
grep -rn "GameManager\." Assets/Project_Ocean/Scripts/Gameplay/Entities
grep -rn "QuestManager" Assets/Project_Ocean/Scripts/Gameplay/Entities/Players/PlayerMoney.cs
grep -rn "CurPlayer" Assets/Project_Ocean/Scripts --include=*.cs
```
Expected: `GameManager` "Used by" ≤ 6; the last three greps empty; the largest loop reported (expected about 31; record the real number, and don't chase it).

- [ ] **Step 4: Docs** — `findings.md` §4, new `### Phase 7a` with a **Status** line: done date, `GameManager` fan-in before/after, the loop size before/after, the three seams added, and the out-of-scope list from "Why this scope". Then `graphify update .`.

- [ ] **Step 5: Hand-off (user commits)** — list the changed files; the suggested commit messages are in Tasks 1–4.

---

## Next plans (not this one)

The `InventoryManager` view/logic split is written as three plans. Run them in order after 7a is committed:
- `2026-09-30-refactor-phase7b-inventory-funnel.md`: every inventory change goes through `InventoryManager`.
- `2026-09-30-refactor-phase7c-inventory-model.md`: a pure `InventoryModel` owns the data, and `InventoryManager` draws it.
- `2026-09-30-refactor-phase7d-generated-slots.md`: the inventory and shop slot grids are built from a slot count.
