using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObjectWrapPoint : MonoBehaviour
{
    [System.Serializable]
    public class WrapPoint
    {
        public bool enabled = true;
        public Vector2 position;
        public int pathIndex;

        public WrapPoint(Vector2 pos, int pathIndex)
        {
            position = pos;
            this.pathIndex = pathIndex;
        }
    }

    [Header("Points")]
    [Tooltip("Generated wrap corners (red gizmos). Regenerate via Generate Points in code or clear list + play.")]
    [SerializeField] private List<WrapPoint> wrapPoints = new();

    [Header("Setting")]
    [Tooltip("Corners closer than this (world units) are merged. Helps CompositeCollider2D seams. Try 0.25–0.5.")]
    [SerializeField] private float mergeDistance = 0.5f;
    [Tooltip("Remove inward corners the rope cannot wrap around physically.")]
    [SerializeField] private bool filterConcave = true;
    [Tooltip("Skip corners sharper than this interior angle (degrees). 0 = keep all convex corners. " +
             "Raise (15–25) if the blue rope still cuts through sharp tips.")]
    [SerializeField][Range(0f, 170f)] private float minAngle = 10f;

    [Header("Debug")]
    [Tooltip("Draw red corner spheres and yellow perimeter edges in the Scene view.")]
    [SerializeField] private bool drawDebug = true;
    [Tooltip("Size of red wrap-point gizmo spheres.")]
    [SerializeField] private float gizmoSize = 0.06f;

    private Collider2D col;

    #region UnityCallback
    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (wrapPoints.Count == 0) GeneratePoints();
    }

#if UNITY_EDITOR
    private void OnValidate() => col = GetComponent<Collider2D>();
