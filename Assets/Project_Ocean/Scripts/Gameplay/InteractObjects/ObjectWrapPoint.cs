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
    [SerializeField] private List<WrapPoint> wrapPoints = new();

    [Header("Setting")]
    [Tooltip("Merge points closer than this distance (fixes composite seam artifacts)")]
    [SerializeField] private float mergeDistance = 0.5f;
    [Tooltip("Skip concave corners (inward corners the rope can't wrap around)")]
    [SerializeField] private bool filterConcave = true;
    [Tooltip("Skip corners sharper than this angle in degrees (0 = keep all convex)")]
    [SerializeField][Range(0f, 170f)] private float minAngle = 10f;

    [Header("Debug")]
    [SerializeField] private bool drawDebug = true;
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