using UnityEngine;

namespace Game.Testing
{
    public static class EnemyMathHelper
    {
        public static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            return new Vector2(v.x * Mathf.Cos(r) - v.y * Mathf.Sin(r), 
                v.x * Mathf.Sin(r) + v.y * Mathf.Cos(r));
        }
    }
}