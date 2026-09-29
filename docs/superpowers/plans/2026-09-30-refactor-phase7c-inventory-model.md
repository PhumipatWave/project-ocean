# Refactor Phase 7c: Pure `InventoryModel` Owns the Inventory, InventoryManager Only Draws It — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Inventory contents, stacking, locks, removal, moves and save data live in a plain C# `InventoryModel` (Gameplay layer, fully EditMode-tested). `InventoryManager` keeps its public API but becomes the view + entry point: it owns the model and re-renders the slot UI when the model changes.

**Architecture:** Second step of 7b → 7c → 7d. After 7b every caller already uses `InventoryManager`'s stack API, so this phase only rewrites `InventoryManager`'s internals and deletes the view-walking helpers. One index space: player slots `0..PlayerSlotCount-1`, building slots after them. The model raises `Changed` and `ItemDelta`; `InventoryManager` forwards them to the existing static events (`OnInventoryChanged`, `OnItemDelta`) and calls `Render()`.

**Tech Stack:** Unity 6000.3.6f1, C#, NUnit EditMode tests, Unity MCP (`refresh_unity`, `read_console`, `run_tests`, `execute_code`), Python 3 (`tools/uml`).

## Why this design (2026-09-30)

- **Model in `Game.Gameplay`, no `MonoBehaviour`, no `UnityEngine.Object` except `Item`:** testable without a scene, and gameplay could hold it directly later without touching UI (not done now: `IInventory` already covers gameplay).
- **Render = sync views to model**, reusing an existing `DraggableItem` when the slot already shows one. A view to remove is detached before `Destroy`, because `Destroy` is deferred and a second render in the same frame must not find it again.
- **Drag keeps Unity's drag handling** (`DraggableItem` still re-parents itself while dragging). On drop, `HandleItemDropped` calls `model.Move`; if the move is refused, `Render()` puts the view back where the model says it is.
- **`ShopManager`'s 0.02 s / 0.05 s `Invoke(RefreshShopInventory)`** existed only because destroyed views stayed visible until end of frame. The model updates at once and `OnInventoryChanged` already refreshes the mirror, so those delays go.
- **Out of scope:** generated slots (7d), the `ShopManager.SellRoutine` dead coroutine, building-slot hint panels (stay hand-wired; there is one building slot).

## Success criteria (all must hold at the end)

1. `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryQuestService.cs` no longer exists (logic moved to the model).
2. `grep -rn "GetComponentInChildren<DraggableItem>\|\.count\b" Assets/Project_Ocean/Scripts/UI --include=*.cs` → no output (`DraggableItem.Count` is a read-only property; `InventoryManager.RenderSlot` uses `GetComponentsInChildren`).
3. `grep -rn "Invoke(nameof(RefreshShopInventory)" Assets/Project_Ocean/Scripts --include=*.cs` → no output.
4. EditMode tests: all pass, baseline B + 12 (B = count before Task 1; B+4 of Phase 7b).
5. Live: the 7b smoke list passes unchanged, plus a New Game after a run that had a Mini Hub starts with an empty building slot (see "Behaviour fix" in Task 2).

## Global Constraints

- Run after Phase 7b is committed.
- No behaviour change, except the one "Behaviour fix" in Task 2 Step 6.
- Save format unchanged (`SaveData`, `ItemSaveData`, `saveVersion`).
- `IInventory` and every public `InventoryManager` member used outside the class keep their exact signatures.
- Files keep their encoding, BOM and line endings. No scene/prefab edits.
- CLAUDE.md comment rules: 1–2 lines, why not what, XML `<summary>` on public API only, 1 sentence.
- The user commits. Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`. File deletion goes through `AssetDatabase.DeleteAsset` (keeps `.meta` in sync).
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`. "Compile" means `refresh_unity(mode=force, compile=request)` then `read_console(filter_text="error CS")` = 0 entries and `EditorUtility.scriptCompilationFailed == false`. "Run tests" means `run_tests(mode=EditMode)` + `get_test_job(wait_timeout=60)`.
- Back up `C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean` before any Play session and restore it byte-identical after (RESTORED). After Play: `git status --short -- "*.unity" "*.prefab"` shows only pre-existing changes.

---

### Task 1: `InventoryModel` (TDD, no callers yet)

**Files:**
- Create: `Assets/Project_Ocean/Scripts/Gameplay/Inventory/InventoryModel.cs`
- Create: `Assets/Project_Ocean/Tests/EditMode/InventoryModelTests.cs`

**Interfaces:**
- Consumes: `Game.Gameplay.ItemStack`, `Game.Tests.TestItems` (Phase 7b Task 1).
- Produces `Game.Gameplay.InventoryModel`:
  - ctor `InventoryModel(int playerSlots, int buildingSlots)` — all slots start unlocked and empty.
  - `event System.Action Changed`; `event System.Action<Item, int> ItemDelta`
  - `int PlayerSlotCount`, `int BuildingSlotCount`, `int SlotCount`
  - `int BuildingSlot(int buildingIndex)`; `ItemStack Get(int slot)`; `bool IsUnlocked(int slot)`
  - `bool Add(Item item)`; `Item RemoveOne(int slot)`; `ItemStack RemoveStack(int slot)`
  - `int CountOf(int itemId)`; `int Remove(int itemId, int count)`
  - `Dictionary<Item, int> RemoveAllWhere(System.Predicate<Item> match)`
  - `bool Move(int from, int to)`; `bool UnlockNext()`; `void ResetLocks(int firstUnlocked)`
  - `void Load(ItemStack[] player, ItemStack[] building)`; `void ClearItems()`
  - `ItemStack[] PlayerStacks()`; `ItemStack[] BuildingStacks()`

