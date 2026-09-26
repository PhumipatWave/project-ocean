# Refactor Phase 3c-2: Each System Saves Its Own Slice — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `SaveBridge` stops reading and writing player money, rope length, inventory and day state by hand. `PlayerHealth`, `PlayerMoney`, `InventoryManager` and `DayManager` implement `ISaveParticipant` and own their slice. The save file content and the apply order stay exactly the same.

**Architecture:** Move each `Collect*`/`Apply*` helper body out of `SaveBridge` verbatim into the owning class's `Collect(SaveData)` / `Apply(SaveData)`. `SaveBridge.BuildParticipants()` stays the single, explicitly ordered list (its documented design). It now starts with the player components, inventory and day, in the order `ApplyAfterFrame` used to call the helpers. Only `CollectPosition` stays in `SaveBridge`, because position is applied by `GameManager` through `SceneLoadContext`. A save → load → save round trip against a baseline file captured with the old code proves nothing changed.

**Tech Stack:** Unity 6000.3.6f1 + Unity MCP, NUnit EditMode (`Assets/Project_Ocean/Tests/EditMode`, asmdef `Game.Tests.EditMode` referencing `Game.Testing`), `JsonUtility`.

## Global Constraints

- Hybrid step 2, approved by the user on 2026-09-26. Runs after Phase 3c-1 (`SaveBridge` now at `Assets/Project_Ocean/Scripts/Gameplay/Save/SaveBridge.cs`).
- Save format unchanged: no `SaveData` field added, removed or renamed; `saveVersion` untouched.
- Apply order unchanged: rope, money, inventory, day, then upgrade, quest, kill tracker, pickup tracker, feature unlocks.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Back up the save folder `C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean` before any Play session. Restore it byte-identical afterwards, and delete test slots. A player standing still in Ocean_Map auto-returns to land after about 4 min, and that auto-saves.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`; retry once on a transient timeout. After Play: `git status --short -- "*.unity" "*.prefab"` must be empty (report it if not; don't restore).
- Comment rules (CLAUDE.md): short WHY comments, no history. Moved code keeps its existing comments.

## File map (paths relative to `Assets/Project_Ocean/Scripts/`)

| File | Change |
|---|---|
| `Gameplay/Entities/Players/PlayerMoney.cs` | add `ISaveParticipant` (money) |
| `Gameplay/Entities/Players/PlayerHealth.cs` | add `ISaveParticipant` (max rope length) |
| `Core/Save/DayReset/DayManager.cs` | add `ISaveParticipant` (day, dives) |
| `UI/Inventory/InventoryManager.cs` | add `ISaveParticipant` (inventory items; body moved from `SaveBridge`) |
| `Gameplay/Save/SaveBridge.cs` | remove `CollectPlayerStats`, `CollectInventory`, `CollectDay`, `ApplyPlayerStats`, `ApplyInventory`, `ApplyDay` and their calls; extend `BuildParticipants()` |
| `Assets/Project_Ocean/Tests/EditMode/SaveParticipantTests.cs` | new round-trip tests for `PlayerMoney` and `DayManager` |

---

### Task 1: Capture a baseline save with the old code

**Files:** none changed.

**Interfaces:**
- Produces: `$TEMP/p3c2_baseline.json`: the decoded `SaveData` JSON of a known game state, saved by the old code in test slot 7.

- [ ] **Step 1: Back up the save folder**

Run:
```bash
rm -rf "$TEMP/p3c2_savebackup" && cp -r "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" "$TEMP/p3c2_savebackup" && echo backed-up
```
Expected: `backed-up`.

- [ ] **Step 2: Build a known state and save to slot 7**

Enter Play from `Assets/Project_Ocean/Levels/Scenes/Loading/Bootstrap.unity`, then set `Application.runInBackground = true`. Go MainMenu → New Game in slot 7 (the SaveMenuUI new-game path, or `SaveBridge.Instance.ResetForNewGame(); SaveBridge.Instance.BeginNewGameSession(7);` then `LoadingManager.Instance.LoadScene("Land_Map", LoadingType.Land)`). When Land_Map is up and `GameManager.CurPlayer` exists, run `execute_code`:
```csharp
var p = Game.Testing.GameManager.CurPlayer;
p.GetComponent<Game.Testing.PlayerMoney>().SetMoneySilent(4321);
p.GetComponent<Game.Testing.PlayerHealth>().SetOriginalMaxLength(37.5f);
var inv = Game.Testing.InventoryManager.Instance;
var item = inv.FindItemInDatabase(1) ?? inv.FindItemInDatabase(2) ?? inv.FindItemInDatabase(100);
if (item != null) inv.SpawnItemAtSlot(item, 1, 3);
Game.Testing.DayManager.Instance.SetDay(5);
Game.Testing.DayManager.Instance.SetDivesToday(2);
Game.Testing.DayManager.Instance.SetTotalDives(9);
Game.Testing.SaveBridge.Instance.SaveGame(7);
var d = Game.Testing.SaveSystem.Load(7);
return UnityEngine.JsonUtility.ToJson(d, true) + "\nitem=" + (item == null ? "NONE" : item.ItemID.ToString());
```
If `FindItemInDatabase` returns null for all three IDs, look at the item database's `ItemID` values (via reflection on `InventoryManager`) and use a valid one. Save the returned JSON (without the `item=` line) to `$TEMP/p3c2_baseline.json` using the Write tool. Expected: `money` 4321, `maxRopeLength` 37.5, `currentDay` 5, `divesToday` 2, `totalDives` 9, one `inventoryItems` entry with `slotIndex` 1 and `count` 3.
Exit Play mode.

---

### Task 2: Participant round-trip tests (red)

**Files:**
- Create: `Assets/Project_Ocean/Tests/EditMode/SaveParticipantTests.cs`

**Interfaces:**
- Consumes: `ISaveParticipant` (`Collect(SaveData)`, `Apply(SaveData)`), `SaveData` fields `money`, `currentDay`, `divesToday`, `totalDives`, `hasDivedToday`.

- [ ] **Step 1: Write the failing tests**

```csharp
using Game.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class SaveParticipantTests
    {
        private GameObject go;

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(go);

        [Test]
        public void PlayerMoney_RoundTripsThroughSaveData()
        {
            go = new GameObject("money");
            var money = go.AddComponent<PlayerMoney>();
            money.SetMoneySilent(4321);

            var data = new SaveData();
            ISaveParticipant participant = money;
            participant.Collect(data);
            money.SetMoneySilent(0);
            participant.Apply(data);

            Assert.AreEqual(4321, data.money);
            Assert.AreEqual(4321, money.CurrentMoney);
        }

        [Test]
        public void DayManager_RoundTripsThroughSaveData()
        {
            go = new GameObject("day");
            var day = go.AddComponent<DayManager>();
            day.SetDay(5);
            day.SetDivesToday(2);
            day.SetTotalDives(9);

            var data = new SaveData();
            ISaveParticipant participant = day;
            participant.Collect(data);
            day.SetDay(1);
            day.SetDivesToday(0);
            day.SetTotalDives(0);
            participant.Apply(data);

            Assert.AreEqual(5, day.CurrentDay);
            Assert.AreEqual(2, day.DivesToday);
            Assert.AreEqual(9, day.TotalDives);
        }

        [Test]
        public void DayManager_OldSaveWithOnlyDivedFlag_CountsAsOneDive()
        {
            go = new GameObject("day");
            var day = go.AddComponent<DayManager>();
            var data = new SaveData { currentDay = 3, divesToday = 0, hasDivedToday = true };

            ((ISaveParticipant)day).Apply(data);

            Assert.AreEqual(1, day.DivesToday);
        }
    }
}
```

- [ ] **Step 2: Confirm red**

Unity MCP: `refresh_unity` (compile request, wait), then `read_console` `error CS`. Expected: compile errors `CS0266` or `CS0029` ("Cannot implicitly convert type 'PlayerMoney' to 'ISaveParticipant'"), and the same for `DayManager`. Record them. The test assembly can't compile, and that is the expected red.

---

### Task 3: Move the slices into their owners

**Files:** `PlayerMoney.cs`, `PlayerHealth.cs`, `DayManager.cs`, `InventoryManager.cs`, `SaveBridge.cs` (see File map).

**Interfaces:**
- Produces: `PlayerMoney`, `PlayerHealth`, `DayManager`, `InventoryManager` each `: ISaveParticipant`. `SaveBridge.BuildParticipants()` returns them in the Global Constraints apply order.

- [ ] **Step 1: `PlayerMoney`**

Change the declaration `public class PlayerMoney : MonoBehaviour` to `public class PlayerMoney : MonoBehaviour, ISaveParticipant`, and add these members at the end of the class:
```csharp
        public void Collect(SaveData data) => data.money = CurrentMoney;

        public void Apply(SaveData data) => SetMoneySilent(data.money);
