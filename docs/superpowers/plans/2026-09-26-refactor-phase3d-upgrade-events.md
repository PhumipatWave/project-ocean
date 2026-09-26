# Refactor Phase 3d: UpgradeService Raises Events, Receivers Apply Their Own Effect — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `UpgradeService` stops reaching into `OxygenRope`, `PlayerHealth` and `InventoryManager` to apply upgrade effects. It raises `OnLevelGranted(UpgradeData)` when a level is granted and `OnLevelsLoaded` after a save is applied. `PlayerHealth` applies the rope effect and `InventoryManager` applies the slot unlocks itself. Gameplay behaviour stays identical.

**Architecture:** Two static events on `UpgradeService`, raised at exactly the points where `ApplyEffect(data)` and `InventoryManager.SetUnlockedExtraSlots(...)` run today, so the effect timing doesn't change. Receivers subscribe in `OnEnable` / `OnDisable`. `ApplyEffect` is deleted. `DevGrantNextLevel`'s precondition checks stay unchanged. Its `InventoryManager.Instance == null` check remains the one `UpgradeService → InventoryManager` link until Phase 3e moves inventory logic to Gameplay.

**Tech Stack:** Unity 6000.3.6f1 + Unity MCP, NUnit EditMode.

## Global Constraints

- Hybrid step 2b, approved by the user on 2026-09-26. Runs after Phase 3c-2.
- Behaviour identical: buying a Rope level adds `data.value` to the rope max length and to `PlayerHealth`'s original max (both, and only when the player has an oxygen rope). Buying an Inventory level unlocks the next slot. Loading restores extra slots from the saved inventory level at the same point in the load sequence. The NormalBullet level still only changes the stored level.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Back up the save folder `C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean` before Play and restore it byte-identical after. Delete test slots.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`; retry once on a transient timeout. After Play: `git status --short -- "*.unity" "*.prefab"` must be empty (report it if not).
- Comment rules (CLAUDE.md): short WHY comments, no history, XML `<summary>` one sentence on public API.

## File map (paths relative to `Assets/Project_Ocean/Scripts/`)

| File | Change |
|---|---|
| `Gameplay/Services/UpgradeService.cs` | add `OnLevelGranted`, `OnLevelsLoaded`; delete `ApplyEffect`; `ApplyLoadedLevels` raises `OnLevelsLoaded` instead of calling `InventoryManager` |
| `Gameplay/Entities/Players/PlayerHealth.cs` | subscribe to `OnLevelGranted`, apply the Rope effect |
| `UI/Inventory/InventoryManager.cs` | subscribe to both events, apply the Inventory effect and the loaded slot unlocks |
| `Assets/Project_Ocean/Tests/EditMode/UpgradeServiceEventTests.cs` | new tests |

---

### Task 1: Event tests (red)

**Files:**
- Create: `Assets/Project_Ocean/Tests/EditMode/UpgradeServiceEventTests.cs`

**Interfaces:**
- Consumes: `UpgradeService.Instance`, `DevGrantNextLevel(UpgradeData, out string)`, `Apply(SaveData)`, `ResetToDefault()`, `GetLevel(UpgradeType)`; `UpgradeData` public fields `type`, `maxLevel`.
- Produces (expected by the tests, implemented in Task 2): `public static event System.Action<UpgradeData> OnLevelGranted;`, `public static event System.Action OnLevelsLoaded;` on `UpgradeService`.

- [ ] **Step 1: Write the failing tests**

