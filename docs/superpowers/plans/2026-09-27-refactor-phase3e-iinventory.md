# Refactor Phase 3e: Gameplay Talks to the Inventory Through IInventory — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Gameplay code (`Player`, `QuestManager`, `ItemDrop`, `EnemyLoot`, `CheckPointItem`, `QuestVisuals`, `TutorialPromptUI`, `DevPanel`, `SaveBridge`, `UpgradeService`) stops referencing the UI class `InventoryManager`. It uses a Gameplay interface `IInventory` plus a small static `Inventory` access point. This removes the 10 inventory wrong-way links with no behaviour change.

**Architecture:** `Gameplay/Inventory/IInventory.cs` declares the members gameplay actually calls. `Gameplay/Inventory/Inventory.cs` holds `Current` (registered by `InventoryManager.Awake`, cleared in `OnDestroy`) and the `OnChanged` delegate. `InventoryManager.OnInventoryChanged` becomes a property forwarding to `Inventory.OnChanged`, so every existing UI subscriber and raiser (`ShopManager`, `DraggableItem`, `InventoryManager.NotifyChanged`) keeps compiling and firing unchanged. Callers swap `InventoryManager.Instance` → `Inventory.Current`. `?.` and `!= null` still work, because `Current` is cleared on destroy.

**Tech Stack:** Unity 6000.3.6f1 + Unity MCP, NUnit EditMode.

## Global Constraints

- The user chose "IInventory interface" on 2026-09-27 (no data/view rewrite).
- Behaviour identical; no scene/prefab edits; `InventoryManager` stays in `UI/Inventory`.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Back up the save folder `C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean` before Play and restore it byte-identical after; after Play `git status --short -- "*.unity" "*.prefab"` → report.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`; retry once on a transient timeout.
- Comment rules (CLAUDE.md).

---

### Task 1: IInventory + Inventory access point (TDD)

**Files:**
- Create: `Assets/Project_Ocean/Scripts/Gameplay/Inventory/IInventory.cs`, `Assets/Project_Ocean/Scripts/Gameplay/Inventory/Inventory.cs` (create the folder `Gameplay/Inventory` via `AssetDatabase.CreateFolder` first)
- Modify: `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryManager.cs`
- Test: `Assets/Project_Ocean/Tests/EditMode/InventoryAccessTests.cs`

**Interfaces:**
- Produces:
  - `public interface IInventory { IReadOnlyList<Item> AllItems { get; } int ActiveBuildingSlotIndex { get; } bool AddItem(Item item); Item FindItemInDatabase(int id); bool ConsumeItem(ItemType checkType, out Item foundItem); Item PeekItem(ItemType checkType); int GetQuestItemCount(int itemId); void RemoveQuestItem(int itemId, int count = 1); void ClearBuildingSlot(int index); void RefreshBuildingHints(); void ResetToDefault(); }`
  - `public static class Inventory { public static IInventory Current { get; } public static System.Action OnChanged; public static void Register(IInventory); public static void Unregister(IInventory); }`

- [ ] **Step 1: Failing test**

`Assets/Project_Ocean/Tests/EditMode/InventoryAccessTests.cs`:
```csharp
using Game.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class InventoryAccessTests
    {
        private class FakeInventory : IInventory
        {
            public System.Collections.Generic.IReadOnlyList<Item> AllItems => null;
            public int ActiveBuildingSlotIndex => -1;
            public bool AddItem(Item item) => true;
            public Item FindItemInDatabase(int id) => null;
            public bool ConsumeItem(ItemType checkType, out Item foundItem) { foundItem = null; return false; }
            public Item PeekItem(ItemType checkType) => null;
            public int GetQuestItemCount(int itemId) => 0;
            public void RemoveQuestItem(int itemId, int count = 1) { }
            public void ClearBuildingSlot(int index) { }
            public void RefreshBuildingHints() { }
            public void ResetToDefault() { }
        }

        [TearDown]
        public void TearDown() => Inventory.Unregister(Inventory.Current);

        [Test]
        public void Register_SetsCurrent_UnregisterOfOtherInstance_KeepsIt()
        {
            var a = new FakeInventory();
            var b = new FakeInventory();
            Inventory.Register(a);
            Inventory.Unregister(b);
            Assert.AreSame(a, Inventory.Current);
            Inventory.Unregister(a);
            Assert.IsNull(Inventory.Current);
        }

        [Test]
        public void InventoryManager_ImplementsIInventory_AndForwardsOnInventoryChanged()
        {
            Assert.IsTrue(typeof(IInventory).IsAssignableFrom(typeof(InventoryManager)));
            int calls = 0;
            System.Action h = () => calls++;
            Inventory.OnChanged += h;
            InventoryManager.OnInventoryChanged?.Invoke();
            Inventory.OnChanged -= h;
            Assert.AreEqual(1, calls);
        }
    }
}
```
`refresh_unity` + `read_console` `error CS`. Expected red: `CS0246` for `IInventory` and `CS0103` for `Inventory`.

- [ ] **Step 2: Create `IInventory.cs`**

```csharp
using System.Collections.Generic;

