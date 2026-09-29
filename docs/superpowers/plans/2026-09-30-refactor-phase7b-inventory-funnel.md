# Refactor Phase 7b: Every Inventory Change Goes Through InventoryManager — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** No class outside `InventoryManager` reads or writes item views (`DraggableItem.count`, `GetComponentInChildren<DraggableItem>()` on inventory slots) or raises inventory events. Shop, slot clicks, drag/drop, trash bin and save all use one small `InventoryManager` API. Save mapping becomes pure and tested.

**Architecture:** First half of the inventory model/view split (7b → 7c → 7d). 7b adds the final API (`ItemStack`, `GetStack`, `RemoveOneAt`, `RemoveAllWhere`, `IndexOfSlot`, `HandleItemDropped`) and moves every caller onto it, while the data still lives in the slot GameObjects. 7c then swaps the storage to a pure `InventoryModel` behind the same API, with no caller changes.

**Tech Stack:** Unity 6000.3.6f1, C#, NUnit EditMode tests, Unity MCP (`refresh_unity`, `read_console`, `run_tests`, `execute_code`), Python 3 (`tools/uml`).

## Why this scope (data, 2026-09-30)

- `InventoryManager` is the largest class (725 raw lines, 539 LOC, 47 methods, 27 deps in `docs/uml/metrics.md`). Its data *is* its UI: an item exists only as a `DraggableItem` child of an `InventorySlot`.
- Classes that reach into those views today: `ShopTransactionService` (reads/writes `count`, destroys views, calls `NotifyChanged`/`RaiseItemDelta`), `ShopManager` (reads `InventorySlots` to mirror them), `InventorySlot` (scans `InventorySlots` on click), `DraggableItem` (raises `OnInventoryChanged` itself), `TrashBin` (destroys the dragged view through `DiscardDraggedItem`).
- Unused public API, 0 callers in code or scenes (checked with grep over `*.cs`, `*.unity`, `*.prefab`): `AddSlot`, `FindItem`, `GetSelectedItem`, `HasQuestItem`, `AddRewardItem`, `SpawnItemAtSlot`. Deleted here.
- Scene-bound methods that must keep their names: `OpenInventory`, `CloseInventoryViaButton`, `RemoveSelectedItem` (UnityEvent `m_MethodName` in scenes/prefabs).
- **Out of scope:** the storage swap (7c), generated slots (7d), `ShopManager.SellRoutine` (dead 1 s coroutine, unrelated), `InventoryQuestService` (only `InventoryManager` calls it; 7c deletes it).

## Success criteria (all must hold at the end)

1. `grep -rn "\.count\b" Assets/Project_Ocean/Scripts/UI --include=*.cs | grep -v "InventoryManager.cs\|InventoryQuestService.cs\|DraggableItem.cs"` → no output. (Only `UI/` is searched: `ItemSaveData.count` and the quest requirement `.count` in Gameplay are different fields.)
2. `grep -rln "GetComponentInChildren<DraggableItem>" Assets/Project_Ocean/Scripts --include=*.cs` → only `InventoryManager.cs` and `InventoryQuestService.cs`.
3. `grep -rn "\bInventorySlots\b\|GetItemInSlot\|RaiseItemDelta\|NotifyChanged\|OnInventoryChanged?.Invoke" Assets/Project_Ocean/Scripts --include=*.cs | grep -v "UI/Inventory/InventoryManager.cs"` → no output.
4. EditMode tests: all pass, baseline B + 4 (B = count before Task 1; 40 if Phase 7a is done).
5. Live: buy, sell one, sell all, discard (bin and button), drag between slots, save → load round trip, a quest that consumes items — all behave as before.

## Global Constraints

