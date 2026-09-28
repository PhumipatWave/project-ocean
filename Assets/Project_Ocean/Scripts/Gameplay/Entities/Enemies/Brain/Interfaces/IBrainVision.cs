using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Implemented by any enemy brain that detects the player through a vision cone or radius.
    /// External systems (alert managers, stealth, debug overlays) can query any enemy's
    /// vision state without knowing the concrete brain type.
    /// </summary>
    public interface IBrainVision
    {
        /// <summary>Maximum distance at which this brain can detect the player.</summary>
        float ViewDistance { get; }

        /// <summary>Vision cone full angle in degrees.</summary>
        float ViewAngle { get; }

        /// <summary>Returns true if the brain currently has a clear line of sight to the player.</summary>
        bool CanSeePlayer();
    }
}