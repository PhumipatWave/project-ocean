using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Routes taut sub-ropes along obstacle wrap corners when a straight chord would cut geometry.
    /// </summary>
    public class RopeBoundaryRouter
    {
        private readonly LayerMask collisionMask;
        private readonly float segmentLength;
        private readonly List<Vector2> pathScratch = new();

        public RopeBoundaryRouter(LayerMask collisionMask, float segmentLength)
        {
            this.collisionMask = collisionMask;
            this.segmentLength = segmentLength;
        }

        public void UpdateAllPaths(
            List<SubRope> subRopes,
            List<RopeAnchor> anchors,
            Vector2 startPos,
            Vector2 endPos)
        {
            if (subRopes == null || subRopes.Count == 0) return;

            for (int i = 0; i < subRopes.Count; i++)
            {
                bool isActive = i == subRopes.Count - 1;
                float sideSign = ResolveSideSign(subRopes, anchors, startPos, isActive, i);
                RopeAnchor? segmentAnchor = ResolveSegmentAnchor(anchors, isActive, i);

                TryApplyPath(subRopes[i], sideSign, endPos, segmentAnchor, isActive);
            }
        }

        private static float ResolveSideSign(
            List<SubRope> subRopes,
            List<RopeAnchor> anchors,
            Vector2 startPos,
            bool isActive,
            int subRopeIndex)
        {
            if (!isActive && subRopeIndex < anchors.Count)
                return anchors[subRopeIndex].wrapSide;

            if (isActive && anchors.Count > 0)
                return anchors[^1].wrapSide;

            SubRope active = subRopes[^1];
            Vector2 chord = active.pinEnd - active.pinStart;
            if (chord.sqrMagnitude < RopeMath.Epsilon) return 1f;

            return RopeMath.SignOrDefault(RopeMath.Cross(active.pinStart - startPos, chord));
        }

        private static RopeAnchor? ResolveSegmentAnchor(List<RopeAnchor> anchors, bool isActive, int subRopeIndex)
        {
            if (anchors.Count == 0) return null;
            if (isActive) return anchors[^1];
            if (subRopeIndex < anchors.Count) return anchors[subRopeIndex];
            return null;
        }

        private void TryApplyPath(
            SubRope sr,
            float sideSign,
            Vector2 sideReference,
            RopeAnchor? segmentAnchor,
            bool isActive)
        {
            if (!FindWrapForSegment(sr.pinStart, sr.pinEnd, segmentAnchor,
                    out ObjectWrapPoint wrapSource, out Collider2D col))
            {
                if (isActive) sr.ClearEdgePath();
                return;
            }

            bool chordCuts = RopeMath.ChordCutsCollider(sr.pinStart, sr.pinEnd, col, collisionMask);

            if (TryBuildAndAssign(sr, wrapSource, col, sideSign, sideReference, chordCuts))
                return;

            if (TryBuildAndAssign(sr, wrapSource, col, -sideSign, sideReference, chordCuts))
                return;

            if (isActive) sr.ClearEdgePath();
        }

        private bool TryBuildAndAssign(
            SubRope sr,
            ObjectWrapPoint wrapSource,
            Collider2D col,
            float sideSign,
            Vector2 sideReference,
            bool chordCuts)
        {
            pathScratch.Clear();
            if (!wrapSource.TryBuildBoundaryPath(sr.pinStart, sr.pinEnd, sideSign, sideReference, pathScratch)
                || pathScratch.Count < 2
                || wrapSource.PathPenetratesCollider(pathScratch))
                return false;

            if (!chordCuts && pathScratch.Count <= 2)
                return false;

            sr.SetEdgePath(pathScratch);
            return true;
        }

        private bool FindWrapForSegment(
            Vector2 from,
            Vector2 to,
            RopeAnchor? segmentAnchor,
            out ObjectWrapPoint wrapSource,
            out Collider2D col)
        {
            wrapSource = null;
            col = null;

            if (Vector2.Distance(from, to) < segmentLength * 0.5f)
                return false;

            if (segmentAnchor.HasValue)
            {
                RopeAnchor anchor = segmentAnchor.Value;
                Collider2D anchorCol = FindColliderById(anchor.obstacleId, anchor.pos);
                if (anchorCol != null)
                {
                    wrapSource = anchorCol.GetComponent<ObjectWrapPoint>();
                    if (wrapSource != null && RopeMath.ChordCutsCollider(from, to, anchorCol, collisionMask))
                    {
                        col = anchorCol;
                        return true;
                    }
                }
            }

            RaycastHit2D[] hits = Physics2D.LinecastAll(from, to, collisionMask);
            float bestDepth = 0f;

            foreach (var h in hits)
            {
                var wp = h.collider.GetComponent<ObjectWrapPoint>();
                if (wp == null) continue;

                float depth = Mathf.Min(h.fraction, 1f - h.fraction);
                if (depth <= 0.01f || depth <= bestDepth) continue;

                bestDepth = depth;
                wrapSource = wp;
                col = h.collider;
            }

            return wrapSource != null;
        }

        private Collider2D FindColliderById(int instanceId, Vector2 near)
        {
            if (instanceId == 0) return null;

            foreach (var c in Physics2D.OverlapCircleAll(near, segmentLength * 4f, collisionMask))
            {
                if (c != null && c.GetInstanceID() == instanceId)
                    return c;
            }

            return null;
        }
    }
}
