using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class RopeCheckPoint
    {
        private int maxCheckpoints;
        private int wrapCooldownFrames;

        public int CheckpointCount => checkpoints.Count;
        public int MaxCheckpoints => maxCheckpoints;

        private class CheckpointEntry
        {
            public Vector2 pos;
            public GameObject obj;
            public RopeSnapshot snapshot;
            // Which sub-rope was active when checkpoint was placed
            public int ropeIndex;
        }

        private readonly List<CheckpointEntry> checkpoints = new();
        private RopeSnapshot savedSnapshot;

        public RopeCheckPoint(int maxCheckpoints, int wrapCooldownFrames)
        {
            this.maxCheckpoints = maxCheckpoints;
            this.wrapCooldownFrames = wrapCooldownFrames;
        }

        /// <summary>
        /// Saves the current rope state as a checkpoint at the endpoint position.
        /// Splits the last sub-rope so the rope continues from the checkpoint.
        /// Returns the new cooldown value.
        /// </summary>
        public int CreateCheckpoint(GameObject checkPointPrefab, List<SubRope> subRopes, List<RopeAnchor> anchors, Transform endPoint, ref int cooldown)
        {
            if (checkpoints.Count >= maxCheckpoints)
            {
                Debug.Log($"[RopeCheckpoint] Limit reached ({maxCheckpoints}). Teleport (T) or delete (K) one first.");
                return cooldown;
            }

            Vector2 pos = endPoint.position;
            GameObject obj = Object.Instantiate(checkPointPrefab, pos, Quaternion.identity);

            savedSnapshot = TakeSnapshot(subRopes, anchors);

            checkpoints.Add(new CheckpointEntry
            {
                pos = pos,
                obj = obj,
                snapshot = savedSnapshot,
                ropeIndex = subRopes.Count - 1
            });

            SplitRopeAtCheckpoint(pos, subRopes, anchors, ref cooldown);

            Debug.Log($"[RopeCheckpoint] Created: {checkpoints.Count}/{maxCheckpoints}");
            return cooldown;
        }

        /// <summary>
        /// Teleports the endpoint to the last checkpoint and restores the rope to that state.
        /// Returns the new cooldown value (0 after teleport).
        /// </summary>
        public int TeleportToLast(List<SubRope> subRopes, List<RopeAnchor> anchors, Transform endPoint)
        {
            if (checkpoints.Count == 0) return 0;

            int lastIndex = checkpoints.Count - 1;
            CheckpointEntry cp = checkpoints[lastIndex];

            Object.Destroy(cp.obj);
            checkpoints.RemoveAt(lastIndex);

            // Snap the endpoint to the checkpoint position
            endPoint.position = cp.pos;

            // Collapse the last sub-rope's segments to the checkpoint position
            if (subRopes.Count > 0)
            {
                SubRope r = subRopes[^1];
                r.pinEnd = cp.pos;

                for (int i = 0; i < r.segs.Count; i++)
                {
                    RopeSegment s = r.segs[i];
                    s.pos = cp.pos;
                    s.old = cp.pos;
                    r.segs[i] = s;
                }
            }

            // Trim sub-ropes and anchors back to where the checkpoint was placed
            int ropeIndex = cp.ropeIndex;

            for (int i = subRopes.Count - 1; i > ropeIndex; i--)
                subRopes.RemoveAt(i);

            if (ropeIndex >= 0 && ropeIndex < subRopes.Count)
                subRopes[ropeIndex].pinEnd = cp.pos;

            anchors.RemoveAll(a => a.ropeIndex > ropeIndex);

            ReindexAnchors(anchors, subRopes);

            Debug.Log($"[RopeCheckpoint] Teleported. Remaining: {checkpoints.Count}/{maxCheckpoints}");
            return 0; // reset cooldown
        }

        /// <summary>
        /// Deletes the last checkpoint without teleporting, and restores the rope to the
        /// previous checkpoint's snapshot (if one exists).
        /// </summary>
        public void DeleteLast(List<SubRope> subRopes, List<RopeAnchor> anchors)
        {
            if (checkpoints.Count == 0) return;

            int lastIndex = checkpoints.Count - 1;
            CheckpointEntry last = checkpoints[lastIndex];

            if (last.obj != null) Object.Destroy(last.obj);
            checkpoints.RemoveAt(lastIndex);

            savedSnapshot = checkpoints.Count > 0 ? checkpoints[^1].snapshot : null;

            if (savedSnapshot != null)
                RestoreSnapshot(savedSnapshot, subRopes, anchors);

            Debug.Log($"[RopeCheckpoint] Deleted. Remaining: {checkpoints.Count}/{maxCheckpoints}");
        }

        /// <summary>
        /// Destroys all checkpoint objects and clears the list (called on rope limit reset).
        /// </summary>
        public void ClearAll()
        {
            foreach (var cp in checkpoints)
                if (cp.obj != null) Object.Destroy(cp.obj);

            checkpoints.Clear();
            savedSnapshot = null;
        }

        /// <summary>
        /// Deep-copies the current rope state into a new RopeSnapshot.
        /// </summary>
        public RopeSnapshot TakeSnapshot(List<SubRope> subRopes, List<RopeAnchor> anchors)
        {
            RopeSnapshot snap = new RopeSnapshot
            {
                subRopes = new List<SubRope>(),
                anchors = new List<RopeAnchor>(anchors)
            };

            foreach (var sr in subRopes)
            {
                SubRope copy = new SubRope(sr.pinStart, sr.pinEnd);
                copy.segs.Clear();

                foreach (var s in sr.segs)
                    copy.segs.Add(new RopeSegment(s.pos) { old = s.old });

                snap.subRopes.Add(copy);
            }

            return snap;
        }

        /// <summary>
        /// Replaces the current rope state with a previously saved snapshot.
        /// </summary>
        public void RestoreSnapshot(RopeSnapshot snap, List<SubRope> subRopes, List<RopeAnchor> anchors)
        {
            subRopes.Clear();
            anchors.Clear();

            foreach (var sr in snap.subRopes)
            {
                SubRope copy = new SubRope(sr.pinStart, sr.pinEnd);
                copy.segs.Clear();

                foreach (var s in sr.segs)
                    copy.segs.Add(new RopeSegment(s.pos) { old = s.old });

                subRopes.Add(copy);
            }

            anchors.AddRange(snap.anchors);
        }

        /// <summary>
        /// Splits the last sub-rope at the checkpoint position so the rope
        /// continues past it rather than terminating there.
        /// </summary>
        private void SplitRopeAtCheckpoint(
            Vector2 cp,
            List<SubRope> subRopes,
            List<RopeAnchor> anchors,
            ref int cooldown)
        {
            if (subRopes.Count == 0) return;

            SubRope last = subRopes[^1];
            last.pinEnd = cp;

            if (Vector2.Distance(last.pinStart, cp) < 0.01f) return;

            subRopes.Add(new SubRope(cp, last.pinEnd));

            anchors.RemoveAll(a => a.ropeIndex == subRopes.Count - 1);
            cooldown = wrapCooldownFrames;
        }

        /// <summary>
        /// Reassigns each anchor's ropeIndex by matching its position to a sub-rope's pin.
        /// Needed after teleporting because sub-rope indices may have shifted.
        /// </summary>
        private void ReindexAnchors(List<RopeAnchor> anchors, List<SubRope> subRopes)
        {
            for (int i = 0; i < anchors.Count; i++)
            {
                for (int r = 0; r < subRopes.Count; r++)
                {
                    if (Vector2.Distance(anchors[i].pos, subRopes[r].pinStart) < 1000f)
                    {
                        anchors[i] = new RopeAnchor
                        {
                            pos = anchors[i].pos,
                            wrapSide = anchors[i].wrapSide,
                            ropeIndex = r
                        };
                        break;
                    }
                }
            }
        }
    }
}
