using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class OxygenRope : MonoBehaviour
    {
        #region Inspector
        [Header("References")]
        [SerializeField] private LineRenderer line;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private GameObject startPointPrefab;
        [Tooltip("Optional. If the player moves via Rigidbody2D, assign it so the hard rope-limit " +
                 "clamp can zero outward velocity instead of just teleporting the position (avoids jitter). " +
                 "Auto-detected from playerTransform if left empty.")]
        [SerializeField] private Rigidbody2D playerRigidbody;

        [SerializeField] private float breakLimitTimer = 10f;
        [SerializeField] private bool enableBreachTimeout = false;

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

        [Header("Performance")]
        [Tooltip("Max segments near the player that run full Verlet simulation each frame. " +
                 "Segments beyond this window are frozen in place — they already settled under gravity. " +
                 "60 = ~18 m active window at default segmentLength. Raise if rope looks stiff near the player.")]
        [SerializeField] private int maxSimulatedSegs = 60;
        #endregion

        #region Public API
        public float CurrentLength  { get; private set; }
        public float MaxRopeLength  { get => maxRopeLength; set => maxRopeLength = value; }

        /// <summary>Dev cheat (Infinite Oxygen): skips the hard length clamp and the breach
        /// countdown without touching maxRopeLength, so saves keep the real length.</summary>
        public bool IgnoreRopeLimit { get; set; }
        public GameObject StartPointObj    { get; private set; }
        public GameObject StartPointPrefab => startPointPrefab;
#if ENABLE_CHECKPOINT_ITEM
        public bool HasCheckpoint => pins.Count > 0 && pins[^1].isCheckpoint;
#endif

        /// <summary>Current health-based rope length, synced from PlayerHealth. Display-only —
        /// does not clamp movement (see PlayerHealth.TakeDamage).</summary>
        public float HealthLimit { get; private set; }
        public void SetHealthLimit(float value) => HealthLimit = value;

        /// <summary>Position of the current rope anchor the player is swinging/swimming from
        /// (the last fixed pin: start, wrap corner, checkpoint, or hub tie-in).</summary>
        public Vector2 CurrentAnchor => pins.Count > 0 ? pins[^1].pos : (Vector2)transform.position;

        /// <summary>Length already used by the fixed pin-to-pin sections before CurrentAnchor
        /// (measured from hubLengthStartIndex, same rule MeasureLength uses). Add the
        /// anchor-to-player leg to this to get the player's total current rope length.</summary>
        public float UsedPinLength
        {
            get
            {
                float len = 0f;
                int pinStart = Mathf.Clamp(hubLengthStartIndex, 0, Mathf.Max(0, pins.Count - 1));
                for (int i = pinStart; i < pins.Count - 1; i++)
                    len += Vector2.Distance(pins[i].pos, pins[i + 1].pos);
                return len;
            }
        }

        /// <summary>
        /// Clamp a desired player position so total rope length (fixed pin sections + the
        /// anchor-to-player leg) never exceeds maxRopeLength. Call this from the swim/movement
        /// controller BEFORE applying the player's move, e.g.:
        /// <code>
        /// Vector2 desired = rb.position + moveInput * speed * Time.fixedDeltaTime;
        /// rb.MovePosition(oxygenRope.ClampToRopeLimit(desired));
        /// </code>
        /// This only clamps position — pair it with cancelling outward radial velocity in the
        /// movement controller (see notes) so the player doesn't jitter against the limit.
        /// </summary>
        public Vector2 ClampToRopeLimit(Vector2 desiredPos)
        {
            if (pins.Count == 0)
                return desiredPos; // rope not set up yet

            Vector2 anchor = CurrentAnchor;
            float remaining = maxRopeLength - UsedPinLength;

            if (remaining <= 0f)
                return anchor; // fixed sections alone already used up the limit

            Vector2 toDesired = desiredPos - anchor;
            float dist = toDesired.magnitude;

            return dist <= remaining ? desiredPos : anchor + toDesired / dist * remaining;
        }

        /// <summary>Raised when the rope first exceeds its max length. float = countdown duration in seconds.</summary>
        public static event System.Action<float> OnRopeBreachStart;
        /// <summary>Raised when the rope returns within limit before the countdown expires.</summary>
        public static event System.Action OnRopeBreachEnd;
        #endregion

        #region Private Data
        private struct WrapPin
        {
            public Vector2 pos;
            public float side;         // cross-product side when this anchor was created
            public bool isCheckpoint;  // true = manually placed, never auto-unwrapped
            public bool isHub;         // true = MiniHub tie-in point; length counted from here onward, never auto-unwrapped
        }

        // pins[0] = fixed rope start, pins[1+] = wrap corners
        private readonly List<WrapPin> pins = new();

        // Verlet segments from pins[^1] to player
        private readonly VerletRope rope = new();

        private readonly List<Vector2> drawBuf = new();
        private readonly List<Vector2> cornerBuf = new();
        private readonly List<Vector2> compositeBuf = new(); // reuse buffer for CompositeCollider2D

        private int wrapCooldown;
        private int unwrapCooldown;
        // Length is measured starting from pins[hubLengthStartIndex] onward, so the section
        // between the original start and the most recent hub tie-in doesn't count against
        // the player's rope limit. Defaults to 0 (measure from the true start, as normal).
        private int hubLengthStartIndex;
        private Coroutine limitRoutine;

        private VerletRope.Settings SimSettings => new()
        {
            segmentLength = segmentLength, damping = damping, segRadius = segRadius,
            solverIterations = solverIterations, maxSimulatedSegs = maxSimulatedSegs,
            gravity = gravity, collisionMask = collisionMask,
        };
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            line ??= GetComponent<LineRenderer>();

            if (playerRigidbody == null && playerTransform != null)
                playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (!InOcean()) 
                return;

            if (StartPointObj == null || playerTransform == null || rope.Count < 2) 
                return;

            if (wrapCooldown > 0) 
                wrapCooldown--;

            if (unwrapCooldown > 0) 
                unwrapCooldown--;

            EnforceRopeLimit(); // hard stop: player cannot physically be farther than maxRopeLength

            rope.Step(pins[^1].pos, playerTransform.position, Time.fixedDeltaTime, SimSettings);

            if (wrapCooldown == 0) 
                TryWrap();

            if (unwrapCooldown == 0) 
                TryUnwrap();

            CurrentLength = MeasureLength();
            CheckLimit();
        }

        private void LateUpdate()
        {
            if (!InOcean()) 
                return;

            // Safety net: catches any player movement applied by other scripts in
            // Update/LateUpdate after this frame's physics step already ran.
            EnforceRopeLimit();

            DrawRope();
        }
        #endregion

        #region Setup
        /// Call this to place/move the fixed start of the rope.
        public void SetStartPoint(Vector3 worldPos)
        {
            if (StartPointObj == null)
                StartPointObj = Instantiate(startPointPrefab, worldPos, Quaternion.identity);
            else
                StartPointObj.transform.position = worldPos;

            BuildRope();
        }

        /// Re-anchor the rope's fixed start (pins[0]) to a new position WITHOUT clearing
        /// existing wrap/checkpoint pins and without touching the player's position.
        /// Use this for structures (e.g. MiniHub) that should re-tie the rope's origin
        /// while preserving the rope's current shape and any active checkpoints.



        /// Clears all wrap pins and rebuilds segments from start to player.
        public void BuildRope()
        {
            pins.Clear();
            pins.Add(new WrapPin { pos = StartPointObj.transform.position, side = 0f });
            hubLengthStartIndex = 0;
            RebuildSegs();
        }

        /// Full reset — call when returning to land.
        public void ResetRope()
        {
            if (limitRoutine != null)
            {
                StopCoroutine(limitRoutine);
                limitRoutine = null;
                OnRopeBreachEnd?.Invoke();
            }

            pins.Clear();
            rope.Clear();
            wrapCooldown   = 0;
            unwrapCooldown = 0;
            hubLengthStartIndex = 0;
            CurrentLength  = 0f;

            if (line != null)
                line.positionCount = 0;
        }

        public void IncreaseMaxLength(float amount) => maxRopeLength += amount;

