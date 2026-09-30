using UnityEngine;
using Game.Core;

namespace Game.Gameplay
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
                    GameLog.Info(inRadius
                        ? $"[Enemy] Player entered view distance ({dist:0.00})"
                        : $"[Enemy] Player left view distance ({dist:0.00})", context.Host);
            }

            if (!inRadius)
            {
                LogSeeChange(false);
                return false;
            }

            if (profile.wallLayer.value == 0)
            {
                Debug.LogWarning($"[EnemyVision] '{context.Host.name}' has no Wall Layer set — fish will see through walls. Assign wallLayer in the profile asset.", context.Host);
                return false;
            }

            bool canSee = EnemyVisionHelper.HasLineOfSightRay(
                context.Transform.position,
                context.PlayerTransform.position,
                context.Facing.GetFacingDirection(),
                profile.viewDistance,
                profile.viewAngle,
                profile.wallLayer,
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
                GameLog.Info(canSee ? "[Enemy] Can see player." : "[Enemy] Cannot see player.", context.Host);
        }
    }
}
