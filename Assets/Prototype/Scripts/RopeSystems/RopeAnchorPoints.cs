using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RopeAnchorPoints : MonoBehaviour
{
    [Header("Circle/Capsule Settings")]
    [SerializeField] private int circleResolution = 32; // was 12 — needs to be dense
    [SerializeField] private float offset = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool drawDebug = true;
    [SerializeField] private float gizmoSize = 0.06f;

    private Collider2D col;
    private readonly List<Vector2> points = new();

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        GeneratePoints();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        col = GetComponent<Collider2D>();
        if (col != null) GeneratePoints();
    }
#endif

    public void GeneratePoints()
    {
        points.Clear();
        if (col == null) return;

        Bounds b = col.bounds;

        if (col is BoxCollider2D)
        {
            Vector2 min = b.min;
            Vector2 max = b.max;
            points.Add(new Vector2(min.x, min.y));
            points.Add(new Vector2(max.x, min.y));
            points.Add(new Vector2(max.x, max.y));
            points.Add(new Vector2(min.x, max.y));
            return;
        }

        if (col is PolygonCollider2D poly)
        {
            foreach (var p in poly.points)
                points.Add(poly.transform.TransformPoint(p));
            return;
        }

        // Circle / Capsule
        Vector2 center = b.center;
        float   radius = b.extents.magnitude;

        for (int i = 0; i < circleResolution; i++)
        {
            float   angle  = Mathf.PI * 2f * i / circleResolution;
            Vector2 dir    = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 origin = center + dir * (radius + offset);
            points.Add(col.ClosestPoint(origin));
        }
    }

    public Vector2 GetClosestPoint(Vector2 pos)
    {
        float   best   = float.MaxValue;
        Vector2 result = pos;
        foreach (var p in points)
        {
            float d = (pos - p).sqrMagnitude;
            if (d < best) { best = d; result = p; }
        }
        return result;
    }

    /// <summary>
    /// Exposes the full point list so DynamicPhysicRope can
    /// walk the convex hull to find the best wrap point.
    /// </summary>
    public IReadOnlyList<Vector2> GetPoints() => points;

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;
        if (col == null) col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = Color.red;
        foreach (var p in points) Gizmos.DrawSphere(p, gizmoSize);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Count; i++)
            Gizmos.DrawLine(points[i], points[(i + 1) % points.Count]);
    }
}