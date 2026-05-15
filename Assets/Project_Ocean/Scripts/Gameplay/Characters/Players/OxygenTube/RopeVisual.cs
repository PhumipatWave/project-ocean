using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class RopeVisual
    {
        private readonly LineRenderer line;
        private readonly List<Vector2> drawBuffer = new();

        public RopeVisual(LineRenderer line, float width = 0.1f)
        {
            this.line = line;
            line.startWidth = width;
            line.endWidth = width;
        }

        public void Draw(List<SubRope> subRopes)
        {
            if (subRopes == null || subRopes.Count == 0)
            {
                line.positionCount = 0;
                return;
            }

            SubRope active = subRopes[^1];
            if (active.segs.Count < 2)
            {
                line.positionCount = 0;
                return;
            }

            drawBuffer.Clear();
            int tautCount = subRopes.Count - 1;

            for (int s = 0; s < tautCount; s++)
                AppendPolylinePath(subRopes[s], s == 0);

            if (active.edgePath != null && active.edgePath.Count >= 2)
                AppendPolylinePath(active, drawBuffer.Count == 0);
            else
            {
                int segStart = tautCount > 0 ? 1 : 0;
                for (int i = segStart; i < active.segs.Count; i++)
                    drawBuffer.Add(active.segs[i].pos);
            }

            line.positionCount = drawBuffer.Count;
            for (int i = 0; i < drawBuffer.Count; i++)
                line.SetPosition(i, drawBuffer[i]);
        }

        private void AppendPolylinePath(SubRope sr, bool includeStart)
        {
            if (sr.edgePath != null && sr.edgePath.Count >= 2)
            {
                int start = 0;
                if (!includeStart && drawBuffer.Count > 0)
                {
                    if (Vector2.Distance(drawBuffer[^1], sr.edgePath[0]) < 0.001f)
                        start = 1;
                }

                for (int i = start; i < sr.edgePath.Count; i++)
                    drawBuffer.Add(sr.edgePath[i]);
                return;
            }

            if (includeStart || drawBuffer.Count == 0)
                drawBuffer.Add(sr.pinStart);

            drawBuffer.Add(sr.pinEnd);
        }
    }
}