- Run after Phase 7a is committed (7a Task 2 edits `InventoryManager.cs`). Use code anchors, not line numbers, in this plan.
- No behaviour change, except the two listed in Task 1 Step 5 and Task 2 Step 5 (event count on load, notification order on Sell All).
- Save format unchanged: `SaveData.inventoryItems` / `buildingItems`, `ItemSaveData` fields and `saveVersion` stay as they are.
- Files keep their encoding, BOM and line endings. No scene/prefab edits.
- CLAUDE.md comment rules: 1–2 lines, why not what, XML `<summary>` on public API only, 1 sentence.
- The user commits. Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`. The "Commit" steps are hand-off suggestions.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`. "Compile" means `refresh_unity(mode=force, compile=request)` then `read_console(filter_text="error CS")` = 0 entries and `EditorUtility.scriptCompilationFailed == false`. "Run tests" means `run_tests(mode=EditMode)` + `get_test_job(wait_timeout=60)`.
- Back up `C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean` before any Play session and restore it byte-identical after (RESTORED).
- After a Play session: `git status --short -- "*.unity" "*.prefab"` shows only changes that were already there (see memory `feedback_check_scene_dirty_after_play.md`).

---

### Task 1: `ItemStack` + `InventorySaveMapper` — save mapping becomes pure

**Files:**
- Create: `Assets/Project_Ocean/Scripts/Gameplay/Inventory/ItemStack.cs`
- Create: `Assets/Project_Ocean/Scripts/Gameplay/Inventory/InventorySaveMapper.cs`
- Create: `Assets/Project_Ocean/Tests/EditMode/TestItems.cs`
- Create: `Assets/Project_Ocean/Tests/EditMode/InventorySaveMapperTests.cs`
- Modify: `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryManager.cs` (`Collect`, `Apply`, delete `SpawnItemAtSlot`)

**Interfaces:**
- Produces: `Game.Gameplay.ItemStack` (readonly struct: `Item Item`, `int Count`, `bool IsEmpty`, `static ItemStack Empty`, ctor `ItemStack(Item item, int count)`).
- Produces: `Game.Gameplay.InventorySaveMapper.ToSave(IReadOnlyList<ItemStack> slots) → List<ItemSaveData>` and `FromSave(List<ItemSaveData> rows, int slotCount, System.Func<int, Item> findItem, string label) → ItemStack[]`.
- Produces: `Game.Tests.TestItems.Make(int id, ItemType type = Normal, int maxStack = 1, int price = 0, bool canSell = true) → Item`, `TestItems.DestroyAll()`. 7b Task 2 and 7c reuse it.

- [ ] **Step 1: Record baseline**

Run tests. Write the passing count as **B** in your task report (expected 40 after Phase 7a).

- [ ] **Step 2: Write the test helper and the failing tests**

`Assets/Project_Ocean/Tests/EditMode/TestItems.cs`:
```csharp
using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>Builds throwaway Item assets for EditMode tests; call DestroyAll in TearDown.</summary>
    public static class TestItems
    {
        private static readonly List<Item> created = new List<Item>();

        public static Item Make(int id, ItemType type = ItemType.Normal, int maxStack = 1, int price = 0, bool canSell = true)
        {
            var item = ScriptableObject.CreateInstance<Item>();
            var so = new UnityEditor.SerializedObject(item);
            so.FindProperty("itemID").intValue = id;
            so.FindProperty("itemName").stringValue = "TestItem" + id;
            so.FindProperty("itemType").enumValueIndex = (int)type;
            so.FindProperty("maxStackable").intValue = maxStack;
            so.FindProperty("itemPrice").intValue = price;
            so.FindProperty("canSell").boolValue = canSell;
            so.ApplyModifiedPropertiesWithoutUndo();
            created.Add(item);
            return item;
        }

        public static void DestroyAll()
        {
            foreach (var item in created)
                if (item != null) Object.DestroyImmediate(item);
            created.Clear();
        }
    }
}
```

