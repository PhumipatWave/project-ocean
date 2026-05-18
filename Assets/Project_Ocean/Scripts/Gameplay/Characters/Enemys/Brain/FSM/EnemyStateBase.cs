using UnityEngine;

/// <summary>
/// Convenience base for all concrete enemy states.
/// Caches Ctx and Fsm on Enter so subclasses don't repeat the boilerplate.
/// </summary>
public abstract class EnemyStateBase : IEnemyState
{
    protected EnemyBrainContext Ctx { get; private set; }
    protected EnemyFSM Fsm { get; private set; }

    public abstract EnemyStateTag StateTag { get; }

    public virtual void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
    {
        Ctx = ctx;
        Fsm = fsm;
    }

    public abstract void Tick(float deltaTime);

    public virtual void Exit() { }
}
