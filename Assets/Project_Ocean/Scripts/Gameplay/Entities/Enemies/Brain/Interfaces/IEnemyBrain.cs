using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public interface IEnemyBrain
    {
        void Initialize(EnemyBrainContext context);
        void Tick(float deltaTime);
        void OnHit(Transform attacker);
        void OnDeath();
        bool IsAttacking { get; }
    }
}