using System.Text.RegularExpressions;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public class SlotBuilderTests
    {
        private GameObject template;
        private GameObject container;

        [SetUp]
        public void SetUp()
        {
            template = new GameObject("SlotBuilderTests_Template", typeof(RectTransform), typeof(InventorySlot));
            container = new GameObject("SlotBuilderTests_Container", typeof(RectTransform));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(container);
            Object.DestroyImmediate(template);
        }

        [Test]
        public void Build_CreatesCountSlots_InSiblingOrder()
        {
            InventorySlot[] slots = SlotBuilder.Build(template.GetComponent<InventorySlot>(), container.transform, 3, "Slot");

            Assert.AreEqual(3, slots.Length);
            for (int i = 0; i < 3; i++)
            {
                Assert.AreSame(container.transform, slots[i].transform.parent);
                Assert.AreEqual(i, slots[i].transform.GetSiblingIndex());
                Assert.AreEqual($"Slot ({i})", slots[i].name);
            }
        }

        [Test]
        public void Build_WithoutPrefab_LogsErrorAndReturnsEmpty()
        {
            LogAssert.Expect(LogType.Error, new Regex("slot prefab or container not set"));
            Assert.AreEqual(0, SlotBuilder.Build(null, container.transform, 3, "Slot").Length);
        }
    }
}
