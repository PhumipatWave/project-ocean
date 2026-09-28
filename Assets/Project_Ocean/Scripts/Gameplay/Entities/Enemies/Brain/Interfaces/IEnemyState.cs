using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Contract for every state in the enemy FSM.
    /// </summary>
    public interface IEnemyState
    {
        EnemyStateTag StateTag { get; }

        /// <summary>Called once when the FSM transitions into this state.</summary>
        void Enter(EnemyBrainContext ctx, EnemyFSM fsm);

        /// <summary>Called every frame while this state is active.</summary>
        void Tick(float deltaTime);

        /// <summary>Called once just before the FSM transitions out of this state.</summary>
        void Exit();
    }
}