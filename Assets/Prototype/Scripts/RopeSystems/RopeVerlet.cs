using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class RopeVerlet : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;

        [Header("Rope")]
        [SerializeField] private int ropeSegmentNum = 50;
        [SerializeField] private float ropeSegmentLength = 0.225f;

        [Header("Physics")]
        [SerializeField] private Vector2 gravityForce = new(0f, -2f);
        [SerializeField] private float dampingFactor = 0.98f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.18f;
        [SerializeField] private float surfaceFriction = 0.9f;

        [Header("Constraints")]
        [SerializeField] private int constraintIterations = 50;

        [Header("Max Length System")]
        [SerializeField] private float maxLength = 8f;
        [SerializeField] private float pullForce = 15f;
        [SerializeField] private float stopDistance = 1f;

        [Header("Debug")]
        [SerializeField] private float currentLength;
        public float CurrentRopeLength { get { return currentLength; } }

        private LineRenderer line;
        private List<RopeSegment> ropeSegments = new();

        private bool isPullBack = false;

        public struct RopeSegment
        {
            public Vector2 CurPos;
            public Vector2 OldPos;

            public RopeSegment(Vector2 pos)
            {
                CurPos = pos;
                OldPos = pos;
            }
        }

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = ropeSegmentNum;

            InitializeRope();
        }

        private void Update()
        {
            DrawRope();
        }

        private void FixedUpdate()
        {
            Simulate();

            for (int i = 0; i < constraintIterations; i++)
            {
                ApplyConstraints();

                if (i % 3 == 0)
                    HandleCollision();
            }

            // ✅ Calculate once per frame
            currentLength = GetRopeLength();

            LimitRopeLength();
            HandleMaxLength();
        }

        private void InitializeRope()
        {
            ropeSegments.Clear();

            Vector2 startPos = startPoint.position;
            Vector2 endPos = endPoint.position;

            for (int i = 0; i < ropeSegmentNum; i++)
            {
                float t = i / (float)(ropeSegmentNum - 1);
                Vector2 pos = Vector2.Lerp(startPos, endPos, t);

                ropeSegments.Add(new RopeSegment(pos));
            }
        }

        private void DrawRope()
        {
            Vector3[] ropePos = new Vector3[ropeSegmentNum];

            for (int i = 0; i < ropeSegments.Count; i++)
            {
                ropePos[i] = ropeSegments[i].CurPos;
            }

            line.SetPositions(ropePos);
        }

        private void Simulate()
        {
            for (int i = 0; i < ropeSegments.Count; i++)
            {
                RopeSegment segment = ropeSegments[i];

                Vector2 velocity = (segment.CurPos - segment.OldPos) * dampingFactor;

                segment.OldPos = segment.CurPos;
                segment.CurPos += velocity;
                segment.CurPos += gravityForce * Time.fixedDeltaTime;

                ropeSegments[i] = segment;
            }
        }

        private void ApplyConstraints()
        {
            // Forward
            for (int i = 0; i < ropeSegmentNum - 1; i++)
            {
                SolveSegment(i, i + 1);
            }

            // Backward
            for (int i = ropeSegmentNum - 1; i > 0; i--)
            {
                SolveSegment(i, i - 1);
            }

            // Lock endpoints
            RopeSegment first = ropeSegments[0];
            first.CurPos = startPoint.position;
            ropeSegments[0] = first;

            RopeSegment last = ropeSegments[ropeSegmentNum - 1];
            last.CurPos = endPoint.position;
            ropeSegments[ropeSegmentNum - 1] = last;
        }

        private void SolveSegment(int i, int j)
        {
            RopeSegment a = ropeSegments[i];
            RopeSegment b = ropeSegments[j];

            Vector2 delta = a.CurPos - b.CurPos;
            float dist = delta.magnitude;

            if (dist == 0) return;

            float diff = (dist - ropeSegmentLength) / dist;
            Vector2 move = delta * 0.5f * diff;

            a.CurPos -= move;
            b.CurPos += move;

            ropeSegments[i] = a;
            ropeSegments[j] = b;
        }

        private float GetRopeLength()
        {
            float length = 0f;

            for (int i = 1; i < ropeSegments.Count; i++)
            {
                length += Vector2.Distance(
                    ropeSegments[i - 1].CurPos,
                    ropeSegments[i].CurPos
                );
            }

            return length;
        }

        private void LimitRopeLength()
        {
            if (currentLength > maxLength)
            {
                RopeSegment first = ropeSegments[0];
                RopeSegment last = ropeSegments[ropeSegments.Count - 1];

                Vector2 dir = (last.CurPos - first.CurPos).normalized;

                last.CurPos = first.CurPos + dir * maxLength;

                ropeSegments[ropeSegments.Count - 1] = last;
            }
        }

        private void HandleMaxLength()
        {
            if (currentLength >= maxLength)
            {
                isPullBack = true;
            }

            if (!isPullBack) return;

            RopeSegment first = ropeSegments[0];
            RopeSegment last = ropeSegments[ropeSegments.Count - 1];

            Vector2 dir = (first.CurPos - last.CurPos).normalized;

            // Apply pull-back force (Verlet style)
            Vector2 velocity = last.CurPos - last.OldPos;
            velocity += dir * pullForce * Time.fixedDeltaTime;

            last.OldPos = last.CurPos - velocity;
            ropeSegments[ropeSegments.Count - 1] = last;

            // Stop condition
            if (Vector2.Distance(first.CurPos, last.CurPos) < stopDistance)
            {
                isPullBack = false;
            }
        }

        private void HandleCollision()
        {
            for (int i = 1; i < ropeSegments.Count - 1; i++)
            {
                RopeSegment segment = ropeSegments[i];
                Vector2 velocity = segment.CurPos - segment.OldPos;

                RaycastHit2D hit = Physics2D.Linecast(segment.OldPos, segment.CurPos, collisionMask);

                if (hit.collider != null)
                {
                    Vector2 normal = hit.normal;
                    segment.CurPos = hit.point + normal * (collisionRadius + 0.001f);

                    float vn = Vector2.Dot(velocity, normal);
                    if (vn < 0f)
                        velocity -= normal * vn;
                }

                Collider2D overlap = Physics2D.OverlapCircle(segment.CurPos, collisionRadius, collisionMask);

                if (overlap != null)
                {
                    Vector2 closest = overlap.ClosestPoint(segment.CurPos);
                    Vector2 dir = segment.CurPos - closest;
                    float dist = dir.magnitude;

                    if (dist > 0f && dist < collisionRadius)
                    {
                        Vector2 normal = dir / dist;

                        float pushOut = Mathf.Min(collisionRadius - dist, 0.02f);
                        segment.CurPos += normal * pushOut;

                        float vn = Vector2.Dot(velocity, normal);
                        if (vn < 0f)
                            velocity -= normal * vn;

                        if (Mathf.Abs(vn) < 0.01f)
                            velocity *= 0.2f;
                    }
                }

                velocity *= surfaceFriction;

                if (velocity.magnitude < 0.001f)
                    velocity = Vector2.zero;

                velocity = Vector2.ClampMagnitude(velocity, 1.5f);

                segment.OldPos = segment.CurPos - velocity;
                ropeSegments[i] = segment;
            }
        }
    }
}