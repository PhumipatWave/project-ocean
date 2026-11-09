using System;
using Game.Core;

namespace Game.Gameplay
{
    public interface IEntityHealth
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool  IsDead { get; }

        event Action OnDamaged;
        event Action OnDied;

        void TakeDamage(float amount);
        void Heal(float amount);
        void Initialize();
    }
}
