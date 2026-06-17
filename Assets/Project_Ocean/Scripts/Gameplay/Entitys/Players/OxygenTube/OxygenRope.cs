using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.Testing
{
    public class OxygenRope : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LineRenderer line;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private GameObject startPointPrefab;
        [SerializeField] private TextMeshProUGUI lengthText;

        [Header("Rope")]
        [SerializeField] private float segmentLength = 0.3f;
        [SerializeField] private int solverIterations = 20;
        [SerializeField] private float maxRopeLength = 150f;
        [SerializeField] private Vector2 gravity = new(0f, -1.5f);
        [SerializeField] private float damping = 0.98f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float segRadius = 0.12f;

        [Header("Wrap")]
        [SerializeField] private int wrapCooldownFrames = 8;

        public float CurrentLength { get; private set; }
        public float MaxRopeLength { get => maxRopeLength; set => maxRopeLength = value; }
        public GameObject StartPointObj { get; private set; }
        public GameObject StartPointPrefab => startPointPrefab;

        private struct Seg
        {
            public Vector2 pos, old;
            public Seg(Vector2 p) { pos = p; old = p; }
            public void Pin(Vector2 p) { pos = p; old = p; }
        }

        private struct WrapPin
        {
            public Vector2 pos;
            public float side; // cross-product side when this anchor was created
        }

        // pins[0] = fixed rope start, pins[1+] = wrap corners
        private readonly List<WrapPin> pins = new();

        // Verlet segments from pins[^1] to player
        private readonly List<Seg> segs = new();

        private readonly List<Vector2> drawBuf      = new();
        private readonly List<Vector2> cornerBuf    = new();
        private readonly List<Vector2> compositeBuf = new(); // reuse buffer for CompositeCollider2D

        private int wrapCooldown;
        private int unwrapCooldown;
        private Coroutine limitRoutine;

        private void Awake()
        {
            line ??= GetComponent<LineRenderer>();
        }

        private void FixedUpdate()
        {
            if (!InOcean()) return;
            if (StartPointObj == null || playerTransform == null || segs.Count < 2) return;

            if (wrapCooldown   > 0) wrapCooldown--;
            if (unwrapCooldown > 0) unwrapCooldown--;

            GrowSegs();   // extend segments so there is something to simulate
            ShrinkSegs(); // prune bunched-up segments when player moves back

            Integrate();
            PinEnds();
            Solve();
            PushOutColliders();

            if (wrapCooldown   == 0) TryWrap();
            if (unwrapCooldown == 0) TryUnwrap();

            CurrentLength = MeasureLength();
            UpdateLengthUI();
            CheckLimit();
        }

        private void LateUpdate()
        {
            if (!InOcean()) return;
            DrawRope();
        }

        /// Call this to place/move the fixed start of the rope.
        public void SetStartPoint(Vector3 worldPos)
        {
            if (StartPointObj == null)
                StartPointObj = Instantiate(startPointPrefab, worldPos, Quaternion.identity);
            else
                StartPointObj.transform.position = worldPos;

            BuildRope();
        }

        /// Clears all wrap pins and rebuilds segments from start to player.
        public void BuildRope()
        {
            pins.Clear();
            pins.Add(new WrapPin { pos = StartPointObj.transform.position, side = 0f });
            RebuildSegs();
        }

        /// Full reset — call when returning to land.
        public void ResetRope()
        {
            if (limitRoutine != null) { StopCoroutine(limitRoutine); limitRoutine = null; }
            pins.Clear();
            segs.Clear();
            wrapCooldown   = 0;
            unwrapCooldown = 0;
            CurrentLength  = 0f;
            if (line != null) line.positionCount = 0;
            if (lengthText != null) lengthText.enabled = false;
        }

        public void IncreaseMaxLength(float amount) => maxRopeLength += amount;

        // ── Segment growth / shrink ───────────────────────────────────────

        // Add one segment per frame when the gap between the second-to-last seg and the
        // player is too wide. This ensures the rope has free segments to simulate
        // even before the first corner wrap.
        private void GrowSegs()
        {
            if (segs.Count < 2) return;
            Vector2 prevPos   = segs[^2].pos;
            Vector2 playerPos = playerTransform.position;

            if (Vector2.Distance(prevPos, playerPos) > segmentLength * 1.5f)
            {
                Vector2 dir = (playerPos - prevPos).normalized;
                segs.Insert(segs.Count - 1, new Seg(prevPos + dir * segmentLength));
            }
        }

        // Remove one segment per frame when we have more segments than the straight-line
        // distance from anchor to player requires. Gravity sags the rope below the player,
        // so checking adjacent-segment proximity alone never triggers when the player
        // returns toward the anchor — this count-based check catches it instead.
        private void ShrinkSegs()
        {
            if (segs.Count <= 2) return;
            float straight = Vector2.Distance(pins[^1].pos, (Vector2)playerTransform.position);
            int   needed   = Mathf.Max(2, Mathf.CeilToInt(straight / segmentLength) + 1);
            if (segs.Count > needed + 1)
                segs.RemoveAt(segs.Count - 2);
        }

        // ── Simulation ────────────────────────────────────────────────────

        private void Integrate()
        {
            float dt = Time.fixedDeltaTime;
            for (int i = 1; i < segs.Count - 1; i++)
            {
                var s = segs[i];
                Vector2 vel = (s.pos - s.old) * damping;
                s.old = s.pos;
                s.pos += vel + gravity * (dt * dt);
                segs[i] = s;
            }
        }

        private void PinEnds()
        {
            var first = segs[0]; first.Pin(pins[^1].pos); segs[0] = first;
            var last  = segs[^1]; last.Pin(playerTransform.position); segs[^1] = last;
        }

        private void Solve()
        {
            for (int iter = 0; iter < solverIterations; iter++)
            {
                for (int i = 0; i < segs.Count - 1; i++)
                    SolvePair(i, i + 1);
                PinEnds();
            }
        }

        private void SolvePair(int a, int b)
        {
            var sa = segs[a]; var sb = segs[b];
            Vector2 d = sa.pos - sb.pos;
            float dist = d.magnitude;
            if (dist < 0.0001f) return;
            Vector2 fix = d * (0.5f * (dist - segmentLength) / dist);
            sa.pos -= fix; sb.pos += fix;
            segs[a] = sa; segs[b] = sb;
        }

        private void PushOutColliders()
        {
            for (int i = 1; i < segs.Count - 1; i++)
            {
                var s = segs[i];
                var col = Physics2D.OverlapCircle(s.pos, segRadius, collisionMask);
                if (col == null) continue;

                Vector2 closest = col.ClosestPoint(s.pos);
                Vector2 dir = s.pos - closest;
                float dist = dir.magnitude;

                if (dist < 0.0001f)
                    dir = (s.pos - (Vector2)col.bounds.center).normalized;
                else
                    dir /= dist;

                Vector2 vel = s.pos - s.old;
                s.pos = closest + dir * segRadius;
                float vn = Vector2.Dot(vel, dir);
                if (vn < 0f) vel -= dir * vn;
                s.old = s.pos - vel * 0.85f;
                segs[i] = s;
            }
        }

        // ── Wrap ──────────────────────────────────────────────────────────

        private void TryWrap()
        {
            // Cast directly from the last anchor to the player.
            // Raycasting between adjacent Verlet segments is unreliable because
            // PushOutColliders has already moved them outside any wall, so the
            // segment-pair linecast never sees a crossing.
            Vector2 from = pins[^1].pos;
            Vector2 to   = playerTransform.position;
            Vector2 dir  = (to - from);
            float   dist = dir.magnitude;
            if (dist < 0.001f) return;
            dir /= dist;

            // Offset start so the linecast doesn't immediately detect the wall
            // that the current anchor is sitting on.
            float   offset = segmentLength * 1.5f;
            if (offset >= dist) return; // anchor and player are too close to need a wrap

            RaycastHit2D hit = Physics2D.Linecast(from + dir * offset, to, collisionMask);
            if (hit.collider == null) return;

            Vector2 segDir = to - from;
            Vector2 corner = FindBestCorner(hit.collider, hit.point, segDir);

            if (!IsValidAnchor(corner)) return;

            Vector2 inDir    = (corner - from).normalized;
            Vector2 toPlayer = (to - corner).normalized;
            float cross      = inDir.x * toPlayer.y - inDir.y * toPlayer.x;
            float side       = Mathf.Abs(cross) < 0.001f ? 1f : Mathf.Sign(cross);

            pins.Add(new WrapPin { pos = corner, side = side });
            RebuildSegs();
            wrapCooldown   = wrapCooldownFrames;
            unwrapCooldown = wrapCooldownFrames; // block unwrap from firing this same wrap event
        }

        private void TryUnwrap()
        {
            if (pins.Count <= 1) return;

            WrapPin last      = pins[^1];
            Vector2 prevPos   = pins[^2].pos;
            Vector2 playerPos = playerTransform.position;

            // Direction of the incoming rope segment (prevPin → corner)
            Vector2 inDir    = (last.pos - prevPos).normalized;
            // Direction from the corner toward the player
            Vector2 toPlayer = (playerPos - last.pos).normalized;
            float   cross    = inDir.x * toPlayer.y - inDir.y * toPlayer.x;

            // Condition A — player swung laterally past the corner to the other side.
            bool sideCrossed = Mathf.Abs(cross) > 0.1f && Mathf.Sign(cross) != last.side;

            // Condition B — player retreated back along the rope past the corner toward prevPin.
            // Cross-product alone cannot detect this: when the player moves straight back,
            // toPlayer is nearly anti-parallel to inDir so cross stays near 0 (no side flip).
            // Project both the corner and the player onto inDir from prevPin to compare.
            float playerAlong = Vector2.Dot(playerPos - prevPos, inDir);
            float anchorAlong = Vector2.Dot(last.pos  - prevPos, inDir); // equals distance(prevPin → corner)
            bool  passedBack  = playerAlong < anchorAlong - segmentLength;

            if (!sideCrossed && !passedBack) return;

            pins.RemoveAt(pins.Count - 1);
            RebuildSegs();
            unwrapCooldown = wrapCooldownFrames;
            wrapCooldown   = wrapCooldownFrames;
        }

        private bool IsValidAnchor(Vector2 pos)
        {
            if (Vector2.Distance(pos, pins[^1].pos) < segmentLength * 2f) return false;
            if (Vector2.Distance(pos, playerTransform.position) < segmentLength * 2f) return false;
            foreach (var p in pins)
                if (Vector2.Distance(p.pos, pos) < segmentLength * 3f) return false;
            return true;
        }

        // Pick the collider corner closest to the hit point.
        // Slightly prefers corners that are off-axis from the rope direction (true corners over edge midpoints).
        private Vector2 FindBestCorner(Collider2D col, Vector2 hitPoint, Vector2 ropeDir)
        {
            GetColliderCorners(col, cornerBuf);
            if (cornerBuf.Count == 0) return hitPoint;

            Vector2 best = cornerBuf[0];
            float bestScore = float.MaxValue;

            foreach (var c in cornerBuf)
            {
                float dist = Vector2.Distance(c, hitPoint);
                Vector2 toC = c - hitPoint;
                float cross = ropeDir.x * toC.y - ropeDir.y * toC.x;
                // Penalise corners that are perfectly collinear with rope (those are edge midpoints)
                float score = dist + (Mathf.Abs(cross) < 0.1f ? 0.5f : 0f);
                if (score < bestScore) { bestScore = score; best = c; }
            }

            return best;
        }

        // If the obstacle has ObjectWrapPoint, use its hand-curated corners only.
        // This is essential for CompositeCollider2D (tile maps) which expose hundreds of
        // raw vertices — ObjectWrapPoint filters these down to the meaningful wrap corners.
        // All other colliders fall back to reading geometry directly.
        private void GetColliderCorners(Collider2D col, List<Vector2> result)
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

        private void DrawRope()
        {
            drawBuf.Clear();

            // Straight taut lines between pins (pins[0] = start, pins[1+] = wrap corners)
            foreach (var p in pins)
                drawBuf.Add(p.pos);

            // Verlet segments — skip index 0 because it sits on pins[^1] already
            for (int i = 1; i < segs.Count; i++)
                drawBuf.Add(segs[i].pos);

            line.positionCount = drawBuf.Count;
            for (int i = 0; i < drawBuf.Count; i++)
                line.SetPosition(i, drawBuf[i]);
        }

        private void RebuildSegs()
        {
            Vector2 from = pins[^1].pos;
            Vector2 to   = playerTransform != null ? (Vector2)playerTransform.position : from;
            int count    = Mathf.Max(2, Mathf.CeilToInt(Vector2.Distance(from, to) / segmentLength) + 1);

            segs.Clear();
            for (int i = 0; i < count; i++)
                segs.Add(new Seg(Vector2.Lerp(from, to, (float)i / (count - 1))));
        }

        private float MeasureLength()
        {
            float len = 0f;
            for (int i = 0; i < pins.Count - 1; i++)
                len += Vector2.Distance(pins[i].pos, pins[i + 1].pos);
            for (int i = 0; i < segs.Count - 1; i++)
                len += Vector2.Distance(segs[i].pos, segs[i + 1].pos);
            return len;
        }

        private void UpdateLengthUI()
        {
            if (lengthText == null) return;
            lengthText.enabled = true;
            lengthText.color = CurrentLength >= maxRopeLength ? Color.red : Color.white;
            lengthText.text = $"Length : {(int)CurrentLength} m.";
        }

        private bool InOcean() =>
            GameManager.Instance != null && GameManager.Instance.GameState == GameState.Ocean;

        private void CheckLimit()
        {
            if (CurrentLength <= maxRopeLength)
            {
                if (limitRoutine != null) { StopCoroutine(limitRoutine); limitRoutine = null; }
            }
            else if (limitRoutine == null)
                limitRoutine = StartCoroutine(LimitBreached());
        }

        private IEnumerator LimitBreached()
        {
            yield return new WaitForSeconds(5f);
            ResetRope();
            GameManager.Instance.ReturnToLand();
        }
    }
}