#endif

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        col ??= GetComponent<Collider2D>();
        if (col == null || wrapPoints.Count == 0) return;

        foreach (var wp in wrapPoints)
        {
            Gizmos.color = wp.enabled ? Color.red : new Color(0.5f, 0.5f, 0.5f, 0.4f);
            Gizmos.DrawSphere(wp.position, gizmoSize);
        }

        // Group by path and draw each as its own closed loop
        var pathGroups = new Dictionary<int, List<Vector2>>();
        foreach (var wp in wrapPoints)
        {
            if (!wp.enabled) continue;
            if (!pathGroups.TryGetValue(wp.pathIndex, out var list))
            {
                list = new List<Vector2>();
                pathGroups[wp.pathIndex] = list;
            }
            list.Add(wp.position);
        }

        Gizmos.color = Color.yellow;
        foreach (var group in pathGroups.Values)
            for (int i = 0; i < group.Count; i++)
                Gizmos.DrawLine(group[i], group[(i + 1) % group.Count]);
    }
    #endregion

    public IEnumerable<Vector2> GetActivePoints()
    {
        foreach (var wp in wrapPoints)
            if (wp.enabled) yield return wp.position;
    }

    public IReadOnlyList<WrapPoint> GetAllWrapPoints() => wrapPoints;

    /// <summary>
    /// True when pos is on an enabled wrap corner (red gizmo), within tolerance.
    /// Attach logic should only pin at these points.
    /// </summary>
    public bool IsNearEnabledWrapPoint(Vector2 pos, float tolerance)
    {
        float tolSqr = tolerance * tolerance;
        foreach (var wp in wrapPoints)
        {
            if (!wp.enabled) continue;
            if ((wp.position - pos).sqrMagnitude <= tolSqr)
                return true;
        }
        return false;
    }

    public Vector2 GetClosestPoint(Vector2 pos)
    {
        float best = float.MaxValue;
        Vector2 result = pos;

        foreach (var wp in wrapPoints)
        {
            if (!wp.enabled) continue;
            float d = (pos - wp.position).sqrMagnitude;
            if (d < best) { best = d; result = wp.position; }
        }

        return result;
    }

    /// <summary>
    /// Picks a corner near the hit that lies on the correct exterior side for wrapping,
    /// instead of always using the globally closest point (which mis-pins at shallow angles).
    /// </summary>
    public Vector2 GetWrapPointForContact(Vector2 hitPoint, Vector2 hitNormal, Vector2 segBefore, Vector2 segAfter)
    {
        Vector2 segDir = segAfter - segBefore;
        float segLen = segDir.magnitude;
        if (segLen < 0.0001f) return GetClosestPoint(hitPoint);
        segDir /= segLen;

        float searchRadius = Mathf.Max(mergeDistance * 4f, 0.75f);
        float searchRadiusSqr = searchRadius * searchRadius;

        Vector2 best = GetClosestPoint(hitPoint);
        float bestScore = float.MaxValue;

        foreach (var wp in wrapPoints)
        {
            if (!wp.enabled) continue;

            Vector2 corner = wp.position;
            Vector2 toCorner = corner - hitPoint;
            float distSqr = toCorner.sqrMagnitude;

            if (distSqr > searchRadiusSqr) continue;

            if (toCorner.sqrMagnitude > 0.0001f)
            {
                float outward = Vector2.Dot(toCorner.normalized, hitNormal);
                if (outward < -0.05f) continue;
            }

            float score = distSqr;

            float crossSeg = segDir.x * toCorner.y - segDir.y * toCorner.x;
            if (Mathf.Abs(crossSeg) < 0.001f) score += 10f;
            else if (Mathf.Sign(crossSeg) != Mathf.Sign(Cross(segDir, hitPoint - segBefore))) score += 20f;

            if (toCorner.sqrMagnitude > 0.0001f && Vector2.Dot(toCorner.normalized, hitNormal) < 0.1f)
                score += 15f;

            if (score < bestScore)
            {
                bestScore = score;
                best = corner;
            }
        }

        return best;
    }

    private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    /// <summary>
    /// Builds a polyline from fromPos to toPos along enabled wrap corners (yellow debug path).
    /// Picks the exterior arc using sideSign and a reference point (usually the player).
    /// </summary>
    public bool TryBuildBoundaryPath(Vector2 fromPos, Vector2 toPos, float sideSign, Vector2 sideReference, List<Vector2> outPath)
    {
        outPath.Clear();

        var pathGroups = BuildOrderedPathGroups();
        if (pathGroups.Count == 0) return false;

        List<Vector2> best = null;
        float bestScore = float.MaxValue;

        float indexSearchRadius = Mathf.Max(mergeDistance * 4f, 1f);

        foreach (var loop in pathGroups.Values)
        {
            if (loop.Count < 2) continue;

            var fromCandidates = FindCandidateIndices(loop, fromPos, indexSearchRadius);
            var toCandidates = FindCandidateIndices(loop, toPos, indexSearchRadius);

            foreach (int iFrom in fromCandidates)
            {
                foreach (int iTo in toCandidates)
                {
                    if (iFrom == iTo)
                    {
                        if (Vector2.Distance(fromPos, toPos) < mergeDistance)
                        {
                            outPath.Add(fromPos);
                            outPath.Add(toPos);
                            return true;
                        }
                        continue;
                    }

                    List<Vector2> cw = BuildArc(loop, iFrom, iTo, +1, fromPos, toPos);
                    List<Vector2> ccw = BuildArc(loop, iFrom, iTo, -1, fromPos, toPos);

                    List<Vector2> chosen = ChooseExteriorArc(cw, ccw, sideSign, sideReference, loop);
                    if (chosen == null || chosen.Count < 2) continue;

                    chosen = PruneBoundaryPath(chosen);
                    if (chosen == null || chosen.Count < 2) continue;

                    bool penetrates = PathPenetratesCollider(chosen);
                    float score = PolylineLength(chosen) + (penetrates ? 10000f : 0f);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = chosen;
                    }
                }
            }
        }

        if (best == null || best.Count < 2) return false;

        if (PathPenetratesCollider(best))
            return false;

        outPath.AddRange(best);
        return true;
    }

    /// <summary>
    /// Removes intermediate corners that are not needed — the rope may follow the yellow
    /// perimeter in debug, but only corners that block a straight segment are kept.
    /// </summary>
    private List<Vector2> PruneBoundaryPath(List<Vector2> path)
    {
        if (path == null || path.Count <= 2) return path;

        var pruned = new List<Vector2> { path[0] };
        int anchor = 0;

        while (anchor < path.Count - 1)
        {
            int furthest = anchor + 1;

            for (int candidate = path.Count - 1; candidate > anchor + 1; candidate--)
            {
                var segment = new List<Vector2> { path[anchor], path[candidate] };
                if (!PathPenetratesCollider(segment))
                {
                    furthest = candidate;
                    break;
                }
            }

            if (furthest != anchor)
                pruned.Add(path[furthest]);

            if (furthest >= path.Count - 1)
                break;

            anchor = furthest;
        }

        if (pruned.Count < 2)
        {
            pruned.Clear();
            pruned.Add(path[0]);
            pruned.Add(path[^1]);
        }

        return pruned;
    }

    private static List<int> FindCandidateIndices(List<Vector2> loop, Vector2 pos, float searchRadius)
    {
        var indices = new List<int>();
        float radiusSqr = searchRadius * searchRadius;

        for (int i = 0; i < loop.Count; i++)
        {
            if ((loop[i] - pos).sqrMagnitude <= radiusSqr)
                indices.Add(i);
        }

        if (indices.Count == 0)
            indices.Add(FindClosestIndex(loop, pos));

        return indices;
    }

    /// <summary>
    /// True if any segment of the path lies inside this obstacle's collider.
    /// </summary>
    public bool PathPenetratesCollider(List<Vector2> path)
    {
        col ??= GetComponent<Collider2D>();
        if (col == null || path == null || path.Count < 2) return false;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 a = path[i];
            Vector2 b = path[i + 1];

            for (int s = 1; s <= 3; s++)
            {
                float t = s / 4f;
                Vector2 sample = Vector2.Lerp(a, b, t);
                if (col.OverlapPoint(sample))
                    return true;
            }

            RaycastHit2D[] hits = Physics2D.LinecastAll(a, b);
            foreach (var h in hits)
            {
                if (h.collider != col) continue;
                if (h.fraction > 0.03f && h.fraction < 0.97f)
                    return true;
            }
        }

        return false;
    }

    public Collider2D GetCollider()
    {
        col ??= GetComponent<Collider2D>();
        return col;
    }

    private Dictionary<int, List<Vector2>> BuildOrderedPathGroups()
    {
        var groups = new Dictionary<int, List<Vector2>>();
        foreach (var wp in wrapPoints)
        {
            if (!wp.enabled) continue;
            if (!groups.TryGetValue(wp.pathIndex, out var list))
            {
                list = new List<Vector2>();
                groups[wp.pathIndex] = list;
            }
            list.Add(wp.position);
        }
        return groups;
    }

    private static int FindClosestIndex(List<Vector2> loop, Vector2 pos)
    {
        int best = 0;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < loop.Count; i++)
        {
            float d = (loop[i] - pos).sqrMagnitude;
            if (d < bestSqr)
            {
                bestSqr = d;
                best = i;
            }
        }
        return best;
    }

    private static List<Vector2> BuildArc(List<Vector2> loop, int iFrom, int iTo, int step, Vector2 fromPos, Vector2 toPos)
    {
        int n = loop.Count;
        var path = new List<Vector2> { fromPos };

        if (iFrom == iTo)
        {
            path.Add(toPos);
            return path;
        }

        int i = iFrom;
        int guard = 0;
        while (i != iTo && guard++ < n + 1)
        {
            i = (i + step + n) % n;
            path.Add(loop[i]);
        }

        if (Vector2.Distance(path[^1], toPos) > 0.001f)
            path.Add(toPos);

        return path;
    }

    private List<Vector2> ChooseExteriorArc(
        List<Vector2> cw,
        List<Vector2> ccw,
        float sideSign,
        Vector2 sideReference,
        List<Vector2> loop)
    {
        bool cwPen = PathPenetratesCollider(cw);
        bool ccwPen = PathPenetratesCollider(ccw);

        if (!cwPen && ccwPen) return cw;
        if (cwPen && !ccwPen) return ccw;

        if (!cwPen && !ccwPen && Mathf.Abs(sideSign) > 0.01f)
        {
            float cwSide = PathSideRelativeTo(cw, sideReference);
            float ccwSide = PathSideRelativeTo(ccw, sideReference);

            bool cwMatch = Mathf.Sign(cwSide) == Mathf.Sign(sideSign);
            bool ccwMatch = Mathf.Sign(ccwSide) == Mathf.Sign(sideSign);

            if (cwMatch && !ccwMatch) return cw;
            if (ccwMatch && !cwMatch) return ccw;
        }

        Vector2 center = ComputeCentroid(loop);
        float extCw = (cw[cw.Count / 2] - center).sqrMagnitude;
        float extCcw = (ccw[ccw.Count / 2] - center).sqrMagnitude;

        if (Mathf.Abs(extCw - extCcw) > 0.0001f)
            return extCw > extCcw ? cw : ccw;

        return PolylineLength(cw) >= PolylineLength(ccw) ? cw : ccw;
    }

    private static float PathSideRelativeTo(List<Vector2> path, Vector2 reference)
    {
        float sum = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 edge = path[i + 1] - path[i];
            Vector2 toRef = reference - path[i];
            sum += Cross(edge, toRef);
        }
        return sum;
    }

    private static Vector2 ComputeCentroid(List<Vector2> points)
    {
        Vector2 sum = Vector2.zero;
        foreach (var p in points) sum += p;
        return sum / points.Count;
    }

    private static float PolylineLength(List<Vector2> path)
    {
        float len = 0f;
        for (int i = 0; i < path.Count - 1; i++)
            len += Vector2.Distance(path[i], path[i + 1]);
        return len;
    }

    public void GeneratePoints()
    {
        col ??= GetComponent<Collider2D>();
        wrapPoints.Clear();

        if (col == null) return;

        var composite = GetComponent<CompositeCollider2D>();
        if (composite != null) { GenerateFromComposite(composite); return; }

        switch (col)
        {
            case BoxCollider2D box: GenerateFromBox(box); break;
            case PolygonCollider2D poly: GenerateFromPolygon(poly); break;
            default:
                Debug.LogWarning($"[ObjectWrapPoint] Unsupported collider type: " +
                                 $"{col.GetType().Name} on {name}. " +
                                 $"Only Box, Polygon, and Composite are supported.");
                break;
        }
    }

    private void GenerateFromComposite(CompositeCollider2D composite)
    {
        var pathBuf = new List<Vector2>();
        for (int i = 0; i < composite.pathCount; i++)
        {
            composite.GetPath(i, pathBuf);
            var worldPts = new List<Vector2>(pathBuf.Count);
            foreach (var localPt in pathBuf)
                worldPts.Add(composite.transform.TransformPoint(localPt));

            AddFilteredPath(worldPts, i);
        }
    }

    private void GenerateFromBox(BoxCollider2D box)
    {
        Bounds b = box.bounds;
        Vector2 mn = b.min;
        Vector2 mx = b.max;

        var pts = new List<Vector2>
        {
            new(mn.x, mn.y),
            new(mx.x, mn.y),
            new(mx.x, mx.y),
            new(mn.x, mx.y),
        };

        AddFilteredPath(pts, 0);
    }

    private void GenerateFromPolygon(PolygonCollider2D poly)
    {
        for (int i = 0; i < poly.pathCount; i++)
        {
            var worldPts = new List<Vector2>();
            foreach (var localPt in poly.GetPath(i))
                worldPts.Add(poly.transform.TransformPoint(localPt));

            AddFilteredPath(worldPts, i);
        }
    }

    /// Adds points from a closed path, optionally skipping concave or
    /// too-sharp corners based on the Inspector settings.
    private void AddFilteredPath(List<Vector2> pts, int pathIndex)
    {
        var merged = MergeClosePoints(pts, mergeDistance);

        int count = merged.Count;
        if (count < 3) return;

        for (int i = 0; i < count; i++)
        {
            Vector2 prev = merged[(i - 1 + count) % count];
            Vector2 curr = merged[i];
            Vector2 next = merged[(i + 1) % count];

            if (IsCollinear(prev, curr, next)) continue;
            if (filterConcave && IsConcave(prev, curr, next)) continue;
            if (IsAngleTooSharp(prev, curr, next, minAngle)) continue;

            Add(curr, pathIndex);
        }
    }

    private static List<Vector2> MergeClosePoints(List<Vector2> pts, float minDist)
    {
        var result = new List<Vector2>(pts.Count);
        float sqrMin = minDist * minDist;

        foreach (var p in pts)
        {
            // Only add if far enough from the last kept point
            if (result.Count == 0 ||
                (p - result[result.Count - 1]).sqrMagnitude > sqrMin)
                result.Add(p);
        }

        // Also check wrap-around: last point vs first point
        if (result.Count > 1 &&
            (result[result.Count - 1] - result[0]).sqrMagnitude <= sqrMin)
            result.RemoveAt(result.Count - 1);

        return result;
    }

    // In the Filter section — add this new helper
    /// Returns true if prev, curr, next are on the same straight line.
    /// Uses cross product — if it's ~0 the points are collinear.
    private static bool IsCollinear(Vector2 prev, Vector2 curr, Vector2 next, float tolerance = 0.001f)
    {
        Vector2 a = (curr - prev).normalized;  // ← normalize so scale doesn't matter
        Vector2 b = (next - curr).normalized;  // ← normalize so scale doesn't matter
        float cross = a.x * b.y - a.y * b.x;
        return Mathf.Abs(cross) < tolerance;
    }

    /// Cross product Z of (curr-prev) × (next-curr).
    /// Positive = convex (left turn), Negative = concave (right turn)
    /// for a counter-clockwise wound polygon (Unity's default winding).
    private static bool IsConcave(Vector2 prev, Vector2 curr, Vector2 next)
    {
        Vector2 a = curr - prev;
        Vector2 b = next - curr;
        float cross = a.x * b.y - a.y * b.x;
        return cross < 0f;  // negative = concave for CCW winding
    }

    /// Returns true if the interior angle at curr is less than minAngle degrees.
    private static bool IsAngleTooSharp(Vector2 prev, Vector2 curr, Vector2 next, float minAngle)
    {
        if (minAngle <= 0f) return false;
        Vector2 toPrev = (prev - curr).normalized;
        Vector2 toNext = (next - curr).normalized;
        float angle = Vector2.Angle(toPrev, toNext);
        return angle < minAngle;
    }

    private void Add(Vector2 pos, int pathIndex) =>
        wrapPoints.Add(new WrapPoint(pos, pathIndex));
}