using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public static class RopeLimits
    {
        public const int MaxAnchors = 15;
    }

    /// <summary>Point along the rope with Verlet state.</summary>
    public struct RopeSegment
    {
        public Vector2 pos;
        public Vector2 old;

        public RopeSegment(Vector2 p)
        {
            pos = p;
            old = p;
        }

        public void PinTo(Vector2 p)
        {
            pos = p;
            old = p;
        }
    }

    /// <summary>Taut or simulated section between two pinned endpoints.</summary>
    public class SubRope
    {
        public Vector2 pinStart;
        public Vector2 pinEnd;
        public List<Vector2> edgePath;
        public List<RopeSegment> segs = new();

        public bool HasEdgePath => edgePath != null && edgePath.Count >= 2;
        public bool IsTaut => HasEdgePath || segs.Count <= 2;

        public SubRope(Vector2 from, Vector2 to)
        {
            pinStart = from;
            pinEnd = to;
            segs.Add(new RopeSegment(from));
            segs.Add(new RopeSegment(to));
        }

        public void ClearEdgePath() => edgePath = null;

        public void SetEdgePath(IReadOnlyList<Vector2> path)
        {
            if (path == null || path.Count < 2) { ClearEdgePath(); return; }

            edgePath ??= new List<Vector2>();
            edgePath.Clear();
            foreach (var p in path) edgePath.Add(p);
        }

        public void CopyEdgePathFrom(SubRope other)
        {
            if (other == null || !other.HasEdgePath) { ClearEdgePath(); return; }
            SetEdgePath(other.edgePath);
        }

        public float TargetLength() =>
            HasEdgePath ? RopeMath.PolylineLength(edgePath) : Vector2.Distance(pinStart, pinEnd);

        public SubRope Clone()
        {
            var copy = new SubRope(pinStart, pinEnd);
            copy.CopyEdgePathFrom(this);
            copy.segs.Clear();
            foreach (var s in segs)
                copy.segs.Add(new RopeSegment(s.pos) { old = s.old });
            return copy;
        }
    }

    /// <summary>Wrap pin on an obstacle corner.</summary>
    public struct RopeAnchor
    {
        public Vector2 pos;
        public float wrapSide;
        public Vector2 wrapNormal;
        public int obstacleId;
        public int ropeIndex;
    }

    [System.Serializable]
    public class RopeWrapSettings
    {
        [Tooltip("Minimum opening angle (degrees) at the wrap corner before unwrap is considered.")]
        public float minUnwrapAngle = 88f;

        [Tooltip("Player must be at least this many Segment Lengths away from the anchor to unwrap.")]
        public float minUnwrapDistanceFactor = 1.5f;

        [Tooltip("How far outward (along the surface normal at wrap) the player must be before 'cleared corner' counts.")]
        public float unwrapNormalDot = 0.35f;

        [Tooltip("How strong the side-cross signal must be (cross product magnitude).")]
        public float crossHysteresis = 0.06f;

        [Tooltip("UNWRAP ONLY: how far past the anchor the player must move before 'passed corner' counts.")]
        public float passedCornerFactor = 0.75f;

        [Tooltip("Enable to allow unwrap using wide angle + passed/cleared instead of side cross.")]
        public bool allowUnwrapWithoutSideCross = true;

        [Tooltip("Corner opening angle (degrees) required for the boundary-path unwrap fallback.")]
        public float minUnwrapAngleForNoSideCross = 100f;
    }
}