```

- [ ] **Step 2: `PlayerHealth`**

Change `public class PlayerHealth : EntityHealth` to `public class PlayerHealth : EntityHealth, ISaveParticipant`, and add at the end of the class:
```csharp
        public void Collect(SaveData data)
        {
            if (oxygenTube != null)
                data.maxRopeLength = oxygenTube.MaxRopeLength;
        }

        // SetOriginalMaxLength syncs both the rope and this snapshot, so ResetToOriginal()
        // after the next dive restores the saved upgraded max.
        public void Apply(SaveData data)
        {
            if (oxygenTube != null)
                SetOriginalMaxLength(data.maxRopeLength);
        }
```

- [ ] **Step 3: `DayManager`**

Change `public class DayManager : MonoBehaviour` to `public class DayManager : MonoBehaviour, ISaveParticipant`, and add at the end of the class:
```csharp
        public void Collect(SaveData data)
        {
            data.currentDay    = CurrentDay;
            data.hasDivedToday = HasDivedToday;
            data.divesToday    = DivesToday;
            data.totalDives    = TotalDives;
        }

        public void Apply(SaveData data)
        {
            SetDay(data.currentDay);
            // Saves from before divesToday existed only carry the bool — count that as one dive.
            SetDivesToday(data.divesToday > 0 ? data.divesToday : (data.hasDivedToday ? 1 : 0));
            SetTotalDives(data.totalDives);
        }
