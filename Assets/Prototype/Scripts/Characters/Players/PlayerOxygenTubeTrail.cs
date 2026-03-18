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

        [SerializeField] private float pointSpacing = .3f;
        [SerializeField] private int smoothResolution = 5;
        [SerializeField] private int maxPoints = 50;

        private List<Vector2> points = new List<Vector2>();

        [SerializeField, Range(2f, 750f)] private float maxRopeLength = 50f;
        [SerializeField] private float currentRopeLength;

        private bool isPullBack;
        [SerializeField] private float pullBackForce = 50f;

        [SerializeField] private SpringJoint2D spring;

        private void Start()
        {
            line = GetComponent<LineRenderer>();
            points.Add(transform.position);
            spring.enabled = false;
        }

        private void FixedUpdate()
        {
            RopeSystemUtil.LimitSmoothRopeDistance(ref points, ref currentRopeLength, maxRopeLength);
            RopeSystemUtil.BreakSmoothRopeLimit(line, spring, anchor, player, ref points, ref isPullBack, maxRopeLength);
            RopeSystemUtil.DrawSmoothRopeLine(line, anchor.transform, player.transform, ref points);
        }
    }
}