```csharp
using Game.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class UpgradeServiceEventTests
    {
        private UpgradeData gun;
        private UpgradeData granted;
        private int grantedLevelSeen;
        private int loadedCalls;

        private void OnGranted(UpgradeData d)
        {
            granted = d;
            grantedLevelSeen = UpgradeService.Instance.GetLevel(d.type);
        }

        private void OnLoaded() => loadedCalls++;

        [SetUp]
        public void SetUp()
        {
            UpgradeService.Instance.ResetToDefault();
            gun = ScriptableObject.CreateInstance<UpgradeData>();
            gun.type = UpgradeType.NormalBullet;
            gun.maxLevel = 3;
            granted = null;
            grantedLevelSeen = -1;
            loadedCalls = 0;
            UpgradeService.OnLevelGranted += OnGranted;
            UpgradeService.OnLevelsLoaded += OnLoaded;
        }

        [TearDown]
        public void TearDown()
        {
            UpgradeService.OnLevelGranted -= OnGranted;
            UpgradeService.OnLevelsLoaded -= OnLoaded;
            UpgradeService.Instance.ResetToDefault();
            Object.DestroyImmediate(gun);
        }

        [Test]
        public void GrantingALevel_RaisesOnLevelGranted_BeforeTheLevelIncrements()
        {
            Assert.IsTrue(UpgradeService.Instance.DevGrantNextLevel(gun, out _));
            Assert.AreSame(gun, granted);
            Assert.AreEqual(0, grantedLevelSeen); // same point where ApplyEffect used to run
            Assert.AreEqual(1, UpgradeService.Instance.GetLevel(UpgradeType.NormalBullet));
        }

        [Test]
        public void AtMaxLevel_DoesNotRaise()
        {
            UpgradeService.Instance.SetLevel(UpgradeType.NormalBullet, 3);
            Assert.IsFalse(UpgradeService.Instance.DevGrantNextLevel(gun, out _));
            Assert.IsNull(granted);
        }

        [Test]
        public void ApplyingASave_RaisesOnLevelsLoaded_AfterLevelsAreSet()
        {
            var data = new SaveData { inventoryUpgradeLevel = 2, ropeUpgradeLevel = 1, normalBulletUpgradeLevel = 0 };
            ((ISaveParticipant)UpgradeService.Instance).Apply(data);
            Assert.AreEqual(1, loadedCalls);
            Assert.AreEqual(2, UpgradeService.Instance.GetLevel(UpgradeType.Inventory));
        }
    }
}
```

- [ ] **Step 2: Confirm red**

Unity MCP: `refresh_unity` (compile request, wait); `read_console` `error CS`. Expected: `CS0117` "'UpgradeService' does not contain a definition for 'OnLevelGranted'" (and the same for `OnLevelsLoaded`). Record them.

---

### Task 2: Raise events, move effects to receivers

**Files:** `UpgradeService.cs`, `PlayerHealth.cs`, `InventoryManager.cs`.

**Interfaces:**
- Produces: `UpgradeService.OnLevelGranted` (raised in `GrantLevel` where `ApplyEffect(data)` was, before the level increments) and `UpgradeService.OnLevelsLoaded` (raised at the end of `ApplyLoadedLevels`, after both levels are set).

- [ ] **Step 1: `UpgradeService`**

Below `public static UpgradeService Instance => ...;` add:
```csharp
        /// <summary>Raised when one level is granted, before it counts; receivers apply their own effect.</summary>
        public static event System.Action<UpgradeData> OnLevelGranted;

        /// <summary>Raised after a save's upgrade levels are restored, so receivers re-apply stored effects.</summary>
        public static event System.Action OnLevelsLoaded;
```
In `GrantLevel`, replace `ApplyEffect(data);` with `OnLevelGranted?.Invoke(data);`.
Delete the whole `ApplyEffect` method, including its `<summary>`.
Replace the body and summary of `ApplyLoadedLevels` with:
```csharp
        /// <summary>
        /// Called after a save is loaded: restores levels, then lets receivers re-apply stored effects
        /// (inventory slots). Rope length is NOT re-applied — PlayerHealth restores it from
        /// SaveData.maxRopeLength (re-applying would double the length).
        /// </summary>
        public void ApplyLoadedLevels(int inventoryLevel, int ropeLevel)
        {
            SetLevel(UpgradeType.Inventory, inventoryLevel);
            SetLevel(UpgradeType.Rope, ropeLevel);
            OnLevelsLoaded?.Invoke();
        }
```
In `Apply(SaveData)`, change the comment `// Rope length is restored by SaveBridge via SaveData.maxRopeLength,` to `// Rope length is restored by PlayerHealth via SaveData.maxRopeLength,`.
Update the class `<summary>` line `UpgradeManager (Land panel UI) and SaveBridge both go through this service.` to `UpgradeManager (Land panel UI) and SaveBridge both go through this service; effects are applied by listeners of OnLevelGranted.`

- [ ] **Step 2: `PlayerHealth`**

Add these members right after the field declarations:
```csharp
        private void OnEnable() => UpgradeService.OnLevelGranted += HandleLevelGranted;
        private void OnDisable() => UpgradeService.OnLevelGranted -= HandleLevelGranted;

        private void HandleLevelGranted(UpgradeData data)
        {
            if (data.type != UpgradeType.Rope || oxygenTube == null) return;
            oxygenTube.IncreaseMaxLength(data.value);
            IncreaseOriginalMaxLength(data.value);
        }
```
Before adding, check that `EntityHealth` (the base class, `Gameplay/Entities/EntityHealth.cs`) does not already declare `OnEnable`/`OnDisable`. If it does, use `protected override` and call `base.OnEnable()` / `base.OnDisable()` first. If the base methods are `private`, stop and report NEEDS_CONTEXT.

