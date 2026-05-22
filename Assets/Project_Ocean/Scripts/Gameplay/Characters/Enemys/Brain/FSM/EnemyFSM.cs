using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Lightweight FSM runner. Brain classes register states in Initialize(),
    /// then call Tick() every frame. TransitionTo() handles Enter/Exit cleanly.
    /// </summary>
    public class EnemyFSM
    {
        private readonly Dictionary<EnemyStateTag, IEnemyState> states =
            new Dictionary<EnemyStateTag, IEnemyState>();

        private IEnemyState current;
        private EnemyBrainContext ctx;

        public EnemyStateTag CurrentTag => current?.StateTag ?? EnemyStateTag.Wander;
        public IEnemyState CurrentState => current;

        public void Initialize(EnemyBrainContext context)
        {
            ctx = context;
        }

        public void Register(IEnemyState state)
        {
            states[state.StateTag] = state;
        }

        /// <summary>Enters the first state without calling Exit on a previous one.</summary>
        public void Start(EnemyStateTag tag)
        {
            if (!states.TryGetValue(tag, out IEnemyState state))
            {
                Debug.LogWarning($"[EnemyFSM] State '{tag}' not registered.");
                return;
            }
            current = state;
            current.Enter(ctx, this);
        }

        public void TransitionTo(EnemyStateTag tag)
        {
            if (current?.StateTag == tag) return;

            if (!states.TryGetValue(tag, out IEnemyState next))
            {
                Debug.LogWarning($"[EnemyFSM] Cannot transition to unregistered state '{tag}'.");
                return;
            }

            current?.Exit();
            current = next;
            current.Enter(ctx, this);
        }

        public void Tick(float deltaTime)
        {
            current?.Tick(deltaTime);
        }

        public bool IsIn(EnemyStateTag tag) => current?.StateTag == tag;

        /// <summary>Returns a registered state by tag, or null if not found.</summary>
        public IEnemyState GetState(EnemyStateTag tag)
        {
            states.TryGetValue(tag, out IEnemyState state);
            return state;
        }

        public bool HasState(EnemyStateTag tag) => states.ContainsKey(tag);
    }
}