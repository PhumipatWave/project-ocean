using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class DynamicPhysicRope : MonoBehaviour
    {
        #region Fields
        [Header("References Points")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;

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

        [Header("Wrap Settings")]
        [SerializeField] private float removeAngle = 120f;
        [SerializeField] private int wrapCooldownFrames = 8;

        [SerializeField] private LineRenderer line;
        #endregion

        #region RopeStructures
        struct Segment
        {
            public Vector2 pos;
            public Vector2 old;
            public Segment(Vector2 p) { pos = p; old = p; }
        }

        class SubRope
        {
            public Vector2 pinStart;
            public Vector2 pinEnd;
            public List<Segment> segs = new();

            public SubRope(Vector2 from, Vector2 to)
            {
                pinStart = from;
                pinEnd = to;
                segs.Add(new Segment(from));
                segs.Add(new Segment(to));
            }
        }

        // Anchor stores the wrap point and the side the rope was on when it wrapped.
        // "side" = sign of the cross product of (inDir × outDir) at wrap time.
        // When endPoint crosses to the opposite side, we know the rope has swung back.
        struct Anchor
        {
            public Vector2 pos;
            public float wrapSide;
        }

        // State
        private List<Anchor> anchors = new();
        private List<SubRope> subRopes = new();
        private int cooldown = 0;
        #endregion

        #region UnityCallbacks
        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            Rebuild();
        }

        private void FixedUpdate()
        {
            if (cooldown > 0) cooldown--;

            subRopes[0].pinStart = startPoint.position;
            subRopes[^1].pinEnd = endPoint.position;

            GrowLastRope();
            ShrinkLastRope();

            Simulate();
            Constrain();
            HandleCollision();
            TryUnwrap();
        }

        private void LateUpdate() => Draw();
        #endregion

        #region RopeBuilding
        private void Draw()
        {
            int total = 0;
            for (int s = 0; s < subRopes.Count; s++)
                total += s == 0 ? subRopes[s].segs.Count : subRopes[s].segs.Count - 1;

            line.positionCount = total;

            int idx = 0;
            for (int s = 0; s < subRopes.Count; s++)
            {
                var segs = subRopes[s].segs;
                int startIdx = s == 0 ? 0 : 1;
                for (int i = startIdx; i < segs.Count; i++)
                    line.SetPosition(idx++, segs[i].pos);
            }
        }

        private void Rebuild()
        {
            anchors.Clear();
            subRopes.Clear();
            subRopes.Add(new SubRope(startPoint.position, endPoint.position));
        }

        private void GrowLastRope()
        {
            SubRope sr = subRopes[^1];
            Segment last = sr.segs[^1];

            float dist = Vector2.Distance(last.pos, sr.pinEnd);
            if (dist > segmentLength)
            {
                Vector2 dir = (sr.pinEnd - last.pos).normalized;
                sr.segs.Add(new Segment(last.pos + dir * segmentLength));
            }
        }

        private void ShrinkLastRope()
        {
            SubRope sr = subRopes[^1];
            if (sr.segs.Count < 3) return;

            Vector2 last = sr.segs[^1].pos;
            Vector2 prev = sr.segs[^2].pos;
            Vector2 ropeDir = (last - prev).normalized;
            Vector2 moveDir = (sr.pinEnd - last).normalized;

            float dot = Vector2.Dot(ropeDir, moveDir);
            float threshold = Mathf.Cos(removeAngle * Mathf.Deg2Rad);

            if (dot < threshold && Vector2.Distance(sr.pinEnd, last) < segmentLength * 2f)
                sr.segs.RemoveAt(sr.segs.Count - 1);
        }
        #endregion

        #region RopePhysics
        private void Simulate()
        {
            foreach (SubRope sr in subRopes)
            {
                var segs = sr.segs;
                for (int i = 0; i < segs.Count; i++)
                {
                    Segment s = segs[i];
                    Vector2 vel = (s.pos - s.old) * damping;
                    s.old = s.pos;
                    s.pos += vel;
                    s.pos += gravity * Time.fixedDeltaTime;
                    segs[i] = s;
                }
            }
        }

        private void Constrain()
        {
            for (int iter = 0; iter < constraintIterations; iter++)
                foreach (SubRope sr in subRopes)
                    SolveSubRope(sr);
        }

        private void SolveSubRope(SubRope sr)
        {
            var segs = sr.segs;

            Segment first = segs[0]; first.pos = sr.pinStart; segs[0] = first;
            Segment last = segs[^1]; last.pos = sr.pinEnd; segs[^1] = last;

            for (int i = 0; i < segs.Count - 1; i++) Solve(segs, i, i + 1);
            for (int i = segs.Count - 1; i > 0; i--) Solve(segs, i, i - 1);
        }

        private void Solve(List<Segment> segs, int i, int j)
        {
            Segment a = segs[i];
            Segment b = segs[j];

            Vector2 delta = a.pos - b.pos;
            float dist = delta.magnitude;
            if (dist < 0.0001f) return;

            float diff = (dist - segmentLength) / dist;
            Vector2 move = delta * 0.5f * diff;

            a.pos -= move;
            b.pos += move;

            segs[i] = a;
            segs[j] = b;
        }

        private void HandleCollision()
        {
            bool wrapDetected = false;

            for (int s = 0; s < subRopes.Count; s++)
            {
                SubRope sr = subRopes[s];
                bool isLast = s == subRopes.Count - 1;
                var segs = sr.segs;

                for (int i = 1; i < segs.Count - 1; i++)
                {
                    Segment seg = segs[i];
                    Vector2 vel = seg.pos - seg.old;

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

                            float vn = Vector2.Dot(vel, normal);
                            if (vn < 0f) vel -= normal * vn;
                        }
                    }

                    vel *= friction;
                    seg.old = seg.pos - vel;
                    segs[i] = seg;

                    if (!isLast || wrapDetected || cooldown > 0) continue;

                    RaycastHit2D lineHit = Physics2D.Linecast(
                        segs[i - 1].pos,
                        segs[i].pos,
                        collisionMask
                    );

                    if (lineHit.collider == null) continue;

                    RopeAnchorPoints anchorComp =
                        lineHit.collider.GetComponent<RopeAnchorPoints>();
                    if (anchorComp == null) continue;

                    Vector2 wrapPoint = anchorComp.GetClosestPoint(lineHit.point);

                    if (anchors.Count >= 15) continue;

                    bool tooClose = false;
                    foreach (Anchor a in anchors)
                    {
                        if (Vector2.Distance(a.pos, wrapPoint) < segmentLength * 2f)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose) continue;

                    if (Vector2.Distance(wrapPoint, startPoint.position) < segmentLength * 2f)
                        continue;

                    AddAnchor(wrapPoint, segs[i - 1].pos, segs[i].pos);
                    wrapDetected = true;
                }
            }
        }
        #endregion

        #region RopeWrapping
        private void AddAnchor(Vector2 wrapPoint, Vector2 segBefore, Vector2 segAfter)
        {
            // inDir from previous stable pin — not from physics segments
            Vector2 prevPin = anchors.Count > 0
                ? anchors[^1].pos
                : (Vector2)startPoint.position;

            Vector2 inDir = (wrapPoint - prevPin).normalized;
            Vector2 toEnd = ((Vector2)endPoint.position - wrapPoint).normalized;

            float cross = inDir.x * toEnd.y - inDir.y * toEnd.x;
            float side = Mathf.Sign(cross);

            // If degenerate, use segAfter direction
            if (Mathf.Abs(cross) < 0.01f)
            {
                Vector2 alt = (segAfter - wrapPoint).normalized;
                float c2 = inDir.x * alt.y - inDir.y * alt.x;
                side = Mathf.Sign(c2);
            }

            subRopes[^1].pinEnd = wrapPoint;
            anchors.Add(new Anchor { pos = wrapPoint, wrapSide = side });
            subRopes.Add(new SubRope(wrapPoint, endPoint.position));

            cooldown = wrapCooldownFrames;
            Debug.Log($"Wrapped at {wrapPoint} side={side}. anchors={anchors.Count}");
        }

        private void TryUnwrap()
        {
            if (anchors.Count == 0) return;
            if (cooldown > 0) return;

            Anchor last = anchors[^1];
            Vector2 anchor = last.pos;

            // Stable inDir: always from the previous fixed pin to this anchor
            // These are world positions from Transform.position — never wobble
            Vector2 prevPin = anchors.Count > 1
                ? anchors[^2].pos
                : (Vector2)startPoint.position;

            Vector2 inDir = (anchor - prevPin).normalized;
            Vector2 outDir = ((Vector2)endPoint.position - anchor).normalized;

            // Current side of endPoint relative to the incoming rope direction
            float cross = inDir.x * outDir.y - inDir.y * outDir.x;
            float currentSide = Mathf.Sign(cross);

            // Only unwrap when:
            // 1. endPoint has crossed to the opposite side (rope no longer bends here)
            // 2. endPoint is far enough from the anchor to be meaningful
            // 3. The incoming and outgoing directions are not nearly opposite
            //    (dot < -0.8 means endPoint is almost directly behind the anchor —
            //     that's a genuine wrap, never unwrap in that case)
            float dot = Vector2.Dot(inDir, outDir);
            float dist = Vector2.Distance(endPoint.position, anchor);

            bool sideCrossed = currentSide != last.wrapSide;
            bool farEnough = dist > segmentLength * 3f;
            bool notBehind = dot > -0.8f;

            if (sideCrossed && farEnough && notBehind)
            {
                anchors.RemoveAt(anchors.Count - 1);
                subRopes.RemoveAt(subRopes.Count - 1);
                subRopes[^1].pinEnd = endPoint.position;
                cooldown = wrapCooldownFrames;
                Debug.Log($"Unwrapped. anchors={anchors.Count}");
            }
        }
        #endregion
    }
}