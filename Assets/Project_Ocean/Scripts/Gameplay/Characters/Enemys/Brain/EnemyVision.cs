using UnityEngine;

namespace Game.Testing
{
    public class EnemyVision
    {
        private readonly EnemyBaseProfile profile;
        private readonly EnemyBrainContext context;

        private bool wasInDetectRadius;
        private bool couldSeeLastTick;

        public EnemyVision(EnemyBaseProfile profile, EnemyBrainContext context)
        {
            this.profile = profile;
            this.context = context;
        }

        public bool CheckLineOfSight()
        {
            if (context.PlayerTransform == null) return false;

            float dist = Vector2.Distance(context.Transform.position, context.PlayerTransform.position);
            bool inRadius = dist <= profile.viewDistance;

            if (inRadius != wasInDetectRadius)
            {
                wasInDetectRadius = inRadius;
                if (profile.debugVision)
                    Debug.Log(inRadius
                        ? $"[Enemy] Player entered view distance ({dist:0.00})"
                        : $"[Enemy] Player left view distance ({dist:0.00})", context.Host);
            }

            if (!inRadius)
            {
                LogSeeChange(false);
                return false;
            }

            LayerMask obstacleMask = profile.visionObstacleMask.value != 0
                ? profile.visionObstacleMask
                : profile.wallLayer;

            bool canSee = EnemyVisionHelper.HasLineOfSightRay(
                context.Transform.position,
                context.PlayerTransform.position,
                context.Facing.GetFacingDirection(),
                profile.viewDistance,
                profile.viewAngle,
                obstacleMask,
                context.PlayerTransform
            );

            LogSeeChange(canSee);
            return canSee;
        }

        private void LogSeeChange(bool canSee)
        {
            if (canSee == couldSeeLastTick) return;
            couldSeeLastTick = canSee;

            if (profile.debugVision)
                Debug.Log(canSee ? "[Enemy] Can see player." : "[Enemy] Cannot see player.", context.Host);
        }
    }
}
