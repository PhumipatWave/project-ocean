using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class InteractionTargetingTests
    {
        private class Target
        {
            public Vector2 Pos;
            public bool Ready = true;
        }

        private static Target Pick(Vector2 from, params Target[] targets) =>
            InteractionTargeting.PickClosest(from, targets, t => t.Pos, t => t.Ready);

        [Test]
        public void PickClosest_ReturnsNearestReadyTarget()
        {
            var near = new Target { Pos = new Vector2(1, 0) };
            var far = new Target { Pos = new Vector2(3, 0) };

            Assert.AreSame(near, Pick(Vector2.zero, far, near));
        }

        [Test]
        public void PickClosest_SkipsTargetsThatAreNotReady()
        {
            var near = new Target { Pos = new Vector2(1, 0), Ready = false };
            var far = new Target { Pos = new Vector2(3, 0) };

            Assert.AreSame(far, Pick(Vector2.zero, near, far));
        }

        [Test]
        public void PickClosest_WithNoReadyTarget_ReturnsNull()
        {
            Assert.IsNull(Pick(Vector2.zero));
            Assert.IsNull(Pick(Vector2.zero, new Target { Ready = false }));
        }
    }
}
