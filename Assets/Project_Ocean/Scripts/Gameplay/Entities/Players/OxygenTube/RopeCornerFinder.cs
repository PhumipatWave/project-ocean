using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>Picks the collider corner a rope wraps around.</summary>
    public static class RopeCornerFinder
    {
        // Pick the collider corner closest to the hit point.
        // Slightly prefers corners that are off-axis from the rope direction (true corners over edge midpoints).
        public static Vector2 FindBestCorner(Collider2D col, Vector2 hitPoint, Vector2 ropeDir, List<Vector2> cornerBuf, List<Vector2> compositeBuf)
        {
            GetColliderCorners(col, cornerBuf, compositeBuf);
            if (cornerBuf.Count == 0)
                return hitPoint;

            Vector2 best      = cornerBuf[0];
            float   bestScore = float.MaxValue;

            foreach (var c in cornerBuf)
            {
                float dist = Vector2.Distance(c, hitPoint);
                Vector2 toC = c - hitPoint;
                float cross = ropeDir.x * toC.y - ropeDir.y * toC.x;
                // Penalise corners that are perfectly collinear with rope (those are edge midpoints)
                float score = dist + (Mathf.Abs(cross) < 0.1f ? 0.5f : 0f);

                if (score < bestScore)
                {
                    bestScore = score;
                    best = c;
                }
            }

            return best;
        }

        // Prefer ObjectWrapPoint's curated corners: a tilemap's CompositeCollider2D has hundreds of raw vertices.
        private static void GetColliderCorners(Collider2D col, List<Vector2> result, List<Vector2> compositeBuf)
        {
            result.Clear();

            var owp = col.GetComponent<ObjectWrapPoint>();
            if (owp != null)
            {
                foreach (var pt in owp.GetActivePoints())
                    result.Add(pt);
                return;
            }

            switch (col)
            {
                case BoxCollider2D box:
                {
                    Bounds b = box.bounds;
                    result.Add(b.min);
                    result.Add(new Vector2(b.max.x, b.min.y));
                    result.Add(b.max);
                    result.Add(new Vector2(b.min.x, b.max.y));
                    break;
                }
                case PolygonCollider2D poly:
                {
                    foreach (var lp in poly.GetPath(0))
                        result.Add(poly.transform.TransformPoint(lp));
                    break;
                }
                case CompositeCollider2D comp:
                {
                    for (int i = 0; i < comp.pathCount; i++)
                    {
                        comp.GetPath(i, compositeBuf);
                        foreach (var p in compositeBuf)
                            result.Add(comp.transform.TransformPoint(p));
                    }
                    break;
                }
                default:
                {
                    Bounds b = col.bounds;
                    result.Add(b.min);
                    result.Add(new Vector2(b.max.x, b.min.y));
                    result.Add(b.max);
                    result.Add(new Vector2(b.min.x, b.max.y));
                    break;
                }
            }
        }
    }
}
