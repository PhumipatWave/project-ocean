using UnityEngine;

/// <summary>
/// FishL Prepare state: stops, faces the player, then charges.
/// Transitions -> Attack when prepareTimer and facing are both satisfied.
/// </summary>
public class FishLPrepareState : EnemyStateBase
{
    public override EnemyStateTag StateTag => EnemyStateTag.Prepare;

    private readonly FishLBrainSettings s;
    private readonly FishLVisionHelper vision;

    private float prepareTimer;

    public float AttackCooldownTimer { get; set; }

    public FishLPrepareState(FishLBrainSettings settings, FishLVisionHelper vision)
    {
        s = settings;
        this.vision = vision;
    }

public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
    {
        base.Enter(ctx, fsm);
        prepareTimer = s.prepareTime;
        AttackCooldownTimer = 0f;  // reset so a fresh prepare always leads to attack
        StopAgent();
        if (Ctx.AlertUI != null) Ctx.AlertUI.ShowAttackWarning();
    }

public override void Tick(float deltaTime)
    {
        StopAgent();
        prepareTimer -= deltaTime;
        if (AttackCooldownTimer > 0f) AttackCooldownTimer -= deltaTime;

        if (Ctx.PlayerTransform == null) { Fsm.TransitionTo(EnemyStateTag.Wander); return; }

        // If player left sight during prepare, fall back to Follow
        if (!vision.CheckLineOfSight())
        {
            Fsm.TransitionTo(EnemyStateTag.Follow);
            return;
        }

        Vector2 dir = Ctx.PlayerTransform.position - Ctx.Transform.position;
        if (dir.sqrMagnitude <= 0.01f) return;

        Ctx.Facing.SetSuppressRotation(true);
        bool facingPlayer = Ctx.Facing.RotateTowards(dir.normalized);
        Ctx.Facing.SetSuppressRotation(false);

        if (prepareTimer <= 0f && facingPlayer && AttackCooldownTimer <= 0f)
        {
            Fsm.TransitionTo(EnemyStateTag.Attack);
        }
    }

    public override void Exit()
    {
        if (Ctx.Agent != null && Ctx.Agent.isOnNavMesh)
        {
            Ctx.Agent.isStopped = false;
            Ctx.Agent.speed = s.boostSpeed;
        }
    }

    private void StopAgent()
    {
        if (Ctx.Agent == null || !Ctx.Agent.isOnNavMesh) return;
        Ctx.Agent.isStopped = true;
        Ctx.Agent.ResetPath();
        Ctx.Agent.velocity = UnityEngine.Vector3.zero;
    }
}
