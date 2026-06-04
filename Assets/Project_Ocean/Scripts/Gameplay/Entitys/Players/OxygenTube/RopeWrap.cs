using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>Attach and detach rope anchors on obstacle wrap corners.</summary>
    public class RopeWrap
    {
        private const float ReattachBlockDistanceFactor = 4f;
        private const float WrapPointMatchFactor = 0.75f;

        private readonly float segmentLength;
        private readonly float collisionRadius;
        private readonly LayerMask collisionMask;
        private readonly int wrapCooldownFrames;
        private readonly RopeWrapSettings settings;
        private readonly List<Vector2> pathScratch = new();

        private int reattachBlockFrames;
        private Vector2 reattachBlockPos;

        public RopeWrap(
            float segmentLength,
            float collisionRadius,
            LayerMask collisionMask,
            int wrapCooldownFrames,
            RopeWrapSettings settings = null)
        {
            this.segmentLength = segmentLength;
            this.collisionRadius = collisionRadius;
            this.collisionMask = collisionMask;
            this.wrapCooldownFrames = wrapCooldownFrames;
            this.settings = settings ?? new RopeWrapSettings();
        }

        public void TickFrame()
        {
            if (reattachBlockFrames > 0)
                reattachBlockFrames--;
        }

        public int TryAttach(
            List<SubRope> subRopes,
            List<RopeAnchor> anchors,
            Transform endPoint,
            GameObject startPoint,
            int cooldown)
        {
            if (cooldown > 0) return cooldown;

            for (int s = subRopes.Count - 1; s >= 0; s--)
            {
                var segs = subRopes[s].segs;
                for (int i = 1; i < segs.Count - 1; i++)
                {
                    if (anchors.Count >= RopeLimits.MaxAnchors) return cooldown;

                    Vector2 segBefore = segs[i - 1].pos;
                    Vector2 segAfter = segs[i].pos;

                    if (!TryFindWrapHit(segs[i - 1].old, segs[i].old, segBefore, segAfter, out RaycastHit2D hit))
                        continue;

                    var wrapSource = hit.collider.GetComponent<ObjectWrapPoint>();
                    if (wrapSource == null) continue;

                    Vector2 wrapPoint = wrapSource.GetWrapPointForContact(
                        hit.point, hit.normal, segBefore, segAfter);

                    if (!wrapSource.IsNearEnabledWrapPoint(wrapPoint, segmentLength * WrapPointMatchFactor))
                        continue;

                    if (!IsValidWrapPoint(wrapPoint, hit.normal, hit.collider, anchors, endPoint, startPoint))
                        continue;

                    Attach(wrapPoint, hit.normal, hit.collider, wrapSource, segBefore, segAfter,
                        subRopes, anchors, endPoint, startPoint);
                    return wrapCooldownFrames;
                }
            }

            return cooldown;
        }

        public int TryDetach(
            List<SubRope> subRopes,
            List<RopeAnchor> anchors,
            Transform endPoint,
            GameObject startPoint,
            int cooldown)
        {
            if (anchors.Count == 0) return cooldown;

            RopeAnchor last = anchors[^1];
            Vector2 prevPin = anchors.Count > 1
                ? anchors[^2].pos
                : (Vector2)startPoint.transform.position;

            if (!ShouldUnwrap(last, last.pos, prevPin, endPoint.position))
                return cooldown;

            anchors.RemoveAt(anchors.Count - 1);
            subRopes.RemoveAt(subRopes.Count - 1);

            SubRope active = subRopes[^1];
            active.pinEnd = endPoint.position;
            active.ClearEdgePath();

            reattachBlockPos = last.pos;
            reattachBlockFrames = wrapCooldownFrames + 4;
            return wrapCooldownFrames;
        }

        private bool ShouldUnwrap(RopeAnchor last, Vector2 anchor, Vector2 prevPin, Vector2 playerPos)
        {
            Vector2 toPlayer = playerPos - anchor;
            float dist = toPlayer.magnitude;
            if (dist < segmentLength * settings.minUnwrapDistanceFactor)
                return false;

            Vector2 inDir = (anchor - prevPin).normalized;
            Vector2 toPlayerDir = toPlayer / dist;

            float openAngle = Vector2.Angle(inDir, toPlayerDir);
            if (openAngle < settings.minUnwrapAngle)
                return false;

            float cross = RopeMath.Cross(inDir, toPlayerDir);
            bool sideCrossed = Mathf.Sign(cross) != last.wrapSide
                               && Mathf.Abs(cross) > settings.crossHysteresis;

            bool clearedCorner = last.wrapNormal.sqrMagnitude < 0.01f
                || Vector2.Dot(toPlayerDir, last.wrapNormal) > settings.unwrapNormalDot;

            Vector2 prevToPlayer = playerPos - prevPin;
            float along = Vector2.Dot(prevToPlayer, inDir);
            float anchorAlong = Vector2.Dot(anchor - prevPin, inDir);
            bool passedCorner = along > anchorAlong + segmentLength * settings.passedCornerFactor;

            if (settings.allowUnwrapWithoutSideCross
                && openAngle >= settings.minUnwrapAngleForNoSideCross
                && (passedCorner || clearedCorner))
                return true;

            if (!sideCrossed)
                return false;

            return clearedCorner || passedCorner;
        }

        private bool TryFindWrapHit(Vector2 oldA, Vector2 oldB, Vector2 a, Vector2 b, out RaycastHit2D bestHit)
        {
            bestHit = default;

            Vector2 delta = b - a;
            float dist = delta.magnitude;
            if (dist < RopeMath.Epsilon) return false;

            Vector2 dir = delta / dist;
            float castRadius = Mathf.Max(collisionRadius * 0.75f, segmentLength * 0.2f);

            RaycastHit2D swept = Physics2D.CircleCast(oldA, castRadius, dir, dist + castRadius, collisionMask);
            if (swept.collider != null)
            {
                bestHit = swept;
                return true;
            }

            RaycastHit2D line = Physics2D.Linecast(a, b, collisionMask);
            if (line.collider != null)
            {
                bestHit = line;
                return true;
            }

            Vector2 oldDelta = b - oldA;
            float oldDist = oldDelta.magnitude;
            if (oldDist < RopeMath.Epsilon) return false;

            RaycastHit2D frameSweep = Physics2D.CircleCast(
                oldA, castRadius, oldDelta / oldDist, oldDist + castRadius, collisionMask);

            if (frameSweep.collider == null) return false;

            bestHit = frameSweep;
            return true;
        }

        private bool IsValidWrapPoint(
            Vector2 wrapPoint,
            Vector2 hitNormal,
            Collider2D hitCollider,
            List<RopeAnchor> anchors,
            Transform endPoint,
            GameObject startPoint)
        {
            float minDist = segmentLength * 3f;
            if (Vector2.Distance(wrapPoint, startPoint.transform.position) < minDist) return false;
            if (Vector2.Distance(wrapPoint, endPoint.position) < minDist) return false;

            if (reattachBlockFrames > 0
                && Vector2.Distance(wrapPoint, reattachBlockPos) < segmentLength * ReattachBlockDistanceFactor)
                return false;

            foreach (RopeAnchor a in anchors)
            {
                if (Vector2.Distance(a.pos, wrapPoint) < segmentLength * 4f)
                    return false;
            }

            Vector2 prevPin = anchors.Count > 0
                ? anchors[^1].pos
                : (Vector2)startPoint.transform.position;

            Vector2 inDir = (wrapPoint - prevPin).normalized;
            Vector2 toEnd = ((Vector2)endPoint.position - wrapPoint).normalized;

            if (Vector2.Dot(inDir, toEnd) < -0.7f) return false;

            if (hitNormal.sqrMagnitude > 0.01f)
            {
                if (Vector2.Dot(inDir, hitNormal) > 0.15f) return false;
                if (Vector2.Dot(toEnd, hitNormal) > 0.35f) return false;
            }

            Vector2 scanStart = wrapPoint + toEnd * (segmentLength * 2f);
            foreach (var h in Physics2D.LinecastAll(scanStart, endPoint.position, collisionMask))
            {
                foreach (RopeAnchor a in anchors)
                {
                    if (Vector2.Distance(h.point, a.pos) < minDist)
                        return false;
                }

                if (h.collider == hitCollider && Vector2.Distance(h.point, wrapPoint) < minDist)
                    return false;
            }

            return true;
        }

        private void Attach(
            Vector2 wrapPoint,
            Vector2 wrapNormal,
            Collider2D hitCollider,
            ObjectWrapPoint wrapSource,
            Vector2 segBefore,
            Vector2 segAfter,
            List<SubRope> subRopes,
            List<RopeAnchor> anchors,
            Transform endPoint,
            GameObject startPoint)
        {
            Vector2 prevPin = anchors.Count > 0
                ? anchors[^1].pos
                : (Vector2)startPoint.transform.position;

            Vector2 inDir = (wrapPoint - prevPin).normalized;
            Vector2 toEnd = ((Vector2)endPoint.position - wrapPoint).normalized;

            float side = RopeMath.SignOrDefault(RopeMath.Cross(inDir, toEnd));
            if (Mathf.Abs(RopeMath.Cross(inDir, toEnd)) < 0.01f)
            {
                Vector2 segDir = (segAfter - segBefore).normalized;
                side = RopeMath.SignOrDefault(RopeMath.Cross(inDir, segDir));
            }

            if (wrapNormal.sqrMagnitude < 0.01f)
            {
                Vector2 fallback = (segAfter - segBefore).normalized;
                wrapNormal = new Vector2(-fallback.y, fallback.x);
            }

            SubRope taut = subRopes[^1];
            taut.pinEnd = wrapPoint;

            RopeEdgePathUtility.TryAssign(
                taut, wrapSource, prevPin, wrapPoint, side, endPoint.position,
                hitCollider, collisionMask, pathScratch);

            anchors.Add(new RopeAnchor
            {
                pos = wrapPoint,
                wrapSide = side,
                wrapNormal = wrapNormal.normalized,
                obstacleId = hitCollider != null ? hitCollider.GetInstanceID() : 0,
                ropeIndex = subRopes.Count - 1
            });

            Vector2 end = endPoint.position;
            int numSegs = Mathf.Max(2, Mathf.CeilToInt(Vector2.Distance(wrapPoint, end) / segmentLength));
            var newRope = new SubRope(wrapPoint, end);
            newRope.segs.Clear();

            for (int i = 0; i <= numSegs; i++)
                newRope.segs.Add(new RopeSegment(Vector2.Lerp(wrapPoint, end, (float)i / numSegs)));

            subRopes.Add(newRope);
        }
    }
}
