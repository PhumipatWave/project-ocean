using System;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Generic, decoupled notification event bus.
    /// Any system can call NotificationManager.Notify("message") to show a
    /// text toast, without knowing anything about the UI that displays it.
    ///
    /// Example:
    ///   NotificationManager.Notify("You already have a quest in process");
    ///   NotificationManager.Notify("Inventory full");
    ///   NotificationManager.Notify("Game Saved!");
    ///
    /// ShowPersistent/ClearPersistent are a separate channel for messages that must stay
    /// on screen until explicitly cleared (e.g. a tutorial prompt), rather than a timed
    /// toast. A persistent message takes priority — the UI drops transient Notify() toasts
    /// while one is active, rather than interrupting it.
    /// </summary>
    public static class NotificationManager
    {
        /// <summary>Raised whenever something wants to show a text notification.</summary>
        public static event Action<string> OnNotify;

        /// <summary>Raised when the persistent message changes. Null/empty message means cleared.</summary>
        public static event Action<string, Sprite> OnPersistentChanged;

        /// <summary>Raised to draw the eye back to an already-showing persistent message (e.g. a
        /// blocked key press) — a transient Notify() toast would otherwise be silently dropped
        /// while a persistent message owns the slot.</summary>
        public static event Action OnPersistentPulse;

        public static void Notify(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            OnNotify?.Invoke(message);
        }

        /// <summary>Image is optional — pass null (or omit) for a text-only guide notification.</summary>
        public static void ShowPersistent(string message, Sprite image = null) => OnPersistentChanged?.Invoke(message, image);

        public static void ClearPersistent() => OnPersistentChanged?.Invoke(null, null);

        public static void PulsePersistent() => OnPersistentPulse?.Invoke();
    }
}
