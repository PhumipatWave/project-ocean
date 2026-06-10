using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class RopeCheckPoint
    {
        private readonly int maxCheckpoints;
        private readonly int wrapCooldownFrames;
        private readonly List<CheckpointEntry> checkpoints = new();

        public int CheckpointCount => checkpoints.Count;
        public int MaxCheckpoints => maxCheckpoints;

        private class CheckpointEntry
        {
            public Vector2 pos;
            public GameObject obj;
            public int ropeIndex;
        }

        public RopeCheckPoint(int maxCheckpoints, int wrapCooldownFrames)
        {
            this.maxCheckpoints = maxCheckpoints;
            this.wrapCooldownFrames = wrapCooldownFrames;
        }

        public int CreateCheckpoint(
            Transform existingTransform,
            List<SubRope> subRopes,
            List<RopeAnchor> anchors,
            Transform endPoint,
            ref int cooldown)
        {
            if (checkpoints.Count >= maxCheckpoints)
            {
                Debug.Log($"[RopeCheckpoint] Limit reached ({maxCheckpoints}).");
                return cooldown;
            }

            Vector2 pos = existingTransform.position;

            checkpoints.Add(new CheckpointEntry
            {
                pos = pos,
                obj = existingTransform.gameObject,
                ropeIndex = subRopes.Count - 1
            });

            SplitRopeAtCheckpoint(pos, subRopes, anchors, ref cooldown);
            Debug.Log($"[RopeCheckpoint] Created: {checkpoints.Count}/{maxCheckpoints}");
            return cooldown;
        }

        public int TeleportToLast(List<SubRope> subRopes, List<RopeAnchor> anchors, Transform endPoint)
        {
            if (checkpoints.Count == 0) return 0;

            CheckpointEntry cp = checkpoints[^1];
            Object.Destroy(cp.obj);
            checkpoints.RemoveAt(checkpoints.Count - 1);

            endPoint.position = cp.pos;
            CollapseActiveRopeAt(subRopes, cp.pos);
            TrimRopeToCheckpoint(subRopes, anchors, cp.ropeIndex, cp.pos);
            ReindexAnchors(anchors, subRopes);

            Debug.Log($"[RopeCheckpoint] Teleported. Remaining: {checkpoints.Count}/{maxCheckpoints}");
            return 0;
        }

        public void ClearAll()
        {
            foreach (var cp in checkpoints)
            {
                if (cp.obj != null) Object.Destroy(cp.obj);
            }

            checkpoints.Clear();
        }

        private static void CollapseActiveRopeAt(List<SubRope> subRopes, Vector2 pos)
        {
            if (subRopes.Count == 0) return;

            SubRope active = subRopes[^1];
            active.pinEnd = pos;
            for (int i = 0; i < active.segs.Count; i++)
            {
                RopeSegment seg = active.segs[i];
                seg.PinTo(pos);
                active.segs[i] = seg;
            }
        }

        private static void TrimRopeToCheckpoint(
            List<SubRope> subRopes,
            List<RopeAnchor> anchors,
            int ropeIndex,
            Vector2 pos)
        {
            for (int i = subRopes.Count - 1; i > ropeIndex; i--)
                subRopes.RemoveAt(i);

            if (ropeIndex >= 0 && ropeIndex < subRopes.Count)
                subRopes[ropeIndex].pinEnd = pos;

            anchors.RemoveAll(a => a.ropeIndex > ropeIndex);
        }

        private void SplitRopeAtCheckpoint(
            Vector2 cp,
            List<SubRope> subRopes,
            List<RopeAnchor> anchors,
            ref int cooldown)
        {
            if (subRopes.Count == 0) return;

            SubRope last = subRopes[^1];
            Vector2 originalEnd = last.pinEnd;
            last.pinEnd = cp;

            if (Vector2.Distance(last.pinStart, cp) < RopeMath.PinMatchDistance) return;

            subRopes.Add(new SubRope(cp, originalEnd));
            anchors.RemoveAll(a => a.ropeIndex == subRopes.Count - 1);
            cooldown = wrapCooldownFrames;
        }

        private static void ReindexAnchors(List<RopeAnchor> anchors, List<SubRope> subRopes)
        {
            for (int i = 0; i < anchors.Count; i++)
            {
                RopeAnchor anchor = anchors[i];
                for (int r = 0; r < subRopes.Count; r++)
                {
                    if (Vector2.Distance(anchor.pos, subRopes[r].pinStart) > RopeMath.PinMatchDistance)
                        continue;

                    anchor.ropeIndex = r;
                    anchors[i] = anchor;
                    break;
                }
            }
        }
    }
}
