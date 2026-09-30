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
