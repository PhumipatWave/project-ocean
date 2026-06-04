using System;

namespace Game.Testing
{
    /// <summary>
    /// Contract for any health source an Entity can use.
    /// EntityHealth implements this for standard HP.
    /// PlayerOxygenTube implements this so rope length acts as player health.
    /// </summary>
    public interface IEntityHealth
    {
        float CurrentHealth { get; }
        float MaxHealth     { get; }
        bool  IsDead        { get; }

        event Action OnDamaged;
        event Action OnDied;

        void TakeDamage(float amount);
        void Heal(float amount);
        void Initialize();
    }
}
