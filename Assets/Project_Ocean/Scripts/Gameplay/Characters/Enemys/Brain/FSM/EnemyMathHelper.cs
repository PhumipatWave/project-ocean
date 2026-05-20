using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Stateless math utilities shared across enemy brain states.
    /// </summary>
    public static class EnemyMathHelper
    {
        /// <summary>Rotates a 2D vector by the given degrees (clockwise-positive).</summary>
        public static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            return new Vector2(
                v.x * Mathf.Cos(r) - v.y * Mathf.Sin(r),
                v.x * Mathf.Sin(r) + v.y * Mathf.Cos(r)
            );
        }
    }
}