```

- [ ] **Step 4: `InventoryManager`**

Change `public class InventoryManager : MonoBehaviour` to `public class InventoryManager : MonoBehaviour, ISaveParticipant`. Add `Collect` and `Apply` at the end of the class. They take the bodies of `SaveBridge.CollectInventory` / `SaveBridge.ApplyInventory` with these substitutions only: `InventoryManager.Instance.InventorySlots` / `inv.InventorySlots` → `InventorySlots`; `inv.FindItemInDatabase` → `FindItemInDatabase`; `inv.SpawnItemAtSlot` → `SpawnItemAtSlot`; the `InventoryManager.Instance == null` / `inv == null` guards removed; log prefixes `[SaveBridge]` → `[InventoryManager]`. Every comment stays, including the LogError data-loss text. Exact code:
```csharp
        public void Collect(SaveData data)
        {
            data.inventoryItems = new List<ItemSaveData>();
            var slots = InventorySlots;
            for (int i = 0; i < slots.Length; i++)
            {
                var item = slots[i].GetComponentInChildren<DraggableItem>();
                if (item?.GetItem() == null) continue;

                data.inventoryItems.Add(new ItemSaveData
                {
                    slotIndex = i,
                    itemID    = item.GetItem().ItemID.ToString(),
                    count     = item.count,
                });
            }
        }

        public void Apply(SaveData data)
        {
            // Clear all existing items first
            foreach (var slot in InventorySlots)
            {
                var existing = slot.GetComponentInChildren<DraggableItem>();
                if (existing != null) Destroy(existing.gameObject);
            }

            foreach (var saved in data.inventoryItems)
            {
                if (!int.TryParse(saved.itemID, out int id))
                {
                    Debug.LogWarning($"[InventoryManager] Invalid itemID: '{saved.itemID}'");
                    continue;
                }

                // Look up item from the allItems database — NOT from current inventory slots
                var item = FindItemInDatabase(id);
                if (item == null)
                {
                    // LogError (not LogWarning): this means the item was silently dropped
                    // from the loaded inventory — a data-loss bug, not a minor issue.
                    // Usually caused by the item's ScriptableObject missing from
                    // InventoryManager's "Item Database -> All Items" list in the Inspector.
                    Debug.LogError($"[InventoryManager] Item ID {id} not found in allItems database — " +
                                    $"item was DROPPED from inventory on load! Add its ScriptableObject to " +
                                    $"InventoryManager's Item Database list.");
                    continue;
                }

                if (saved.slotIndex >= InventorySlots.Length)
                {
                    Debug.LogWarning($"[InventoryManager] slotIndex {saved.slotIndex} out of range");
                    continue;
                }

                // Spawn with the saved stack count in one step — the item is born
                // with the correct count and a single RefreshCount(), so the count
                // label shows immediately regardless of load order.
                SpawnItemAtSlot(item, saved.slotIndex, Mathf.Max(1, saved.count));
            }
        }
