using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

namespace Game.Testing
{
    public static class RopeSystemUtil
    {
        public static void DrawNormalRope(LineRenderer line, GameObject startObj, GameObject endObj, int segment = 50, float ropeSag = .5f)
        {
            for (int i = 0; i < segment; i++)
            {
                float t = i / (float)(segment - 1);
                Vector2 startPos = startObj.transform.position;
                Vector2 endPos = endObj.transform.position;
                Vector2 pos = Vector2.Lerp(startPos, endPos, t);

                float sag = Mathf.Sin(t * Mathf.PI) * ropeSag;
                pos.y -= sag;

                line.SetPosition(i, pos);
            }
        }

        public static void LimitNormalRopeDistance(GameObject startObj, GameObject endObj, float maxLength)
        {
            Vector2 anchorPos = startObj.transform.position;
            Vector2 objsPos = endObj.transform.position;

            float dist = Vector2.Distance(objsPos, anchorPos);

            if (dist > maxLength)
            {
                Vector2 dir = (objsPos - anchorPos).normalized;
                Vector2 limitedPos = anchorPos + dir * maxLength;
                endObj.transform.position = limitedPos;
            }
        }

        public static void BreakNormalRopeLimit(GameObject startObj, GameObject endObj, ref bool isPullBack, ref float currentLength, float maxLength, float pullBackForce)
        {
            Vector2 startPos = startObj.transform.position;
            Vector2 endPos = endObj.transform.position;
            currentLength = Vector2.Distance(endPos, startPos);

            if (currentLength > maxLength)
            {
                isPullBack = true;
            }

            if (isPullBack)
            {
                Vector2 dir = (startPos - endPos).normalized;

                if (currentLength > 1f)
                {
                    endObj.GetComponent<Rigidbody2D>().AddForce(dir * pullBackForce, ForceMode2D.Force);
                }
                else
                {
                    endObj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                    isPullBack = false;
                }
            }
        }

        public static void DrawSmoothRopeLine(LineRenderer line, Transform anchor, Transform curPos, ref List<Vector2> points, float pointSpacing = .75f, int smoothResolution = 5)
        {
            if (points.Count == 0)
                points.Add(anchor.position);

            points[0] = anchor.position;

            Vector2 currentPos = curPos.position;

            float removeAngleDeg = 120f;
            float threshold = Mathf.Cos(removeAngleDeg * Mathf.Deg2Rad);


            // Backtrack check using dir
            while (points.Count > 2)
            {
                Vector2 last = points[points.Count - 1];
                Vector2 prev = points[points.Count - 2];
                Vector2 current = currentPos;

                Vector2 ropeDir = (last - prev).normalized;
                Vector2 moveDir = (current - last).normalized;

                float dot = Vector2.Dot(ropeDir, moveDir);

                // Only remove if moving backward (not just closer)
                if (dot < threshold && Vector2.Distance(current, last) < pointSpacing * 2f)
                {
                    points.RemoveAt(points.Count - 1);
                }
                else
                {
                    break;
                }
            }

            // Add new point
            if (Vector2.Distance(points[points.Count - 1], currentPos) > pointSpacing)
            {
                points.Add(currentPos);
            }

            // Smooth
            List<Vector2> smoothPoints = SmoothLine(points, smoothResolution);

            line.positionCount = smoothPoints.Count;

            for (int i = 0; i < smoothPoints.Count; i++)
            {
                line.SetPosition(i, smoothPoints[i]);
            }
        }

        private static Vector2 GetCatmullRomPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return .5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        private static List<Vector2> SmoothLine(List<Vector2> points, int resolution)
        {
            List<Vector2> smoothPoints = new List<Vector2>();

            if (points.Count < 2)
                return points;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 p0 = points[Mathf.Max(i - 1, 0)];
                Vector2 p1 = points[i];
                Vector2 p2 = points[i + 1];
                Vector2 p3 = points[Mathf.Min(i + 2, points.Count - 1)];

                for (int j = 0; j < resolution; j++)
                {
                    float t = j / (float)resolution;
                    smoothPoints.Add(GetCatmullRomPoint(p0, p1, p2, p3, t));
                }
            }

            smoothPoints.Add(points[points.Count - 1]);
            return smoothPoints;
        }

        private static float GetRopeLength(List<Vector2> points)
        {
            float length = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                length += Vector2.Distance(points[i - 1], points[i]);
            }

            return length;
        }

        public static void LimitSmoothRopeDistance(ref List<Vector2> points, ref float currentLength, float maxLength)
        {
            currentLength = GetRopeLength(points);

            if (currentLength > maxLength)
            {
                Vector2 anchorPos = points[0];
                Vector2 playerPos = points[points.Count - 1];
                Vector2 dir = (playerPos - anchorPos).normalized;

                Vector2 limitedPos = anchorPos + dir * maxLength;
                points[points.Count - 1] = limitedPos;
            }
        }

        public static void BreakSmoothRopeLimit(LineRenderer line, SpringJoint2D spring, GameObject startObj, GameObject endObj, ref List<Vector2> points, ref bool isPullBack, float maxLength)
        {
            float currentLength = GetRopeLength(points);

            if (currentLength > maxLength)
            {
                isPullBack = true;
                spring.enabled = true;
                spring.connectedBody = startObj.GetComponent<Rigidbody2D>();
            }

            if (isPullBack)
            {
                points.Clear();

                points.Add(startObj.transform.position);
                points.Add(endObj.transform.position);

                line.positionCount = points.Count;

                for (int i = 0; i < points.Count; i++)
                {
                    line.SetPosition(i, points[i]);
                }

                if (currentLength < 1f)
                {
                    spring.enabled = false;
                    isPullBack = false;

                    endObj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                }
            }
        }
    }
}