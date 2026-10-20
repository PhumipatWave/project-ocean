using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>Fires once per interval; used to throttle path queries instead of repathing every frame.</summary>
    public sealed class RepathTimer
    {
        private readonly float interval;
        private float elapsed;

        /// <param name="initialOffset">Time already elapsed. Pass Random.value * interval so schooling fish do not repath on the same frame.</param>
        public RepathTimer(float interval, float initialOffset = 0f)
        {
            this.interval = Mathf.Max(0.0001f, interval);
            elapsed = initialOffset;
        }

        /// <summary>True when the interval has elapsed; the timer then restarts from zero (at most one true per call).</summary>
        public bool Tick(float deltaTime)
        {
            elapsed += deltaTime;
            if (elapsed < interval)
                return false;

            elapsed = 0f;
            return true;
        }

        public void Reset() => elapsed = 0f;
    }
}