- [ ] **Step 1: Record baseline** — run tests, write the count as **B**.

- [ ] **Step 2: Write the failing tests**

`Assets/Project_Ocean/Tests/EditMode/InventoryModelTests.cs`:
```csharp
using System.Collections.Generic;
using Game.Core;
using Game.Gameplay;
using NUnit.Framework;

namespace Game.Tests
{
    public class InventoryModelTests
    {
        [TearDown] public void TearDown() => TestItems.DestroyAll();

        [Test]
        public void Add_StacksUpToMax_ThenUsesNextFreeSlot()
        {
            var model = new InventoryModel(3, 1);
            Item fish = TestItems.Make(1, maxStack: 2);

            Assert.IsTrue(model.Add(fish));
            Assert.IsTrue(model.Add(fish));
            Assert.IsTrue(model.Add(fish));

            Assert.AreEqual(2, model.Get(0).Count);
            Assert.AreEqual(1, model.Get(1).Count);
            Assert.IsTrue(model.Get(2).IsEmpty);
        }

        [Test]
        public void Add_EarlierEmptySlotWinsOverLaterStack()
        {
            // Same scan order as the old slot-based code: first slot that fits, empty or stackable.
            var model = new InventoryModel(2, 0);
            Item fish = TestItems.Make(1, maxStack: 5);
            model.Load(new[] { ItemStack.Empty, new ItemStack(fish, 1) }, null);

            model.Add(fish);

            Assert.AreEqual(1, model.Get(0).Count);
            Assert.AreEqual(1, model.Get(1).Count);
        }

        [Test]
        public void Add_SkipsLockedSlots_AndFailsWhenFull()
        {
            var model = new InventoryModel(2, 0);
            model.ResetLocks(1);
            Item a = TestItems.Make(1), b = TestItems.Make(2);

            Assert.IsTrue(model.Add(a));
            Assert.IsFalse(model.Add(b));
            Assert.IsTrue(model.Get(1).IsEmpty);
        }

        [Test]
        public void Add_BuildingItem_OnlyGoesToBuildingSlots()
        {
            var model = new InventoryModel(2, 1);
            Item hub = TestItems.Make(9, ItemType.Building);

            Assert.IsTrue(model.Add(hub));
            Assert.IsFalse(model.Add(hub)); // max stack 1, no second building slot

            Assert.AreSame(hub, model.Get(model.BuildingSlot(0)).Item);
            Assert.IsTrue(model.Get(0).IsEmpty && model.Get(1).IsEmpty);
        }

        [Test]
        public void Add_RaisesChangedThenPositiveDelta_LoadRaisesNoDelta()
        {
            var model = new InventoryModel(1, 0);
            Item fish = TestItems.Make(1);
            var log = new List<string>();
            model.Changed += () => log.Add("changed");
            model.ItemDelta += (item, d) => log.Add("delta " + d);

            model.Add(fish);
            model.Load(new[] { new ItemStack(fish, 1) }, null);

            CollectionAssert.AreEqual(new[] { "changed", "delta 1", "changed" }, log);
        }

        [Test]
        public void RemoveOne_DecrementsThenEmpties_NullWhenEmpty()
        {
            var model = new InventoryModel(1, 0);
            Item fish = TestItems.Make(1, maxStack: 5);
            model.Load(new[] { new ItemStack(fish, 2) }, null);

            Assert.AreSame(fish, model.RemoveOne(0));
            Assert.AreEqual(1, model.Get(0).Count);
            Assert.AreSame(fish, model.RemoveOne(0));
            Assert.IsTrue(model.Get(0).IsEmpty);
            Assert.IsNull(model.RemoveOne(0));
        }

        [Test]
        public void RemoveStack_EmptiesSlot_AndReportsFullCount()
        {
            var model = new InventoryModel(1, 0);
            Item fish = TestItems.Make(1, maxStack: 5);
            model.Load(new[] { new ItemStack(fish, 4) }, null);
            int delta = 0;
            model.ItemDelta += (item, d) => delta += d;

            ItemStack removed = model.RemoveStack(0);

            Assert.AreEqual(4, removed.Count);
            Assert.AreEqual(-4, delta);
            Assert.IsTrue(model.Get(0).IsEmpty);
        }

        [Test]
        public void CountOf_And_Remove_SpanSlots_PlayerSlotsOnly()
        {
            var model = new InventoryModel(2, 1);
            Item fish = TestItems.Make(1, maxStack: 2);
            model.Load(new[] { new ItemStack(fish, 2), new ItemStack(fish, 1) }, new[] { new ItemStack(fish, 1) });
            var deltas = new List<int>();
            model.ItemDelta += (item, d) => deltas.Add(d);

            Assert.AreEqual(3, model.CountOf(1));
            Assert.AreEqual(3, model.Remove(1, 5)); // only 3 held: removes what is there
            Assert.AreEqual(0, model.CountOf(1));
            CollectionAssert.AreEqual(new[] { -3 }, deltas);
            Assert.AreEqual(1, model.Get(model.BuildingSlot(0)).Count);
        }

        [Test]
        public void RemoveAllWhere_GroupsCountsPerItem_LeavesBuildingSlots()
        {
            var model = new InventoryModel(3, 1);
            Item fish = TestItems.Make(1, maxStack: 5), shell = TestItems.Make(2, maxStack: 5), hub = TestItems.Make(9, ItemType.Building);
            model.Load(new[] { new ItemStack(fish, 2), new ItemStack(shell, 1), new ItemStack(fish, 3) }, new[] { new ItemStack(hub, 1) });

            Dictionary<Item, int> removed = model.RemoveAllWhere(item => item.ItemType == ItemType.Normal);

            Assert.AreEqual(5, removed[fish]);
            Assert.AreEqual(1, removed[shell]);
            Assert.IsTrue(model.Get(0).IsEmpty && model.Get(1).IsEmpty && model.Get(2).IsEmpty);
            Assert.AreSame(hub, model.Get(model.BuildingSlot(0)).Item);
        }

        [Test]
        public void Move_OnlyIntoEmptyUnlockedOtherSlot()
        {
            var model = new InventoryModel(3, 0);
            model.ResetLocks(2);
            Item a = TestItems.Make(1), b = TestItems.Make(2);
            model.Load(new[] { new ItemStack(a, 1), new ItemStack(b, 1), ItemStack.Empty }, null);

            Assert.IsFalse(model.Move(0, 1)); // occupied
            Assert.IsFalse(model.Move(0, 2)); // locked
            Assert.IsFalse(model.Move(0, 0)); // same slot
            Assert.IsFalse(model.Move(-1, 2)); // not an inventory slot
            model.RemoveStack(1);
            Assert.IsTrue(model.Move(0, 1));
            Assert.IsTrue(model.Get(0).IsEmpty);
            Assert.AreSame(a, model.Get(1).Item);
        }

        [Test]
        public void ResetLocks_And_UnlockNext()
        {
            var model = new InventoryModel(4, 1);
            model.ResetLocks(2);

            Assert.IsTrue(model.IsUnlocked(1));
            Assert.IsFalse(model.IsUnlocked(2));
            Assert.IsTrue(model.IsUnlocked(model.BuildingSlot(0))); // building slots are never locked

            Assert.IsTrue(model.UnlockNext());
            Assert.IsTrue(model.IsUnlocked(2));
            Assert.IsTrue(model.UnlockNext());
            Assert.IsFalse(model.UnlockNext());
        }

        [Test]
        public void Stacks_AreCopies_AndLoadTruncatesExtraRows()
        {
            var model = new InventoryModel(2, 1);
            Item fish = TestItems.Make(1), hub = TestItems.Make(9, ItemType.Building);
            model.Load(new[] { new ItemStack(fish, 1), ItemStack.Empty, new ItemStack(fish, 1) }, new[] { new ItemStack(hub, 1) });

            ItemStack[] player = model.PlayerStacks();
            player[0] = ItemStack.Empty;

            Assert.AreEqual(2, player.Length);
            Assert.AreSame(fish, model.Get(0).Item);
            Assert.AreSame(hub, model.BuildingStacks()[0].Item);
            model.ClearItems();
            Assert.IsTrue(model.Get(model.BuildingSlot(0)).IsEmpty);
        }
    }
}
```

