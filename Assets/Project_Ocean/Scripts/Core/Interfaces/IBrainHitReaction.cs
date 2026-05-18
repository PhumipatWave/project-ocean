/// <summary>
/// Implemented by any enemy brain that reacts to being hit by a bullet or touched by the player.
/// Allows EnemyNavAgentBase to route hit events generically — no per-fish overrides needed
/// for standard collision/bullet handling.
/// FishS uses flee instead, so it does NOT implement this.
/// FishM and FishL both implement this.
/// </summary>
public interface IBrainHitReaction
{
    /// <summary>Called when a player bullet hits this enemy.</summary>
    void OnBulletHit();

    /// <summary>Called when the player's collider overlaps this enemy.</summary>
    void OnPlayerCollision();
}
