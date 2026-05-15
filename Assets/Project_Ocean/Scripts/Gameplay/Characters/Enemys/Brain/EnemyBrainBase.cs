using UnityEngine;

public abstract class EnemyBrainBase : IEnemyBrain
{
    protected EnemyBrainContext Ctx;

    public virtual bool IsAttacking => false;

    public virtual void Initialize(EnemyBrainContext context)
    {
        Ctx = context;
    }

    public abstract void Tick(float deltaTime);

    public virtual void OnHit(Transform attacker) { }

    public virtual void OnDeath()
    {
        HideAttackWarning();
        SetSeeingPlayer(false);
    }

    protected void ShowAttackWarning()
    {
        if (Ctx?.AlertUI != null)
            Ctx.AlertUI.ShowAttackWarning();
    }

    protected void HideAttackWarning()
    {
        if (Ctx?.AlertUI != null)
            Ctx.AlertUI.HideAttackWarning();
    }

    protected void SetSeeingPlayer(bool canSee)
    {
        if (Ctx?.AlertUI != null)
            Ctx.AlertUI.SetSeeingPlayer(canSee);
    }

    protected void HideAllAlerts()
    {
        if (Ctx?.AlertUI != null)
            Ctx.AlertUI.HideAll();
    }
}