- [ ] **Step 3: Compile — expect red** (`CS0246: The type or namespace name 'InventoryModel' could not be found`).

- [ ] **Step 4: Create `InventoryModel`**

`Assets/Project_Ocean/Scripts/Gameplay/Inventory/InventoryModel.cs`:
```csharp
using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Inventory contents and rules without Unity objects: player slots first, then building slots.</summary>
    public sealed class InventoryModel
    {
        /// <summary>Raised after any change to contents or locks.</summary>
        public event Action Changed;
        /// <summary>Signed count of a player-visible item change; Load and ClearItems never raise it.</summary>
        public event Action<Item, int> ItemDelta;

        private readonly ItemStack[] stacks;
        private readonly bool[] unlocked;

        public int PlayerSlotCount { get; }
        public int BuildingSlotCount { get; }
        public int SlotCount => stacks.Length;

        public InventoryModel(int playerSlots, int buildingSlots)
        {
            PlayerSlotCount = playerSlots;
            BuildingSlotCount = buildingSlots;
            stacks = new ItemStack[playerSlots + buildingSlots];
            unlocked = new bool[stacks.Length];
            for (int i = 0; i < unlocked.Length; i++) unlocked[i] = true;
        }

        public int BuildingSlot(int buildingIndex) => PlayerSlotCount + buildingIndex;
        public ItemStack Get(int slot) => IsValid(slot) ? stacks[slot] : ItemStack.Empty;
        public bool IsUnlocked(int slot) => IsValid(slot) && unlocked[slot];

        /// <summary>Puts one item in the first slot that fits (empty, or same item below max stack); false when none does.</summary>
        public bool Add(Item item)
        {
            if (item == null) return false;

            bool building = item.ItemType == ItemType.Building;
            int end = building ? SlotCount : PlayerSlotCount;
            for (int i = building ? PlayerSlotCount : 0; i < end; i++)
            {
                if (!unlocked[i]) continue;

                ItemStack s = stacks[i];
                if (!s.IsEmpty && (s.Item != item || s.Count >= item.MaxStackable)) continue;

                stacks[i] = new ItemStack(item, s.IsEmpty ? 1 : s.Count + 1);
                Raise(item, 1);
                return true;
            }
            return false;
        }

        public Item RemoveOne(int slot)
        {
            ItemStack s = Get(slot);
            if (s.IsEmpty) return null;

            stacks[slot] = s.Count > 1 ? new ItemStack(s.Item, s.Count - 1) : ItemStack.Empty;
            Raise(s.Item, -1);
            return s.Item;
        }

        public ItemStack RemoveStack(int slot)
        {
            ItemStack s = Get(slot);
            if (s.IsEmpty) return ItemStack.Empty;

            stacks[slot] = ItemStack.Empty;
            Raise(s.Item, -s.Count);
            return s;
        }

        /// <summary>Total count of an item id in the player slots; a stack may span several slots.</summary>
        public int CountOf(int itemId)
        {
            int total = 0;
            for (int i = 0; i < PlayerSlotCount; i++)
                if (!stacks[i].IsEmpty && stacks[i].Item.ItemID == itemId) total += stacks[i].Count;
            return total;
        }

        /// <summary>Removes up to count of an item id from the player slots; returns how many were removed.</summary>
        public int Remove(int itemId, int count)
        {
            int removed = 0;
            Item item = null;
            for (int i = 0; i < PlayerSlotCount && removed < count; i++)
            {
                ItemStack s = stacks[i];
                if (s.IsEmpty || s.Item.ItemID != itemId) continue;

                int take = Math.Min(s.Count, count - removed);
                stacks[i] = s.Count > take ? new ItemStack(s.Item, s.Count - take) : ItemStack.Empty;
                removed += take;
                item = s.Item;
            }

            if (removed > 0) Raise(item, -removed);
            return removed;
        }

        /// <summary>Empties every player slot whose item matches; returns the removed count per item.</summary>
        public Dictionary<Item, int> RemoveAllWhere(Predicate<Item> match)
        {
            var removed = new Dictionary<Item, int>();
            for (int i = 0; i < PlayerSlotCount; i++)
            {
                ItemStack s = stacks[i];
                if (s.IsEmpty || !match(s.Item)) continue;

                removed[s.Item] = (removed.TryGetValue(s.Item, out int n) ? n : 0) + s.Count;
                stacks[i] = ItemStack.Empty;
            }

            if (removed.Count == 0) return removed;
            Changed?.Invoke();
            foreach (var pair in removed)
                ItemDelta?.Invoke(pair.Key, -pair.Value);
            return removed;
        }

        /// <summary>Moves a stack into an empty, unlocked, different slot; false and no change otherwise.</summary>
        public bool Move(int from, int to)
        {
            if (from == to || Get(from).IsEmpty || !IsUnlocked(to) || !Get(to).IsEmpty) return false;

            stacks[to] = stacks[from];
            stacks[from] = ItemStack.Empty;
            Changed?.Invoke();
            return true;
        }

        /// <summary>Unlocks the first locked player slot; false when all are unlocked.</summary>
        public bool UnlockNext()
        {
            for (int i = 0; i < PlayerSlotCount; i++)
            {
                if (unlocked[i]) continue;
                unlocked[i] = true;
                Changed?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>New-game lock state: only the first firstUnlocked player slots are open.</summary>
        public void ResetLocks(int firstUnlocked)
        {
            for (int i = 0; i < PlayerSlotCount; i++) unlocked[i] = i < firstUnlocked;
            Changed?.Invoke();
        }

        /// <summary>Replaces all contents (save load); arrays longer than the slot count are cut.</summary>
        public void Load(ItemStack[] player, ItemStack[] building)
        {
            Array.Clear(stacks, 0, stacks.Length);
            if (player != null) Array.Copy(player, 0, stacks, 0, Math.Min(player.Length, PlayerSlotCount));
            if (building != null) Array.Copy(building, 0, stacks, PlayerSlotCount, Math.Min(building.Length, BuildingSlotCount));
            Changed?.Invoke();
        }

        public void ClearItems()
        {
            Array.Clear(stacks, 0, stacks.Length);
            Changed?.Invoke();
        }

        public ItemStack[] PlayerStacks() => Slice(0, PlayerSlotCount);
        public ItemStack[] BuildingStacks() => Slice(PlayerSlotCount, BuildingSlotCount);

        private ItemStack[] Slice(int start, int length)
        {
            var copy = new ItemStack[length];
            Array.Copy(stacks, start, copy, 0, length);
            return copy;
        }

        private bool IsValid(int slot) => slot >= 0 && slot < stacks.Length;

        private void Raise(Item item, int delta)
        {
            Changed?.Invoke();
            if (item != null && delta != 0) ItemDelta?.Invoke(item, delta);
        }
    }
}
```
Building slots are never locked (only `ResetLocks`/`UnlockNext` touch locks, player range only), so the `unlocked[i]` check in `Add` matches the old code, which skipped locked player slots and never checked building slots.