`Assets/Project_Ocean/Tests/EditMode/InventorySaveMapperTests.cs`:
```csharp
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Core;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public class InventorySaveMapperTests
    {
        [TearDown] public void TearDown() => TestItems.DestroyAll();

        [Test]
        public void ToSave_ThenFromSave_RestoresSlotsAndCounts()
        {
            Item fish = TestItems.Make(1, maxStack: 5);
            Item hub = TestItems.Make(2);
            var slots = new[] { new ItemStack(fish, 3), ItemStack.Empty, new ItemStack(hub, 1) };

            List<ItemSaveData> rows = InventorySaveMapper.ToSave(slots);

            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual(0, rows[0].slotIndex);
            Assert.AreEqual("1", rows[0].itemID);
            Assert.AreEqual(3, rows[0].count);
            Assert.AreEqual(2, rows[1].slotIndex);

            ItemStack[] back = InventorySaveMapper.FromSave(rows, 3, id => id == 1 ? fish : id == 2 ? hub : null, "Inventory");
            Assert.AreSame(fish, back[0].Item);
            Assert.AreEqual(3, back[0].Count);
            Assert.IsTrue(back[1].IsEmpty);
            Assert.AreSame(hub, back[2].Item);
        }

        [Test]
        public void FromSave_SkipsBadRows_AndClampsCountToOne()
        {
            Item fish = TestItems.Make(1);
            var rows = new List<ItemSaveData>
            {
                new ItemSaveData { slotIndex = 0, itemID = "abc", count = 1 }, // not a number
                new ItemSaveData { slotIndex = 1, itemID = "99",  count = 1 }, // unknown id
                new ItemSaveData { slotIndex = 5, itemID = "1",   count = 1 }, // out of range
                new ItemSaveData { slotIndex = 2, itemID = "1",   count = 0 }, // count 0 in an old save
            };
            LogAssert.Expect(LogType.Error, new Regex("item ID 99"));

            ItemStack[] back = InventorySaveMapper.FromSave(rows, 3, id => id == 1 ? fish : null, "Inventory");

            Assert.IsTrue(back[0].IsEmpty);
            Assert.IsTrue(back[1].IsEmpty);
            Assert.AreSame(fish, back[2].Item);
            Assert.AreEqual(1, back[2].Count);
        }

        [Test]
        public void FromSave_NullRows_GivesEmptySlots()
        {
            ItemStack[] back = InventorySaveMapper.FromSave(null, 2, id => null, "Building");
            Assert.AreEqual(2, back.Length);
            Assert.IsTrue(back[0].IsEmpty && back[1].IsEmpty);
        }
    }
}
```

- [ ] **Step 3: Compile — expect red**

Expected: `error CS0246: The type or namespace name 'ItemStack' could not be found`.

- [ ] **Step 4: Create `ItemStack` and `InventorySaveMapper`**

`Assets/Project_Ocean/Scripts/Gameplay/Inventory/ItemStack.cs`:
```csharp
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>An item and how many of it one slot holds; Item is null for an empty slot.</summary>
    public readonly struct ItemStack
    {
        public static readonly ItemStack Empty = default;

        public readonly Item Item;
        public readonly int Count;

        public ItemStack(Item item, int count)
        {
            Item = item;
            Count = count;
        }

        public bool IsEmpty => Item == null || Count <= 0;
    }
}
```

`Assets/Project_Ocean/Scripts/Gameplay/Inventory/InventorySaveMapper.cs`:
```csharp
using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>Converts slot contents to and from save rows; slotIndex is the index in the given list.</summary>
    public static class InventorySaveMapper
    {
        public static List<ItemSaveData> ToSave(IReadOnlyList<ItemStack> slots)
        {
            var rows = new List<ItemSaveData>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty) continue;
                rows.Add(new ItemSaveData
                {
                    slotIndex = i,
                    itemID    = slots[i].Item.ItemID.ToString(),
                    count     = slots[i].Count,
                });
            }
            return rows;
        }

        /// <summary>Rebuilds slot contents from save rows; bad, unknown or out-of-range rows are logged and skipped.</summary>
        public static ItemStack[] FromSave(List<ItemSaveData> rows, int slotCount, System.Func<int, Item> findItem, string label)
        {
            var slots = new ItemStack[slotCount];
            // Null on saves written before building slots were saved.
            if (rows == null) return slots;

            foreach (var saved in rows)
            {
                if (!int.TryParse(saved.itemID, out int id))
                {
                    Debug.LogWarning($"[InventorySaveMapper] {label} itemID '{saved.itemID}' is not a number");
                    continue;
                }

                Item item = findItem(id);
                if (item == null)
                {
                    // Error, not warning: the item is lost from the player's inventory.
                    Debug.LogError($"[InventorySaveMapper] {label} item ID {id} not found in the item database — dropped on load. " +
                                   "Add its asset to InventoryManager's Item Database list.");
                    continue;
                }

                if (saved.slotIndex < 0 || saved.slotIndex >= slotCount)
                {
                    Debug.LogWarning($"[InventorySaveMapper] {label} slotIndex {saved.slotIndex} out of range");
                    continue;
                }

                slots[saved.slotIndex] = new ItemStack(item, Mathf.Max(1, saved.count));
            }
            return slots;
        }
    }
}
```

