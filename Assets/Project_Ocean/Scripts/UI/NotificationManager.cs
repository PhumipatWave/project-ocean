using System;

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

        /// <summary>Raised when the persistent message changes. Null/empty means cleared.</summary>
        public static event Action<string> OnPersistentChanged;

        public static void Notify(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            OnNotify?.Invoke(message);
        }

        public static void ShowPersistent(string message) => OnPersistentChanged?.Invoke(message);

        public static void ClearPersistent() => OnPersistentChanged?.Invoke(null);
    }
}
