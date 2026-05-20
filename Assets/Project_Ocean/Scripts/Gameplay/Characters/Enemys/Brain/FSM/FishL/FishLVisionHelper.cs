using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Encapsulates the line-of-sight check shared by all FishL states,
    /// keeping each state free of raw physics calls.
    /// </summary>
    public class FishLVisionHelper
    {
        private FishLBrainSettings fishLBrainSettings;
        private EnemyBrainContext enemyBrainContext;

        private bool wasInDetectRadius;
        private bool couldSeeLastTick;

        public FishLVisionHelper(FishLBrainSettings settings, EnemyBrainContext context)
        {
            fishLBrainSettings = settings;
            enemyBrainContext = context;
        }

        public bool CheckLineOfSight()
        {
            if (enemyBrainContext.PlayerTransform == null) return false;

            float dist = Vector2.Distance(enemyBrainContext.Transform.position, enemyBrainContext.PlayerTransform.position);
            bool inRadius = dist <= fishLBrainSettings.detectDistance;

            if (inRadius != wasInDetectRadius)
            {
                wasInDetectRadius = inRadius;
                if (fishLBrainSettings.debugVision)
                    Debug.Log(inRadius
                        ? $"[FishL] Player entered detect radius ({dist:0.00})"
                        : $"[FishL] Player left detect radius ({dist:0.00})", enemyBrainContext.Host);
            }

            if (!inRadius)
            {
                LogSeeChange(false);
                return false;
            }

            LayerMask obstacleMask = fishLBrainSettings.visionObstacleMask.value != 0 ? fishLBrainSettings.visionObstacleMask : fishLBrainSettings.wallLayer;

            bool canSee = EnemyVisionHelper.HasLineOfSightRay(
                enemyBrainContext.Transform.position,
                enemyBrainContext.PlayerTransform.position,
                Vector2.zero,
                fishLBrainSettings.detectDistance,
                fishLBrainSettings.viewAngle,
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
            if (fishLBrainSettings.debugVision)
                Debug.Log(canSee ? "[FishL] Can see player." : "[FishL] Cannot see player.", enemyBrainContext.Host);
        }
    }
}