- [ ] **Step 5: Compile + run tests** — expected 0 errors; B + 12 pass.

- [ ] **Step 6: Commit (user)** — suggested: `feat: pure InventoryModel with EditMode tests (not wired yet)`.

---

### Task 2: `InventoryManager` owns the model and renders it

**Files:**
- Modify (full rewrite): `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryManager.cs`
- Delete: `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryQuestService.cs` (+ `.meta`)
- Modify: `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryPlayer/DraggableItem.cs` (`count` → read-only `Count`)
- Modify: `Assets/Project_Ocean/Scripts/UI/Shop/ShopManager.cs` (drop the two `Invoke(nameof(RefreshShopInventory), …)` delays)

**Interfaces:**
- Consumes: `InventoryModel` (Task 1), `InventorySaveMapper`, `ItemStack` (7b).
- Produces: unchanged public API of `InventoryManager` (IInventory members, `PlayerSlotCount`, `GetStack`, `IsSlotUnlocked`, `IndexOfSlot`, `RemoveOneAt`, `RemoveAllWhere`, `HandleItemDropped`, `DiscardDraggedItem`, `RemoveSelectedItem`, `SelectSlot`, `OpenInventory`, `CloseInventory`, `CloseInventoryViaButton`, `UnlockNextSlot`, `SetUnlockedExtraSlots`, `ClearBuildingSlot`, `RefreshBuildingHints`, `ResetToDefault`, `Collect`, `Apply`, `AllItems`, `ActiveBuildingSlotIndex`, `InventoryMenu`, static `OnInventoryChanged`, `OnInventoryClosed`, `OnItemDelta`, `Instance`).
- Produces: `DraggableItem.Count` (`int`, public get, private set).

