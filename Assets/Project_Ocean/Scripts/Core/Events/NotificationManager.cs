using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>Static notification bus — callers pick a channel, views listen for theirs.</summary>
    public static class NotificationManager
    {
        public static event Action<NotificationMessage> OnNotify;
        public static event Action<NotificationChannel> OnClear;

        public static void Notify(string message, NotificationChannel channel = NotificationChannel.General, Sprite icon = null, bool sticky = false)
        {
            if (string.IsNullOrEmpty(message))
                return;

            OnNotify?.Invoke(new NotificationMessage(message, channel, icon, sticky));
        }

        /// <summary>Ends a sticky notification on this channel.</summary>
        public static void Clear(NotificationChannel channel) => OnClear?.Invoke(channel);
    }
}
