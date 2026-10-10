
using UnityEngine;
using Game.Core;

namespace Game.UI
{
    public class TimePeriodUI : MonoBehaviour
    {
        [Header("Pointer")]
        [SerializeField] private RectTransform timePointer;

        [Header("Pointer Targets")]
        [SerializeField] private RectTransform morningTarget;
        [SerializeField] private RectTransform afternoonTarget;
        [SerializeField] private RectTransform eveningTarget;

        private void OnEnable()
        {
            Debug.Log("[TimeDebug] TimePeriodUI เปิดใช้งาน");

            DayManager.OnTimePeriodChanged += HandleTimePeriodChanged;

            Refresh();
        }

        private void OnDisable()
        {
            DayManager.OnTimePeriodChanged -= HandleTimePeriodChanged;
        }

        private void HandleTimePeriodChanged(TimePeriod period)
        {
            Debug.Log($"[TimeDebug] UI ได้รับ Event | Period = {period}");

            MovePointerTo(period);
        }

        public void Refresh()
        {
            if (DayManager.Instance == null)
                return;

            MovePointerTo(DayManager.Instance.CurrentPeriod);
        }

        private void MovePointerTo(TimePeriod period)
        {
            switch (period)
            {
                case TimePeriod.Morning:
                    MovePointer(morningTarget);
                    break;

                case TimePeriod.Afternoon:
                    MovePointer(afternoonTarget);
                    break;

                case TimePeriod.Evening:
                    MovePointer(eveningTarget);
                    break;
            }
        }

        private void MovePointer(RectTransform target)
        {
            if (timePointer == null || target == null)
                return;

            timePointer.position = target.position;
        }

    }
}
