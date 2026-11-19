using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    public class GameFlowEventsTests
    {
        [Test]
        public void RequestReturnToLand_RaisesOnce_AndStopsAfterUnsubscribe()
        {
            int calls = 0;
            System.Action handler = () => calls++;
            GameFlowEvents.OnReturnToLandRequested += handler;
            GameFlowEvents.RequestReturnToLand();
            GameFlowEvents.OnReturnToLandRequested -= handler;
            GameFlowEvents.RequestReturnToLand();
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void RequestReturnToLand_WithNoListener_DoesNotThrow()
        {
            Assert.DoesNotThrow(GameFlowEvents.RequestReturnToLand);
        }
    }
}
