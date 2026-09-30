using System.Collections.Generic;
using Game.Core;
using Game.Gameplay;
using Game.UI;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class QuestSplitTests
    {
        private class RecordingInventory : IInventory
        {
            public readonly Dictionary<int, int> Held = new();
            public readonly List<(int id, int count)> Removed = new();
            public IReadOnlyList<Item> AllItems => null;
            public int ActiveBuildingSlotIndex => -1;
            public bool AddItem(Item item) => true;
            public Item FindItemInDatabase(int id) => null;
            public bool ConsumeItem(ItemType checkType, out Item foundItem) { foundItem = null; return false; }
            public Item PeekItem(ItemType checkType) => null;
            public int GetQuestItemCount(int itemId) => Held.TryGetValue(itemId, out var n) ? n : 0;
            public void RemoveQuestItem(int itemId, int count = 1) { Removed.Add((itemId, count)); Held[itemId] = GetQuestItemCount(itemId) - count; }
            public void ClearBuildingSlot(int index) { }
            public void RefreshBuildingHints() { }
            public void ResetToDefault() { }
        }

        private RecordingInventory inventory;

        [SetUp]
        public void SetUp()
        {
            inventory = new RecordingInventory();
            Inventory.Register(inventory);
        }

        [TearDown]
        public void TearDown() => Inventory.Unregister(inventory);

        private static QuestStep CombinedDelivery(bool consume) => new QuestStep
        {
            Type = QuestType.Delivery,
            DeliverySource = DeliverySource.InventoryHeld,
            DeliveryCountMode = DeliveryCountMode.CombinedTotal,
            ConsumeOnComplete = consume,
            TargetQuestItemIds = new List<string> { "1", "2" },
            TargetCount = 4,
        };

        [Test]
        public void RemoveQuestItems_CombinedTotal_TakesGreedilyInListOrder()
        {
            inventory.Held[1] = 2;
            inventory.Held[2] = 5;
            QuestStepEffects.RemoveQuestItems(CombinedDelivery(consume: true));
            CollectionAssert.AreEqual(new[] { (1, 2), (2, 2) }, inventory.Removed);
        }

        [Test]
        public void RemoveQuestItems_ConsumeOff_RemovesNothing()
        {
            inventory.Held[1] = 9;
            QuestStepEffects.RemoveQuestItems(CombinedDelivery(consume: false));
            Assert.IsEmpty(inventory.Removed);
        }

        [Test]
        public void MoneyRequirementMet_NoRequiredMoney_IsTrue()
        {
            Assert.IsTrue(QuestStepEffects.MoneyRequirementMet(new QuestStep { RequiredMoney = 0 }));
        }

        private static Quest TwoStepQuest(int id, QuestType firstType, string target)
        {
            var data = ScriptableObject.CreateInstance<QuestData>();
            data.questId = id;
            data.status = QuestStatus.InProgress;
            data.steps = new List<QuestStepData>
            {
                new QuestStepData { type = firstType, targetEnemyId = target, targetCount = 1 },
                new QuestStepData { type = QuestType.Tutorial, targetCount = 3 },
            };
            var quest = new Quest(data);
            Object.DestroyImmediate(data);
            return quest;
        }

        [Test]
        public void SaveMapper_RoundTrip_RestoresStatusStepAndCount()
        {
            var quest = TwoStepQuest(7, QuestType.Tutorial, null);
            quest.Status = QuestStatus.ReadyToTurnIn;
            quest.CurrentStepIndex = 1;
            quest.CurrentCount = 2;
            var saved = QuestSaveMapper.ToSaveData(new[] { quest });

            var fresh = TwoStepQuest(7, QuestType.Tutorial, null);
            QuestSaveMapper.Apply(saved, id => id == 7 ? fresh : null);

            Assert.AreEqual(QuestStatus.ReadyToTurnIn, fresh.Status);
            Assert.AreEqual(1, fresh.CurrentStepIndex);
            Assert.AreEqual(2, fresh.CurrentCount);
        }

        [Test]
        public void Tracker_AreaReached_CompletesOnlyMatchingStep()
        {
            var match = TwoStepQuest(1, QuestType.ExploreArea, "AR001");
            var other = TwoStepQuest(2, QuestType.ExploreArea, "AR999");
            var completed = new List<int>();
            var tracker = new QuestProgressTracker(new List<Quest> { match, other }, (q, n) => completed.Add(q.QuestID), () => { });

            tracker.Subscribe();
            try { ExploreEvents.RaiseAreaReached("AR001"); }
            finally { tracker.Unsubscribe(); }

            CollectionAssert.AreEqual(new[] { 1 }, completed);
        }
    }
}