#if ENABLE_CHECKPOINT_ITEM
        /// Add a manual pin at worldPos (checkpoint placed by the player).
        /// Rope runs: start → [...wraps...] → checkpoint → player.
        public void AttachToCheckpoint(Vector2 worldPos)
        {
            pins.Add(new WrapPin { pos = worldPos, side = 0f, isCheckpoint = true });
            RebuildSegs();
            wrapCooldown   = wrapCooldownFrames;
            unwrapCooldown = wrapCooldownFrames;
        }
#endif

        /// Tie the rope in at worldPos without moving or clearing the original start point —
        /// the old start stays exactly where it is and the rope keeps its current shape
        /// (wraps + checkpoints preserved). From this point on, rope-length measurement
        /// (used for the oxygen/max-length limit) counts only from this tie-in to the
        /// player — the section behind it no longer counts against the limit.
        public void AttachToHub(Vector2 worldPos)
        {
            pins.Add(new WrapPin { pos = worldPos, side = 0f, isHub = true });
            hubLengthStartIndex = pins.Count - 1;
            RebuildSegs();
            wrapCooldown   = wrapCooldownFrames;
            unwrapCooldown = wrapCooldownFrames;
        }


#if ENABLE_CHECKPOINT_ITEM
        /// Remove the checkpoint pin at worldPos and all pins added after it.
        /// Supports multiple checkpoints — finds the matching one by position.
        public void DetachFromCheckpoint(Vector2 worldPos)
        {
            float toleranceSqr = 0.25f; // 0.5m tolerance
            for (int i = pins.Count - 1; i >= 1; i--)
            {
                if (!pins[i].isCheckpoint) continue;
                if ((pins[i].pos - worldPos).sqrMagnitude > toleranceSqr) continue;

                pins.RemoveRange(i, pins.Count - i);
                RebuildSegs();
                wrapCooldown   = wrapCooldownFrames;
                unwrapCooldown = wrapCooldownFrames;
                return;
            }
        }