- [ ] **Step 5: `InventoryManager` saves and loads through the mapper**

In `InventoryManager.cs`, replace the whole `Collect(SaveData data)` and `Apply(SaveData data)` methods (from `public void Collect(SaveData data)` to the end of `Apply`) with:
```csharp
        public void Collect(SaveData data)
        {
            data.inventoryItems = InventorySaveMapper.ToSave(ReadStacks(inventorySlots));
            data.buildingItems  = InventorySaveMapper.ToSave(ReadStacks(buildingSlots));
        }

        public void Apply(SaveData data)
        {
            ClearViews(inventorySlots);
            ClearViews(buildingSlots);
            SpawnStacks(inventorySlots, InventorySaveMapper.FromSave(data.inventoryItems, inventorySlots.Length, FindItemInDatabase, "Inventory"));
            if (buildingSlots != null)
                SpawnStacks(buildingSlots, InventorySaveMapper.FromSave(data.buildingItems, buildingSlots.Count, FindItemInDatabase, "Building"));
            NotifyChanged();
            RefreshBuildingHints();
        }

        private static ItemStack[] ReadStacks(IReadOnlyList<InventorySlot> slots)
        {
            if (slots == null) return new ItemStack[0];
            var stacks = new ItemStack[slots.Count];
            for (int i = 0; i < slots.Count; i++)
            {
                var view = slots[i] != null ? slots[i].GetComponentInChildren<DraggableItem>() : null;
                if (view != null && view.GetItem() != null)
                    stacks[i] = new ItemStack(view.GetItem(), view.count);
            }
            return stacks;
        }

        private static void ClearViews(IReadOnlyList<InventorySlot> slots)
        {
            if (slots == null) return;
            foreach (var slot in slots)
            {
                var view = slot != null ? slot.GetComponentInChildren<DraggableItem>() : null;
                if (view != null) Destroy(view.gameObject);
            }
        }

        private void SpawnStacks(IReadOnlyList<InventorySlot> slots, ItemStack[] stacks)
        {
            for (int i = 0; i < stacks.Length; i++)
                if (!stacks[i].IsEmpty && slots[i] != null)
                    SpawnNewItem(stacks[i].Item, slots[i], stacks[i].Count);
        }
```
Delete the whole `SpawnItemAtSlot(Item item, int slotIndex, int count = 1)` method (its only caller was the old `Apply`).

Behaviour note (accepted): the old `Apply` raised `OnInventoryChanged` once per loaded item (through `SpawnItemAtSlot`) and not at all for an empty save; the new one raises it exactly once. Listeners (`ShopManager.RefreshShopInventory`, quest tracking) only re-read state, so one call gives the same end state.

- [ ] **Step 6: Compile + run tests**

Expected: 0 `error CS`; tests B + 3 pass.

- [ ] **Step 7: Commit (user)** — suggested: `refactor: inventory save rows via pure InventorySaveMapper (ItemStack)`.

---

### Task 2: Read/remove API — shop and slot clicks stop touching item views

**Files:**
- Modify: `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryManager.cs`
- Modify: `Assets/Project_Ocean/Scripts/UI/Shop/ShopTransactionService.cs`
- Modify: `Assets/Project_Ocean/Scripts/UI/Shop/ShopManager.cs`
- Modify: `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryPlayer/InventorySlot.cs`
- Create: `Assets/Project_Ocean/Tests/EditMode/ShopSellValueTests.cs`

