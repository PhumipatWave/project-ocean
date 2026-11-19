using System.Collections.Generic;
using Game.Core;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public class QuestItemTextTests
    {
        private class DatabaseInventory : IInventory
        {
            public readonly Dictionary<int, Item> Database = new();
            public IReadOnlyList<Item> AllItems => null;
            public int ActiveBuildingSlotIndex => -1;
            public bool AddItem(Item item) => true;
            public Item FindItemInDatabase(int id) => Database.TryGetValue(id, out var item) ? item : null;
            public bool ConsumeItem(ItemType checkType, out Item foundItem) { foundItem = null; return false; }
            public Item PeekItem(ItemType checkType) => null;
            public int GetQuestItemCount(int itemId) => 0;
            public void RemoveQuestItem(int itemId, int count = 1) { }
            public void ClearBuildingSlot(int index) { }
            public void RefreshBuildingHints() { }
            public void ResetToDefault() { }
        }

        private DatabaseInventory inventory;

        [SetUp]
        public void SetUp()
        {
            inventory = new DatabaseInventory();
            inventory.Database[1001] = TestItems.Make(1001);
            inventory.Database[1002] = TestItems.Make(1002);
            Inventory.Register(inventory);
        }

        [TearDown]
        public void TearDown()
        {
            Inventory.Unregister(inventory);
            TestItems.DestroyAll();
        }

        [Test]
        public void ExplicitId_ResolvesToItemName() =>
            Assert.AreEqual("Deliver TestItem1001 : 3", QuestVisuals.FormatItemTokens("Deliver {item:1001} : 3", null));

        [Test]
        public void UnknownId_LeavesTokenVisible()
        {
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Unknown item token"));
            Assert.AreEqual("Get {item:9}", QuestVisuals.FormatItemTokens("Get {item:9}", null));
        }

        [Test]
        public void NoToken_ReturnsTextUnchanged() =>
            Assert.AreEqual("Talk to Hunter", QuestVisuals.FormatItemTokens("Talk to Hunter", null));

        [Test]
        public void BareToken_EnemyKills_UsesLootItem()
        {
            var step = new QuestStep { Type = QuestType.Delivery, DeliverySource = DeliverySource.EnemyKills, TargetQuestItemId = "1002" };
            Assert.AreEqual("Bring TestItem1002", QuestVisuals.FormatItemTokens("Bring {item}", step));
        }

        [Test]
        public void BareToken_InventoryHeld_UsesFirstListItem_NotStaleSingleId()
        {
            var step = new QuestStep
            {
                Type = QuestType.Delivery,
                DeliverySource = DeliverySource.InventoryHeld,
                DeliveryCountMode = DeliveryCountMode.CombinedTotal,
                TargetQuestItemId = "1002",
                TargetQuestItemIds = new List<string> { "1001" },
            };
            Assert.AreEqual("Collect TestItem1001", QuestVisuals.FormatItemTokens("Collect {item}", step));
        }
    }
}
