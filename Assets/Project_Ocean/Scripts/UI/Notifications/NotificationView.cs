using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>Base for anything that displays notifications of one channel.</summary>
    public abstract class NotificationView : MonoBehaviour
    {
        [SerializeField] private NotificationChannel channel;

        protected virtual void OnEnable()
        {
            NotificationManager.OnNotify += HandleNotify;
            NotificationManager.OnClear += HandleClear;
        }

        protected virtual void OnDisable()
        {
            NotificationManager.OnNotify -= HandleNotify;
            NotificationManager.OnClear -= HandleClear;
        }

        private void HandleNotify(NotificationMessage msg)
        {
            if (msg.Channel == channel)
                Show(msg);
        }

        private void HandleClear(NotificationChannel cleared)
        {
            if (cleared == channel)
                Clear();
        }

        protected abstract void Show(NotificationMessage msg);

        protected virtual void Clear() { }
    }
}