- [ ] **Step 1: Check no `DraggableItem` sits in the inventory slots at edit time** (they would be wiped by the first render)

```bash
grep -c "54432ef608248134bb0e909449e8f352" Assets/Project_Ocean/Levels/Scenes/Menus/AS_PlayerHUD.unity
```
Expected: `1` (only the `InventoryItemPrefab` field reference; checked 2026-09-30). More than 1 → stop and report.

- [ ] **Step 2: Replace `InventoryManager.cs`**

Keep the file's encoding/line endings (read the first bytes; write back with the same BOM state). Full new content:
```csharp
using System.Collections.Generic;
using Game.Core;
using TMPro;
using UnityEngine;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>Inventory entry point and view: owns the InventoryModel and draws it into the slot UI.</summary>
    public class InventoryManager : MonoBehaviour, ISaveParticipant, IInventory
    {
        // Forwards to Inventory.OnChanged so gameplay listens without referencing this UI class.
        public static System.Action OnInventoryChanged
        {
            get => Inventory.OnChanged;
            set => Inventory.OnChanged = value;
        }
        public static System.Action OnInventoryClosed;

        /// <summary>Signed count of a player-visible item change; save load and reset never raise it.</summary>
        public static event System.Action<Item, int> OnItemDelta;

        // Kept public so existing scene and button references do not break.
        public GameObject InventoryMenu;

        [Header("Inventory Slots")]
        [SerializeField] private InventorySlot[] inventorySlots;
        [SerializeField] private int firstUnlockedSlotCount = 3;
        [SerializeField] private List<InventorySlot> buildingSlots;

        [Header("Item View")]
        [SerializeField] private GameObject InventoryItemPrefab;

        [Header("Item Database")]
        [SerializeField] private Item[] allItems;
        public IReadOnlyList<Item> AllItems => allItems;

        [Header("Input")]
        [SerializeField] private InputReader inputReader;

        [Header("Building Slot Hint UI")]
        [SerializeField] private List<GameObject> buildingSlotUsePanels;     // F key — shown on active slot
        [SerializeField] private List<GameObject> buildingSlotKeyPanels;     // special action key — shown on active slot if item has ActionHint
        [SerializeField] private List<TextMeshProUGUI> buildingSlotKeyTexts; // text label for special key panel

        private int selectedSlot = -1;
        private int activeBuildingSlotIndex = 0;
        private InventoryUIController uiController;
        private InventoryItemDatabase itemDatabase;
        private InventoryModel model;
        private InventorySlot[] slotViews; // same indices as the model: player slots, then building slots

        public static InventoryManager Instance { get; private set; }
        public int ActiveBuildingSlotIndex => activeBuildingSlotIndex;
        public int PlayerSlotCount => model.PlayerSlotCount;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            // Persistence is handled by the parent GameManager root's DontDestroyOnLoad.

            uiController = GetComponent<InventoryUIController>();
            if (uiController == null)
                uiController = gameObject.AddComponent<InventoryUIController>();

            uiController.Initialize(InventoryMenu);
            itemDatabase = new InventoryItemDatabase(allItems);
            CreateModel();
            Inventory.Register(this);
        }

        private void CreateModel()
        {
            int buildingCount = buildingSlots != null ? buildingSlots.Count : 0;
            model = new InventoryModel(inventorySlots.Length, buildingCount);
            slotViews = new InventorySlot[model.SlotCount];
            inventorySlots.CopyTo(slotViews, 0);
            for (int i = 0; i < buildingCount; i++)
                slotViews[model.BuildingSlot(i)] = buildingSlots[i];

            model.Changed += HandleModelChanged;
            model.ItemDelta += (item, delta) => OnItemDelta?.Invoke(item, delta);
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Inventory.Unregister(this);
            Instance = null;
        }

        private void OnEnable()
        {
            UpgradeService.OnLevelGranted += HandleUpgradeGranted;
            UpgradeService.OnLevelsLoaded += HandleUpgradesLoaded;

            if (inputReader != null)
                inputReader.OnInventoryToggled += HandleInventoryToggle;
        }

        private void OnDisable()
        {
            UpgradeService.OnLevelGranted -= HandleUpgradeGranted;
            UpgradeService.OnLevelsLoaded -= HandleUpgradesLoaded;

            if (inputReader != null)
                inputReader.OnInventoryToggled -= HandleInventoryToggle;
        }

        private void Start()
        {
            SelectSlot(0);
            model.ResetLocks(firstUnlockedSlotCount);
        }

        private void Update()
        {
            if (Input.inputString != null)
            {
                bool isNumber = int.TryParse(Input.inputString, out int number);
                if (isNumber && number >= 1 && number <= 9)
                    SelectSlot(number - 1);
            }
        }

        // ── Open / close / select ─────────────────────────────

        private void HandleInventoryToggle(bool pressed)
        {
            if (!pressed) return;
            // Always allow closing, even though the inventory itself holds the UI lock.
            if (uiController.IsOpen) { HideAndDeselect(); return; }
            if (PlayerUIState.IsLocked) return;
            // TutorialGate never blocks OpenInventory: checking the bag can't derail a guided step.
            if (!FeatureUnlockService.Instance.IsUnlocked(GameFeature.Inventory))
            {
                NotificationManager.Notify("Locked — progress the main quest to unlock the inventory.");
                return;
            }
            OpenInventory();
        }

        public void OpenInventory()
        {
            uiController.ShowInventory();
            PlayerUIState.SetCloser(CloseInventory);
            TutorialEvents.RaiseAction(TutorialActionId.OpenInventory);
        }

        public void CloseInventory() => HideAndDeselect();

        public void CloseInventoryViaButton() => HideAndDeselect();

        private void HideAndDeselect()
        {
            uiController.HideInventory();
            if (IsValidSlotIndex(selectedSlot))
                inventorySlots[selectedSlot].Deselect();
            selectedSlot = -1;
            OnInventoryClosed?.Invoke();
        }

        public void SelectSlot(int newValue)
        {
            if (!IsValidSlotIndex(newValue)) return;

            if (IsValidSlotIndex(selectedSlot))
                inventorySlots[selectedSlot].Deselect();

            inventorySlots[newValue].Select();
            selectedSlot = newValue;
        }

        // ── Contents (IInventory + shop API) ──────────────────

        public bool AddItem(Item item)
        {
            if (item == null) return false;
            if (model.Add(item)) return true;

            NotificationManager.Notify("Inventory is full");
            return false;
        }

        public Item FindItemInDatabase(int id)
        {
            Item found = itemDatabase.FindById(id);
            if (found != null) return found;

            // allItems here and in GameObjectSpawnManager are separate Inspector lists; fall back
            // so a desynced item isn't dropped on load, and log loudly so the list gets fixed.
            if (GameObjectSpawnManager.Instance != null)
            {
                Item fallback = GameObjectSpawnManager.Instance.GetItem(id);
                if (fallback != null)
                {
                    Debug.LogError($"[InventoryManager] Item ID {id} ('{fallback.ItemName}') is missing from " +
                                   $"InventoryManager.allItems but exists in GameObjectSpawnManager — add it to " +
                                   $"the InventoryManager 'Item Database' list in the Inspector to fix this properly.");
                    return fallback;
                }
            }

            return null;
        }

        /// <summary>What a slot holds; player slots are 0..PlayerSlotCount-1 and building slots follow.</summary>
        public ItemStack GetStack(int slot) => model.Get(slot);

        public bool IsSlotUnlocked(int slot) => model.IsUnlocked(slot);

        /// <summary>Slot index of a slot transform, or -1 when it isn't an inventory slot (e.g. a shop slot).</summary>
        public int IndexOfSlot(Transform slotTransform)
        {
            for (int i = 0; i < slotViews.Length; i++)
                if (slotViews[i] != null && slotViews[i].transform == slotTransform) return i;
            return -1;
        }

        /// <summary>Removes one item from a slot and returns it, or null when the slot is empty.</summary>
        public Item RemoveOneAt(int slot) => model.RemoveOne(slot);

        /// <summary>Empties every player slot whose item matches; returns the removed count per item.</summary>
        public Dictionary<Item, int> RemoveAllWhere(System.Predicate<Item> match) => model.RemoveAllWhere(match);

        public bool ConsumeItem(ItemType checkType, out Item foundItem)
        {
            foundItem = PeekItem(checkType);
            if (foundItem == null) return false;
            model.RemoveOne(ActiveBuildingSlot());
            return true;
        }

        public Item PeekItem(ItemType checkType)
        {
            if (model.BuildingSlotCount == 0) return null;
            ItemStack stack = model.Get(ActiveBuildingSlot());
            return !stack.IsEmpty && stack.Item.ItemType == checkType ? stack.Item : null;
        }

        public int GetQuestItemCount(int itemId) => model.CountOf(itemId);

        public void RemoveQuestItem(int itemId, int count = 1) => model.Remove(itemId, count);

        public void ClearBuildingSlot(int index)
        {
            if (index < 0 || index >= model.BuildingSlotCount) return;
            model.RemoveOne(model.BuildingSlot(index));
            RefreshHintUI();
        }

        /// <summary>Destroys every held item and relocks upgrade slots, so a new game starts clean.</summary>
        public void ResetToDefault()
        {
            model.ClearItems();
            model.ResetLocks(firstUnlockedSlotCount);
        }

        // ── Slot unlocks ──────────────────────────────────────

        public void UnlockNextSlot()
        {
            if (!model.UnlockNext())
                GameLog.Info("UnLocked Slot Full!!!");
        }

        /// <summary>Idempotent restore for save loading: new-game locks, then unlocks extraSlots more.</summary>
        public void SetUnlockedExtraSlots(int extraSlots)
        {
            model.ResetLocks(firstUnlockedSlotCount);
            for (int i = 0; i < extraSlots; i++)
                UnlockNextSlot();
        }

        private void HandleUpgradeGranted(UpgradeData data)
        {
            if (data.type == UpgradeType.Inventory)
                UnlockNextSlot();
        }

        private void HandleUpgradesLoaded() =>
            SetUnlockedExtraSlots(UpgradeService.Instance.GetLevel(UpgradeType.Inventory));

        // ── Drag, drop, discard ───────────────────────────────

        /// <summary>Called by DraggableItem when a drag ends; moves the stack if it landed on a free slot.</summary>
        public void HandleItemDropped(Transform fromSlot, Transform toSlot)
        {
            // A refused move leaves the dragged view in the wrong place; re-render puts it back.
            if (!model.Move(IndexOfSlot(fromSlot), IndexOfSlot(toSlot)))
                Render();
        }

        public void RemoveSelectedItem()
        {
            if (!IsValidSlotIndex(selectedSlot)) return;

            Item item = model.Get(selectedSlot).Item;
            if (item == null)
            {
                Debug.LogWarning("Selected slot has no item to remove.");
                return;
            }

            int slot = selectedSlot;
            ConfirmationDialog.Instance?.Show($"Discard {item.ItemName}?", () => DiscardStack(slot, item));
        }

        public void DiscardDraggedItem(DraggableItem itemToDiscard)
        {
            if (itemToDiscard == null || itemToDiscard.GetItem() == null) return;

            // The bin doesn't re-parent, so parentAfterDrag is still the slot the drag started in.
            int slot = IndexOfSlot(itemToDiscard.parentAfterDrag);
            if (slot < 0) return;

            Item item = itemToDiscard.GetItem();
            ConfirmationDialog.Instance?.Show($"Discard {item.ItemName}?", () => DiscardStack(slot, item));
        }

        // The dialog waits for the player; only discard if the slot still holds what they confirmed.
        private void DiscardStack(int slot, Item item)
        {
            if (model.Get(slot).Item != item) return;
            GameLog.Info($"[Inventory] Discard {item.ItemName} from slot {slot}");
            model.RemoveStack(slot);
        }

        // ── View ──────────────────────────────────────────────

        private void HandleModelChanged()
        {
            Render();
            OnInventoryChanged?.Invoke();
        }

        private void Render()
        {
            for (int i = 0; i < slotViews.Length; i++)
            {
                InventorySlot view = slotViews[i];
                if (view == null) continue;

                // Building slots have no lock state in the model; leave their scene setting alone.
                if (i < model.PlayerSlotCount && view.isUnlocked != model.IsUnlocked(i))
                    view.SetLocked(!model.IsUnlocked(i));

                RenderSlot(view, model.Get(i));
            }
        }

        private void RenderSlot(InventorySlot view, ItemStack stack)
        {
            DraggableItem shown = null;
            foreach (DraggableItem child in view.GetComponentsInChildren<DraggableItem>(true))
            {
                if (shown == null && !stack.IsEmpty) { shown = child; continue; }
                // Detach first: Destroy is deferred and a same-frame render must not find it again.
                child.transform.SetParent(null, false);
                Destroy(child.gameObject);
            }

            if (stack.IsEmpty) return;
            if (shown == null)
                shown = Instantiate(InventoryItemPrefab, view.transform).GetComponent<DraggableItem>();
            shown.InitialiseItem(stack.Item, stack.Count);
        }

        public void RefreshBuildingHints() => RefreshHintUI();

        private void RefreshHintUI()
        {
            if (buildingSlots == null) return;

            for (int i = 0; i < buildingSlots.Count; i++)
            {
                bool isActive = i == activeBuildingSlotIndex;

                if (buildingSlotUsePanels != null && i < buildingSlotUsePanels.Count && buildingSlotUsePanels[i] != null)
                    buildingSlotUsePanels[i].SetActive(isActive);

                if (buildingSlotKeyPanels == null || i >= buildingSlotKeyPanels.Count || buildingSlotKeyPanels[i] == null)
                    continue;

                if (!isActive)
                {
                    buildingSlotKeyPanels[i].SetActive(false);
                    continue;
                }

                Item item = model.Get(model.BuildingSlot(i)).Item;
                var effect = item != null && item.ItemPrefab != null ? item.ItemPrefab.GetComponent<IItemEffect>() : null;
                string hint = effect?.ActionHint ?? string.Empty;

#if ENABLE_CHECKPOINT_ITEM
                // Show "T" in the active slot while any checkpoint is placed in the world
                if (string.IsNullOrEmpty(hint) && PlayerRegistry.Local?.CheckpointCount > 0)
                    hint = "T";
#endif

                bool show = !string.IsNullOrEmpty(hint);
                buildingSlotKeyPanels[i].SetActive(show);
                if (show && buildingSlotKeyTexts != null && i < buildingSlotKeyTexts.Count && buildingSlotKeyTexts[i] != null)
                    buildingSlotKeyTexts[i].text = hint;
            }
        }

        private int ActiveBuildingSlot() => model.BuildingSlot(activeBuildingSlotIndex);

        private bool IsValidSlotIndex(int slotIndex) =>
            slotIndex >= 0 && slotIndex < inventorySlots.Length && inventorySlots[slotIndex] != null;

        // ── Save ──────────────────────────────────────────────

        public void Collect(SaveData data)
        {
            data.inventoryItems = InventorySaveMapper.ToSave(model.PlayerStacks());
            data.buildingItems  = InventorySaveMapper.ToSave(model.BuildingStacks());
        }

        public void Apply(SaveData data)
        {
            model.Load(
                InventorySaveMapper.FromSave(data.inventoryItems, model.PlayerSlotCount, FindItemInDatabase, "Inventory"),
                InventorySaveMapper.FromSave(data.buildingItems, model.BuildingSlotCount, FindItemInDatabase, "Building"));
            RefreshBuildingHints();
        }
    }
}
```
Notes for the reviewer:
- Removed dead code: the commented-out `HandleCycleBuildingSlot` / `SelectBuildingSlot` / `CycleBuildingSlot` / `OnAmmoSelected` lines (Phase 5c left them commented; git keeps them). Removed helpers now in the model: `TryAddToBuildingSlots`, `TryAddToPlayerSlots`, `TryAddToSlot`, `SpawnNewItem`, `RemoveOneItem`, `LockStartingSlots`, `IsBuildingSlot`, `NotifyChanged`, `RaiseItemDelta`, `ReadStacks`, `ClearViews`, `SpawnStacks`, `SlotAt`, `ViewAt`, `BuildingSlotCount`, `GetItemInSlot`.
- Removed info logs `"No space for building item!"` / `"Full of this building item!"` (editor-only `GameLog.Info`; the player still gets "Inventory is full").
- `Inventory.Register(this)` moved after `CreateModel()` so gameplay never sees an instance without a model.
- `#if ENABLE_CHECKPOINT_ITEM` line uses `PlayerRegistry.Local` (Phase 7a renamed `GameManager.CurPlayer`). If 7a isn't done, keep whatever the current file has there.

