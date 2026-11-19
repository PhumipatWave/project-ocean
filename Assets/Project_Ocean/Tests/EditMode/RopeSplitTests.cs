using System.Collections.Generic;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class RopeSplitTests
    {
        private static VerletRope.Settings Sim(float segmentLength, Vector2 gravity) => new VerletRope.Settings
        {
            segmentLength = segmentLength,
            damping = 0.98f,
            segRadius = 0.12f,
            solverIterations = 20,
            maxSimulatedSegs = 60,
            gravity = gravity,
            collisionMask = 0,
        };

        [Test]
        public void Rebuild_StraightLine_SegmentCountMatchesLength()
        {
            var rope = new VerletRope();
            rope.Rebuild(Vector2.zero, new Vector2(3f, 0f), 1f);
            Assert.AreEqual(4, rope.Count);
            Assert.AreEqual(3f, rope.MeasureSegments(60), 1e-4f);
        }

        [Test]
        public void Step_NoGravity_KeepsLengthWithinOneSegment()
        {
            var rope = new VerletRope();
            var s = Sim(0.5f, Vector2.zero);
            rope.Rebuild(Vector2.zero, new Vector2(5f, 0f), s.segmentLength);
            for (int i = 0; i < 50; i++)
                rope.Step(Vector2.zero, new Vector2(5f, 0f), 0.02f, s);
            Assert.AreEqual(5f, rope.MeasureSegments(s.maxSimulatedSegs), s.segmentLength);
        }

        [Test]
        public void Step_PlayerMovesCloser_ShrinksToNeededCount()
        {
            var rope = new VerletRope();
            var s = Sim(1f, Vector2.zero);
            rope.Rebuild(Vector2.zero, new Vector2(10f, 0f), s.segmentLength);
            Assert.AreEqual(11, rope.Count);
            for (int i = 0; i < 20; i++)
                rope.Step(Vector2.zero, new Vector2(3f, 0f), 0.02f, s);
            // ShrinkSegs stops at ceil(3 / 1) + 1 needed, plus one slack segment.
            Assert.AreEqual(5, rope.Count);
        }

        private GameObject box;

        [TearDown]
        public void TearDown()
        {
            if (box != null) Object.DestroyImmediate(box);
        }

        [Test]
        public void CornerFinder_BoxCollider_PicksNearestTrueCorner()
        {
            box = new GameObject("RopeSplitTests_Box");
            var col = box.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 2f);
            Physics2D.SyncTransforms();

            var corner = RopeCornerFinder.FindBestCorner(col, new Vector2(0.9f, 1f), Vector2.right, new List<Vector2>(), new List<Vector2>());
            Assert.AreEqual(new Vector2(1f, 1f), corner);
        }
    }
}
