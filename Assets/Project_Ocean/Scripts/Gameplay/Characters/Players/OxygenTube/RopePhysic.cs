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
        public RopePhysic(float segmentLength, int constraintIterations, Vector2 gravity, float damping, LayerMask collisionMask, float collisionRadius, float friction, float removeAngle)
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

            float targetLen = sr.edgePath != null && sr.edgePath.Count >= 2
                ? PolylineLength(sr.edgePath)
                : Vector2.Distance(sr.pinStart, sr.pinEnd);

            float currentLen = 0f;
            for (int i = 0; i < sr.segs.Count - 1; i++)
                currentLen += Vector2.Distance(sr.segs[i].pos, sr.segs[i + 1].pos);

            if (currentLen + segmentLength * 0.5f >= targetLen)
                return;

            if (sr.edgePath != null && sr.edgePath.Count >= 2)
            {
                float walked = currentLen + segmentLength;
                sr.segs.Add(new RopeSegment(SamplePolyline(sr.edgePath, walked)));
                return;
            }

            if (Vector2.Distance(last.pos, sr.pinEnd) > segmentLength)
            {
                Vector2 dir = (sr.pinEnd - last.pos).normalized;
                sr.segs.Add(new RopeSegment(last.pos + dir * segmentLength));
            }
        }

        private static float PolylineLength(List<Vector2> path)
        {
            float len = 0f;
            for (int i = 0; i < path.Count - 1; i++)
                len += Vector2.Distance(path[i], path[i + 1]);
            return len;
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

            if (active.edgePath != null && active.edgePath.Count >= 2)
            {
                FlattenAlongPolyline(active);
                return;
            }

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
            {
                foreach (SubRope sr in subRopes)
                {
                    if (sr == subRopes[^1] && sr.edgePath != null && sr.edgePath.Count >= 2)
                        continue;
                    SolveSubRope(sr);
                }
            }

            SubRope activeRope = subRopes[^1];
            if (activeRope.edgePath != null && activeRope.edgePath.Count >= 2)
                FlattenAlongPolyline(activeRope);
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
                    Vector2 move = seg.pos - seg.old;
                    float moveDist = move.magnitude;

                    Collider2D hitCollider = null;
                    Vector2 closest = seg.pos;
                    Vector2 pushNormal = Vector2.zero;

                    if (moveDist > 0.0001f)
                    {
                        Vector2 moveDir = move / moveDist;
                        RaycastHit2D swept = Physics2D.CircleCast(
                            seg.old, collisionRadius, moveDir, moveDist + collisionRadius * 0.1f, collisionMask);

                        if (swept.collider != null)
                        {
                            hitCollider = swept.collider;
                            pushNormal = swept.normal;
                            seg.pos = swept.point + pushNormal * collisionRadius;
                            closest = swept.point;
                        }
                    }

                    if (hitCollider == null)
                    {
                        Collider2D overlap = Physics2D.OverlapCircle(seg.pos, collisionRadius, collisionMask);
                        if (overlap != null)
                        {
                            hitCollider = overlap;
                            closest = overlap.ClosestPoint(seg.pos);
                            Vector2 dir = seg.pos - closest;
                            float dist = dir.magnitude;

                            if (dist > 0.0001f && dist < collisionRadius)
                            {
                                pushNormal = dir / dist;
                                seg.pos = closest + pushNormal * collisionRadius;
                            }
                        }
                    }

                    if (hitCollider != null && pushNormal.sqrMagnitude > 0.0001f)
                    {
                        float vn = Vector2.Dot(vel, pushNormal);
                        if (vn < 0f) vel -= pushNormal * vn;
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
                if (subRopes[s].edgePath != null && subRopes[s].edgePath.Count >= 2)
                {
                    var path = subRopes[s].edgePath;
                    for (int i = 0; i < path.Count - 1; i++)
                        length += Vector2.Distance(path[i], path[i + 1]);
                    continue;
                }

                var segs = subRopes[s].segs;
                int startIdx = s == 0 ? 0 : 1;

                for (int i = startIdx; i < segs.Count - 1; i++)
                    length += Vector2.Distance(segs[i].pos, segs[i + 1].pos);
            }

            return length;
        }

        /// <summary>
        /// Redistributes segment positions along pinStart→pinEnd, or along edgePath when wrapped on an obstacle.
        /// </summary>
        private void FlattenToStraightLine(SubRope sr)
        {
            int count = sr.segs.Count;
            if (count < 2) return;

            if (sr.edgePath != null && sr.edgePath.Count >= 2)
            {
                FlattenAlongPolyline(sr);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                Vector2 p = Vector2.Lerp(sr.pinStart, sr.pinEnd, t);
                RopeSegment seg = sr.segs[i];
                seg.pos = p;
                seg.old = p;
                sr.segs[i] = seg;
            }
        }

        private void FlattenAlongPolyline(SubRope sr)
        {
            var path = sr.edgePath;
            int count = sr.segs.Count;
            float totalLen = 0f;
            for (int i = 0; i < path.Count - 1; i++)
                totalLen += Vector2.Distance(path[i], path[i + 1]);

            if (totalLen < 0.0001f) return;

            for (int i = 0; i < count; i++)
            {
                float targetDist = totalLen * ((float)i / (count - 1));
                Vector2 p = SamplePolyline(path, targetDist);
                RopeSegment seg = sr.segs[i];
                seg.pos = p;
                seg.old = p;
                sr.segs[i] = seg;
            }
        }

        private static Vector2 SamplePolyline(List<Vector2> path, float distance)
        {
            float walked = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                float segLen = Vector2.Distance(path[i], path[i + 1]);
                if (walked + segLen >= distance)
                {
                    float t = segLen < 0.0001f ? 0f : (distance - walked) / segLen;
                    return Vector2.Lerp(path[i], path[i + 1], t);
                }
                walked += segLen;
            }
            return path[^1];
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
