using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class RopeVisual : MonoBehaviour
    {
        private LineRenderer line;

        public RopeVisual(LineRenderer line, float width = 0.1f)
        {
            this.line = line;
            line.startWidth = width;
            line.endWidth = width;
        }

        /// <summary>
        /// Updates the LineRenderer to match the current rope state.
        /// Call this from LateUpdate (after physics has settled).
        /// </summary>
        public void Draw(List<SubRope> subRopes)
        {
            if (subRopes == null || subRopes.Count == 0)
            {
                line.positionCount = 0;
                return;
            }
            int tautCount = subRopes.Count - 1;
            SubRope active = subRopes[^1];

            if (active.segs.Count < 2)
            {
                line.positionCount = 0;
                return;
            }

            int total = 1 + tautCount + (active.segs.Count - 1);
            line.positionCount = total;

            int idx = 0;

            for (int s = 0; s < tautCount; s++)
            {
                if (s == 0)
                    line.SetPosition(idx++, subRopes[s].pinStart);

                line.SetPosition(idx++, subRopes[s].pinEnd);
            }

            int segStart = tautCount > 0 ? 1 : 0;

            for (int i = segStart; i < active.segs.Count; i++)
                line.SetPosition(idx++, active.segs[i].pos);
        }
    }
}
