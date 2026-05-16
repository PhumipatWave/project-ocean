/// <summary>
/// Implemented by any enemy brain that can attack the player.
/// Lets the Agent side and external systems (combat log, UI) query attack state
/// and trigger attack attempts without knowing the concrete brain type.
/// FishS does NOT implement this — FishM and FishL do.
/// </summary>
public interface IBrainAttack
{
    /// <summary>True while the brain is executing an attack (charge, bite, etc.).</summary>
    bool IsAttacking { get; }

    /// <summary>Distance from the player at which this brain will land a hit.</summary>
    float AttackRange { get; }

    /// <summary>
    /// Called by the brain's own Tick to check range and deal damage.
    /// Can also be called externally to force an attack attempt.
    /// </summary>
    void TryAttack();
}