#endif
        #endregion

        #region Simulation
        // Directly overwrites the player's position (and, if a Rigidbody2D is assigned,
        // its outward velocity) so the total rope length can never exceed maxRopeLength —
        // regardless of what any other script does. This is the sole hard limit; the
        // ClampToRopeLimit()/UsedPinLength helpers above exist for other scripts that
        // want to predict the limit in advance, but this method is what actually enforces it.
        private void EnforceRopeLimit()
        {
            if (IgnoreRopeLimit || pins.Count == 0 || playerTransform == null)
                return;

            Vector2 anchor    = CurrentAnchor;
            float   remaining = maxRopeLength - UsedPinLength;
            Vector2 playerPos = playerTransform.position;

            if (remaining <= 0f)
            {
                // The fixed pin sections alone already consume the whole limit —
                // player is pinned to the anchor with no leg length left at all.
                SnapPlayer(anchor, anchor);
                return;
            }

            Vector2 toPlayer = playerPos - anchor;
            float   dist     = toPlayer.magnitude;

            if (dist > remaining)
                SnapPlayer(anchor + toPlayer / dist * remaining, anchor);
        }

        // Moves the player to pos. If a Rigidbody2D is present, also strips the velocity
        // component pointing further outward from anchor, so the player doesn't keep
        // pushing into the limit and jitter/rubber-band against it every frame.
        private void SnapPlayer(Vector2 pos, Vector2 anchor)
        {
            if (playerRigidbody != null)
            {
                Vector2 radial = pos - anchor;
                if (radial.sqrMagnitude > 0.0001f)
                {
                    Vector2 dir     = radial.normalized;
                    float   outward = Vector2.Dot(playerRigidbody.linearVelocity, dir);
                    if (outward > 0f)
                        playerRigidbody.linearVelocity -= dir * outward;
                }
                else
                {
                    // Zero-length remaining leg — no valid outward direction, so stop the player outright.
                    playerRigidbody.linearVelocity = Vector2.zero;
                }

                playerRigidbody.position = pos; // syncs the Transform automatically
            }
            else
            {
                playerTransform.position = pos;
            }
        }
        #endregion

        #region Wrap & Unwrap
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

            if (dist < 0.001f) 
                return;

            dir /= dist;

            // Offset start so the linecast doesn't immediately detect the wall
            // that the current anchor is sitting on.
            float offset = segmentLength * 1.5f;

            if (offset >= dist)
                return; // anchor and player are too close to need a wrap

            RaycastHit2D hit = Physics2D.Linecast(from + dir * offset, to, collisionMask);
            if (hit.collider == null) 
                return;

            Vector2 segDir = to - from;
            Vector2 corner = RopeCornerFinder.FindBestCorner(hit.collider, hit.point, segDir, cornerBuf, compositeBuf);

            if (!IsValidAnchor(corner)) 
                return;

            Vector2 inDir = (corner - from).normalized;
            Vector2 toPlayer = (to - corner).normalized;
            float cross = inDir.x * toPlayer.y - inDir.y * toPlayer.x;
            float side = Mathf.Abs(cross) < 0.001f ? 1f : Mathf.Sign(cross);

            pins.Add(new WrapPin { pos = corner, side = side });
            RebuildSegs();
            wrapCooldown = wrapCooldownFrames;
            unwrapCooldown = wrapCooldownFrames; // block unwrap from firing this same wrap event
        }

        private void TryUnwrap()
        {
            if (pins.Count <= 1)
                return;
            // Never auto-unwrap a checkpoint or hub tie-in pin — only DetachFromCheckpoint()
            // removes a checkpoint, and hub tie-ins are never removed automatically.
            if (pins[^1].isCheckpoint || pins[^1].isHub)
                return;

            WrapPin last = pins[^1];
            Vector2 prevPos = pins[^2].pos;
            Vector2 playerPos = playerTransform.position;

            // Direction of the incoming rope segment (prevPin → corner)
            Vector2 inDir = (last.pos - prevPos).normalized;
            // Direction from the corner toward the player
            Vector2 toPlayer = (playerPos - last.pos).normalized;
            float cross = inDir.x * toPlayer.y - inDir.y * toPlayer.x;

            // Condition A — player swung laterally past the corner to the other side.
            bool sideCrossed = Mathf.Abs(cross) > 0.1f && Mathf.Sign(cross) != last.side;

            // Condition B — player retreated back along the rope past the corner toward prevPin.
            // Cross-product alone cannot detect this: when the player moves straight back,
            // toPlayer is nearly anti-parallel to inDir so cross stays near 0 (no side flip).
            // Project both the corner and the player onto inDir from prevPin to compare.
            float playerAlong = Vector2.Dot(playerPos - prevPos, inDir);
            float anchorAlong = Vector2.Dot(last.pos  - prevPos, inDir); // equals distance(prevPin → corner)
            bool  passedBack  = playerAlong < anchorAlong - segmentLength;

            if (!sideCrossed && !passedBack)
                return;

            pins.RemoveAt(pins.Count - 1);
            if (hubLengthStartIndex >= pins.Count)
                hubLengthStartIndex = Mathf.Max(0, pins.Count - 1);
            RebuildSegs();
            unwrapCooldown = wrapCooldownFrames;
            wrapCooldown = wrapCooldownFrames;
        }

        private bool IsValidAnchor(Vector2 pos)
        {
            if (Vector2.Distance(pos, pins[^1].pos) < segmentLength * 2f) 
                return false;

            if (Vector2.Distance(pos, playerTransform.position) < segmentLength * 2f) 
                return false;

            // Only guard against the pin immediately behind the last one — this stops
            // an instant flap right after unwrapping, but (unlike scanning the whole
            // pins list) it still lets the rope wrap the same corner again once the
            // player has looped further around the object.
            if (pins.Count >= 2 && Vector2.Distance(pins[^2].pos, pos) < segmentLength * 3f)
                return false;


            return true;
        }
        #endregion

        #region Draw
        private void DrawRope()
        {
            drawBuf.Clear();

            // Straight taut lines between pins (pins[0] = start, pins[1+] = wrap corners)
            foreach (var p in pins)
                drawBuf.Add(p.pos);

            // Verlet segments — skip index 0 because it sits on pins[^1] already
            rope.AppendDrawPoints(drawBuf);

            line.positionCount = drawBuf.Count;
            for (int i = 0; i < drawBuf.Count; i++)
                line.SetPosition(i, drawBuf[i]);
        }
        #endregion

        #region Utility
        private void RebuildSegs()
        {
            Vector2 from = pins[^1].pos;
            Vector2 to = playerTransform != null ? (Vector2)playerTransform.position : from;
            rope.Rebuild(from, to, segmentLength);
        }

        private float MeasureLength()
        {
            float len = 0f;

            // Taut pin-to-pin sections, starting from hubLengthStartIndex (normally 0 — the
            // true rope start; advances to the latest hub tie-in once one has been used).
            int pinStart = Mathf.Clamp(hubLengthStartIndex, 0, Mathf.Max(0, pins.Count - 1));
            for (int i = pinStart; i < pins.Count - 1; i++)
                len += Vector2.Distance(pins[i].pos, pins[i + 1].pos);

            return len + rope.MeasureSegments(maxSimulatedSegs);
        }

        private bool InOcean() =>
            GameManager.Instance != null && GameManager.Instance.GameState == GameState.Ocean;

        // EnforceRopeLimit() already hard-clamps the player to exactly maxRopeLength every
        // frame before this runs, so CurrentLength (summed from Verlet segments, not the same
        // math as that snap) essentially never lands strictly ABOVE maxRopeLength — "> " here
        // was nearly unreachable. Reaching the limit (>=, within this tolerance of it) is the
        // real "broke the limit" moment — 1m instead of a hair's-width epsilon so it reliably
        // triggers, not just on a razor-exact snap value. The limit checked is
        // min(maxRopeLength, HealthLimit), so damage-reduced range also starts the countdown.
        private const float RopeLimitEpsilon = 1f;

        private void CheckLimit()
        {
            // ปิดใช้งานระบบนับถอยหลัง/บังคับกลับขึ้นฝั่งไว้ชั่วคราว
            // ติ๊ก enableBreachTimeout ใน Inspector เพื่อเปิดกลับมาใช้ตามเดิม โดยไม่ต้องแก้โค้ดส่วนนี้เลย
            if (!enableBreachTimeout)
                return;

            if (IgnoreRopeLimit)
            {
                // Cheat switched on mid-countdown: cancel it so the HUD timer clears.
                if (limitRoutine != null)
                {
                    StopCoroutine(limitRoutine);
                    limitRoutine = null;
                    OnRopeBreachEnd?.Invoke();
                }
                return;
            }

            // Fish damage lowers HealthLimit (not maxRopeLength), so the breach countdown
            // must key off whichever is smaller. HealthLimit is 0 until PlayerHealth syncs it.
            float limit = HealthLimit > 0f ? Mathf.Min(maxRopeLength, HealthLimit) : maxRopeLength;

            if (CurrentLength < limit - RopeLimitEpsilon)
            {
                if (limitRoutine != null)
                {
                    StopCoroutine(limitRoutine);
                    limitRoutine = null;
                    OnRopeBreachEnd?.Invoke();
                }
            }
            else if (limitRoutine == null)
            {
                limitRoutine = StartCoroutine(LimitBreached());
                OnRopeBreachStart?.Invoke(breakLimitTimer);
                TutorialEvents.RaiseAction(TutorialActionId.BreakRopeLimit);
            }
        }

        private IEnumerator LimitBreached()
        {
            yield return new WaitForSeconds(breakLimitTimer);
            ResetRope();
            GameManager.Instance.ReturnToLand();
        }
        #endregion
    }
}
