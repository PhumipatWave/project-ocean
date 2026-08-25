using System;
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

        /// <summary>Fired only on the 0→1 and 1→0 transitions (not on every nested Open/Close call).</summary>
        public static event Action<bool> OnLockChanged;

        public static void Open()
        {
            openCount++;
            if (openCount == 1) OnLockChanged?.Invoke(true);
        }

        public static void Close()
        {
            bool wasLocked = openCount > 0;
            openCount = Mathf.Max(0, openCount - 1);
            if (wasLocked && openCount == 0) OnLockChanged?.Invoke(false);
        }

        /// <summary>Force-clear all locks — call on scene transitions to prevent stuck state.</summary>
        public static void Reset()
        {
            bool wasLocked = openCount > 0;
            openCount = 0;
            if (wasLocked) OnLockChanged?.Invoke(false);
        }
    }
}
