using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>Verlet-simulated rope segments between the current anchor and the player.</summary>
    public class VerletRope
    {
        /// <summary>Simulation tuning, read from OxygenRope's Inspector fields each step.</summary>
        public struct Settings
        {
            public float segmentLength, damping, segRadius;
            public int solverIterations, maxSimulatedSegs;
            public Vector2 gravity;
            public LayerMask collisionMask;
        }

        private struct Seg
        {
            public Vector2 pos, old;
            public Seg(Vector2 p) { pos = p; old = p; }
            public void Pin(Vector2 p) { pos = p; old = p; }
        }

        // Verlet segments from the anchor pin to the player
        private readonly List<Seg> segs = new();

        public int Count => segs.Count;

        // First segment index that receives full Verlet simulation this frame.
        // Segments before this index are frozen (already settled under gravity).
        private int SimStart(int maxSimulatedSegs) => Mathf.Max(1, segs.Count - maxSimulatedSegs);

        public void Clear() => segs.Clear();

        public void Rebuild(Vector2 from, Vector2 to, float segmentLength)
        {
            int count = Mathf.Max(2, Mathf.CeilToInt(Vector2.Distance(from, to) / segmentLength) + 1);

            segs.Clear();
            for (int i = 0; i < count; i++)
                segs.Add(new Seg(Vector2.Lerp(from, to, (float)i / (count - 1))));
        }

        /// <summary>One physics step: grow, shrink, integrate, pin both ends, solve, push out of colliders.</summary>
        public void Step(Vector2 anchor, Vector2 player, float dt, in Settings s)
        {
            GrowSegs(player, s.segmentLength); // extend segments so there is something to simulate
            ShrinkSegs(anchor, player, s.segmentLength); // prune bunched-up segments when player moves back

            Integrate(dt, s);
            PinEnds(anchor, player);
            Solve(anchor, player, s);
            PushOutColliders(s);
        }

        #region Segment Growth & Shrink
        // Add one segment per frame when the gap between the second-to-last seg and the
        // player is too wide. This ensures the rope has free segments to simulate
        // even before the first corner wrap.
        private void GrowSegs(Vector2 playerPos, float segmentLength)
        {
            if (segs.Count < 2)
                return;

            Vector2 prevPos = segs[^2].pos;

            if (Vector2.Distance(prevPos, playerPos) > segmentLength * 1.5f)
            {
                Vector2 dir = (playerPos - prevPos).normalized;
                segs.Insert(segs.Count - 1, new Seg(prevPos + dir * segmentLength));
            }
        }

        // Remove one segment per frame when we have more segments than the straight-line
        // distance from anchor to player requires. Gravity sags the rope below the player,
        // so checking adjacent-segment proximity alone never triggers when the player
        // returns toward the anchor — this count-based check catches it instead.
        private void ShrinkSegs(Vector2 anchor, Vector2 playerPos, float segmentLength)
        {
            if (segs.Count <= 2)
                return;

            float straight = Vector2.Distance(anchor, playerPos);
            int   needed   = Mathf.Max(2, Mathf.CeilToInt(straight / segmentLength) + 1);

            if (segs.Count > needed + 1)
                segs.RemoveAt(segs.Count - 2);
        }
        #endregion

        #region Simulation
        private void Integrate(float dt, in Settings s)
        {
            int   start = SimStart(s.maxSimulatedSegs); // skip frozen segments far from the player
            for (int i = start; i < segs.Count - 1; i++)
            {
                var seg = segs[i];
                Vector2 vel = (seg.pos - seg.old) * s.damping;
                seg.old = seg.pos;
                seg.pos += vel + s.gravity * (dt * dt);
                segs[i] = seg;
            }
        }

        private void PinEnds(Vector2 anchor, Vector2 playerPos)
        {
            var first = segs[0]; first.Pin(anchor); segs[0] = first;
            var last = segs[^1]; last.Pin(playerPos); segs[^1] = last;
        }

        private void Solve(Vector2 anchor, Vector2 playerPos, in Settings s)
        {
            int start = SimStart(s.maxSimulatedSegs);
            for (int iter = 0; iter < s.solverIterations; iter++)
            {
                // Include the boundary pair so the simulation window stays connected
                // to the frozen section. segs[start-1] is the frozen anchor; only
                // segs[start] moves in that pair.
                int pairStart = Mathf.Max(0, start - 1);
                for (int i = pairStart; i < segs.Count - 1; i++)
                    SolvePair(i, i + 1, s.segmentLength);
                PinEnds(anchor, playerPos);
            }
        }

        private void SolvePair(int a, int b, float segmentLength)
        {
            var sa = segs[a]; var sb = segs[b];
            Vector2 d = sa.pos - sb.pos;
            float dist = d.magnitude;

            if (dist < 0.0001f)
                return;

            Vector2 fix = d * (0.5f * (dist - segmentLength) / dist);
            sa.pos -= fix; sb.pos += fix;
            segs[a] = sa; segs[b] = sb;
        }

        private void PushOutColliders(in Settings s)
        {
            int start = SimStart(s.maxSimulatedSegs); // no need to push frozen segments — they already settled
            for (int i = start; i < segs.Count - 1; i++)
            {
                var seg = segs[i];
                var col = Physics2D.OverlapCircle(seg.pos, s.segRadius, s.collisionMask);

                if (col == null)
                    continue;

                Vector2 closest = col.ClosestPoint(seg.pos);
                Vector2 dir = seg.pos - closest;
                float dist = dir.magnitude;

                if (dist < 0.0001f)
                    dir = (seg.pos - (Vector2)col.bounds.center).normalized;
                else
                    dir /= dist;

                Vector2 vel = seg.pos - seg.old;
                seg.pos = closest + dir * s.segRadius;
                float vn = Vector2.Dot(vel, dir);

                if (vn < 0f)
                    vel -= dir * vn;

                seg.old = seg.pos - vel * 0.85f;
                segs[i] = seg;
            }
        }
        #endregion

        /// <summary>Length of the segment chain: the frozen part as one straight line, the simulated part summed.</summary>
        public float MeasureSegments(int maxSimulatedSegs)
        {
            float len = 0f;
            int start = SimStart(maxSimulatedSegs);

            // Frozen section: segs[0..start-1] are a straight line set by Rebuild.
            // Compute their contribution as a single distance instead of looping all of them.
            if (start > 1)
                len += Vector2.Distance(segs[0].pos, segs[start - 1].pos);

            // Simulated section: sum actual positions (only maxSimulatedSegs iterations)
            for (int i = start - 1; i < segs.Count - 1; i++)
                len += Vector2.Distance(segs[i].pos, segs[i + 1].pos);

            return len;
        }

        /// <summary>Appends segs[1..] — segs[0] sits on the anchor pin, which the caller already drew.</summary>
        public void AppendDrawPoints(List<Vector2> buf)
        {
            for (int i = 1; i < segs.Count; i++)
                buf.Add(segs[i].pos);
        }
    }
}
