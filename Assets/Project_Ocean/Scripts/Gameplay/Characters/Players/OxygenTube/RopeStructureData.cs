using System.Collections.Generic;
using UnityEngine;

/// <RopeStructureData>
/// Shared data structures used by all rope components.
/// </RopeStructureData>
namespace Game.Testing
{
    /// <summary>
    /// Point along the rope. Stores current 
    /// and previous pos for Verlet integration.
    /// </summary>
    public struct RopeSegment
    {
        public Vector2 pos;
        public Vector2 old;

        public RopeSegment(Vector2 p)
        {
            pos = p;
            old = p;
        }
    }

    /// <summary>
    /// Section of rope between two pinned endpoints.
    /// The rope is made of multiple RopeSegments.
    /// </summary>
    public class SubRope
    {
        public Vector2 pinStart;
        public Vector2 pinEnd;
        /// <summary>When set, taut rope follows obstacle corners instead of a straight chord.</summary>
        public List<Vector2> edgePath;
        public List<RopeSegment> segs = new();

        public SubRope(Vector2 from, Vector2 to)
        {
            pinStart = from;
            pinEnd = to;
            segs.Add(new RopeSegment(from));
            segs.Add(new RopeSegment(to));
        }

        public void CopyEdgePathFrom(SubRope other)
        {
            if (other?.edgePath == null || other.edgePath.Count < 2)
            {
                edgePath = null;
                return;
            }

            edgePath ??= new List<Vector2>();
            edgePath.Clear();
            edgePath.AddRange(other.edgePath);
        }
    }

    /// <summary>
    /// A point where the rope has wrapped around a corner.
    /// wrapSide tracks which side the player was on when wrapping happened,
    /// so we know when they've unwrapped by crossing back to the other side.
    /// </summary>
    public struct RopeAnchor
    {
        public Vector2 pos;
        // +1 or -1 (cross product sign at wrap time)
        public float wrapSide;
        // Outward surface normal at the wrap contact (used for stable unwrap)
        public Vector2 wrapNormal;
        // Collider instance id for boundary routing on the same obstacle
        public int obstacleId;
        // Which SubRope ends at this anchor
        public int ropeIndex;
    }

    /// <summary>
    /// A full snapshot of the rope state — used by the checkpoint system
    /// to restore the rope if the player teleports back.
    /// </summary>
    [System.Serializable]
    public class RopeWrapSettings
    {
        [Tooltip("Minimum opening angle (degrees) at the wrap corner before unwrap is considered. " +
                 "Lower = easier unwrap. Raise (95–110) if the rope detaches too early.")]
        public float minUnwrapAngle = 88f;

        [Tooltip("Player must be at least this many Segment Lengths away from the anchor to unwrap. " +
                 "Raise (2–2.5) if unwrap happens while hugging the corner.")]
        public float minUnwrapDistanceFactor = 1.5f;

        [Tooltip("How far outward (along the surface normal at wrap) the player must be before " +
                 "'cleared corner' counts. Range ~0.2–0.5. Higher = must step further away from the wall.")]
        public float unwrapNormalDot = 0.35f;

        [Tooltip("How strong the side-cross signal must be (cross product magnitude). " +
                 "Raise if unwrap triggers from tiny jitter; lower if unwrap never triggers.")]
        public float crossHysteresis = 0.06f;

        [Tooltip("UNWRAP ONLY: how far past the anchor (in Segment Lengths) along the incoming rope " +
                 "direction the player must move before 'passed corner' counts. " +
                 "Does NOT fix the blue rope drawing through walls — use Object Wrap Point on obstacles for that. " +
                 "Raise (0.8–1.2) if unwrap happens before you clear the corner; lower (0.3–0.5) if you cannot unwrap.")]
        public float passedCornerFactor = 0.75f;

        [Tooltip("When the rope follows an obstacle edge (boundary path), cross-sign may not flip. " +
                 "Enable to allow unwrap using wide angle + passed/cleared instead.")]
        public bool allowUnwrapWithoutSideCross = true;

        [Tooltip("Corner opening angle (degrees) required for the boundary-path unwrap fallback " +
                 "(only used when Allow Unwrap Without Side Cross is on).")]
        public float minUnwrapAngleForNoSideCross = 100f;
    }

    [System.Serializable]
    public class RopeSnapshot
    {
        public List<SubRope> subRopes;
        public List<RopeAnchor> anchors;
    }
}
