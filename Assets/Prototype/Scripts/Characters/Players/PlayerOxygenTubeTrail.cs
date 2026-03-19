using System;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Testing
{
    public class PlayerOxygenTubeTrail : MonoBehaviour
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private GameObject anchor;
        [SerializeField] private GameObject player;
        [SerializeField] private SpringJoint2D spring;

        [SerializeField] private float pointSpacing = .3f;
        [SerializeField] private int smoothResolution = 5;
        [SerializeField] private int maxPoints = 50;

        private List<Vector2> points = new List<Vector2>();

        [SerializeField, Range(2f, 750f)] private float maxRopeLength = 50f;

        [SerializeField] private float currentRopeLength;
        public float CurrentRopeLength { get { return currentRopeLength; } }

        [SerializeField] private float pullBackForce = 50f;

        private bool isPullBack;
        public bool IsPullBack { get { return isPullBack; } }

        private void Start()
        {
            line = GetComponent<LineRenderer>();
            points.Add(transform.position);
            spring.enabled = false;
        }

        private void Update()
        {
            if (isPullBack)
            {
                player.GetComponent<Player>().input.DisablePlayerInput();
            }
            else
            {
                player.GetComponent<Player>().input.EnablePlayerInput();
            }
        }

        private void FixedUpdate()
        {
            RopeSystemUtil.LimitSmoothRopeDistance(ref points, ref currentRopeLength, maxRopeLength);
            RopeSystemUtil.BreakSmoothRopeLimit(line, spring, anchor, player, ref points, ref isPullBack, maxRopeLength);
            RopeSystemUtil.DrawSmoothRopeLine(line, anchor.transform, player.transform, ref points);
        }
    }
}
