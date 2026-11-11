using UnityEngine;

namespace Game.Testing
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