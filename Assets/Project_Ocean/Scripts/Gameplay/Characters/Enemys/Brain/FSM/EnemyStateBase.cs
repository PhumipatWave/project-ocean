using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Convenience base for all concrete enemy states.
    /// Caches Ctx and Fsm on Enter so subclasses don't repeat the boilerplate.
    /// </summary>
    public abstract class EnemyStateBase : IEnemyState
    {
        protected EnemyBrainContext enemyBrainContext { get; private set; }
        protected EnemyFSM enemyFSM { get; private set; }

        public abstract EnemyStateTag StateTag { get; }

        public virtual void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            enemyBrainContext = ctx;
            enemyFSM = fsm;
        }

        public abstract void Tick(float deltaTime);

        public virtual void Exit() { }
    }
}