**Interfaces:**
- Consumes: `ItemStack`, `TestItems` (Task 1).
- Produces on `InventoryManager` (7c keeps these exact signatures):
  - `int PlayerSlotCount { get; }`
  - `ItemStack GetStack(int slot)` — player slots `0..PlayerSlotCount-1`, building slots follow.
  - `bool IsSlotUnlocked(int slot)`
  - `int IndexOfSlot(Transform slotTransform)` — `-1` if not an inventory slot.
  - `Item RemoveOneAt(int slot)` — `null` if the slot is empty.
  - `Dictionary<Item, int> RemoveAllWhere(System.Predicate<Item> match)` — player slots only.
- Produces on `ShopTransactionService`: `int SellValue(ItemStack stack)`, `int CalculateSellTotal(InventoryManager inventory)`.
- Removes from `InventoryManager`: `InventorySlots`, `GetItemInSlot`, `AddSlot`, `FindItem`, `GetSelectedItem`, `HasQuestItem`, `AddRewardItem`; `NotifyChanged` and `RaiseItemDelta` become private.

- [ ] **Step 1: Write the failing test**

`Assets/Project_Ocean/Tests/EditMode/ShopSellValueTests.cs`:
```csharp
using Game.Core;
using Game.Gameplay;
using Game.UI;
using NUnit.Framework;

namespace Game.Tests
{
    public class ShopSellValueTests
    {
        private readonly ShopTransactionService shop = new ShopTransactionService();

        [TearDown] public void TearDown() => TestItems.DestroyAll();

        [Test]
        public void SellValue_CountsOnlySellableNormalItems()
        {
            Item fish   = TestItems.Make(1, ItemType.Normal, maxStack: 5, price: 100);
            Item locked = TestItems.Make(2, ItemType.Normal, price: 100, canSell: false);
            Item hub    = TestItems.Make(3, ItemType.Building, price: 100);

            // (int)(100 * 0.9f) truncates to 89, not 90: pins current pricing.
            Assert.AreEqual(267, shop.SellValue(new ItemStack(fish, 3)));
            Assert.AreEqual(0, shop.SellValue(new ItemStack(locked, 1)));
            Assert.AreEqual(0, shop.SellValue(new ItemStack(hub, 1)));
            Assert.AreEqual(0, shop.SellValue(ItemStack.Empty));
        }
    }
}
```

- [ ] **Step 2: Compile — expect red** (`CS1061: 'ShopTransactionService' does not contain a definition for 'SellValue'`).

- [ ] **Step 3: Add the API to `InventoryManager`**

Add directly below `public int ActiveBuildingSlotIndex => activeBuildingSlotIndex;`:
```csharp
        public int PlayerSlotCount => inventorySlots.Length;
        private int BuildingSlotCount => buildingSlots != null ? buildingSlots.Count : 0;
```

