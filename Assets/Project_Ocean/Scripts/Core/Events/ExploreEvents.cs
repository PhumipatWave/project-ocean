using System;

namespace Game.Core
{
    public static class ExploreEvents
    {
        /// <summary>Raised when the player reaches/uses an ExploreAreaTrigger. Payload = its Area Id.</summary>
        public static event Action<string> OnAreaReached;

        public static void RaiseAreaReached(string areaId)
        {
            OnAreaReached?.Invoke(areaId);
        }
    }
}
