using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public static class RopeSystemUtil
    {
        #region NormalRopeLine
        public static void DrawNormalRope(LineRenderer line, GameObject startObj, GameObject endObj, int segment = 20, float ropeSag = .5f)
        {
            line.enabled = true;
            line.positionCount = segment;

            for (int i = 0; i < segment; i++)
            {
                float t = i / (float)(segment - 1);
                Vector2 startPos = startObj.transform.position;
                Vector2 endPos = endObj.transform.position;
                Vector2 pos = Vector2.Lerp(startPos, endPos, t);

                float sag = Mathf.Sin(t * Mathf.PI) * ropeSag;
                pos.y -= sag;

                line.SetPosition(i, pos);
            }
        }

        public static void BreakNormalRopeLimit(GameObject startObj, GameObject endObj, ref bool isPullBack, ref float currentLength, float maxLength, float pullBackForce)
        {
            Vector2 startPos = startObj.transform.position;
            Vector2 endPos = endObj.transform.position;
            currentLength = Vector2.Distance(endPos, startPos);

            if (currentLength >= maxLength)
            {
                isPullBack = true;
            }

            if (isPullBack)
            {
                Vector2 dir = (startPos - endPos).normalized;

                if (currentLength > 1f)
                {
                    endObj.GetComponent<Rigidbody2D>().AddForce(dir * pullBackForce, ForceMode2D.Force);
                }
                else
                {
                    endObj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                    isPullBack = false;
                }
            }
        }

        public static void ClearNormalRope(LineRenderer line)
        {
            line.positionCount = 2;
            line.enabled = false;

            GameLog.Info("Clear Rope");
        }
        #endregion

    }
}