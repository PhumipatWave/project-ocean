using UnityEngine;

public class RopeAnchorPoints : MonoBehaviour
{
    public Transform[] points;

    public Vector2 GetClosestPoint(Vector2 pos)
    {
        float minDist = float.MaxValue;
        Vector2 closest = pos;

        foreach (var p in points)
        {
            if (p == transform) continue;

            float dist = Vector2.Distance(pos, p.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = p.position;
            }
        }

        return closest;
    }
}