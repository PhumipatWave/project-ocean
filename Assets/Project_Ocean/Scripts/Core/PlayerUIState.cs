using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Lightweight counter that tracks how many land UI panels are currently open.
    /// Any system that opens a blocking UI calls Open(); when it closes, calls Close().
    /// Player movement and inventory are gated on IsLocked.
    /// </summary>
    public static class PlayerUIState
    {
        private static int openCount;

        // One slot per Open() call, so nested panels (e.g. a panel opened from within
        // another panel) unwind in the same order they were opened — Esc closes the
        // topmost one first, then the next Esc closes what's underneath it.
        private static readonly Stack<Action> closers = new Stack<Action>();

        public static bool IsLocked => openCount > 0;

        // Bumped by Reset(). A holder that Open()ed before a Reset must not Close() after it —
        // that Close would eat a lock someone else took since (see CutsceneManager's start lock).
        public static int ResetCount { get; private set; }

        /// <summary>Fired only on the 0→1 and 1→0 transitions (not on every nested Open/Close call).</summary>
        public static event Action<bool> OnLockChanged;

        public static void Open()
        {
            openCount++;
            closers.Push(null);
            if (openCount == 1) OnLockChanged?.Invoke(true);
        }

        public static void Close()
        {
            bool wasLocked = openCount > 0;
            openCount = Mathf.Max(0, openCount - 1);
            if (closers.Count > 0) closers.Pop();
            if (wasLocked && openCount == 0) OnLockChanged?.Invoke(false);
        }

        /// <summary>Registers how to close whichever panel is currently on top — call right after Open().</summary>
        public static void SetCloser(Action closeAction)
        {
            if (closers.Count > 0) closers.Pop();
            closers.Push(closeAction);
        }

        /// <summary>Closes whichever panel is currently on top (used by the Esc key). No-op if it registered no closer.</summary>
        public static void CloseCurrent()
        {
            if (closers.Count == 0) return;
            closers.Peek()?.Invoke();
        }

        /// <summary>Force-clear all locks — call on scene transitions to prevent stuck state.</summary>
        public static void Reset()
        {
            bool wasLocked = openCount > 0;
            openCount = 0;
            closers.Clear();
            ResetCount++;
            if (wasLocked) OnLockChanged?.Invoke(false);
        }
    }
}
