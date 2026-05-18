using Game.Testing;
using UnityEngine;

/// <summary>
/// FishL Attack state: charges toward the player at boost speed and deals damage on contact.
/// Transitions -> Follow when player is lost or after hitting.
/// </summary>
public class FishLAttackState : EnemyStateBase
{
    public override EnemyStateTag StateTag => EnemyStateTag.Attack;

    private readonly FishLBrainSettings s;
    private readonly EnemyNavPathfinder pathfinder;
    private readonly FishLVisionHelper vision;

    public bool IsAttacking { get; private set; }

    public FishLAttackState(FishLBrainSettings settings, EnemyNavPathfinder pathfinder, FishLVisionHelper vision)
    {
        s = settings;
        this.pathfinder = pathfinder;
        this.vision = vision;
    }

    public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
    {
        base.Enter(ctx, fsm);
        IsAttacking = true;
        if (Ctx.Agent != null) Ctx.Agent.speed = s.boostSpeed;
    }

public override void Tick(float deltaTime)
    {
        if (Ctx.PlayerTransform == null) { EndAttack(false); Fsm.TransitionTo(EnemyStateTag.Wander); return; }

        bool canSee = vision.CheckLineOfSight();
        if (!canSee)
        {
            EndAttack(false);
            Fsm.TransitionTo(EnemyStateTag.Follow);
            return;
        }

        Vector3 playerPos = Ctx.PlayerTransform.position;
        float dist = Vector2.Distance(Ctx.Transform.position, playerPos);

        // Hit player
        if (dist <= s.attackRange)
        {
            // Try Ctx.Player first; fall back to component on the player transform
            Player target = Ctx.Player;
            if (target == null)
                target = Ctx.PlayerTransform.GetComponent<Player>();

            if (target != null)
                target.TakeDamage(s.attackDamage);

            EndAttack(true);
            Fsm.TransitionTo(EnemyStateTag.Follow);
            return;
        }

        // Too far — give up charge
        if (dist > s.detectDistance)
        {
            EndAttack(false);
            Fsm.TransitionTo(EnemyStateTag.Follow);
            return;
        }

        // Chase
        if (!pathfinder.SetSafeDestination(playerPos))
            pathfinder.SetSafeDestinationNear(playerPos);

        if (Ctx.Agent.velocity.sqrMagnitude > 0.01f)
            Ctx.Facing.RotateFromVelocity(Ctx.Agent.velocity);
    }

    public override void Exit()
    {
        IsAttacking = false;
        if (Ctx.Agent != null) Ctx.Agent.speed = s.speed;
        if (Ctx.AlertUI != null) Ctx.AlertUI.HideAttackWarning();
    }

    private void EndAttack(bool applyCooldown)
    {
        IsAttacking = false;
        // Pass cooldown back to FollowState so it won't immediately Prepare again
        if (applyCooldown && Fsm.GetState(EnemyStateTag.Follow) is FishLFollowState follow)
            follow.SetAttackCooldown(s.attackCooldown);
    }
}
