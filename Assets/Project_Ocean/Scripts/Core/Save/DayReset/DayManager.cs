
using UnityEngine;
using UnityEngine.Events;

namespace Game.Core
{
    public enum TimePeriod
    {
        Morning,
        Afternoon,
        Evening
    }

    public class DayManager : Singleton<DayManager>, ISaveParticipant
    {
        public int CurrentDay { get; private set; } = 1;

        public const int MaxDivesPerDay = 3;

        /// <summary>
        /// จำนวนครั้งที่ดำน้ำในวันนี้
        /// </summary>
        public int DivesToday { get; private set; }

        /// <summary>
        /// ช่วงเวลาที่ UI แสดงอยู่จริง
        /// จะเปลี่ยนเมื่อกลับขึ้นบก หรือเริ่มวันใหม่
        /// </summary>
        private TimePeriod currentPeriod = TimePeriod.Morning;

        public TimePeriod CurrentPeriod => currentPeriod;

        public string CurrentPeriodDisplayName
        {
            get
            {
                return CurrentPeriod switch
                {
                    TimePeriod.Morning => "Morning",
                    TimePeriod.Afternoon => "Afternoon",
                    TimePeriod.Evening => "Evening",
                    _ => "Morning"
                };
            }
        }

        /// <summary>
        /// จำนวนครั้งที่ดำน้ำสะสมตลอดเกม
        /// </summary>
        public int TotalDives { get; private set; }

        public int DivesRemaining =>
            Mathf.Max(0, MaxDivesPerDay - DivesToday);

        public bool CanDive => DivesToday < MaxDivesPerDay;

        public bool HasDivedToday => DivesToday > 0;

        [Header("Events")]
        public UnityEvent<int> onNewDay;

        public static event System.Action OnEnemyReset;
        public static event System.Action OnDayAdvanced;
        public static event System.Action OnDivedToday;
        public static event System.Action<TimePeriod> OnTimePeriodChanged;

        private void OnEnable()
        {
            Debug.Log("[TimeDebug] DayManager OnEnable");

            LoadingManager.OnReturnedToLand += OnReturnToLand;
        }

        private void OnDisable()
        {
            Debug.Log("[TimeDebug] DayManager OnDisable");

            LoadingManager.OnReturnedToLand -= OnReturnToLand;
        }

        /// <summary>
        /// เรียกโดย SaveBridge หลังโหลดเซฟ
        /// </summary>
        public void SetDay(int day)
        {
            CurrentDay = Mathf.Max(1, day);
        }

        /// <summary>
        /// ตั้งค่าจำนวนครั้งที่ดำน้ำ และคืนค่าช่วงเวลาให้ตรงกับเซฟ
        /// </summary>
        public void SetDivesToday(int count)
        {
            DivesToday = Mathf.Clamp(count, 0, MaxDivesPerDay);

            currentPeriod = DivesToday switch
            {
                0 => TimePeriod.Morning,
                1 => TimePeriod.Afternoon,
                _ => TimePeriod.Evening
            };

            OnDivedToday?.Invoke();
            OnTimePeriodChanged?.Invoke(currentPeriod);
        }

        public void SetTotalDives(int count)
        {
            TotalDives = Mathf.Max(0, count);
        }

        /// <summary>
        /// เรียกเมื่อผู้เล่นเริ่มดำน้ำ
        /// นับจำนวนครั้ง แต่ยังไม่เปลี่ยนช่วงเวลาที่แสดง
        /// </summary>

        public bool TryRecordDive()
        {
            if (!CanDive)
                return false;

            DivesToday++;
            TotalDives++;

            OnDivedToday?.Invoke();

            return true;
        }


        /// <summary>
        /// เรียกอัตโนมัติเมื่อ LoadingManager โหลดกลับขึ้นบกเสร็จ
        /// </summary>

        public void OnReturnToLand()
        {
            if (DivesToday <= 0)
                currentPeriod = TimePeriod.Morning;
            else if (DivesToday == 1)
                currentPeriod = TimePeriod.Afternoon;
            else
                currentPeriod = TimePeriod.Evening;

            OnTimePeriodChanged?.Invoke(currentPeriod);
        }


        /// <summary>
        /// เรียกเมื่อผู้เล่นนอนเพื่อเริ่มวันใหม่
        /// </summary>
        public void AdvanceDay()
        {
            CurrentDay++;
            DivesToday = 0;
            currentPeriod = TimePeriod.Morning;

            GameLog.Info($"[DayManager] Day {CurrentDay} begins");

            onNewDay?.Invoke(CurrentDay);
            OnEnemyReset?.Invoke();
            OnDivedToday?.Invoke();
            OnTimePeriodChanged?.Invoke(currentPeriod);
            OnDayAdvanced?.Invoke();

            TutorialEvents.RaiseAction(TutorialActionId.Sleep);
        }

        public void Collect(SaveData data)
        {
            data.currentDay = CurrentDay;
            data.hasDivedToday = HasDivedToday;
            data.divesToday = DivesToday;
            data.totalDives = TotalDives;
        }

        public void Apply(SaveData data)
        {
            SetDay(data.currentDay);

            // รองรับเซฟเก่าที่มีเพียง hasDivedToday
            SetDivesToday(
                data.divesToday > 0
                    ? data.divesToday
                    : (data.hasDivedToday ? 1 : 0)
            );

            SetTotalDives(data.totalDives);
        }
    }
}
