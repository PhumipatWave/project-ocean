using UnityEngine;

/// <summary>
/// Encapsulates the line-of-sight check shared by all FishL states,
/// keeping each state free of raw physics calls.
/// </summary>
public class FishLVisionHelper
{
    private readonly FishLBrainSettings s;
    private readonly EnemyBrainContext ctx;

    private bool wasInDetectRadius;
    private bool couldSeeLastTick;

    public FishLVisionHelper(FishLBrainSettings settings, EnemyBrainContext context)
    {
        s = settings;
        ctx = context;
    }

    public bool CheckLineOfSight()
    {
        if (ctx.PlayerTransform == null) return false;

        float dist = Vector2.Distance(ctx.Transform.position, ctx.PlayerTransform.position);
        bool inRadius = dist <= s.detectDistance;

        if (inRadius != wasInDetectRadius)
        {
            wasInDetectRadius = inRadius;
            if (s.debugVision)
                Debug.Log(inRadius
                    ? $"[FishL] Player entered detect radius ({dist:0.00})"
                    : $"[FishL] Player left detect radius ({dist:0.00})", ctx.Host);
        }

        if (!inRadius)
        {
            LogSeeChange(false);
            return false;
        }

        LayerMask obstacleMask = s.visionObstacleMask.value != 0 ? s.visionObstacleMask : s.wallLayer;

        bool canSee = EnemyVisionHelper.HasLineOfSightRay(
            ctx.Transform.position,
            ctx.PlayerTransform.position,
            Vector2.zero,
            s.detectDistance,
            s.viewAngle,
            obstacleMask,
            ctx.PlayerTransform
        );

        LogSeeChange(canSee);
        return canSee;
    }

    private void LogSeeChange(bool canSee)
    {
        if (canSee == couldSeeLastTick) return;
        couldSeeLastTick = canSee;
        if (s.debugVision)
            Debug.Log(canSee ? "[FishL] Can see player." : "[FishL] Cannot see player.", ctx.Host);
    }
}
