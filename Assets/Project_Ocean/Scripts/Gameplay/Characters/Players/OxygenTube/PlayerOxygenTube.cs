using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class PlayerOxygenTube : MonoBehaviour
    {
        #region Fields
        [Header("Reference Points")]
        [Tooltip("LineRenderer for the blue rope. Auto-filled from this GameObject if left empty.")]
        [SerializeField] private LineRenderer line;
        [Tooltip("Spawned when Start Point is not set in the scene (see SetNewStartPoint).")]
        [SerializeField] private GameObject startPointPrefab;
        [Tooltip("Fixed end of the tube (oxygen source). Can be assigned at runtime.")]
        [SerializeField] private GameObject startPoint;
        [Tooltip("Moving end of the tube — usually the Player transform.")]
        [SerializeField] private Transform endPoint;

        public GameObject StartPointPrefab => startPointPrefab;
        public GameObject StartPoint
        {
            get => startPoint;
            set => startPoint = value;
        }

        [Header("Rope Settings")]
        private RopeVisual ropeVisual;

        [Tooltip("UI text that shows current rope length in meters.")]
        [SerializeField] private TextMeshProUGUI currentOxygenTubeLength;

        [Tooltip("Distance between simulated rope points. Smaller = smoother rope, more CPU.")]
        [SerializeField] private float segmentLength = 0.25f;
        [Tooltip("Constraint solver passes per physics frame. Higher = stiffer rope.")]
        [SerializeField] private int constraintIterations = 50;

        private float currentLength;
        public float CurrentLength => currentLength;

        private float reduchMaxRopeLenght;
        public float ReduceMaxRopeLenght { get { return reduchMaxRopeLenght; } set {  reduchMaxRopeLenght = value;} }

        [Tooltip("Maximum allowed rope length before the player is pulled back to start.")]
        [SerializeField] private float maxRopeLength = 150f;
        public float MaxRopeLength
        {
            get => maxRopeLength;
            set => maxRopeLength = value;
        }

        private List<SubRope> subRopes = new();
        private List<RopeAnchor> anchors = new();
        private int cooldown;

        [Header("Physics")]
        private RopePhysic physics;

        [Tooltip("Gravity applied to the active (unpinned) part of the rope each FixedUpdate.")]
        [SerializeField] private Vector2 gravity = new(0f, -2f);
        [Tooltip("Velocity damping per frame (0–1). Lower = more drag.")]
        [SerializeField] private float damping = 0.98f;

        [Header("Collision")]
        [Tooltip("Layers the rope wraps against and collides with. Obstacles must use these layers.")]
        [SerializeField] private LayerMask collisionMask;
        [Tooltip("Radius used for rope–wall collision and wrap linecasts.")]
        [SerializeField] private float collisionRadius = 0.15f;
        [Tooltip("Friction on rope segments after hitting colliders (0–1).")]
        [SerializeField] private float friction = 0.9f;

        [Header("Wrap System")]
        private RopeWrap wrap;
        private RopeBoundaryRouter boundaryRouter;

        [Tooltip("If the active rope folds back sharper than this angle (degrees), the last segment is removed.")]
        [SerializeField] private float removeAngle = 120f;
        [Tooltip("Frames after unwrap where new wrap points are ignored (prevents instant re-wrap on the same corner).")]
        [SerializeField] private int wrapCooldownFrames = 8;
        [Tooltip("Fine-tune when corners unwrap. Expand for tooltips on each field.")]
        [SerializeField] private RopeWrapSettings wrapSettings = new();

        [Header("Checkpoint System")]
        [Tooltip("Maximum number of rope checkpoints stored at once.")]
        [SerializeField] private int maxCheckpoints = 3;

        private RopeCheckPoint checkpoint;
        [Tooltip("Prefab spawned when the player creates a rope checkpoint.")]
        [SerializeField] private GameObject checkPointPrefab;

        /*[Header("Mini Hub System")]
        private MiniHub miniHub;
        public MiniHub MiniHub { get => miniHub;}*/

        [Tooltip("Prefab for optional mini hub points along the tube (if your game mode uses them).")]
        [SerializeField] private GameObject miniHubPrefab;
        #endregion

        #region UnityCallbacks

        private void Awake()
        {
            line = GetComponent<LineRenderer>();

            physics = new RopePhysic(segmentLength, constraintIterations, gravity, damping, collisionMask, collisionRadius, friction, removeAngle);
            wrap = new RopeWrap(segmentLength, collisionRadius, collisionMask, wrapCooldownFrames, wrapSettings);
            boundaryRouter = new RopeBoundaryRouter(collisionMask, segmentLength);
            checkpoint = new RopeCheckPoint(maxCheckpoints, wrapCooldownFrames);
            ropeVisual = new RopeVisual(line);
            //miniHub = new MiniHub(miniHubPrefab);

            ValidateSetup();
        }

#if UNITY_EDITOR
        private void OnValidate() => ValidateSetup();
#endif

        private void ValidateSetup()
        {
            if (line == null)
                line = GetComponent<LineRenderer>();

            if (endPoint == null)
                Debug.LogWarning("[PlayerOxygenTube] End Point is not assigned.", this);

            if (startPoint == null && startPointPrefab == null)
                Debug.LogWarning("[PlayerOxygenTube] Assign Start Point or Start Point Prefab.", this);

            if (collisionMask.value == 0)
                Debug.LogWarning("[PlayerOxygenTube] Collision Mask is empty — wrap/unwrap will not detect walls.", this);
        }

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Ocean) 
            {
                currentOxygenTubeLength.enabled = false;
                return;
            }

            // ── Debug / test inputs (remove in production) ────────────
            /*if (Input.GetKeyDown(KeyCode.H)) 
                SpawnCheckPoint(checkPointPrefab);*/

            currentOxygenTubeLength.enabled = true;

            currentOxygenTubeLength.color = Color.white;
            currentOxygenTubeLength.text = $"Length : {(int)currentLength} m.";

            if (currentLength >= maxRopeLength - 20)
            {
                currentOxygenTubeLength.color = Color.red;
            }
        }

        private void FixedUpdate()
        {
            SimulateRopeSystem();
        }

        private void LateUpdate()
        {
            if (GameManager.Instance.GameState != GameState.Ocean) return;

            ropeVisual.Draw(subRopes);
        }
        #endregion

        private void SimulateRopeSystem()
        {
            if (GameManager.Instance.GameState != GameState.Ocean) return;

            if (startPoint == null || endPoint == null || subRopes.Count == 0)
                return;

            if (cooldown > 0) cooldown--;

            // Keep rope pins in sync with the moving endpoints
            subRopes[0].pinStart = startPoint.transform.position;
            subRopes[^1].pinEnd = endPoint.position;

            boundaryRouter.UpdateAllPaths(
                subRopes,
                anchors,
                startPoint.transform.position,
                endPoint.position);

            // Simulate Physic
            physics.GrowLastRope(subRopes);
            physics.ShrinkLastRope(subRopes);
            physics.Simulate(subRopes);
            physics.Constrain(subRopes);
            physics.HandleSegmentCollision(subRopes);

            wrap.TickFrame();

            // Detach before attach so unwrap isn't overwritten by a new wrap the same frame
            cooldown = wrap.TryDetach(subRopes, anchors, endPoint, startPoint, cooldown);
            cooldown = wrap.TryAttach(subRopes, anchors, endPoint, startPoint, cooldown);

            // Length & limit
            currentLength = physics.CalculateTotalLength(subRopes);
            CheckRopeLimit();
        }

        /// <summary>
        /// Resets the rope data.
        /// </summary>
        public void Rebuild()
        {
            anchors.Clear();
            subRopes.Clear();
            subRopes.Add(new SubRope(startPoint.transform.position, endPoint.position));
        }

        public void SetNewStartPoint(Vector3 newPos)
        {
            if (startPoint != null)
            {
                startPoint.transform.position = newPos;
            }
            else
            {
                startPoint = Instantiate(startPointPrefab, newPos, Quaternion.identity);
            }

            Rebuild();
        }

        private void CheckRopeLimit()
        {
            if (currentLength <= maxRopeLength) return;

            Debug.Log("[PlayerOxygenTube] Rope limit exceeded — resetting.");

            // Snap the player back to the start
            endPoint.position = startPoint.transform.position;

            // Reset rope state
            anchors.Clear();
            subRopes.Clear();
            cooldown = 0;
            currentLength = 0f;

            checkpoint.ClearAll();
            Rebuild();

            ropeVisual.Draw(subRopes);

            GameManager.Instance.ReturnToLand();
        }

        public void IncreaseMaxLength(float amount) => maxRopeLength += amount;
        public float GetMaxLength() => maxRopeLength;

        public void SpawnCheckPoint(GameObject checkPointPrefab)
        {
            checkpoint.CreateCheckpoint(checkPointPrefab, subRopes, anchors, endPoint, ref cooldown);
        }

        public void TeleportToCheckPoint()
        {
            cooldown = checkpoint.TeleportToLast(subRopes, anchors, endPoint);
        }
    }
}