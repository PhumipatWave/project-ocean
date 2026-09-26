using UnityEngine;

namespace Game.Testing
{
    /// <summary>One notification: what to show, where, and an optional icon.</summary>
    public class NotificationMessage
    {
        public string Text { get; }
        public Sprite Icon { get; }
        public NotificationChannel Channel { get; }
        /// <summary>Stays on screen until NotificationManager.Clear is called for its channel.</summary>
        public bool Sticky { get; }

        public NotificationMessage(string text, NotificationChannel channel, Sprite icon, bool sticky = false)
        {
            Text = text;
            Channel = channel;
            Icon = icon;
            Sticky = sticky;
        }
    }
}
