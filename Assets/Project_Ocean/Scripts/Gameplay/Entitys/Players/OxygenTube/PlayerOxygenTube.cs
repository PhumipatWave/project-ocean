using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.Testing
{
        public class PlayerOxygenTube : MonoBehaviour
    {
        [Header("Reference Points")]
        [SerializeField] private LineRenderer line;
        [SerializeField] private GameObject startPointPrefab;
        [SerializeField] private GameObject startPoint;
        [SerializeField] private Transform endPoint;

        public GameObject StartPointPrefab => startPointPrefab;
        public GameObject StartPoint
        {
            get => startPoint;
            set => startPoint = value;
        }

        [Header("Rope Settings")]
        [SerializeField] private TextMeshProUGUI currentOxygenTubeLength;
        [SerializeField] private float segmentLength = 0.25f;
        [SerializeField] private int constraintIterations = 50;
        [SerializeField] private float maxRopeLength = 150f;

        [Header("Physics")]
        [SerializeField] private Vector2 gravity = new(0f, -2f);
        [SerializeField] private float damping = 0.98f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.15f;
        [SerializeField] private float friction = 0.9f;

        [Header("Wrap System")]
        [SerializeField] private float removeAngle = 120f;
        [SerializeField] private int wrapCooldownFrames = 8;
        [SerializeField] private RopeWrapSettings wrapSettings = new();

        [Header("Checkpoint System")]
        [SerializeField] private int maxCheckpoints = 3;
        [SerializeField] private GameObject checkPointPrefab;

        [Tooltip("Prefab for optional mini hub points along the tube (if your game mode uses them).")]
        [SerializeField] private GameObject miniHubPrefab;

        private readonly List<SubRope> subRopes = new();
        private readonly List<RopeAnchor> anchors = new();

        private RopeVisual ropeVisual;
        private RopePhysics ropePhysics;
        private RopeWrap ropeWrap;
        private RopeBoundaryRouter boundaryRouter;
        private RopeCheckPoint checkpoint;

        private float currentLength;
        private float reduchMaxRopeLenght;
        private int wrapCooldown;

        private Coroutine breakLimitCoroutine;

        public float CurrentLength => currentLength;
        public float ReduceMaxRopeLenght
        {
            get => reduchMaxRopeLenght;
            set => reduchMaxRopeLenght = value;
        }
        public float MaxRopeLength
        {
            get => maxRopeLength;
            set => maxRopeLength = value;
        }

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            InitializeRopeSystems();
            ValidateSetup();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (line == null) line = GetComponent<LineRenderer>();
            ValidateSetup();
        }
#endif

        private void InitializeRopeSystems()
        {
            ropePhysics = new RopePhysics(
                segmentLength, constraintIterations, gravity, damping,
                collisionMask, collisionRadius, friction, removeAngle);

            ropeWrap = new RopeWrap(
                segmentLength, collisionRadius, collisionMask, wrapCooldownFrames, wrapSettings);

            boundaryRouter = new RopeBoundaryRouter(collisionMask, segmentLength);
            checkpoint = new RopeCheckPoint(maxCheckpoints, wrapCooldownFrames);
            ropeVisual = new RopeVisual(line);
        }

        private void ValidateSetup()
        {
            if (endPoint == null)
                Debug.LogWarning("[PlayerOxygenTube] End Point is not assigned.", this);

            if (startPoint == null && startPointPrefab == null)
                Debug.LogWarning("[PlayerOxygenTube] Assign Start Point or Start Point Prefab.", this);

            if (collisionMask.value == 0)
                Debug.LogWarning("[PlayerOxygenTube] Collision Mask is empty.", this);
        }

        private void Update()
        {
            if (!IsOceanMode())
            {
                if (currentOxygenTubeLength != null)
                    currentOxygenTubeLength.enabled = false;
                return;
            }

            UpdateLengthUI();
        }

        private void FixedUpdate() => SimulateRopeSystem();

        private void LateUpdate()
        {
            if (!IsOceanMode()) return;
            ropeVisual.Draw(subRopes);
        }

        private bool IsOceanMode() =>
            GameManager.Instance != null && GameManager.Instance.GameState == GameState.Ocean;

        private void UpdateLengthUI()
        {
            if (currentOxygenTubeLength == null) return;

            currentOxygenTubeLength.enabled = true;
            currentOxygenTubeLength.color = currentLength >= maxRopeLength ? Color.red : Color.white;
            currentOxygenTubeLength.text = $"Length : {(int)currentLength} m.";
        }

        private void SimulateRopeSystem()
        {
            if (!IsOceanMode()) return;
            if (startPoint == null || endPoint == null || subRopes.Count == 0) return;

            if (wrapCooldown > 0) wrapCooldown--;

            SyncEndpoints();
            boundaryRouter.UpdateAllPaths(subRopes, anchors, StartPosition, EndPosition);

            ropePhysics.GrowLastRope(subRopes);
            ropePhysics.ShrinkLastRope(subRopes);
            ropePhysics.Simulate(subRopes);
            ropePhysics.Constrain(subRopes);
            ropePhysics.HandleSegmentCollision(subRopes);

            ropeWrap.TickFrame();
            wrapCooldown = ropeWrap.TryDetach(subRopes, anchors, endPoint, startPoint, wrapCooldown);
            wrapCooldown = ropeWrap.TryAttach(subRopes, anchors, endPoint, startPoint, wrapCooldown);

            currentLength = ropePhysics.CalculateTotalLength(subRopes);
            CheckRopeLimit();
        }

        private void SyncEndpoints()
        {
            subRopes[0].pinStart = StartPosition;
            subRopes[^1].pinEnd = EndPosition;
        }

        private Vector2 StartPosition => startPoint.transform.position;
        private Vector2 EndPosition => endPoint.position;

        public void Rebuild()
        {
            anchors.Clear();
            subRopes.Clear();
            subRopes.Add(new SubRope(StartPosition, EndPosition));
        }

        public void SetNewStartPoint(Vector3 newPos)
        {
            if (startPoint != null)
                startPoint.transform.position = newPos;
            else
                startPoint = Instantiate(startPointPrefab, newPos, Quaternion.identity);

            Rebuild();
        }

        private IEnumerator BreakLimitRoutine()
        {
            yield return new WaitForSeconds(5f);

            ResetRopeData();
            GameManager.Instance.ReturnToLand();
        }

        private void CheckRopeLimit()
        {
            if (currentLength <= maxRopeLength)
            {
                if (breakLimitCoroutine != null)
                {
                    StopCoroutine(breakLimitCoroutine);
                    breakLimitCoroutine = null;

                    Debug.Log($"Stopping break limit routine. {breakLimitCoroutine}");
                }
            }
            else
            {
                if (breakLimitCoroutine == null)
                {
                    breakLimitCoroutine = StartCoroutine(BreakLimitRoutine());
                    Debug.Log($"Starting break limit routine. {breakLimitCoroutine}");
                }
            }
        }

        public void ResetRopeData()
        {
            Debug.Log("[PlayerOxygenTube] Rope limit exceeded — resetting.");

            endPoint.position = startPoint == null ? endPoint.position = Vector2.zero : startPoint.transform.position;
            anchors.Clear();
            subRopes.Clear();
            wrapCooldown = 0;
            currentLength = 0f;

            checkpoint.ClearAll();
            Rebuild();
            ropeVisual.Draw(subRopes);
        }

        public void IncreaseMaxLength(float amount) => maxRopeLength += amount;
        public float GetMaxLength() => maxRopeLength;

        public bool IsCheckpointAtMax() => checkpoint.CheckpointCount >= checkpoint.MaxCheckpoints;

        public void SpawnCheckPoint(Transform existingCheckpoint) =>
            checkpoint.CreateCheckpoint(existingCheckpoint, subRopes, anchors, endPoint, ref wrapCooldown);

        public void TeleportToCheckPoint() =>
            wrapCooldown = checkpoint.TeleportToLast(subRopes, anchors, endPoint);
    }
}
