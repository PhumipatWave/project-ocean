using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    public class RewardEventsTests
    {
        [Test]
        public void RaiseMoneyRewarded_PassesAmount()
        {
            int got = 0;
            System.Action<int> handler = a => got += a;
            RewardEvents.OnMoneyRewarded += handler;
            try { RewardEvents.RaiseMoneyRewarded(500); }
            finally { RewardEvents.OnMoneyRewarded -= handler; }
            Assert.AreEqual(500, got);
        }
    }
}