namespace Game.Testing
{
    /// <summary>The inventory operations gameplay needs; implemented by the UI InventoryManager.</summary>
    public interface IInventory
    {
        IReadOnlyList<Item> AllItems { get; }
        int ActiveBuildingSlotIndex { get; }
        bool AddItem(Item item);
        Item FindItemInDatabase(int id);
        bool ConsumeItem(ItemType checkType, out Item foundItem);
        Item PeekItem(ItemType checkType);
        int GetQuestItemCount(int itemId);
        void RemoveQuestItem(int itemId, int count = 1);
        void ClearBuildingSlot(int index);
        void RefreshBuildingHints();
        void ResetToDefault();
    }
}
```

- [ ] **Step 3: Create `Inventory.cs`**

```csharp
namespace Game.Testing
{
    /// <summary>Access point to the live inventory for gameplay code, without referencing the UI class.</summary>
    public static class Inventory
    {
        public static IInventory Current { get; private set; }

        /// <summary>Raised whenever inventory contents change (InventoryManager.OnInventoryChanged forwards here).</summary>
        public static System.Action OnChanged;

        public static void Register(IInventory inventory) => Current = inventory;

        // Only the registered instance may clear it, so a destroyed duplicate can't wipe the live one.
        public static void Unregister(IInventory inventory)
        {
            if (Current == inventory) Current = null;
        }
    }
}
```

- [ ] **Step 4: `InventoryManager` implements and registers**

- Declaration: change `public class InventoryManager : MonoBehaviour, ISaveParticipant` to `public class InventoryManager : MonoBehaviour, ISaveParticipant, IInventory`.
- Replace `        public static System.Action OnInventoryChanged;` with:
```csharp
        // Forwards to Inventory.OnChanged so gameplay listens without referencing this UI class.
        public static System.Action OnInventoryChanged
        {
            get => Inventory.OnChanged;
            set => Inventory.OnChanged = value;
        }
```
- In `Awake()`, directly after `Instance = this;`, add `            Inventory.Register(this);`.
- Add (or extend an existing `OnDestroy`):
```csharp
        private void OnDestroy()
        {
            if (Instance == this)
                Inventory.Unregister(this);
        }
```
- The existing public members already match the interface signatures (`AllItems`, `ActiveBuildingSlotIndex`, `AddItem`, `FindItemInDatabase`, `ConsumeItem`, `PeekItem`, `GetQuestItemCount`, `RemoveQuestItem(int, int count = 1)`, `ClearBuildingSlot`, `RefreshBuildingHints`, `ResetToDefault`). If any signature differs, adjust the interface to the existing signature, not the other way round, and note it.

- [ ] **Step 5: Green**

`refresh_unity` (force, compile request, wait); `read_console` `error CS`: 0; `run_tests` EditMode: 19/19 (17 + 2).

---

### Task 2: Gameplay callers switch to `Inventory.Current`

**Files (paths relative to `Assets/Project_Ocean/Scripts/`):** `Gameplay/Entities/Players/DevPanel.cs`, `Gameplay/Entities/Players/Player.cs`, `Gameplay/InteractObjects/ItemDrop.cs`, `Gameplay/InteractObjects/CheckPointItem.cs`, `Gameplay/Entities/Enemies/EnemyLoot.cs`, `Gameplay/QuestSystems/Quest/QuestManager.cs`, `Gameplay/QuestSystems/Quest/QuestVisuals.cs`, `Gameplay/QuestSystems/Quest/TutorialPromptUI.cs`, `Gameplay/Services/UpgradeService.cs`, `Gameplay/Save/SaveBridge.cs`.

- [ ] **Step 1: Mechanical replacement (code only)**

Run (project root, Git Bash):
```bash
S=Assets/Project_Ocean/Scripts
for f in Gameplay/Entities/Players/DevPanel.cs Gameplay/Entities/Players/Player.cs Gameplay/InteractObjects/ItemDrop.cs Gameplay/InteractObjects/CheckPointItem.cs Gameplay/Entities/Enemies/EnemyLoot.cs Gameplay/QuestSystems/Quest/QuestManager.cs Gameplay/QuestSystems/Quest/QuestVisuals.cs Gameplay/QuestSystems/Quest/TutorialPromptUI.cs Gameplay/Services/UpgradeService.cs Gameplay/Save/SaveBridge.cs; do
  sed -i 's/\bInventoryManager\.Instance\b/Inventory.Current/g; s/\bInventoryManager\.OnInventoryChanged\b/Inventory.OnChanged/g' "$S/$f"
