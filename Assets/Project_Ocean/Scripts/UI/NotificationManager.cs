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
    /// </summary>
    public static class NotificationManager
    {
        /// <summary>Raised whenever something wants to show a text notification.</summary>
        public static event Action<string> OnNotify;

        public static void Notify(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            OnNotify?.Invoke(message);
        }
    }
}