```
If `InventoryManager.cs` has no `using System.Collections.Generic;`, add it at the top.
Special case: when there was no `InventoryManager`, the old `CollectInventory` still set `data.inventoryItems` to an empty list. `SaveBridge` keeps that default (Step 5), so the file content is the same when the manager is missing.

- [ ] **Step 5: `SaveBridge`**

Replace `BuildParticipants()` with:
```csharp
        // Order is intentional and stable: it is also the apply order on load. Built fresh each
        // call because the player and scene-level managers may not exist at save time.
        private List<ISaveParticipant> BuildParticipants()
        {
            var list = new List<ISaveParticipant>();
            var player = GameManager.CurPlayer;
            if (player != null)
            {
                var health = player.GetComponent<PlayerHealth>();
                if (health != null) list.Add(health);
                var money = player.GetComponent<PlayerMoney>();
                if (money != null) list.Add(money);
            }
            if (InventoryManager.Instance != null)     list.Add(InventoryManager.Instance);
            if (DayManager.Instance != null)           list.Add(DayManager.Instance);
            list.Add(UpgradeService.Instance);
            if (QuestManager.Instance != null)         list.Add(QuestManager.Instance);
            if (EnemyKillTracker.Instance != null)     list.Add(EnemyKillTracker.Instance);
            if (OneTimePickupTracker.Instance != null) list.Add(OneTimePickupTracker.Instance);
            // Must come after QuestManager — it re-derives unlocks from quest progress on load.
            list.Add(FeatureUnlockService.Instance);
            return list;
        }
```
In `ApplyAfterFrame`, delete these three lines:
```csharp
                ApplyPlayerStats(data);
                ApplyInventory(data);
                ApplyDay(data);
```
and change the comment above the `foreach` to `// Every slice is applied by its owning system, in the fixed order of BuildParticipants.`
In `SaveGame(int slot)`, replace:
```csharp
            CollectPosition(data);
            CollectPlayerStats(data);
            CollectInventory(data);
            CollectDay(data);
```
with:
```csharp
            CollectPosition(data);
            data.inventoryItems = new List<ItemSaveData>();
```
and change the comment above the `foreach` to `// Every slice is collected by its owning system (see BuildParticipants).`
Delete the methods `ApplyPlayerStats`, `ApplyInventory`, `ApplyDay`, `CollectPlayerStats`, `CollectInventory`, `CollectDay`. Keep `CollectPosition`.

- [ ] **Step 6: Compile and test (green)**

Unity MCP: `refresh_unity` (force, compile request, wait). `read_console` `error CS`: 0. `run_tests` EditMode: 12/12 pass (9 old + 3 new).

