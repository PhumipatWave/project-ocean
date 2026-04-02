using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class DynamicPhysicRope : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;

        private List<Vector2> anchors = new();
        private List<int> anchorIndices = new();

        [Header("Rope Settings")]
        [SerializeField] private float segmentLength = 0.25f;
        [SerializeField] private int constraintIterations = 50;

        [Header("Physics")]
        [SerializeField] private Vector2 gravity = new(0f, -2f);
        [SerializeField] private float damping = 0.98f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.15f;
        [SerializeField] private float friction = 0.9f;

        [Header("Backtrack Settings")]
        [SerializeField] private float removeAngle = 120f;

        private List<RopeSegment> segments = new();
        [SerializeField] private LineRenderer line;

        struct RopeSegment
        {
            public Vector2 pos;
            public Vector2 oldPos;

            public RopeSegment(Vector2 p)
            {
                pos = p;
                oldPos = p;
            }
        }

        private void Awake()
        {
            line = GetComponent<LineRenderer>();

            line.startWidth = 0.1f;
            line.endWidth = 0.1f;

            Initialize();
        }

        private void Initialize()
        {
            segments.Clear();

            Vector2 start = startPoint.position;
            Vector2 end = endPoint.position;

            segments.Add(new RopeSegment(start));
            segments.Add(new RopeSegment(end));
        }

        private void FixedUpdate()
        {
            GrowRope();
            ShrinkRope();

            LimitLength();

            Simulate();

            for (int i = 0; i < constraintIterations; i++)
            {
                ApplyConstraints();

                if (i % 2 == 0)
                    HandleCollision();
            }

            UpdateAnchors();
        }

        private void LateUpdate()
        {
            Draw();
        }

        /// <summary>
        /// Create rope object with dynamic to grow up the rope by player distance
        /// </summary>
        private void GrowRope()
        {
            Vector2 target = endPoint.position;

            RopeSegment last = segments[segments.Count - 1];
            float dist = Vector2.Distance(last.pos, target);

            // If distance > segment length, add new segment
            if (dist > segmentLength)
            {
                Vector2 dir = (target - last.pos).normalized;
                Vector2 newPos = last.pos + dir * segmentLength;

                segments.Add(new RopeSegment(newPos));
            }
        }

        private void ShrinkRope()
        {
            if (segments.Count < 3) return;

            Vector2 current = endPoint.position;
            Vector2 last = segments[^1].pos;
            Vector2 prev = segments[^2].pos;

            Vector2 ropeDir = (last - prev).normalized;
            Vector2 moveDir = (current - last).normalized;

            float dot = Vector2.Dot(ropeDir, moveDir);
            float threshold = Mathf.Cos(removeAngle * Mathf.Deg2Rad);

            if (dot < threshold && Vector2.Distance(current, last) < segmentLength * 2f)
            {
                segments.RemoveAt(segments.Count - 1);
            }
        }

        private void Simulate()
        {
            for (int i = 0; i < segments.Count; i++)
            {
                RopeSegment seg = segments[i];

                Vector2 velocity = (seg.pos - seg.oldPos) * damping;

                seg.oldPos = seg.pos;
                seg.pos += velocity;
                seg.pos += gravity * Time.fixedDeltaTime;

                segments[i] = seg;
            }
        }

        private void ApplyConstraints()
        {
            // Keep first point locked to player
            RopeSegment first = segments[0];
            first.pos = startPoint.position;
            segments[0] = first;

            // Keep last point at target
            RopeSegment last = segments[^1];
            last.pos = endPoint.position;
            segments[^1] = last;

            // Solve distances
            for (int i = 0; i < segments.Count - 1; i++)
            {
                Solve(i, i + 1);
            }

            for (int i = segments.Count - 1; i > 0; i--)
            {
                Solve(i, i - 1);
            }

            // ##################################################################
            // APPLY ANCHORS
            for (int a = 0; a < anchors.Count; a++)
            {
                int index = Mathf.Clamp(anchorIndices[a], 1, segments.Count - 2);

                RopeSegment seg = segments[index];
                seg.pos = anchors[a];
                segments[index] = seg;
            }
                // ##################################################################
        }

        // ##################################################################
        private void UpdateAnchors()
        {
            if (anchors.Count == 0) return;

            Vector2 lastAnchor = anchors[^1];

            // Previous point (anchor or player)
            Vector2 prev = anchors.Count > 1
                ? anchors[^2]
                : (Vector2)startPoint.position;

            // Rope direction into anchor
            Vector2 ropeDir = (lastAnchor - prev).normalized;

            // Player movement direction from anchor
            Vector2 playerDir = ((Vector2)endPoint.position - lastAnchor).normalized;

            float dot = Vector2.Dot(ropeDir, playerDir);

            // If player moves to opposite side → unwrap
            if (dot > 0.3f || Vector2.Distance(endPoint.position, lastAnchor) < 0.5f)
            {
                anchors.RemoveAt(anchors.Count - 1);
                anchorIndices.RemoveAt(anchorIndices.Count - 1);
            }
        }
        // ##################################################################

        private void Solve(int i, int j)
        {
            RopeSegment a = segments[i];
            RopeSegment b = segments[j];

            Vector2 delta = a.pos - b.pos;
            float dist = delta.magnitude;

            if (dist == 0) return;

            float diff = (dist - segmentLength) / dist;
            Vector2 move = delta * 0.5f * diff;

            a.pos -= move;
            b.pos += move;

            segments[i] = a;
            segments[j] = b;
        }

        private void HandleCollision()
        {
            for (int i = 1; i < segments.Count - 1; i++)
            {
                RopeSegment seg = segments[i];
                Vector2 velocity = seg.pos - seg.oldPos;

                Collider2D hit = Physics2D.OverlapCircle(seg.pos, collisionRadius, collisionMask);

                if (hit != null)
                {
                    Vector2 closest = hit.ClosestPoint(seg.pos);
                    Vector2 dir = seg.pos - closest;
                    float dist = dir.magnitude;

                    if (dist > 0f && dist < collisionRadius)
                    {
                        Vector2 normal = dir / dist;
                        seg.pos = closest + normal * collisionRadius;

                        float vn = Vector2.Dot(velocity, normal);
                        if (vn < 0f)
                            velocity -= normal * vn;
                    }
                }

                velocity *= friction;
                seg.oldPos = seg.pos - velocity;

                segments[i] = seg;

                // ##################################################################
                // WRAP DETECTION
                RaycastHit2D lineHit = Physics2D.Linecast(
                    segments[i - 1].pos,
                    segments[i].pos,
                    collisionMask
                );

                if (lineHit.collider != null)
                {
                    RopeAnchorPoints anchorComp = lineHit.collider.GetComponent<RopeAnchorPoints>();

                    if (anchorComp != null)
                    {
                        Vector2 wrapPoint = anchorComp.GetClosestPoint(lineHit.point);

                        // Prevent too many anchors
                        if (anchors.Count > 15) continue;

                        // Prevent duplicates (check ALL anchors, not just last)
                        bool tooClose = false;

                        foreach (var a in anchors)
                        {
                            if (Vector2.Distance(a, wrapPoint) < 0.4f)
                            {
                                tooClose = true;
                                break;
                            }
                        }

                        if (!tooClose)
                        {
                            anchors.Add(wrapPoint);
                            anchorIndices.Add(i);
                        }
                    }
                }
                // ##################################################################
            }
        }

        private void Draw()
        {
            line.positionCount = segments.Count;

            for (int i = 0; i < segments.Count; i++)
            {
                line.SetPosition(i, segments[i].pos);
            }
        }

        private void LimitLength()
        {
            float maxLength = Vector2.Distance(startPoint.position, endPoint.position);

            float currentLength = segments.Count * segmentLength;

            while (segments.Count > 2 && currentLength > maxLength)
            {
                segments.RemoveAt(segments.Count - 1);
                currentLength -= segmentLength;
            }
        }
    }
}
