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
            int total = 0;
            for (int s = 0; s < subRopes.Count; s++)
                total += s == 0 ? subRopes[s].segs.Count : subRopes[s].segs.Count - 1;

            line.positionCount = total;

            int idx = 0;
            for (int s = 0; s < subRopes.Count; s++)
            {
                var segs = subRopes[s].segs;
                int startIdx = s == 0 ? 0 : 1;

                for (int i = startIdx; i < segs.Count; i++)
                    line.SetPosition(idx++, segs[i].pos);
            }
        }
    }
}
