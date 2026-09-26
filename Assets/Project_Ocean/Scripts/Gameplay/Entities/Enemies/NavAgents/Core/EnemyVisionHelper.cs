using UnityEngine;

namespace Game.Testing
{
    public static class EnemyVisionHelper
    {
        public static bool HasLineOfSightRay(Vector2 origin, Vector2 target, Vector2 facingDirection, float maxDistance, float viewAngle, LayerMask obstacleMask, Transform ignoreTarget = null)
        {
            Vector2 toTarget = target - origin;
            float dist = toTarget.magnitude;

            if (dist > maxDistance)
                return false;

            if (dist < 0.01f)
                return true;

            Vector2 checkDir = facingDirection.sqrMagnitude > 0.01f
                ? facingDirection : toTarget.normalized;

            float angle = Vector2.Angle(checkDir, toTarget);
            if (angle > viewAngle * 0.5f)
                return false;

            RaycastHit2D hit = Physics2D.Raycast(origin, toTarget.normalized, dist, obstacleMask);
            if (hit.collider == null)
                return true;

            return ignoreTarget != null && hit.transform == ignoreTarget;
        }
    }
}
