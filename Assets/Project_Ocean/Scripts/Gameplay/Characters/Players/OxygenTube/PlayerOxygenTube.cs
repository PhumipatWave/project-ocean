using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private float segmentLength = 0.25f;
        [SerializeField] private int constraintIterations = 50;
        [SerializeField] private float maxRopeLength = 150f;

        public float MaxRopeLength
        {
            get => maxRopeLength;
            set => maxRopeLength = value;
        }

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
        [SerializeField] private float mergeDistance = 1.0f;

        [Header("Checkpoint System")]
        [SerializeField] private GameObject checkpointPrefab;
        [SerializeField] private int maxCheckpoints = 3;

        public float CurrentLength => currentLength;
        public int CurrentCheckpointCount => checkpoint.CheckpointCount;
        public int MaxCheckpointCount => checkpoint.MaxCheckpoints;

        private List<SubRope> subRopes = new();
        private List<RopeAnchor> anchors = new();
        private float currentLength;
        private int cooldown;

        private RopePhysic physics;
        private RopeWrap wrap;
        private RopeCheckPoint checkpoint;
        private RopeVisual ropeVisual;
        #endregion

        #region UnityCallbacks
        private void Awake()
        {
            line = GetComponent<LineRenderer>();

            physics = new RopePhysic(segmentLength, constraintIterations, gravity, damping, collisionMask, collisionRadius, friction, removeAngle);
            wrap = new RopeWrap(segmentLength, collisionMask, collisionRadius, wrapCooldownFrames);
            checkpoint = new RopeCheckPoint(checkpointPrefab, maxCheckpoints, wrapCooldownFrames);
            ropeVisual = new RopeVisual(line);

            //Rebuild();
        }

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Ocean) return;

            // ── Debug / test inputs (remove in production) ────────────
            if (Input.GetKeyDown(KeyCode.H)) checkpoint.CreateCheckpoint(subRopes, anchors, endPoint, ref cooldown);
            if (Input.GetKeyDown(KeyCode.T)) cooldown = checkpoint.TeleportToLast(subRopes, anchors, endPoint);
        }

        private void FixedUpdate()
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

        private void LateUpdate()
        {
            if (GameManager.Instance.GameState != GameState.Ocean) return;

            ropeVisual.Draw(subRopes);
        }
        #endregion

        /// <summary>
        /// Resets the rope data.
        /// </summary>
        public void Rebuild()
        {
            anchors.Clear();
            subRopes.Clear();
            subRopes.Add(new SubRope(startPoint.transform.position, endPoint.position));
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
            subRopes.Add(new SubRope(startPoint.transform.position, endPoint.position));

            // Destroy all checkpoint markers
            checkpoint.ClearAll();

            GameManager.Instance.ReturnToLand();
        }

        public void IncreaseMaxLength(float amount) => maxRopeLength += amount;
        public float GetMaxLength() => maxRopeLength;
    }
}