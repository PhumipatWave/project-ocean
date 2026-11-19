using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>An enemy brain's vision state, readable without knowing the brain type.</summary>
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