using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
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

        public IEnemyState GetState(EnemyStateTag tag)
        {
            states.TryGetValue(tag, out IEnemyState state);
            return state;
        }

        public bool HasState(EnemyStateTag tag) => states.ContainsKey(tag);
    }
}