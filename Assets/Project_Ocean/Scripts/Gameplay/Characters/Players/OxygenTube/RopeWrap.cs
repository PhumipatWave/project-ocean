using System.Collections.Generic;
using UnityEngine;
using static ObjectWrapPoint;

namespace Game.Testing
{
    public class RopeWrap : MonoBehaviour
    {
        private float segmentLength;
        private LayerMask collisionMask;
        private int wrapCooldownFrames;

        public RopeWrap(float segmentLength, LayerMask collisionMask, int wrapCooldownFrames)
        {
            this.segmentLength = segmentLength;
            this.collisionMask = collisionMask;
            this.wrapCooldownFrames = wrapCooldownFrames;
        }

        /// <summary>
        /// Scans every segment for a surface crossing and wraps if valid.
        /// Returns the new cooldown value (unchanged if no wrap happened).
        /// </summary>
        public int TryAttach(List<SubRope> subRopes, List<RopeAnchor> anchors, Transform endPoint, GameObject startPoint, int cooldown)
        {
            if (cooldown > 0) return cooldown;

            for (int s = 0; s < subRopes.Count; s++)
            {
                SubRope sr = subRopes[s];
                var segs = sr.segs;

                for (int i = 1; i < segs.Count - 1; i++)
                {
                    if (anchors.Count >= 15) return cooldown;

                    RaycastHit2D lineHit = Physics2D.Linecast(segs[i - 1].pos, segs[i].pos, collisionMask);
                    if (lineHit.collider == null) continue;

                    ObjectWrapPoint anchorComp = lineHit.collider.GetComponent<ObjectWrapPoint>();
                    if (anchorComp == null) continue;

                    Vector2 wrapPoint = anchorComp.GetClosestPoint(lineHit.point);

                    if (!IsValidWrapPoint(wrapPoint, lineHit.collider, anchors, endPoint, startPoint))
                        continue;

                    Attach(wrapPoint, segs[i - 1].pos, segs[i].pos, subRopes, anchors, endPoint, startPoint);
                    return Mathf.Max(wrapCooldownFrames, 20);
                }
            }

            return cooldown;
        }

        /// Removes the last anchor if the player has swung back past it.
        /// Returns the new cooldown value (unchanged if no unwrap happened).
        public int TryDetach(List<SubRope> subRopes, List<RopeAnchor> anchors, Transform endPoint, GameObject startPoint, int cooldown)
        {
            if (cooldown > 0 || anchors.Count == 0) 
                return cooldown;

            RopeAnchor last = anchors[^1];
            Vector2 anchor = last.pos;
            Vector2 prevPin = anchors.Count > 1 ? anchors[^2].pos : (Vector2)startPoint.transform.position;

            Vector2 inDir = (anchor - prevPin).normalized;
            Vector2 outDir = ((Vector2)endPoint.position - anchor).normalized;
            Vector2 toEnd = (Vector2)endPoint.position - anchor;

            float currentCross = inDir.x * toEnd.y - inDir.y * toEnd.x;

            bool sideCrossed = Mathf.Sign(currentCross) != last.wrapSide;

            float alongIn = Vector2.Dot(toEnd, inDir);
            bool passedAnchor = alongIn < segmentLength;

            float dist = toEnd.magnitude;
            bool notTooClose = dist > segmentLength * 1.5f;

            if ((sideCrossed || passedAnchor) && notTooClose)
            {
                anchors.RemoveAt(anchors.Count - 1);
                subRopes.RemoveAt(subRopes.Count - 1);
                subRopes[^1].pinEnd = endPoint.position;

                return wrapCooldownFrames;
            }

            return cooldown;
        }

        /// <summary>
        /// Validates a candidate wrap point. Returns false if any of these are true:
        ///   - Too close to either endpoint
        ///   - Too close to an existing anchor
        ///   - The rope would fold back on itself (>~135°)
        ///   - The new sub-rope path crosses an existing anchor or the same collider
        /// </summary>
        private bool IsValidWrapPoint(Vector2 wrapPoint, Collider2D hitCollider, List<RopeAnchor> anchors, Transform endPoint, GameObject startPoint)
        {
            if (Vector2.Distance(wrapPoint, startPoint.transform.position) < segmentLength * 3f) return false;
            if (Vector2.Distance(wrapPoint, endPoint.position) < segmentLength * 3f) return false;

            foreach (RopeAnchor a in anchors)
                if (Vector2.Distance(a.pos, wrapPoint) < segmentLength * 4f) return false;

            // Reject if rope would double back on itself
            Vector2 prevPin = anchors.Count > 0 ? anchors[^1].pos : (Vector2)startPoint.transform.position;
            Vector2 inDir = (wrapPoint - prevPin).normalized;
            Vector2 toEnd = ((Vector2)endPoint.position - wrapPoint).normalized;
            if (Vector2.Dot(inDir, toEnd) < -0.7f) return false;

            // Reject if the new sub-rope path would immediately cross an existing anchor or the same collider
            Vector2 scanStart = wrapPoint + toEnd * (segmentLength * 2f);
            RaycastHit2D[] hits = Physics2D.LinecastAll(scanStart, endPoint.position, collisionMask);

            foreach (var h in hits)
            {
                foreach (RopeAnchor a in anchors)
                    if (Vector2.Distance(h.point, a.pos) < segmentLength * 3f) return false;

                if (h.collider == hitCollider) return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a new anchor at wrapPoint and adds a new sub-rope from there to the endpoint.
        /// </summary>
        private void Attach(Vector2 wrapPoint, Vector2 segBefore, Vector2 segAfter, List<SubRope> subRopes, List<RopeAnchor> anchors, Transform endPoint, GameObject startPoint)
        {
            Vector2 prevPin = anchors.Count > 0 ? anchors[^1].pos : (Vector2)startPoint.transform.position;
            Vector2 inDir = (wrapPoint - prevPin).normalized;
            Vector2 toEnd = ((Vector2)endPoint.position - wrapPoint).normalized;

            float cross = inDir.x * toEnd.y - inDir.y * toEnd.x;
            float side = Mathf.Sign(cross);

            // Fall back to segment direction if cross product is near-zero
            if (Mathf.Abs(cross) < 0.01f)
            {
                Vector2 alt = (segAfter - wrapPoint).normalized;
                side = Mathf.Sign(inDir.x * alt.y - inDir.y * alt.x);
            }

            // Pin the current last sub-rope at the wrap point
            subRopes[^1].pinEnd = wrapPoint;

            anchors.Add(new RopeAnchor
            {
                pos = wrapPoint,
                wrapSide = side,
                ropeIndex = subRopes.Count - 1
            });

            // Build a new sub-rope from wrapPoint → endPoint
            Vector2 end = endPoint.position;
            int numSegs = Mathf.Max(2, Mathf.CeilToInt(Vector2.Distance(wrapPoint, end) / segmentLength));
            SubRope newRope = new SubRope(wrapPoint, end);
            newRope.segs.Clear();

            for (int i = 0; i <= numSegs; i++)
                newRope.segs.Add(new RopeSegment(Vector2.Lerp(wrapPoint, end, (float)i / numSegs)));

            subRopes.Add(newRope);

            Debug.Log($"[RopeWrap] Wrapped at {wrapPoint} side={side}. Total anchors: {anchors.Count}");
        }
    }
}