done
grep -nE "\bInventoryManager\b" $S/Gameplay -r --include=*.cs | grep -vE "^\S+:\s*//|//.*InventoryManager|\"[^\"]*InventoryManager"
```
Expected: the last grep prints only lines where `InventoryManager` appears inside a comment or a string literal, and at most the `SaveBridge` participant line handled in Step 2. List what remains in the report.

- [ ] **Step 2: `SaveBridge` participant line**

After Step 1, `BuildParticipants` has `if (Inventory.Current != null)     list.Add(Inventory.Current);`. That fails to compile, because `IInventory` is not an `ISaveParticipant`. Replace that line with:
```csharp
            if (Inventory.Current is ISaveParticipant inventory) list.Add(inventory);
```

- [ ] **Step 3: Green + no references left**

`refresh_unity` (force, compile request, wait); `read_console` `error CS`: 0; `run_tests` EditMode: 19/19.
```bash
python tools/uml/gen_uml.py && grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
grep -E "\| InventoryManager \|" docs/uml/metrics.md | head
```
Expected: the count drops by about 10 (from 16 to about 6). No layer-violation row has `InventoryManager` or `DraggableItem` as its To type.

---

### Task 3: Verify live, docs

- [ ] **Step 1: Back up saves**

```bash
rm -rf "$TEMP/p3e_savebackup" && cp -r "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" "$TEMP/p3e_savebackup" && echo backed-up
```

- [ ] **Step 2: Play checks**

Play from Bootstrap (`Application.runInBackground = true`; memory `feedback_unity_play_mode_unfocused.md`, `reference_live_enemy_test_route.md`). Check each item with `execute_code`:
1. `Inventory.Current` is the live `InventoryManager` (`ReferenceEquals(Inventory.Current, InventoryManager.Instance)`).
2. Item pickup: an `ItemDrop` or `EnemyLoot` collected in Ocean_Map goes into the inventory (count before and after, via `GetQuestItemCount` or slot contents). Leave Ocean_Map within 3 min.
3. Quest item flow: `QuestManager` reacts to inventory change. Add a quest-relevant item with `Inventory.Current.AddItem(item)` and confirm `QuestManager`'s handler ran: the tracked step count updates, or `OnQuestUpdated` fires.
4. Building slot / Mini Hub: `Player` uses `PeekItem`/`ConsumeItem(ItemType.Building)`. If a Building item is available, place one via the player path and confirm the item count drops and the hints refresh. Otherwise mark UNVERIFIED with the reason.
5. Save/load: `SaveBridge.Instance.SaveGame(9)` → `LoadGame(9)`. Inventory contents are identical after load (slot, item ID, count).
Exit Play.

- [ ] **Step 3: Restore, dirty check, docs**

```bash
rm -rf "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && cp -r "$TEMP/p3e_savebackup" "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && diff -rq "$TEMP/p3e_savebackup" "C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean" && echo RESTORED
git status --short -- "*.unity" "*.prefab"
python tools/uml/test_gen_uml.py
graphify update .
```
In `docs/uml/findings.md`: append ` 3e done: gameplay uses IInventory/Inventory.Current instead of InventoryManager (<new count> links).` to the `**Progress:**` line under `### Phase 3`. At the end of the Phase 3 bullet starting with "- `InventoryManager`:", add ` — done in 3e as an interface (IInventory); a full data/view split is still possible later if inventory grows.`

- [ ] **Step 4: Hand-off**

Suggested commit: "refactor(inventory): gameplay uses IInventory instead of InventoryManager". Do not commit.
