
using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    public class TimePeriodUI : MonoBehaviour
    {
        [Header("Clock Needle")]
        [SerializeField] private RectTransform timePointer;

        [Header("Needle Angles (Z Rotation)")]
        [SerializeField] private float morningAngle = 35f;
        [SerializeField] private float afternoonAngle = 0f;
        [SerializeField] private float eveningAngle = -40f;

        [Header("Time Period Image")]
        [SerializeField] private Image timePeriodImage;

        [Header("Period Sprites")]
        [SerializeField] private Sprite morningSprite;
        [SerializeField] private Sprite afternoonSprite;
        [SerializeField] private Sprite eveningSprite;

        private void OnEnable()
        {
            DayManager.OnTimePeriodChanged += HandleTimePeriodChanged;

            Refresh();
        }

        private void OnDisable()
        {
            DayManager.OnTimePeriodChanged -= HandleTimePeriodChanged;
        }

        private void HandleTimePeriodChanged(TimePeriod period)
        {
            UpdateTimePeriodUI(period);
        }

        public void Refresh()
        {
            if (DayManager.Instance == null)
                return;

            UpdateTimePeriodUI(
                DayManager.Instance.CurrentPeriod
            );
        }

        private void UpdateTimePeriodUI(TimePeriod period)
        {
            RotateNeedle(period);
            UpdatePeriodImage(period);
        }

        private void RotateNeedle(TimePeriod period)
        {
            if (timePointer == null)
                return;

            float angle;

            switch (period)
            {
                case TimePeriod.Morning:
                    angle = morningAngle;
                    break;

                case TimePeriod.Afternoon:
                    angle = afternoonAngle;
                    break;

                case TimePeriod.Evening:
                    angle = eveningAngle;
                    break;

                default:
                    return;
            }

            timePointer.localRotation =
                Quaternion.Euler(0f, 0f, angle);
        }

        private void UpdatePeriodImage(TimePeriod period)
        {
            if (timePeriodImage == null)
                return;

            Sprite targetSprite = null;

            switch (period)
            {
                case TimePeriod.Morning:
                    targetSprite = morningSprite;
                    break;

                case TimePeriod.Afternoon:
                    targetSprite = afternoonSprite;
                    break;

                case TimePeriod.Evening:
                    targetSprite = eveningSprite;
                    break;
            }

            if (targetSprite != null)
            {
                timePeriodImage.sprite = targetSprite;
            }
        }
    }
}
