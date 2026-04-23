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
        public List<RopeSegment> segs = new();

        public SubRope(Vector2 from, Vector2 to)
        {
            pinStart = from;
            pinEnd = to;
            segs.Add(new RopeSegment(from));
            segs.Add(new RopeSegment(to));
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
        // Which SubRope ends at this anchor
        public int ropeIndex;
    }

    /// <summary>
    /// A full snapshot of the rope state — used by the checkpoint system
    /// to restore the rope if the player teleports back.
    /// </summary>
    [System.Serializable]
    public class RopeSnapshot
    {
        public List<SubRope> subRopes;
        public List<RopeAnchor> anchors;
    }
}
