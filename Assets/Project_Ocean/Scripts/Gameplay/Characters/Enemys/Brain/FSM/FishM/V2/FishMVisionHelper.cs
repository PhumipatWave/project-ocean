using UnityEngine;

namespace Game.Testing
{
    public class FishMVisionHelper
    {
        private readonly FishMBrainSettings settings;
        private readonly EnemyBrainContext  enemyBrainContext;

        private bool wasInDetectRadius;
        private bool couldSeeLastTick;

        public FishMVisionHelper(FishMBrainSettings settings, EnemyBrainContext context)
        {
            this.settings          = settings;
            this.enemyBrainContext = context;
        }

        public bool CheckLineOfSight()
        {
            if (enemyBrainContext.PlayerTransform == null) return false;

            float dist     = Vector2.Distance(enemyBrainContext.Transform.position, enemyBrainContext.PlayerTransform.position);
            bool  inRadius = dist <= settings.viewDistance;

            if (inRadius != wasInDetectRadius)
            {
                wasInDetectRadius = inRadius;
                if (settings.debugVision)
                    Debug.Log(inRadius
                        ? $"[FishM] Player entered view distance ({dist:0.00})"
                        : $"[FishM] Player left view distance ({dist:0.00})", enemyBrainContext.Host);
            }

            if (!inRadius)
            {
                LogSeeChange(false);
                return false;
            }

            bool canSee = EnemyVisionHelper.HasLineOfSightRay(
                enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position,
                Vector2.zero,
                settings.viewDistance,
                settings.viewAngle,
                settings.obstacleLayer
            );

            LogSeeChange(canSee);
            return canSee;
        }

        private void LogSeeChange(bool canSee)
        {
            if (canSee == couldSeeLastTick) return;
            couldSeeLastTick = canSee;
            if (settings.debugVision)
                Debug.Log(canSee ? "[FishM] Can see player." : "[FishM] Cannot see player.", enemyBrainContext.Host);
        }
    }
}