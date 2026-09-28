using Game.Core;
using Game.Gameplay;
using Game.UI;
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