Add directly above `public DraggableItem GetItemInSlot(int slotIndex)`:
```csharp
        /// <summary>What a slot holds; player slots are 0..PlayerSlotCount-1 and building slots follow.</summary>
        public ItemStack GetStack(int slot)
        {
            DraggableItem view = ViewAt(slot);
            return view != null && view.GetItem() != null ? new ItemStack(view.GetItem(), view.count) : ItemStack.Empty;
        }

        public bool IsSlotUnlocked(int slot)
        {
            InventorySlot view = SlotAt(slot);
            return view != null && view.isUnlocked;
        }

        /// <summary>Slot index of a slot transform, or -1 when it isn't an inventory slot (e.g. a shop slot).</summary>
        public int IndexOfSlot(Transform slotTransform)
        {
            for (int i = 0; i < PlayerSlotCount + BuildingSlotCount; i++)
            {
                InventorySlot view = SlotAt(i);
                if (view != null && view.transform == slotTransform) return i;
            }
            return -1;
        }

        /// <summary>Removes one item from a slot and returns it, or null when the slot is empty.</summary>
        public Item RemoveOneAt(int slot)
        {
            DraggableItem view = ViewAt(slot);
            if (view == null || view.GetItem() == null) return null;

            Item item = view.GetItem();
            RemoveOneItem(view);
            NotifyChanged();
            RaiseItemDelta(item, -1);
            return item;
        }

        /// <summary>Empties every player slot whose item matches; returns the removed count per item.</summary>
        public Dictionary<Item, int> RemoveAllWhere(System.Predicate<Item> match)
        {
            var removed = new Dictionary<Item, int>();
            for (int i = 0; i < PlayerSlotCount; i++)
            {
                DraggableItem view = ViewAt(i);
                if (view == null || view.GetItem() == null || !match(view.GetItem())) continue;

                Item item = view.GetItem();
                removed[item] = (removed.TryGetValue(item, out int n) ? n : 0) + view.count;
                Destroy(view.gameObject);
            }

            if (removed.Count == 0) return removed;
            NotifyChanged();
            foreach (var pair in removed)
                RaiseItemDelta(pair.Key, -pair.Value);
            return removed;
        }

        private InventorySlot SlotAt(int slot)
        {
            if (slot >= 0 && slot < PlayerSlotCount) return inventorySlots[slot];
            int b = slot - PlayerSlotCount;
            return b >= 0 && b < BuildingSlotCount ? buildingSlots[b] : null;
        }

        private DraggableItem ViewAt(int slot)
        {
            InventorySlot view = SlotAt(slot);
            return view != null ? view.GetComponentInChildren<DraggableItem>() : null;
        }
```

- [ ] **Step 4: Remove the old API from `InventoryManager`**

1. Delete the line `public InventorySlot[] InventorySlots => inventorySlots;`.
2. Delete the whole methods `AddSlot`, `FindItem`, `GetSelectedItem`, `HasQuestItem`, `AddRewardItem`, `GetItemInSlot`.
3. `ResetToDefault`: replace its loop body `var item = GetItemInSlot(i);` with `var item = ViewAt(i);` (rest unchanged).
4. `RemoveSelectedItem`: replace both `GetItemInSlot(` with `ViewAt(` (Task 3 rewrites it; this keeps it compiling).
5. `RaiseItemDelta`: delete its `/// <summary>Public so ShopTransactionService…</summary>` line and change `public void RaiseItemDelta` → `private void RaiseItemDelta`.
6. `NotifyChanged`: change `public void NotifyChanged()` → `private void NotifyChanged()`.
7. `Collect` no longer needs `InventorySlots` (Task 1 already uses the field).

- [ ] **Step 5: `ShopTransactionService` uses the API**

Replace `CalculateSellTotal`, `SellSingle`, `SellAll` and delete `RemoveOne`:
```csharp
        public int SellValue(ItemStack stack) =>
            IsSellAllItem(stack.Item) ? GetSellPrice(stack.Item) * stack.Count : 0;

        public int CalculateSellTotal(InventoryManager inventory)
        {
            if (inventory == null) return 0;

            int total = 0;
            for (int i = 0; i < inventory.PlayerSlotCount; i++)
                total += SellValue(inventory.GetStack(i));
            return total;
        }

        public bool SellSingle(InventoryManager inventory, PlayerMoney playerMoney, int slotIndex)
        {
            if (inventory == null || playerMoney == null) return false;

            Item item = inventory.GetStack(slotIndex).Item;
            if (item == null) return false;

            if (!item.CanSell)
            {
                NotificationManager.Notify("This item can't be sold.");
                return false;
            }

            playerMoney.AddMoney(GetSellPrice(item));
            inventory.RemoveOneAt(slotIndex);
            return true;
        }

        public bool SellAll(InventoryManager inventory, PlayerMoney playerMoney)
        {
            if (inventory == null || playerMoney == null) return false;

            var sold = inventory.RemoveAllWhere(IsSellAllItem);
            if (sold.Count == 0) return false;

            int total = 0;
            foreach (var pair in sold)
                total += GetSellPrice(pair.Key) * pair.Value;
            playerMoney.AddMoney(total);

            // Everything sellable is gone in this one pass, so the tutorial step is done.
            TutorialEvents.RaiseAction(TutorialActionId.SellItem);
            return true;
        }
```
`IsSellAllItem` already takes an `Item`, so it converts to `System.Predicate<Item>`.

