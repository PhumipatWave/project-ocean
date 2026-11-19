using System;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>Walks a ring around a center point; each fish gets its own start angle so a school does not stack.</summary>
    public sealed class LurkOrbit
    {
        private const int MaxAlternates = 7;
        private const float AlternateStepDeg = 45f;

        private readonly float radius;
        private readonly float angularSpeedDeg;
        private float angle;

        public LurkOrbit(float radius, float angularSpeedDeg, float startAngleDeg)
        {
            this.radius = radius;
            this.angularSpeedDeg = angularSpeedDeg;
            angle = startAngleDeg;
        }

        /// <summary>Advances the angle, then returns the first valid ring point (current angle, then +45 degree steps).</summary>
        public bool TryPick(Vector3 center, float deltaTime, Func<Vector3, bool> isValid, out Vector3 point)
        {
            angle += angularSpeedDeg * deltaTime;

            for (int k = 0; k <= MaxAlternates; k++)
            {
                float candidateAngle = angle + k * AlternateStepDeg;
                Vector3 candidate = PointAt(center, candidateAngle);

                if (!isValid(candidate))
                    continue;

                angle = candidateAngle;
                point = candidate;
                return true;
            }

            point = center;
            return false;
        }

        private Vector3 PointAt(Vector3 center, float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector3(center.x + Mathf.Cos(rad) * radius, center.y + Mathf.Sin(rad) * radius, center.z);
        }
    }
}
