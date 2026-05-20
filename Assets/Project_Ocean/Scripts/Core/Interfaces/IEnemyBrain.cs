using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Per-species AI logic. Hosted by <see cref="EnemyNavAgentBase"/> on the same GameObject as <see cref="Game.Testing.Enemy"/>.
    /// </summary>
    public interface IEnemyBrain
    {
        void Initialize(EnemyBrainContext context);
        void Tick(float deltaTime);
        void OnHit(Transform attacker);
        void OnDeath();
        bool IsAttacking { get; }
    }
}