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
            // First slot that fits wins, empty or stackable; an earlier empty slot beats a later stack.
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
        public void Swap_ExchangesStacks_RespectingLocksAndSlotKinds()
        {
            var model = new InventoryModel(3, 1);
            model.ResetLocks(2);
            Item a = TestItems.Make(1), b = TestItems.Make(2), hub = TestItems.Make(9, ItemType.Building);
            model.Load(new[] { new ItemStack(a, 1), new ItemStack(b, 2), ItemStack.Empty }, new[] { new ItemStack(hub, 1) });

            Assert.IsTrue(model.Swap(0, 1));
            Assert.AreSame(b, model.Get(0).Item);
            Assert.AreEqual(2, model.Get(0).Count);
            Assert.AreSame(a, model.Get(1).Item);

            Assert.IsFalse(model.Swap(0, 2)); // locked
            Assert.IsFalse(model.Swap(0, 0)); // same slot
            Assert.IsFalse(model.Swap(0, 3)); // building slot rejects a normal item
            Assert.AreSame(hub, model.Get(3).Item);
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