Behaviour note (accepted): Sell All used to add the money before the item-removed notifications; now the item notifications come first, then the money one. Same totals, order of two toast lines only.

- [ ] **Step 6: `ShopManager` mirrors through the API**

Replace `RefreshShopInventory`:
```csharp
        public void RefreshShopInventory()
        {
            ClearShopInventoryMirror();

            var inventory = InventoryManager.Instance;
            if (inventory == null) return;

            for (int i = 0; i < shopInventorySlots.Length && i < inventory.PlayerSlotCount; i++)
                MirrorInventorySlot(inventory.GetStack(i), inventory.IsSlotUnlocked(i), shopInventorySlots[i]);

            UpdateClickedItemPriceDisplay();
        }
```
Replace `MirrorInventorySlot`:
```csharp
        private void MirrorInventorySlot(ItemStack stack, bool unlocked, InventorySlot shopSlot)
        {
            if (shopSlot == null) return;

            if (!unlocked) { shopSlot.SetLocked(true); return; }

            shopSlot.SetLocked(false);
            shopSlot.Deselect();
            if (stack.IsEmpty) return;

            DraggableItem shopItem = Instantiate(inventoryItemPrefab, shopSlot.transform).GetComponent<DraggableItem>();
            shopItem.InitialiseItem(stack.Item, stack.Count);
            shopItem.enabled = false;
        }
```
Replace `CalculateTotal` body with:
```csharp
            return transactionService.CalculateSellTotal(InventoryManager.Instance);
```
Replace `GetSelectedSellItem` with (the shop slot index is the inventory slot index):
```csharp
        private Item GetSelectedSellItem()
        {
            InventorySlot slot = GetSelectedSlot();
            if (slot == null || InventoryManager.Instance == null) return null;
            return InventoryManager.Instance.GetStack(GetShopSlotIndex(slot)).Item;
        }
```

- [ ] **Step 7: `InventorySlot` click uses `IndexOfSlot`**

In `InventorySlot.OnPointerClick`, replace the `if (InventoryManager.Instance != null) { for … }` block with:
```csharp
            var inventory = InventoryManager.Instance;
            if (inventory == null) return;

            int index = inventory.IndexOfSlot(transform);
            if (index >= 0) inventory.SelectSlot(index);
```
(`SelectSlot` ignores building-slot indices, as the old scan of `InventorySlots` did.)

- [ ] **Step 8: Compile + run tests**

Expected: 0 `error CS` (a missed caller shows as `CS0117`/`CS0122`); tests B + 4. Criterion 3 grep → only `ShopManager.cs`/`InventorySlot.cs` hits must be gone; `DraggableItem.cs` (`OnInventoryChanged?.Invoke`) is left for Task 3.

- [ ] **Step 9: Commit (user)** — suggested: `refactor: shop and slot clicks use InventoryManager stack API`.

---

### Task 3: Drag, drop and discard go through `InventoryManager`

**Files:**
- Modify: `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryPlayer/DraggableItem.cs`
- Modify: `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryManager.cs` (`RemoveSelectedItem`, `DiscardDraggedItem`, new `HandleItemDropped`, `DiscardStack`)

**Interfaces:**
- Consumes: `IndexOfSlot`, `GetStack`, `ViewAt` (Task 2).
- Produces: `InventoryManager.HandleItemDropped(Transform fromSlot, Transform toSlot)` — 7c turns it into `model.Move`.

- [ ] **Step 1: `DraggableItem` reports the drop**

Add a field under `[HideInInspector] public Transform parentAfterDrag;`:
```csharp
        private Transform slotBeforeDrag;
```
In `OnBeginDrag`, first line: `slotBeforeDrag = transform.parent;` (keep `parentAfterDrag = transform.parent;`).
In `OnEndDrag`, replace `InventoryManager.OnInventoryChanged?.Invoke();` with:
```csharp
            InventoryManager.Instance?.HandleItemDropped(slotBeforeDrag, parentAfterDrag);
```

- [ ] **Step 2: `InventoryManager` handles drop and discard**

