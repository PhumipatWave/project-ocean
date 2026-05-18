using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>Verlet rope simulation, constraints, and collider response.</summary>
    public class RopePhysics
    {
        private readonly float segmentLength;
        private readonly int constraintIterations;
        private readonly Vector2 gravity;
        private readonly float damping;
        private readonly LayerMask collisionMask;
        private readonly float collisionRadius;
        private readonly float friction;
        private readonly float removeAngle;

        public RopePhysics(
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

        public void GrowLastRope(List<SubRope> subRopes)
        {
            SubRope sr = subRopes[^1];
            RopeSegment last = sr.segs[^1];

            float currentLen = SegmentChainLength(sr.segs);
            float targetLen = sr.TargetLength();

            if (currentLen + segmentLength * 0.5f >= targetLen)
                return;

            if (sr.HasEdgePath)
            {
                sr.segs.Add(new RopeSegment(RopeMath.SamplePolyline(sr.edgePath, currentLen + segmentLength)));
                return;
            }

            if (Vector2.Distance(last.pos, sr.pinEnd) > segmentLength)
            {
                Vector2 dir = (sr.pinEnd - last.pos).normalized;
                sr.segs.Add(new RopeSegment(last.pos + dir * segmentLength));
            }
        }

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

        public void Simulate(List<SubRope> subRopes)
        {
            for (int s = 0; s < subRopes.Count - 1; s++)
                FlattenTaut(subRopes[s]);

            SubRope active = subRopes[^1];
            if (active.HasEdgePath)
            {
                FlattenAlongPolyline(active);
                return;
            }

            var segs = active.segs;
            float dt = Time.fixedDeltaTime;

            for (int i = 0; i < segs.Count; i++)
            {
                RopeSegment seg = segs[i];
                Vector2 vel = (seg.pos - seg.old) * damping;
                seg.old = seg.pos;
                seg.pos += vel + gravity * dt;
                segs[i] = seg;
            }
        }

        public void Constrain(List<SubRope> subRopes)
        {
            for (int s = 0; s < subRopes.Count - 1; s++)
                FlattenTaut(subRopes[s]);

            for (int iter = 0; iter < constraintIterations; iter++)
            {
                foreach (SubRope sr in subRopes)
                {
                    if (sr == subRopes[^1] && sr.HasEdgePath)
                        continue;
                    SolveSubRope(sr);
                }
            }

            SubRope active = subRopes[^1];
            if (active.HasEdgePath)
                FlattenAlongPolyline(active);
        }

        public void HandleSegmentCollision(List<SubRope> subRopes)
        {
            foreach (SubRope sr in subRopes)
            {
                var segs = sr.segs;
                for (int i = 1; i < segs.Count - 1; i++)
                    segs[i] = ResolveSegmentCollision(segs[i]);
            }
        }

        public float CalculateTotalLength(List<SubRope> subRopes)
        {
            float length = 0f;

            for (int s = 0; s < subRopes.Count; s++)
            {
                SubRope sr = subRopes[s];
                if (sr.HasEdgePath)
                {
                    length += RopeMath.PolylineLength(sr.edgePath);
                    continue;
                }

                int startIdx = s == 0 ? 0 : 1;
                var segs = sr.segs;
                for (int i = startIdx; i < segs.Count - 1; i++)
                    length += Vector2.Distance(segs[i].pos, segs[i + 1].pos);
            }

            return length;
        }

        private RopeSegment ResolveSegmentCollision(RopeSegment seg)
        {
            Vector2 vel = seg.pos - seg.old;
            Vector2 move = vel;
            float moveDist = move.magnitude;

            Collider2D hitCollider = null;
            Vector2 pushNormal = Vector2.zero;

            if (moveDist > RopeMath.Epsilon)
            {
                Vector2 moveDir = move / moveDist;
                RaycastHit2D swept = Physics2D.CircleCast(
                    seg.old, collisionRadius, moveDir, moveDist + collisionRadius * 0.1f, collisionMask);

                if (swept.collider != null)
                {
                    hitCollider = swept.collider;
                    pushNormal = swept.normal;
                    seg.pos = swept.point + pushNormal * collisionRadius;
                }
            }

            if (hitCollider == null)
            {
                Collider2D overlap = Physics2D.OverlapCircle(seg.pos, collisionRadius, collisionMask);
                if (overlap != null)
                {
                    Vector2 closest = overlap.ClosestPoint(seg.pos);
                    Vector2 dir = seg.pos - closest;
                    float dist = dir.magnitude;

                    if (dist > RopeMath.Epsilon && dist < collisionRadius)
                    {
                        hitCollider = overlap;
                        pushNormal = dir / dist;
                        seg.pos = closest + pushNormal * collisionRadius;
                    }
                }
            }

            if (hitCollider != null && pushNormal.sqrMagnitude > RopeMath.Epsilon)
            {
                float vn = Vector2.Dot(vel, pushNormal);
                if (vn < 0f) vel -= pushNormal * vn;
            }

            vel *= friction;
            seg.old = seg.pos - vel;
            return seg;
        }

        private void FlattenTaut(SubRope sr)
        {
            if (sr.segs.Count < 2) return;

            if (sr.HasEdgePath)
            {
                FlattenAlongPolyline(sr);
                return;
            }

            int count = sr.segs.Count;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                RopeSegment seg = sr.segs[i];
                seg.PinTo(Vector2.Lerp(sr.pinStart, sr.pinEnd, t));
                sr.segs[i] = seg;
            }
        }

        private void FlattenAlongPolyline(SubRope sr)
        {
            var path = sr.edgePath;
            int count = sr.segs.Count;
            float totalLen = RopeMath.PolylineLength(path);
            if (totalLen < RopeMath.Epsilon) return;

            for (int i = 0; i < count; i++)
            {
                float targetDist = totalLen * ((float)i / (count - 1));
                RopeSegment seg = sr.segs[i];
                seg.PinTo(RopeMath.SamplePolyline(path, targetDist));
                sr.segs[i] = seg;
            }
        }

        private void SolveSubRope(SubRope subRope)
        {
            var segs = subRope.segs;

            RopeSegment first = segs[0];
            first.PinTo(subRope.pinStart);
            segs[0] = first;

            RopeSegment last = segs[^1];
            last.PinTo(subRope.pinEnd);
            segs[^1] = last;

            for (int i = 0; i < segs.Count - 1; i++)
                SolveSegmentPair(segs, i, i + 1);
            for (int i = segs.Count - 1; i > 0; i--)
                SolveSegmentPair(segs, i, i - 1);
        }

        private void SolveSegmentPair(List<RopeSegment> segs, int i, int j)
        {
            RopeSegment a = segs[i];
            RopeSegment b = segs[j];

            Vector2 delta = a.pos - b.pos;
            float dist = delta.magnitude;
            if (dist < RopeMath.Epsilon) return;

            Vector2 move = delta * 0.5f * ((dist - segmentLength) / dist);
            a.pos -= move;
            b.pos += move;
            segs[i] = a;
            segs[j] = b;
        }

        private static float SegmentChainLength(List<RopeSegment> segs)
        {
            float len = 0f;
            for (int i = 0; i < segs.Count - 1; i++)
                len += Vector2.Distance(segs[i].pos, segs[i + 1].pos);
            return len;
        }
    }
}
