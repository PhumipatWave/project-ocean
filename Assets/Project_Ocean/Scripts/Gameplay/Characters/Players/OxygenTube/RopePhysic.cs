using System.Collections.Generic;
using UnityEngine;

/// <RopePhysic>
/// Responsible for simulating rope movement.
/// </RopePhysic>
namespace Game.Testing
{
    public class RopePhysic
    {
        #region Field
        private float segmentLength;
        private int constraintIterations;
        private Vector2 gravity;
        private float damping;
        private LayerMask collisionMask;
        private float collisionRadius;
        private float friction;
        private float removeAngle;
        #endregion

        // Constructor
        public RopePhysic(
            float segmentLength,
            int constraintIterations,
            Vector2 gravity,
            float damping,
            LayerMask collisionMask,
            float collisionRadius,
            float friction,
            float removeAngle)
        {
            this.segmentLength = segmentLength;
            this.constraintIterations = constraintIterations;
            this.gravity = gravity;
            this.damping = damping;
            this.collisionMask = collisionMask;
            this.collisionRadius = collisionRadius;
            this.friction = friction;
            this.removeAngle = removeAngle;
        }

        /// <summary>
        /// Adds a new segment to the last sub-rope when the endpoint moves away.
        /// </summary>
        public void GrowLastRope(List<SubRope> subRopes)
        {
            SubRope sr = subRopes[^1];
            RopeSegment last = sr.segs[^1];

            if (Vector2.Distance(last.pos, sr.pinEnd) > segmentLength)
            {
                Vector2 dir = (sr.pinEnd - last.pos).normalized;
                sr.segs.Add(new RopeSegment(last.pos + dir * segmentLength));
            }
        }

        /// <summary>
        /// Removes the last segment when the rope doubles back on itself.
        /// </summary>
        public void ShrinkLastRope(List<SubRope> subRopes)
        {
            SubRope sr = subRopes[^1];
            if (sr.segs.Count < 3) return;

            Vector2 last = sr.segs[^1].pos;
            Vector2 prev = sr.segs[^2].pos;

            Vector2 ropeDir = (last - prev).normalized;
            Vector2 moveDir = (sr.pinEnd - last).normalized;
            float threshold = Mathf.Cos(removeAngle * Mathf.Deg2Rad);

            if (Vector2.Dot(ropeDir, moveDir) < threshold
                && Vector2.Distance(sr.pinEnd, last) < segmentLength * 2f)
            {
                sr.segs.RemoveAt(sr.segs.Count - 1);
            }
        }

        /// <summary>
        /// Applies gravity and damping to every segment (Verlet integration).
        /// </summary>
        public void Simulate(List<SubRope> subRopes)
        {
            for (int s = 0; s < subRopes.Count - 1; s++)
                FlattenToStraightLine(subRopes[s]);

            SubRope active = subRopes[^1];
            var segs = active.segs;

            for (int i = 0; i < segs.Count; i++)
            {
                RopeSegment seg = segs[i];
                Vector2 vel = (seg.pos - seg.old) * damping;
                seg.old = seg.pos;
                seg.pos += vel + gravity * Time.fixedDeltaTime;
                segs[i] = seg;
            }
        }

        /// <summary>
        /// Runs multiple constraint passes to keep segment lengths correct.
        /// </summary>
        public void Constrain(List<SubRope> subRopes)
        {
            // Re-flatten taut ropes each frame to fight any drift
            for (int s = 0; s < subRopes.Count - 1; s++)
                FlattenToStraightLine(subRopes[s]);

            for (int iter = 0; iter < constraintIterations; iter++)
                foreach (SubRope sr in subRopes)
                    SolveSubRope(sr);
        }

        /// <summary>
        /// Pushes segments out of colliders and applies friction to their velocity.
        /// Returns a list of (segIndex, subRopeIndex, hit) for the wrapping system to inspect.
        /// </summary>
        public void HandleSegmentCollision(List<SubRope> subRopes)
        {
            for (int s = 0; s < subRopes.Count; s++)
            {
                SubRope sr = subRopes[s];
                var segs = sr.segs;

                for (int i = 1; i < segs.Count - 1; i++)
                {
                    RopeSegment seg = segs[i];
                    Vector2 vel = seg.pos - seg.old;

                    Collider2D hit = Physics2D.OverlapCircle(seg.pos, collisionRadius, collisionMask);
                    if (hit != null)
                    {
                        Vector2 closest = hit.ClosestPoint(seg.pos);
                        Vector2 dir = seg.pos - closest;
                        float dist = dir.magnitude;

                        if (dist > 0f && dist < collisionRadius)
                        {
                            Vector2 normal = dir / dist;
                            seg.pos = closest + normal * collisionRadius;

                            float vn = Vector2.Dot(vel, normal);
                            if (vn < 0f) vel -= normal * vn;
                        }
                    }

                    vel *= friction;
                    seg.old = seg.pos - vel;
                    segs[i] = seg;
                }
            }
        }


        /// <summary>
        /// Sums the length of all segments across all sub-ropes.
        /// </summary>
        public float CalculateTotalLength(List<SubRope> subRopes)
        {
            float length = 0f;

            for (int s = 0; s < subRopes.Count; s++)
            {
                var segs = subRopes[s].segs;
                int startIdx = s == 0 ? 0 : 1;

                for (int i = startIdx; i < segs.Count - 1; i++)
                    length += Vector2.Distance(segs[i].pos, segs[i + 1].pos);
            }

            return length;
        }

        /// <summary>
        /// Redistributes segment positions along a straight line from pinStart to pinEnd.
        /// This makes wrapped sub-ropes visually taut and stops them from sagging.
        /// </summary>
        private void FlattenToStraightLine(SubRope sr)
        {
            int count = sr.segs.Count;
            if (count < 2) return;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                Vector2 p = Vector2.Lerp(sr.pinStart, sr.pinEnd, t);
                RopeSegment seg = sr.segs[i];
                seg.pos = p;
                seg.old = p; // Zero out velocity so it won't drift when it becomes active again
                sr.segs[i] = seg;
            }
        }

        /// <summary>
        /// Helper function
        /// </summary>
        private void SolveSubRope(SubRope subRope)
        {
            var segs = subRope.segs;

            // Pin both ends
            RopeSegment first = segs[0]; first.pos = subRope.pinStart; segs[0] = first;
            RopeSegment last = segs[^1]; last.pos = subRope.pinEnd; segs[^1] = last;

            // Forward pass
            for (int i = 0; i < segs.Count - 1; i++) Solve(segs, i, i + 1);
            // Backward pass
            for (int i = segs.Count - 1; i > 0; i--) Solve(segs, i, i - 1);
        }

        private void Solve(List<RopeSegment> segs, int i, int j)
        {
            RopeSegment a = segs[i];
            RopeSegment b = segs[j];

            Vector2 delta = a.pos - b.pos;
            float dist = delta.magnitude;

            if (dist < 0.0001f) return;

            Vector2 move = delta * 0.5f * ((dist - segmentLength) / dist);
            a.pos -= move;
            b.pos += move;

            segs[i] = a;
            segs[j] = b;
        }
    }
}
