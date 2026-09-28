using Game.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class SaveDataCompatTests
    {
        [Test]
        public void OldSaveWithoutBuildingItems_LoadsWithEmptyList()
        {
            var data = JsonUtility.FromJson<SaveData>("{\"money\":5}");
            Assert.IsNotNull(data.buildingItems);
            Assert.AreEqual(0, data.buildingItems.Count);
            Assert.AreEqual(5, data.money);
        }

        [Test]
        public void BuildingItems_RoundTripThroughJson()
        {
            var data = new SaveData();
            data.buildingItems.Add(new ItemSaveData { slotIndex = 0, itemID = "42", count = 1 });
            var back = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(data));
            Assert.AreEqual(1, back.buildingItems.Count);
            Assert.AreEqual("42", back.buildingItems[0].itemID);
        }
    }
}
