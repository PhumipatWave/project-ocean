using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small fish AI refactored to FSM.
/// Registers 2 states: Flock (boids wander/lead) and Panic (flee).
/// Public API (FleeFrom, ReflectDirection, etc.) forwards to the active state.
/// </summary>
public class FishSBrain : EnemyBrainBase, IBrainVision
{
    private readonly FishSBrainSettings settings;
    private FishS_Agent host;
    private FishSFlockState flockState;
    private FishSPanicState panicState;
    private readonly List<FishS_Agent> neighbors = new List<FishS_Agent>();

    public Vector2 CurrentDirection => flockState?.CurrentDirection ?? Vector2.zero;

    #region IBrainVision
    public float ViewDistance => settings.viewDistance;
    public float ViewAngle    => settings.viewAngle;
    public bool CanSeePlayer() => Ctx?.PlayerTransform != null;
    #endregion

    public FishSBrain(FishSBrainSettings settings)
    {
        this.settings = settings;
    }

    public override void Initialize(EnemyBrainContext context)
    {
        base.Initialize(context);
        host = context.Host as FishS_Agent;

        flockState = new FishSFlockState(settings, host, neighbors);
        panicState = new FishSPanicState(settings, host);

        FSM.Register(flockState);
        FSM.Register(panicState);

        Ctx.Agent.speed = settings.speed;
        FSM.Start(EnemyStateTag.Flock);
    }

    // ── Public API forwarded from FishS_Agent ────────────────────────────────

    public void SetPlayerTransform(Transform player) { /* Ctx.PlayerTransform is the live reference */ }

public void FleeFrom(Vector2 targetPos)
    {
        // Transition first so Enter() sets Ctx on the panicState before SetFleeSource uses it
        FSM.TransitionTo(EnemyStateTag.Panic);
        panicState.SetFleeSource(targetPos);
    }

    public void ReflectDirection(Vector2 normal, bool randomizePanic)
    {
        // Delegate to whichever state is active (only meaningful during Panic)
        if (FSM.IsIn(EnemyStateTag.Panic))
            panicState.SetFleeSource(Ctx.Transform.position - (Vector3)normal);
    }

    public void ApplyBounceDestination()
    {
        Ctx.Agent.SetDestination(
            (Vector3)Ctx.Transform.position + (Vector3)CurrentDirection * settings.navDestinationDistance
        );
    }

    public bool IsDirectionBlocked()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            Ctx.Transform.position, CurrentDirection, settings.wallDetectDistance, settings.wallLayer
        );
        return hit.collider != null;
    }

    public void NudgeDirection(float degrees)
    {
        // Not needed with FSM wall avoidance built into states — kept for compatibility
    }
}
