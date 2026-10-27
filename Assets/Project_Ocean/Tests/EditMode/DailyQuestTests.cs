using System.Collections.Generic;
using System.Linq;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class DailyQuestTests
    {
        private static Quest Daily(int id, QuestCategory category, float weight = 1f, bool isDaily = true)
        {
            var data = ScriptableObject.CreateInstance<QuestData>();
            data.questId = id;
            data.category = category;
            data.isDaily = isDaily;
            data.dailyWeight = weight;
            data.steps = new List<QuestStepData> { new QuestStepData { type = QuestType.Tutorial, targetCount = 3 } };
            var quest = new Quest(data);
            Object.DestroyImmediate(data);
            return quest;
        }

        [Test]
        public void Roll_PicksOnePerCategory_SkipsEmptyCategory()
        {
            var pool = new[]
            {
                Daily(1, QuestCategory.FishS), Daily(2, QuestCategory.FishS), Daily(3, QuestCategory.FishS),
                Daily(4, QuestCategory.FishM),
            };

            var picks = DailyQuestRoller.Roll(pool, _ => true, () => 0.5f);

            CollectionAssert.AreEqual(new[] { QuestCategory.FishS, QuestCategory.FishM }, picks.Select(q => q.Category));
        }

        [Test]
        public void Roll_IgnoresNonDailyAndIneligible()
        {
            var pool = new[] { Daily(1, QuestCategory.FishS, isDaily: false), Daily(2, QuestCategory.FishS) };

            Assert.IsEmpty(DailyQuestRoller.Roll(pool, q => q.QuestID != 2, () => 0f));
        }

        [Test]
        public void Roll_HeavierWeightWinsMostOfTheRange()
        {
            // Weights 1 and 3: a roll past the first quarter lands on the heavy quest.
            var pool = new[] { Daily(1, QuestCategory.FishS, weight: 1f), Daily(2, QuestCategory.FishS, weight: 3f) };

            Assert.AreEqual(1, DailyQuestRoller.Roll(pool, _ => true, () => 0.1f)[0].QuestID);
            Assert.AreEqual(2, DailyQuestRoller.Roll(pool, _ => true, () => 0.6f)[0].QuestID);
        }

        [Test]
        public void ResetForReroll_ClearsStatusAndProgress()
        {
            var quest = Daily(1, QuestCategory.FishS);
            quest.Status = QuestStatus.InProgress;
            quest.AddProgress(2);
            quest.CurrentStep.RequirementPending = true;

            quest.ResetForReroll();

            Assert.AreEqual(QuestStatus.New, quest.Status);
            Assert.AreEqual(0, quest.CurrentCount);
            Assert.IsFalse(quest.CurrentStep.RequirementPending);
        }
    }
}
