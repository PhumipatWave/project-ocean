using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>Builds and assigns obstacle boundary paths for taut sub-ropes.</summary>
    internal static class RopeEdgePathUtility
    {
        private const float ChordVsPathTolerance = 1.005f;

        public static bool TryAssign(
            SubRope rope,
            ObjectWrapPoint wrapSource,
            Vector2 fromPos,
            Vector2 toPos,
            float sideSign,
            Vector2 sideReference,
            Collider2D obstacleCollider,
            LayerMask collisionMask,
            List<Vector2> scratch)
        {
            rope.ClearEdgePath();
            if (wrapSource == null) return false;

            scratch.Clear();
            if (!wrapSource.TryBuildBoundaryPath(fromPos, toPos, sideSign, sideReference, scratch)
                || scratch.Count < 2)
                return false;

            if (wrapSource.PathPenetratesCollider(scratch))
                return false;

            float chord = Vector2.Distance(fromPos, toPos);
            float along = RopeMath.PolylineLength(scratch);
            bool cuts = RopeMath.ChordCutsCollider(fromPos, toPos, obstacleCollider, collisionMask);

            if (!cuts && scratch.Count <= 2 && along <= chord * ChordVsPathTolerance)
                return false;

            rope.SetEdgePath(scratch);
            return true;
        }
    }
}