- [ ] **Step 3: Delete `InventoryQuestService`**

Unity MCP `execute_code`:
```csharp
return UnityEditor.AssetDatabase.DeleteAsset("Assets/Project_Ocean/Scripts/UI/Inventory/InventoryQuestService.cs") ? "deleted" : "FAILED";
```
Expected: `deleted`.

- [ ] **Step 4: `DraggableItem.count` becomes read-only**

In `DraggableItem.cs` replace:
```csharp
        [HideInInspector] public int count = 1;
```
with:
```csharp
        public int Count { get; private set; } = 1;
```
and in `InitialiseItem` and `RefreshCount` replace `count` with `Count` (3 places: `count = Mathf.Max(1, newCount);`, `countText.text = $"{count}/{max}";`).
The prefab's serialized `count: 1` is dropped silently on next save of the prefab; `InitialiseItem` always sets it.

- [ ] **Step 5: `ShopManager` drops the frame-delay refreshes**

In `ShopManager.Sell()` replace:
```csharp
            if (transactionService.SellSingle(InventoryManager.Instance, playerMoney, slotIndex))
                Invoke(nameof(RefreshShopInventory), 0.02f);
```
with:
```csharp
            transactionService.SellSingle(InventoryManager.Instance, playerMoney, slotIndex);
```
In `ShopManager.SellAll()` replace:
```csharp
            if (transactionService.SellAll(InventoryManager.Instance, playerMoney))
                Invoke(nameof(RefreshShopInventory), 0.05f);
```
with:
```csharp
            transactionService.SellAll(InventoryManager.Instance, playerMoney);
```
(`OnInventoryChanged` → `RefreshShopInventory` runs inside the sale; the model already holds the new contents.)

