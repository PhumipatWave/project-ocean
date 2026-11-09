using System.Diagnostics;

namespace Game.Core
{
    /// <summary>Info logging compiled out of release builds (editor and development builds only).</summary>
    public static class GameLog
    {
        // [Conditional] also skips evaluating the arguments, so release builds don't build the message string.
        // HideInCallstack: console double-click opens the caller, not this file.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), UnityEngine.HideInCallstack]
        public static void Info(object message, UnityEngine.Object context = null) =>
            UnityEngine.Debug.Log(message, context);
    }
}
