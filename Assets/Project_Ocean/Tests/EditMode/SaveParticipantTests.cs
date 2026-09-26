using Game.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class SaveParticipantTests
    {
        private GameObject go;

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(go);

        [Test]
        public void PlayerMoney_RoundTripsThroughSaveData()
        {
            go = new GameObject("money");
            var money = go.AddComponent<PlayerMoney>();
            money.SetMoneySilent(4321);

            var data = new SaveData();
            ISaveParticipant participant = money;
            participant.Collect(data);
            money.SetMoneySilent(0);
            participant.Apply(data);

            Assert.AreEqual(4321, data.money);
            Assert.AreEqual(4321, money.CurrentMoney);
        }

        [Test]
        public void DayManager_RoundTripsThroughSaveData()
        {
            go = new GameObject("day");
            var day = go.AddComponent<DayManager>();
            day.SetDay(5);
            day.SetDivesToday(2);
            day.SetTotalDives(9);

            var data = new SaveData();
            ISaveParticipant participant = day;
            participant.Collect(data);
            day.SetDay(1);
            day.SetDivesToday(0);
            day.SetTotalDives(0);
            participant.Apply(data);

            Assert.AreEqual(5, day.CurrentDay);
            Assert.AreEqual(2, day.DivesToday);
            Assert.AreEqual(9, day.TotalDives);
        }

        [Test]
        public void DayManager_OldSaveWithOnlyDivedFlag_CountsAsOneDive()
        {
            go = new GameObject("day");
            var day = go.AddComponent<DayManager>();
            var data = new SaveData { currentDay = 3, divesToday = 0, hasDivedToday = true };

            ((ISaveParticipant)day).Apply(data);

            Assert.AreEqual(1, day.DivesToday);
        }
    }
}
