using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Encapsulates the line-of-sight check shared by all FishS states,
    /// keeping each state free of raw physics calls.
    /// </summary>
    public class FishSVisionHelper
    {
        private FishSBrainSettings fishSBrainSettings;
        private EnemyBrainContext enemyBrainContext;

        private bool wasInDetectRadius;
        private bool couldSeeLastTick;

        public FishSVisionHelper(FishSBrainSettings settings, EnemyBrainContext context)
        {
            fishSBrainSettings = settings;
            enemyBrainContext = context;
        }

        public bool CheckLineOfSight()
        {
            if (enemyBrainContext.PlayerTransform == null) return false;

            float dist = Vector2.Distance(enemyBrainContext.Transform.position, enemyBrainContext.PlayerTransform.position);
            bool inRadius = dist <= fishSBrainSettings.detectDistance;

            if (inRadius != wasInDetectRadius)
            {
                wasInDetectRadius = inRadius;
                if (fishSBrainSettings.debugVision)
                    Debug.Log(inRadius
                        ? $"[FishS] Player entered detect radius ({dist:0.00})"
                        : $"[FishS] Player left detect radius ({dist:0.00})", enemyBrainContext.Host);
            }

            if (!inRadius)
            {
                LogSeeChange(false);
                return false;
            }

            LayerMask obstacleMask = fishSBrainSettings.visionObstacleMask.value != 0
                ? fishSBrainSettings.visionObstacleMask
                : fishSBrainSettings.wallLayer;

            bool canSee = EnemyVisionHelper.HasLineOfSightRay(
                enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position,
                Vector2.zero,
                fishSBrainSettings.detectDistance,
                fishSBrainSettings.viewAngle,
                obstacleMask,
                enemyBrainContext.PlayerTransform
            );

            LogSeeChange(canSee);
            return canSee;
        }

        private void LogSeeChange(bool canSee)
        {
            if (canSee == couldSeeLastTick) return;
            couldSeeLastTick = canSee;
            if (fishSBrainSettings.debugVision)
                Debug.Log(canSee ? "[FishS] Can see player." : "[FishS] Cannot see player.", enemyBrainContext.Host);
        }
    }
}