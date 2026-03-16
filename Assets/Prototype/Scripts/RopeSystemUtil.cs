using System.Collections;
using UnityEngine;

namespace Game.Testing
{
    public static class RopeSystemUtil
    {
        public static void DrawRope(LineRenderer line, Vector2 startPos, Vector2 endPos, int segment = 50, float ropeSag = .5f)
        {
            for (int i = 0; i < segment; i++)
            {
                float t = i / (float)(segment - 1);
                Vector2 pos = Vector2.Lerp(startPos, endPos, t);

                float sag = Mathf.Sin(t * Mathf.PI) * ropeSag;
                pos.y -= sag;

                line.SetPosition(i, pos);
            }
        }

        public static void LimitDistance(Transform firstPos, Transform secondPos, float maxLength)
        {
            Vector2 anchorPos = firstPos.position;
            Vector2 objsPos = secondPos.position;

            float dist = Vector2.Distance(objsPos, anchorPos);

            if (dist > maxLength)
            {
                Vector2 dir = (objsPos - anchorPos).normalized;
                Vector2 limitedPos = anchorPos + dir * maxLength;
                secondPos.position = limitedPos;
            }
        }

        public static void BreakRopeLimit(Vector2 startPos, Vector2 endPos, ref bool isPullBack, ref float currentLength, float maxLength, Rigidbody2D obj, float pullBackForce)
        {
            currentLength = Vector2.Distance(endPos, startPos);

            if (currentLength > maxLength)
            {
                isPullBack = true;
            }

            if (isPullBack)
            {
                Vector2 dir = (startPos - endPos).normalized;

                if (currentLength > 1f)
                {
                    obj.AddForce(dir * pullBackForce, ForceMode2D.Force);
                }
                else
                {
                    obj.linearVelocity = Vector2.zero;
                    isPullBack = false;
                }
            }
        }
    }
}
