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
        private RopeVisual ropeVisual;

        [SerializeField] private TextMeshProUGUI currentOxygenTubeLength;

        [SerializeField] private float segmentLength = 0.25f;
        [SerializeField] private int constraintIterations = 50;

        private float currentLength;
        public float CurrentLength => currentLength;

        private float reduchMaxRopeLenght;
        public float ReduceMaxRopeLenght { get { return reduchMaxRopeLenght; } set {  reduchMaxRopeLenght = value;} }

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

        [SerializeField] private Vector2 gravity = new(0f, -2f);
        [SerializeField] private float damping = 0.98f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.15f;
        [SerializeField] private float friction = 0.9f;

        [Header("Wrap System")]
        private RopeWrap wrap;

        [SerializeField] private float removeAngle = 120f;
        [SerializeField] private int wrapCooldownFrames = 8;
        [SerializeField] private float mergeDistance = 1.0f;

        [Header("Checkpoint System")]
        [SerializeField] private int maxCheckpoints = 3;

        private RopeCheckPoint checkpoint;
        [SerializeField] private GameObject checkPointPrefab;

        /*[Header("Mini Hub System")]
        private MiniHub miniHub;
        public MiniHub MiniHub { get => miniHub;}*/

        [SerializeField] private GameObject miniHubPrefab;
        #endregion

        #region UnityCallbacks

        private void Awake()
        {
            line = GetComponent<LineRenderer>();

            physics = new RopePhysic(segmentLength, constraintIterations, gravity, damping, collisionMask, collisionRadius, friction, removeAngle);
            wrap = new RopeWrap(segmentLength, collisionMask, wrapCooldownFrames);
            checkpoint = new RopeCheckPoint(maxCheckpoints, wrapCooldownFrames);
            ropeVisual = new RopeVisual(line);
            //miniHub = new MiniHub(miniHubPrefab);

            //Rebuild();
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

            if (cooldown > 0) cooldown--;

            // Keep rope pins in sync with the moving endpoints
            subRopes[0].pinStart = startPoint.transform.position;
            subRopes[^1].pinEnd = endPoint.position;

            // Simulate Physic
            physics.GrowLastRope(subRopes);
            physics.ShrinkLastRope(subRopes);
            physics.Simulate(subRopes);
            physics.Constrain(subRopes);
            physics.HandleSegmentCollision(subRopes);

            // Wrap & unwrap
            cooldown = wrap.TryAttach(subRopes, anchors, endPoint, startPoint, cooldown);
            cooldown = wrap.TryDetach(subRopes, anchors, endPoint, startPoint, cooldown);

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