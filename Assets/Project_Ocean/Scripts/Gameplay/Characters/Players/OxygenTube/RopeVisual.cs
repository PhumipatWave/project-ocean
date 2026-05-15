using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>Draws the combined rope polyline through taut and active sections.</summary>
    public class RopeVisual
    {
        private const float VertexMergeDistance = 0.001f;

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
                ClearLine();
                return;
            }

            SubRope active = subRopes[^1];
            if (active.segs.Count < 2)
            {
                ClearLine();
                return;
            }

            drawBuffer.Clear();
            int tautCount = subRopes.Count - 1;

            for (int s = 0; s < tautCount; s++)
                AppendSection(subRopes[s], includeStart: s == 0);

            if (active.HasEdgePath)
                AppendSection(active, drawBuffer.Count == 0);
            else
                AppendActiveSegments(active, skipFirst: tautCount > 0);

            ApplyToLineRenderer();
        }

        private void AppendSection(SubRope sr, bool includeStart)
        {
            if (sr.HasEdgePath)
            {
                AppendEdgePath(sr.edgePath, includeStart);
                return;
            }

            if (includeStart || drawBuffer.Count == 0)
                drawBuffer.Add(sr.pinStart);

            drawBuffer.Add(sr.pinEnd);
        }

        private void AppendEdgePath(List<Vector2> path, bool includeStart)
        {
            int start = 0;
            if (!includeStart && drawBuffer.Count > 0
                && Vector2.Distance(drawBuffer[^1], path[0]) < VertexMergeDistance)
            {
                start = 1;
            }

            for (int i = start; i < path.Count; i++)
                drawBuffer.Add(path[i]);
        }

        private void AppendActiveSegments(SubRope active, bool skipFirst)
        {
            int start = skipFirst ? 1 : 0;
            for (int i = start; i < active.segs.Count; i++)
                drawBuffer.Add(active.segs[i].pos);
        }

        private void ApplyToLineRenderer()
        {
            line.positionCount = drawBuffer.Count;
            for (int i = 0; i < drawBuffer.Count; i++)
                line.SetPosition(i, drawBuffer[i]);
        }

        private void ClearLine() => line.positionCount = 0;
    }
}
