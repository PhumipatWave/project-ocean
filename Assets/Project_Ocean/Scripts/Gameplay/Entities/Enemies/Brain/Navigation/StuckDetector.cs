using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>Reports when a fish barely moved over a time window, so the state can pick a new target once.</summary>
    public sealed class StuckDetector
    {
        private readonly float window;
        private readonly float minProgress;

        private Vector2 anchor;
        private float elapsed;

        public StuckDetector(float window, float minProgress)
        {
            this.window = window;
            this.minProgress = minProgress;
        }

        public void Reset(Vector3 position)
        {
            anchor = position;
            elapsed = 0f;
        }

        /// <summary>True when a full window passed and the fish moved less than minProgress (2D). Always re-anchors after a full window.</summary>
        public bool Tick(float deltaTime, Vector3 position)
        {
            elapsed += deltaTime;
            if (elapsed < window)
                return false;

            Vector2 current = position;
            bool stuck = (current - anchor).sqrMagnitude < minProgress * minProgress;

            anchor = current;
            elapsed = 0f;
            return stuck;
        }
    }
}
