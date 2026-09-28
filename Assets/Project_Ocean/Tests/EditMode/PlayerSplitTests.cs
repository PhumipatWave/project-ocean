using System.Collections.Generic;
using Game.Core;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class PlayerSplitTests
    {
        private class OneItemInventory : IInventory
        {
            public Item Held;
            public int Consumed;
            public IReadOnlyList<Item> AllItems => null;
            public int ActiveBuildingSlotIndex => 0;
            public bool AddItem(Item item) => true;
            public Item FindItemInDatabase(int id) => null;
            public bool ConsumeItem(ItemType checkType, out Item foundItem)
            {
                foundItem = Held != null && Held.ItemType == checkType ? Held : null;
                if (foundItem == null) return false;
                Consumed++;
                return true;
            }
            public Item PeekItem(ItemType checkType) => Held != null && Held.ItemType == checkType ? Held : null;
            public int GetQuestItemCount(int itemId) => 0;
            public void RemoveQuestItem(int itemId, int count = 1) { }
            public void ClearBuildingSlot(int index) { }
            public void RefreshBuildingHints() { }
            public void ResetToDefault() { }
        }

        private readonly List<Object> created = new();
        private OneItemInventory inventory;
        private Player player;

        [SetUp]
        public void SetUp()
        {
            inventory = new OneItemInventory();
            Inventory.Register(inventory);
            var go = new GameObject("PlayerSplitTests_Player");
            go.transform.position = new Vector3(4f, -7f, 0f);
            created.Add(go);
            player = go.AddComponent<Player>();
        }

        [TearDown]
        public void TearDown()
        {
            Inventory.Unregister(inventory);
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (go.name.StartsWith("PlayerSplitTests_")) created.Add(go);
            foreach (var o in created) if (o != null) Object.DestroyImmediate(o);
            created.Clear();
        }

        private Item BuildingItem(GameObject prefab)
        {
            var item = ScriptableObject.CreateInstance<Item>();
            item.ItemType = ItemType.Building;
            item.ItemName = "TestHub";
            item.ItemPrefab = prefab;
            created.Add(item);
            return item;
        }

        [Test]
        public void Use_PlainBuildingItem_ConsumesItAndSpawnsPrefabAtPlayer()
        {
            var prefab = new GameObject("PlayerSplitTests_Hub");
            created.Add(prefab);
            inventory.Held = BuildingItem(prefab);

            PlayerItemUse.Use(player, ItemType.Building);

            Assert.AreEqual(1, inventory.Consumed);
            var spawned = GameObject.Find("PlayerSplitTests_Hub(Clone)");
            Assert.IsNotNull(spawned);
            Assert.AreEqual(player.transform.position, spawned.transform.position);
        }

        [Test]
        public void Use_NoItemHeld_ConsumesNothing()
        {
            PlayerItemUse.Use(player, ItemType.Building);
            Assert.AreEqual(0, inventory.Consumed);
        }
    }
}
