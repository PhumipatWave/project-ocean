using System;
using UnityEngine;
using UnityEngine.Windows;

namespace Game.Testing
{
    public class PlayerOxygenTube : MonoBehaviour
    {
        [SerializeField] private Transform anchor;

        [SerializeField] private float minRopeLength = 1f;
        [SerializeField] private float maxRopeLength = 50f;

        [SerializeField] private int segment = 20;
        [SerializeField] private float ropeSag = .5f;

        [SerializeField] private LineRenderer line;
        [SerializeField] private Player player;

        private Rigidbody2D rb;

        private void Start()
        {
            rb = player.RB;
            line = GetComponent<LineRenderer>();

            line.positionCount = segment;
        }

        private void Update()
        {
            DrawRope();
        }

        private void FixedUpdate()
        {
            LimitDistance();
        }

        private void LimitDistance()
        {
            Vector2 anchorPos = anchor.position;
            Vector2 playerPos = rb.position;

            float dist = Vector2.Distance(playerPos, anchorPos);

            if (dist > maxRopeLength)
            {
                Vector2 dir = (playerPos - anchorPos).normalized;
                Vector2 limitedPos = anchorPos + dir * maxRopeLength;
                rb.position = limitedPos;
            }
        }

        private void DrawRope()
        {
            Vector2 startPos = anchor.position;
            Vector2 endPos = player.transform.position;

            for (int i = 0; i < segment; i++)
            {
                float t = i / (float)(segment - 1);
                Vector2 pos = Vector2.Lerp(startPos, endPos, t);

                float sag = Mathf.Sin(t * Mathf.PI) * ropeSag;
                pos.y -= sag;

                line.SetPosition(i, pos);
            }
        }
    }
}