- [ ] **Step 7: Nothing left behind**

Run (project root):
```bash
grep -nE "ApplyPlayerStats|ApplyInventory|ApplyDay|CollectPlayerStats|CollectInventory|CollectDay|DraggableItem|PlayerMoney|DayManager.Instance.Set" Assets/Project_Ocean/Scripts/Gameplay/Save/SaveBridge.cs
```
Expected: only lines inside `BuildParticipants` (`PlayerMoney`) and `ResetForNewGame` (`DayManager.Instance.SetDay(1)` etc. are unchanged there). No helper names remain.

---

### Task 4: Prove the save is unchanged (live round trip)

**Files:** none changed.

- [ ] **Step 1: Load the baseline, save again, compare**

Enter Play from Bootstrap (`Application.runInBackground = true`). Run `SaveBridge.Instance.LoadGame(7)` and wait until loading has finished (`!LoadingManager.IsLoading`, and `GameManager.CurPlayer` is non-null in Land_Map). Then `execute_code`:
```csharp
var p = Game.Testing.GameManager.CurPlayer;
string live = "money=" + p.GetComponent<Game.Testing.PlayerMoney>().CurrentMoney +
              " rope=" + p.OxygenTube.MaxRopeLength +
              " day=" + Game.Testing.DayManager.Instance.CurrentDay +
              " dives=" + Game.Testing.DayManager.Instance.DivesToday +
              " total=" + Game.Testing.DayManager.Instance.TotalDives;
Game.Testing.SaveBridge.Instance.SaveGame(8);
return live + "\n" + UnityEngine.JsonUtility.ToJson(Game.Testing.SaveSystem.Load(8), true);
```
Expected live line: `money=4321 rope=37.5 day=5 dives=2 total=9`, and the inventory slot 1 shows the item with count 3.
Write the JSON to `$TEMP/p3c2_after.json`, then compare:
```bash
diff <(grep -v '"saveTime"' "$TEMP/p3c2_baseline.json") <(grep -v '"saveTime"' "$TEMP/p3c2_after.json") && echo "SAVE CONTENT IDENTICAL"
```
Expected: `SAVE CONTENT IDENTICAL`. If position fields differ slightly because of spawn placement, report the diff lines. Any other difference means STOP and report DONE_WITH_CONCERNS with the diff.
Exit Play mode.

- [ ] **Step 2: New-game path still saves**

In Play again: MainMenu → New Game (slot 9) → Land_Map, wait for the auto-save, then `JsonUtility.ToJson(SaveSystem.Load(9))`. Expected: the file exists with default values (day 1, `money` = the prefab default, empty or default inventory). There is no baseline for this, so check that nothing is missing or zero where the old game would have had a value. Exit Play.

- [ ] **Step 3: Restore and clean**

```bash
rm -rf "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && cp -r "$TEMP/p3c2_savebackup" "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && diff -rq "$TEMP/p3c2_savebackup" "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && echo RESTORED
git status --short -- "*.unity" "*.prefab"
```
Expected: `RESTORED`; the git status output is empty.

- [ ] **Step 4: Metric and docs**

Run (project root):
```bash
python tools/uml/gen_uml.py
grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
grep -E "\| SaveBridge \|" docs/uml/metrics.md | head -5
graphify update .
```
Record the count. `SaveBridge` should no longer depend on `PlayerHealth`, `PlayerMoney` or `DraggableItem`, so the "Most dependencies (fan-out)" row for `SaveBridge` drops. Append to the `**Progress:**` line under `### Phase 3` in `docs/uml/findings.md`: ` 3c-2 done: PlayerHealth, PlayerMoney, InventoryManager, DayManager save their own slice via ISaveParticipant (save content verified identical).` In the Phase 3 bullet starting with "`SaveBridge`:", add ` — done in 3c-2` at the end.

- [ ] **Step 5: Hand-off**

Suggested commit: "refactor(save): each system saves its own slice via ISaveParticipant". Do not commit.
