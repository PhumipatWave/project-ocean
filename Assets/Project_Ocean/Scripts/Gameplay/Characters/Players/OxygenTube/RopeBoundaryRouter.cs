using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Keeps taut and active sub-ropes following obstacle wrap corners when a straight chord would cut through geometry.
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
                SubRope sr = subRopes[i];
                bool isActive = i == subRopes.Count - 1;

                float sideSign = ResolveSideSign(subRopes, anchors, startPos, endPos);
                if (!isActive && i < anchors.Count)
                    sideSign = anchors[i].wrapSide;
                else if (isActive && anchors.Count > 0)
                    sideSign = anchors[^1].wrapSide;

                RopeAnchor? segmentAnchor = null;
                if (!isActive && i < anchors.Count)
                    segmentAnchor = anchors[i];
                else if (anchors.Count > 0)
                    segmentAnchor = anchors[^1];

                TryApplyPath(sr, sr.pinStart, sr.pinEnd, sideSign, endPos, segmentAnchor, isActive);
            }
        }

        private float ResolveSideSign(List<SubRope> subRopes, List<RopeAnchor> anchors, Vector2 startPos, Vector2 endPos)
        {
            if (anchors.Count > 0)
                return anchors[^1].wrapSide;

            SubRope active = subRopes[^1];
            Vector2 chord = active.pinEnd - active.pinStart;
            if (chord.sqrMagnitude < 0.0001f) return 1f;

            float cross = Cross(active.pinStart - startPos, chord);
            float side = Mathf.Sign(cross);
            return side == 0f ? 1f : side;
        }

        private void TryApplyPath(
            SubRope sr,
            Vector2 from,
            Vector2 to,
            float sideSign,
            Vector2 sideReference,
            RopeAnchor? segmentAnchor,
            bool isActive)
        {
            if (!FindWrapForSegment(from, to, segmentAnchor, out ObjectWrapPoint wrapSource, out Collider2D col))
            {
                if (isActive)
                    sr.edgePath = null;
                return;
            }

            bool chordCuts = ChordCutsCollider(from, to, col, collisionMask);

            pathScratch.Clear();
            if (!wrapSource.TryBuildBoundaryPath(from, to, sideSign, sideReference, pathScratch)
                || pathScratch.Count < 2)
            {
                if (chordCuts && !isActive)
                {
                    pathScratch.Clear();
                    if (wrapSource.TryBuildBoundaryPath(from, to, -sideSign, sideReference, pathScratch)
                        && pathScratch.Count >= 2
                        && !wrapSource.PathPenetratesCollider(pathScratch))
                    {
                        AssignPath(sr, pathScratch, isActive);
                        return;
                    }
                }

                if (isActive) sr.edgePath = null;
                return;
            }

            if (wrapSource.PathPenetratesCollider(pathScratch))
            {
                pathScratch.Clear();
                if (!wrapSource.TryBuildBoundaryPath(from, to, -sideSign, sideReference, pathScratch)
                    || pathScratch.Count < 2
                    || wrapSource.PathPenetratesCollider(pathScratch))
                {
                    if (isActive) sr.edgePath = null;
                    return;
                }
            }

            if (!chordCuts && pathScratch.Count <= 2 && !wrapSource.PathPenetratesCollider(pathScratch))
            {
                if (isActive) sr.edgePath = null;
                return;
            }

            AssignPath(sr, pathScratch, isActive);
        }

        private void AssignPath(SubRope sr, List<Vector2> path, bool isActive)
        {
            sr.edgePath ??= new List<Vector2>();
            sr.edgePath.Clear();
            sr.edgePath.AddRange(path);
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
                RopeAnchor a = segmentAnchor.Value;
                Collider2D anchorCol = FindColliderById(a.obstacleId, a.pos);
                if (anchorCol != null)
                {
                    var wp = anchorCol.GetComponent<ObjectWrapPoint>();
                    if (wp != null && ChordCutsCollider(from, to, anchorCol, collisionMask))
                    {
                        wrapSource = wp;
                        col = anchorCol;
                        return true;
                    }
                }
            }

            RaycastHit2D[] hits = Physics2D.LinecastAll(from, to, collisionMask);
            float bestDepth = 0f;
            ObjectWrapPoint bestWrap = null;
            Collider2D bestCol = null;

            foreach (var h in hits)
            {
                var wp = h.collider.GetComponent<ObjectWrapPoint>();
                if (wp == null) continue;

                float depth = Mathf.Min(h.fraction, 1f - h.fraction);
                if (depth <= 0.01f) continue;

                if (depth > bestDepth)
                {
                    bestDepth = depth;
                    bestWrap = wp;
                    bestCol = h.collider;
                }
            }

            if (bestWrap != null)
            {
                wrapSource = bestWrap;
                col = bestCol;
                return true;
            }

            return false;
        }

        private Collider2D FindColliderById(int instanceId, Vector2 near)
        {
            if (instanceId == 0) return null;

            Collider2D[] nearCols = Physics2D.OverlapCircleAll(near, segmentLength * 4f, collisionMask);
            foreach (var c in nearCols)
            {
                if (c != null && c.GetInstanceID() == instanceId)
                    return c;
            }
            return null;
        }

        public static bool ChordCutsCollider(Vector2 fromPos, Vector2 toPos, Collider2D col, LayerMask mask)
        {
            if (col == null) return false;

            float span = Vector2.Distance(fromPos, toPos);
            if (span < 0.01f) return false;

            for (int s = 1; s <= 3; s++)
            {
                float t = s / 4f;
                Vector2 sample = Vector2.Lerp(fromPos, toPos, t);
                if (col.OverlapPoint(sample))
                    return true;
            }

            RaycastHit2D[] hits = Physics2D.LinecastAll(fromPos, toPos, mask);
            foreach (var h in hits)
            {
                if (h.collider != col) continue;
                if (h.fraction > 0.02f && h.fraction < 0.98f)
                    return true;
            }

            return false;
        }

        private static float PolylineLength(List<Vector2> path)
        {
            float len = 0f;
            for (int i = 0; i < path.Count - 1; i++)
                len += Vector2.Distance(path[i], path[i + 1]);
            return len;
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    }
}
