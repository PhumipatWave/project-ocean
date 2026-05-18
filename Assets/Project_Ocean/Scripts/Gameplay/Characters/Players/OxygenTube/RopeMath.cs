using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>Shared 2D rope geometry helpers.</summary>
    internal static class RopeMath
    {
        public const float Epsilon = 0.0001f;
        public const float PinMatchDistance = 0.01f;

        public static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        public static float PolylineLength(IReadOnlyList<Vector2> path)
        {
            if (path == null || path.Count < 2) return 0f;

            float len = 0f;
            for (int i = 0; i < path.Count - 1; i++)
                len += Vector2.Distance(path[i], path[i + 1]);
            return len;
        }

        public static Vector2 SamplePolyline(IReadOnlyList<Vector2> path, float distance)
        {
            float walked = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                float segLen = Vector2.Distance(path[i], path[i + 1]);
                if (walked + segLen >= distance)
                {
                    float t = segLen < Epsilon ? 0f : (distance - walked) / segLen;
                    return Vector2.Lerp(path[i], path[i + 1], t);
                }
                walked += segLen;
            }
            return path[^1];
        }

        /// <summary>True when the chord between two points passes through the collider interior.</summary>
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

        public static float SignOrDefault(float value, float fallback = 1f) =>
            Mathf.Abs(value) < 0.01f ? fallback : Mathf.Sign(value);
    }
}
