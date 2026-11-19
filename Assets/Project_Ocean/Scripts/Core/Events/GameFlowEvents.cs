namespace Game.Core
{
    /// <summary>Scene-flow requests from gameplay code; GameManager performs them.</summary>
    public static class GameFlowEvents
    {
        public static event System.Action OnReturnToLandRequested;

        public static void RequestReturnToLand() => OnReturnToLandRequested?.Invoke();
    }
}