- [ ] **Step 3: `InventoryManager`**

Extend the existing `OnEnable` / `OnDisable` (they already exist; keep their current bodies):
- In `OnEnable`, add at the top:
```csharp
            UpgradeService.OnLevelGranted += HandleUpgradeGranted;
            UpgradeService.OnLevelsLoaded += HandleUpgradesLoaded;
```
- In `OnDisable`, add at the top:
```csharp
            UpgradeService.OnLevelGranted -= HandleUpgradeGranted;
            UpgradeService.OnLevelsLoaded -= HandleUpgradesLoaded;
```
Add these methods directly after `SetUnlockedExtraSlots`:
```csharp
        private void HandleUpgradeGranted(UpgradeData data)
        {
            if (data.type == UpgradeType.Inventory)
                UnlockNextSlot();
        }

        private void HandleUpgradesLoaded() =>
            SetUnlockedExtraSlots(UpgradeService.Instance.GetLevel(UpgradeType.Inventory));
```

- [ ] **Step 4: Compile and test (green)**

Unity MCP: `refresh_unity` (force, compile request, wait). `read_console` `error CS`: 0. `run_tests` EditMode: all pass (12 from 3c-2 + 3 new = 15).
Run (project root): `grep -nE "OxygenRope|PlayerHealth|UnlockNextSlot|SetUnlockedExtraSlots" Assets/Project_Ocean/Scripts/Gameplay/Services/UpgradeService.cs`. Expected: only the `DevGrantNextLevel` precondition line that mentions `OxygenRope`, and comment text.

---

### Task 3: Verify live, then docs

- [ ] **Step 1: Back up saves**

```bash
rm -rf "$TEMP/p3d_savebackup" && cp -r "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" "$TEMP/p3d_savebackup" && echo backed-up
```

- [ ] **Step 2: Rope, inventory and gun upgrades in Play**

Play from Bootstrap (`Application.runInBackground = true`) → MainMenu → New Game (slot 9) → Land_Map. With `GameManager.CurPlayer` present, run `execute_code` for each upgrade asset. Find them with `UnityEditor.AssetDatabase.FindAssets("t:UpgradeData")` and load the one per `type`. Record before and after:
- Rope: `tube.MaxRopeLength`, `PlayerHealth` private `originalMaxRopeLength` (reflection), `MaxHealth`. Call `UpgradeService.Instance.DevGrantNextLevel(rope, out var r)`. Expected: rope max and original max both rise by `rope.value`; `GetLevel(Rope)` is +1.
- Inventory: count the unlocked inventory slots before and after `DevGrantNextLevel(inventory, out _)`. Use whatever `InventorySlot` exposes for its lock state; read the fields with reflection if needed. Expected: +1 unlocked slot.
- NormalBullet: `DevGrantNextLevel(gun, out _)`. Expected: level +1, no errors.
- Also buy one Rope level the normal way: `SetMoneySilent(99999)` on `PlayerMoney`, then `UpgradeService.Instance.TryBuyUpgrade(rope)`. Expected: returns true, and the same +value effect.

- [ ] **Step 3: Load restores inventory slots**

`SaveBridge.Instance.SaveGame(9)`, then note the unlocked slot count and `GetLevel(Inventory)`. `SaveBridge.Instance.LoadGame(9)`, wait until `!LoadingManager.IsLoading`, and compare. Expected: the same unlocked slot count and levels. Rope max length equals the saved value, not doubled. Exit Play.

- [ ] **Step 4: Restore, dirty check, metric, docs**

```bash
rm -rf "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && cp -r "$TEMP/p3d_savebackup" "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && diff -rq "$TEMP/p3d_savebackup" "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && echo RESTORED
git status --short -- "*.unity" "*.prefab"
python tools/uml/gen_uml.py
grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
graphify update .
```
Append to the `**Progress:**` line under `### Phase 3` in `docs/uml/findings.md`: ` 3d done: UpgradeService raises OnLevelGranted/OnLevelsLoaded; PlayerHealth and InventoryManager apply their own effects (<new count> links).` Add ` — done in 3d (DevGrantNextLevel's InventoryManager check waits for 3e)` at the end of the Phase 3 bullet starting with "`UpgradeService`:".

- [ ] **Step 5: Hand-off**

Suggested commit: "refactor(upgrades): UpgradeService raises events, receivers apply effects". Do not commit.
