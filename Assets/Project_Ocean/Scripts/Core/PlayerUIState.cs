using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Lightweight counter that tracks how many land UI panels are currently open.
    /// Any system that opens a blocking UI calls Open(); when it closes, calls Close().
    /// Player movement and inventory are gated on IsLocked.
    /// </summary>
    public static class PlayerUIState
    {
        private static int openCount;

        public static bool IsLocked => openCount > 0;

        public static void Open()  => openCount++;

        public static void Close() => openCount = Mathf.Max(0, openCount - 1);

        /// <summary>Force-clear all locks — call on scene transitions to prevent stuck state.</summary>
        public static void Reset() => openCount = 0;
    }
}
