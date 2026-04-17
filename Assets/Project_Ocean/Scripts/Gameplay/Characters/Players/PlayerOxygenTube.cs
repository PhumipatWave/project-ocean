using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class PlayerOxygenTube : MonoBehaviour
    {
        #region Fields
        [Header("References Points")]
        [SerializeField] private LineRenderer line;

        [SerializeField] private GameObject startPointPrefab;
        public GameObject StartPointPrefab { get { return startPointPrefab; } }

        [SerializeField] private GameObject startPoint;
        public GameObject StartPoint { get { return startPoint; } set { startPoint = value; } }

        [SerializeField] private Transform endPoint;

        [Header("Rope Settings")]
        [SerializeField] private float segmentLength = 0.25f;
        [SerializeField] private int constraintIterations = 50;

        [SerializeField] private float maxRopeLength = 150f;
        public float MaxRopeLength { get { return maxRopeLength; } set { maxRopeLength = value; } }

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
        
        [SerializeField] private float mergeDistance = 1.0f;

        [SerializeField] private float currentLength;
        public float CurrentLength { get { return currentLength; } }

        [Header("Checkpoint System")]
        [SerializeField] private GameObject checkpointPrefab;

        private List<CheckpointAnchor> checkpoints = new();
        [SerializeField] private int maxCheckpoints = 3;

        public int CurrentCheckpointCount => checkpoints.Count;
        public int MaxCheckpointCount => maxCheckpoints;

        private RopeSnapshot savedSnapshot;
        #endregion

        #region RopeStructures
        struct Segment
        {
            public Vector2 pos;
            public Vector2 old;

            public Segment(Vector2 p)
            {
                pos = p;
                old = p;
            }
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

        struct Anchor
        {
            public Vector2 pos;
            public float wrapSide;
            public int ropeIndex;
        }

        private List<Anchor> anchors = new();
        private List<SubRope> subRopes = new();
        private int cooldown = 0;

        class CheckpointAnchor
        {
            public Vector2 pos;
            public GameObject obj;
            public RopeSnapshot snapshot;
            public int ropeIndex;
        }

        [System.Serializable]
        class RopeSnapshot
        {
            public List<SubRope> subRopes;
            public List<Anchor> anchors;
        }
        #endregion

        #region UnityCallbacks
        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            //Rebuild();
        }

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Ocean)
                return;

            if (Input.GetKeyDown(KeyCode.H))
                CreateCheckpoint();

            if (Input.GetKeyDown(KeyCode.T))
                TeleportToLastCheckpoint();

            /*if (Input.GetKeyDown(KeyCode.K))
                DeleteLastCheckpoint();*/
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.GameState != GameState.Ocean)
                return;

            if (cooldown > 0) cooldown--;

            subRopes[0].pinStart = startPoint.transform.position;
            subRopes[^1].pinEnd = endPoint.position;

            GrowLastRope();
            ShrinkLastRope();

            Simulate();
            Constrain();
            HandleCollision();
            UnattachRope();
            UpdateCurrentLength();
            CheckRopeLimit();
        }

        private void LateUpdate() 
        {
            if (GameManager.Instance.GameState != GameState.Ocean)
                return;

            Draw();
        } 
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

        public void Rebuild()
        {
            anchors.Clear();
            subRopes.Clear();
            subRopes.Add(new SubRope(startPoint.transform.position, endPoint.position));
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

            Segment first = segs[0];
            first.pos = sr.pinStart;
            segs[0] = first;

            Segment last = segs[^1];
            last.pos = sr.pinEnd;
            segs[^1] = last;

            for (int i = 0; i < segs.Count - 1; i++)
                Solve(segs, i, i + 1);

            for (int i = segs.Count - 1; i > 0; i--)
                Solve(segs, i, i - 1);
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

        /// Returns true if this wrapPoint is a valid new anchor.
        /// Rejects points that are: behind the incoming rope direction,
        /// already wrapped (any anchor within minDist),
        /// or would cause the new subrope to immediately re-cross an existing anchor.
        private bool IsValidWrapPoint(Vector2 wrapPoint, Collider2D hitCollider)
        {
            if (Vector2.Distance(wrapPoint, startPoint.transform.position) < segmentLength * 3f) return false;
            if (Vector2.Distance(wrapPoint, endPoint.position) < segmentLength * 3f) return false;

            foreach (Anchor a in anchors)
                if (Vector2.Distance(a.pos, wrapPoint) < segmentLength * 4f) return false;

            // The rope comes FROM prevPin TO wrapPoint.
            // The new segment goes FROM wrapPoint TO endPoint.
            // If the angle between inDir and toEnd is > 170°, the point is
            // "behind" us — the rope would fold back on itself.
            Vector2 prevPin = anchors.Count > 0
                ? anchors[^1].pos
                : (Vector2)startPoint.transform.position;

            Vector2 inDir = (wrapPoint - prevPin).normalized;
            Vector2 toEnd = ((Vector2)endPoint.position - wrapPoint).normalized;
            float dot = Vector2.Dot(inDir, toEnd);
            if (dot < -0.7f) return false;  // > ~135° reversal → reject

            // Check if the new subrope (wrapPoint => endPoint) passes through
            // ANY existing anchor's collider — this catches the cascade where
            // new rope crosses a previous wrap point on a different GameObject.
            Vector2 towardEnd = ((Vector2)endPoint.position - wrapPoint).normalized;
            Vector2 scanStart = wrapPoint + towardEnd * (segmentLength * 2f);

            RaycastHit2D[] hits = Physics2D.LinecastAll(scanStart, endPoint.position, collisionMask);
            foreach (var h in hits)
            {
                // If new path crosses any existing anchor position, reject
                foreach (Anchor a in anchors)
                    if (Vector2.Distance(h.point, a.pos) < segmentLength * 3f) return false;

                // If new path immediately hits the same collider we just wrapped on, reject
                if (h.collider == hitCollider) return false;
            }

            return true;
        }

        private void HandleCollision()
        {
            bool wrappedThisCall = false;

            for (int s = 0; s < subRopes.Count; s++)
            {
                if (wrappedThisCall) break;

                SubRope sr = subRopes[s];
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

                    if (cooldown > 0) continue;
                    if (wrappedThisCall) continue;
                    if (anchors.Count >= 15) continue;

                    RaycastHit2D lineHit = Physics2D.Linecast(
                        segs[i - 1].pos,
                        segs[i].pos,
                        collisionMask
                    );

                    if (lineHit.collider == null) continue;

                    ObjectWrapPoint anchorComp = lineHit.collider.GetComponent<ObjectWrapPoint>();
                    if (anchorComp == null) continue;

                    Vector2 wrapPoint = anchorComp.GetClosestPoint(lineHit.point);

                    // Single validation call replaces all the scattered checks
                    if (!IsValidWrapPoint(wrapPoint, lineHit.collider)) continue;

                    AttachRope(wrapPoint, segs[i - 1].pos, segs[i].pos);
                    wrappedThisCall = true;
                }
            }
        }

        private void UpdateCurrentLength()
        {
            float length = 0f;

            for (int s = 0; s < subRopes.Count; s++)
            {
                var segs = subRopes[s].segs;
                int startIdx = s == 0 ? 0 : 1;

                for (int i = startIdx; i < segs.Count - 1; i++)
                    length += Vector2.Distance(segs[i].pos, segs[i + 1].pos);
            }

            currentLength = length;
        }
        #endregion

        #region RopeWrapping
        private void AttachRope(Vector2 wrapPoint, Vector2 segBefore, Vector2 segAfter)
        {
            Vector2 prevPin = anchors.Count > 0
                ? anchors[^1].pos
                : (Vector2)startPoint.transform.position;

            Vector2 inDir = (wrapPoint - prevPin).normalized;
            Vector2 toEnd = ((Vector2)endPoint.position - wrapPoint).normalized;

            float cross = inDir.x * toEnd.y - inDir.y * toEnd.x;
            float side = Mathf.Sign(cross);

            if (Mathf.Abs(cross) < 0.01f)
            {
                Vector2 alt = (segAfter - wrapPoint).normalized;
                float c2 = inDir.x * alt.y - inDir.y * alt.x;
                side = Mathf.Sign(c2);
            }

            subRopes[^1].pinEnd = wrapPoint;

            anchors.Add(new Anchor
            {
                pos = wrapPoint,
                wrapSide = side,
                ropeIndex = subRopes.Count - 1
            });

            Vector2 end = endPoint.position;
            float totalDist = Vector2.Distance(wrapPoint, end);
            int numSegs = Mathf.Max(2, Mathf.CeilToInt(totalDist / segmentLength));

            SubRope newRope = new SubRope(wrapPoint, end);
            newRope.segs.Clear();

            for (int i = 0; i <= numSegs; i++)
            {
                float t = (float)i / numSegs;
                Vector2 pos = Vector2.Lerp(wrapPoint, end, t);
                newRope.segs.Add(new Segment(pos));
            }

            subRopes.Add(newRope);

            // Set cooldown large enough to survive multiple Unity fixed catchup steps
            cooldown = Mathf.Max(wrapCooldownFrames, 20);

            Debug.Log($"Wrapped at {wrapPoint} side={side}. anchors={anchors.Count}");
        }

        private void UnattachRope()
        {
            if (cooldown > 0 || anchors.Count == 0) return;

            Anchor last = anchors[anchors.Count - 1];
            Vector2 anchor = last.pos;

            Vector2 prevPin = anchors.Count > 1
                ? anchors[anchors.Count - 2].pos
                : (Vector2)startPoint.transform.position;

            Vector2 inDir = (anchor - prevPin).normalized;
            Vector2 outDir = ((Vector2)endPoint.position - anchor).normalized;
            Vector2 toEnd = (Vector2)endPoint.position - anchor;

            float cross = inDir.x * outDir.y - inDir.y * outDir.x;
            float currentSide = Mathf.Sign(cross);
            float dot = Vector2.Dot(inDir, outDir);
            float dist = Vector2.Distance(endPoint.position, anchor);

            // How far endPoint is along the incoming rope direction.
            // Negative = player moved back BEHIND the anchor = genuine unwrap territory.
            // Positive = player is still ahead/beside = keep wrapped (even if below).
            float alongInDir = Vector2.Dot(toEnd, inDir);

            bool sideCrossed = currentSide != last.wrapSide;
            bool farEnough = dist > segmentLength * 3f;
            bool notBehind = dot > -0.5f;
            bool clearCross = Mathf.Abs(cross) > 0.3f;
            bool movedPastCorner = alongInDir < -segmentLength * 2f;

            if (sideCrossed && farEnough && notBehind && clearCross && movedPastCorner)
            {
                anchors.RemoveAt(anchors.Count - 1);
                subRopes.RemoveAt(subRopes.Count - 1);
                subRopes[^1].pinEnd = endPoint.position;
                cooldown = wrapCooldownFrames;
            }
        }
        #endregion

        #region RopeLengthLimit
        private void CheckRopeLimit()
        {
            if (currentLength <= maxRopeLength)
                return;

            Debug.Log("Rope limit exceeded! Resetting rope and clearing checkpoints.");

            endPoint.position = startPoint.transform.position;

            anchors.Clear();
            subRopes.Clear();
            cooldown = 0;

            SubRope root = new SubRope(startPoint.transform.position, endPoint.position);
            subRopes.Add(root);

            currentLength = 0f;

            foreach (var cp in checkpoints)
                if (cp.obj != null)
                    Destroy(cp.obj);

            checkpoints.Clear();

            savedSnapshot = null;

            GameManager.Instance.ReturnToLand();
        }
        
        public void IncreaseMaxLength(float amount)
        {
            maxRopeLength += amount;
            
        } // IncreaseMaxLength ===

        public float GetMaxLength()
        {
            return maxRopeLength;
        } // GetMaxLength ===
        #endregion

        #region RopeCheckpoint
        private RopeSnapshot CreateSnapshot()
        {
            RopeSnapshot snap = new RopeSnapshot();

            snap.subRopes = new List<SubRope>();

            foreach (var sr in subRopes)
            {
                SubRope copy = new SubRope(sr.pinStart, sr.pinEnd);
                copy.segs.Clear();

                foreach (var s in sr.segs)
                {
                    copy.segs.Add(new Segment(s.pos) { old = s.old });
                }

                snap.subRopes.Add(copy);
            }

            snap.anchors = new List<Anchor>(anchors);

            return snap;
        }

        private void CreateCheckpoint()
        {
            if (checkpoints.Count >= maxCheckpoints)
            {
                Debug.Log($"Checkpoint limit reached ({maxCheckpoints}). Use T to teleport or K to delete one.");
                return;
            }

            Vector2 pos = endPoint.position;

            GameObject obj = Instantiate(checkpointPrefab, pos, Quaternion.identity);

            savedSnapshot = CreateSnapshot();

            checkpoints.Add(new CheckpointAnchor
            {
                pos = pos,
                obj = obj,
                snapshot = savedSnapshot,
                ropeIndex = subRopes.Count - 1
            });

            SplitRopeAtCheckpoint(pos);

            Debug.Log($"Checkpoint created: {checkpoints.Count}/{maxCheckpoints}");
        }

        private void SplitRopeAtCheckpoint(Vector2 cp)
        {
            if (subRopes.Count == 0)
                return;

            SubRope last = subRopes[^1];

            last.pinEnd = cp;

            if (Vector2.Distance(last.pinStart, cp) < 0.01f)
                return;

            SubRope newRope = new SubRope(cp, endPoint.position);
            subRopes.Add(newRope);

            anchors.RemoveAll(a => a.ropeIndex == subRopes.Count - 1);
            cooldown = wrapCooldownFrames;
        }

        private void TeleportToLastCheckpoint()
        {
            if (checkpoints.Count == 0)
                return;

            int lastIndex = checkpoints.Count - 1;
            var cp = checkpoints[lastIndex];

            Vector2 pos = cp.pos;

            Destroy(cp.obj);
            checkpoints.RemoveAt(lastIndex);

            endPoint.position = pos;

            if (subRopes.Count > 0)
            {
                SubRope r = subRopes[^1];
                r.pinEnd = pos;

                for (int i = 0; i < r.segs.Count; i++)
                {
                    Segment s = r.segs[i];
                    s.pos = pos;
                    s.old = pos;
                    r.segs[i] = s;
                }
            }

            int ropeIndex = cp.ropeIndex;

            for (int i = subRopes.Count - 1; i > ropeIndex; i--)
                subRopes.RemoveAt(i);

            if (ropeIndex >= 0 && ropeIndex < subRopes.Count)
                subRopes[ropeIndex].pinEnd = pos;

            anchors.RemoveAll(a => a.ropeIndex > ropeIndex);

            cooldown = 0;

            ReindexAnchors();
        }

        private void DeleteLastCheckpoint()
        {
            if (checkpoints.Count == 0)
                return;

            int lastIndex = checkpoints.Count - 1;
            var last = checkpoints[lastIndex];

            if (last.obj != null)
                Destroy(last.obj);

            checkpoints.RemoveAt(lastIndex);

            savedSnapshot = checkpoints.Count > 0
                ? checkpoints[^1].snapshot
                : null;

            if (savedSnapshot != null)
                RestoreSnapshot(savedSnapshot);

            Debug.Log($"Checkpoint deleted: {checkpoints.Count}/{maxCheckpoints}");
        }

        private void RestoreSnapshot(RopeSnapshot snap)
        {
            subRopes.Clear();
            anchors.Clear();

            foreach (var sr in snap.subRopes)
            {
                SubRope copy = new SubRope(sr.pinStart, sr.pinEnd);

                copy.segs.Clear();
                foreach (var s in sr.segs)
                {
                    copy.segs.Add(new Segment(s.pos)
                    {
                        old = s.old
                    });
                }

                subRopes.Add(copy);
            }

            anchors = new List<Anchor>(snap.anchors);

            cooldown = 0;
        }

        private void ReindexAnchors()
        {
            for (int i = 0; i < anchors.Count; i++)
            {
                for (int r = 0; r < subRopes.Count; r++)
                {
                    if (Vector2.Distance(anchors[i].pos, subRopes[r].pinStart) < 1000f)
                    {
                        anchors[i] = new Anchor
                        {
                            pos = anchors[i].pos,
                            wrapSide = anchors[i].wrapSide,
                            ropeIndex = r
                        };
                        break;
                    }
                }
            }
        }
        #endregion
    }
}