Add below `RemoveAllWhere`:
```csharp
        /// <summary>Called by DraggableItem when a drag ends; the view already moved, so only listeners need telling.</summary>
        public void HandleItemDropped(Transform fromSlot, Transform toSlot) => NotifyChanged();
```
Replace the whole `RemoveSelectedItem` and `DiscardDraggedItem` methods with:
```csharp
        public void RemoveSelectedItem()
        {
            if (!IsValidSlotIndex(selectedSlot)) return;

            Item item = GetStack(selectedSlot).Item;
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
            DraggableItem view = ViewAt(slot);
            if (view == null || view.GetItem() != item) return;

            GameLog.Info($"[Inventory] Discard {item.ItemName} from slot {slot}");
            int count = view.count;
            Destroy(view.gameObject);
            NotifyChanged();
            RaiseItemDelta(item, -count);
        }
```
Behaviour note: dragging a non-inventory `DraggableItem` (a shop display item) onto the bin used to destroy that display object; now it's ignored (`slot < 0`). The shop's display items have `enabled = false`, so this path wasn't reachable in play.

- [ ] **Step 3: Compile + run tests + success-criteria greps**

Expected: 0 `error CS`; tests B + 4; criteria 1–3 greps give exactly the output stated in "Success criteria".

- [ ] **Step 4: Commit (user)** — suggested: `refactor: drag/drop and discard routed through InventoryManager`.

---

### Task 4: Verify live, measure, docs

**Files:** `docs/uml/findings.md` (new `### Phase 7b` under §4), regenerated `docs/uml/*`, `graphify-out/*`.

- [ ] **Step 1: Live smoke** (back up saves first; route in memory `reference_live_enemy_test_route.md`: `LoadScene("MainMenuScene")` → `SaveBridge.Instance.LoadGame(0)`, call again if dropped; `Application.runInBackground = true` after entering Play)
  1. On Land: `Inventory.Current.AddItem(<a Normal item from AllItems>)` ×3 → `InventoryManager.Instance.GetStack(0)` shows it with the right count; `OnItemDelta` fires +1 each time (subscribe a counter via `execute_code`).
  2. Open the Fish shop (`ShopManager.Instance.OpenFishShopWindow()`): mirror shows the same items; `CalculateTotal()` = sum of `0.9 × price × count` for sellable Normal items.
  3. Select shop slot 0, `ShopManager.Instance.Sell()` → money +`GetSellPrice`, stack −1. `SellAll()` → all sellable Normal items gone, money +total, tutorial event raised.
  4. Drag: ask the user to do one real drag in the Game view from slot 0 to an empty slot (calling `HandleItemDropped` directly skips the view move). Then save and check that `SaveData.inventoryItems` has the new slotIndex.
  5. Discard: `InventoryManager.Instance.SelectSlot(0)` + `RemoveSelectedItem()` → confirm the dialog (`ConfirmationDialog.Instance` confirm button) → slot empty, delta −count.
  6. Save → load (`SaveBridge.Instance.SaveGame()` then `LoadGame(slot)`): every stack back in the same slot, building slot (Mini Hub) too.
  7. Quest consumption: a tracked step needing items (as in Phase 5a) → `QuestManager.Instance.DevSkipCurrentStep()` or complete it → `GetQuestItemCount` drops by the target count.
  8. Console: no new errors. Exit Play, restore saves (RESTORED), scene-dirty check.

- [ ] **Step 2: Measure**

```bash
python tools/uml/gen_uml.py && python tools/uml/test_gen_uml.py
grep "^| InventoryManager |" docs/uml/metrics.md
```
Record `InventoryManager` LOC / methods / deps before (539 / 47 / 27) and after.

- [ ] **Step 3: Docs** — `findings.md` §4, new `### Phase 7b` with a **Status** line: done date, the API added, callers moved, methods deleted, the two accepted behaviour notes. Then `graphify update .`.

- [ ] **Step 4: Hand-off (user commits)** — list the changed files; suggested commit messages are in Tasks 1–3.

---

## Next plan

`2026-09-30-refactor-phase7c-inventory-model.md` — swap the storage to a pure `InventoryModel` behind the API added here.
