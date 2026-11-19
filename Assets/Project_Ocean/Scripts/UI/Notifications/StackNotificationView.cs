using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>Shows several notifications at once as stacked lines, newest at the bottom.</summary>
    public class StackNotificationView : NotificationView
    {
        [SerializeField] private NotificationLine linePrefab;
        [SerializeField] private Transform container;
        [SerializeField] private int maxLines = 4;

        protected override void OnDisable()
        {
            base.OnDisable();
            // Line coroutines stop while disabled; clear so no frozen lines remain on re-enable.
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        protected override void Show(NotificationMessage msg)
        {
            // Destroy runs at end of frame, so detach first or childCount stays over the cap.
            while (container.childCount >= maxLines)
            {
                var oldest = container.GetChild(0);
                oldest.SetParent(null, false);
                Destroy(oldest.gameObject);
            }

            Instantiate(linePrefab, container).Setup(msg);
        }
    }
}
