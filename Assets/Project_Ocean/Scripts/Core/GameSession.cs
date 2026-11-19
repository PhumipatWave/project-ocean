namespace Game.Core
{
    /// <summary>Current game state (Menu / Land / Ocean); GameManager writes it, anyone may read it.</summary>
    public static class GameSession
    {
        // Land = default(GameState): what readers see before GameManager.Awake sets it.
        public static GameState State { get; set; } = GameState.Land;
    }
}