- [ ] **Step 6: Behaviour fix — record it**

`ResetToDefault` now also empties the building slot (`model.ClearItems()`); the old code only destroyed player-slot items, so a New Game started after a run that held a Mini Hub kept it in the HUD slot. Write this in the task report; Task 3 checks it live.

- [ ] **Step 7: Compile + run tests + criteria greps**

Expected: 0 `error CS` (a leftover `.count` use shows as `CS1061`); tests B + 12; criteria 1–3 hold.

- [ ] **Step 8: Commit (user)** — suggested: `refactor: InventoryManager renders a pure InventoryModel (InventoryQuestService removed)`.

---

### Task 3: Verify live, measure, docs

**Files:** `docs/uml/findings.md` (new `### Phase 7c` under §4), regenerated `docs/uml/*`, `graphify-out/*`.

- [ ] **Step 1: Live smoke** (back up saves; route in memory `reference_live_enemy_test_route.md`; `Application.runInBackground = true` after entering Play)
  1. Run the whole Phase 7b Task 4 Step 1 list (add, shop mirror and totals, sell one, sell all, real drag, discard via button and bin, save → load, quest item consumption).
  2. Drag onto an occupied slot and onto the same slot → item returns to its slot, no duplicate view (`GetComponentsInChildren<DraggableItem>().Length` per slot ≤ 1).
  3. Upgrade: `UpgradeService.Instance.DevGrantNextLevel(<the Inventory UpgradeData asset>, out _)` (or the DevPanel upgrade button) → one more slot visible; save → load keeps it.
  4. Mini Hub: in the Ocean with feature `UseItem` unlocked, use the building item (F) → hub spawns, building slot empties, hint panels update.
  5. New Game after holding a Mini Hub (`SaveBridge.Instance.ResetForNewGame()` path via the main menu New Game) → building slot empty (behaviour fix), 3 player slots open.
  6. Console: no new errors. Exit Play, restore saves (RESTORED), scene-dirty check.

- [ ] **Step 2: Measure**

```bash
python tools/uml/gen_uml.py && python tools/uml/test_gen_uml.py
grep "^| InventoryManager \|^| InventoryModel " docs/uml/metrics.md
wc -l Assets/Project_Ocean/Scripts/UI/Inventory/InventoryManager.cs Assets/Project_Ocean/Scripts/Gameplay/Inventory/InventoryModel.cs
```
Record `InventoryManager` LOC / methods / deps before (from 7b) and after, and `InventoryModel`'s numbers.

- [ ] **Step 3: Docs** — `findings.md` §4, new `### Phase 7c` with a **Status** line: done date, model API, what moved, the behaviour fix, the numbers. Update the §4 Phase 3 bullet "a full data/view split is still possible later" → "done in 7c". Then `graphify update .`.

- [ ] **Step 4: Hand-off (user commits)** — list changed/deleted files; suggested messages in Tasks 1–2.

---

## Next plan

`2026-09-30-refactor-phase7d-generated-slots.md` — build the inventory and shop slot grids from a slot count instead of hand-placed slot objects.
