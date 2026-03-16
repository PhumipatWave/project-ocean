using System;
using UnityEngine;
using UnityEngine.Windows;

namespace Game.Testing
{
    public class PlayerOxygenTube : MonoBehaviour
    {
        [Header("Oxygen Tube Compopnent")]
        [SerializeField] private LineRenderer line;
        [SerializeField] private Player player;
        private Rigidbody2D rb;

        [Header("Oxygen Tube Object")]
        /*[SerializeField] private GameObject ropePrefab;
        private Rigidbody2D previousRope;*/

        [SerializeField] private Transform anchor;


        [Header("Oxygen Tube Status")]
        private float minRopeLength = 1f;
        [SerializeField, Range(2f, 750f)] private float maxRopeLength = 50f;
        [SerializeField] private float currentRopeLength;

        [SerializeField] private int segment = 20;
        [SerializeField] private float ropeSag = .5f;
        [SerializeField] private float pullBackForce = 50f;
        private bool isPullBack;

        private int oxygen;
        private int maxOxygen = 100;

        private void Start()
        {
            rb = player.RB;
            line = GetComponent<LineRenderer>();

            line.positionCount = segment;

            /*previousRope = anchor.GetComponent<Rigidbody2D>();
            BuildRope(ropePrefab, anchor, previousRope, rb, segment);*/
        }

        private void Update()
        {
            RopeSystemUtil.DrawRope(line, anchor.position, player.transform.position, segment, ropeSag);
        }

        private void FixedUpdate()
        {
            RopeSystemUtil.LimitDistance(anchor, rb.transform, maxRopeLength);
            RopeSystemUtil.BreakRopeLimit(anchor.position, player.transform.position, ref isPullBack, ref currentRopeLength, maxRopeLength, rb, pullBackForce);
        }

        /*public static void BuildRope(GameObject ropePrefab, Transform startPos, Rigidbody2D previousRope, Rigidbody2D playerRB, int segment)
        {
            previousRope = startPos.GetComponent<Rigidbody2D>();

            for (int i = 0; i < segment; i++)
            {
                GameObject seg = Instantiate(ropePrefab);
                seg.transform.position = startPos.position + Vector3.down * i * .5f;

                Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
                HingeJoint2D joint = seg.GetComponent<HingeJoint2D>();
                joint.connectedBody = previousRope;

                previousRope = rb;
            }

            HingeJoint2D playerJoint = playerRB.GetComponent<HingeJoint2D>();
            playerJoint.connectedBody = previousRope;
        }*/
    }
}