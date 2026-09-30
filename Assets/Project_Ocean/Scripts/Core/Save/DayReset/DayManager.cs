using UnityEngine;
using UnityEngine.Events;

namespace Game.Core
{
    public class DayManager : Singleton<DayManager>, ISaveParticipant
    {

        public int CurrentDay { get; private set; } = 1;

        public const int MaxDivesPerDay = 3;

        /// How many times the player has dived today. Reset to 0 each time a new day begins.
        public int DivesToday { get; private set; }

        /// Lifetime dive count — never reset by AdvanceDay. Drives permanent, one-time unlocks
        /// (e.g. RevealAfterTotalDives) that should stay revealed once reached.
        public int TotalDives { get; private set; }

        public int DivesRemaining => Mathf.Max(0, MaxDivesPerDay - DivesToday);
        public bool CanDive => DivesToday < MaxDivesPerDay;

        /// True once the player has dived into the ocean at least once today.
        public bool HasDivedToday => DivesToday > 0;

        [Header("Events")]
        public UnityEvent<int> onNewDay;

        /// <summary>
        /// Fired by AdvanceDay() — EnemySpawnManager subscribes via OnEnable/OnDisable.
        /// DayManager never holds a direct reference to EnemySpawnManager.
        /// </summary>
        public static event System.Action OnEnemyReset;

        /// <summary>
        /// Fired LAST in AdvanceDay(), after onNewDay and OnEnemyReset, so subscribers
        /// (e.g. SaveBridge auto-save) see the fully updated world state.
        /// DayManager never holds a direct reference to the save system.
        /// </summary>
        public static event System.Action OnDayAdvanced;

        /// <summary>Fired whenever today's dive count changes — gates the sleep interaction and the dive panel text.</summary>
        public static event System.Action OnDivedToday;


        /// Called by SaveBridge after a save is loaded.
        public void SetDay(int day) => CurrentDay = Mathf.Max(1, day);

        /// Called by SaveBridge after a save is loaded.
        public void SetDivesToday(int count)
        {
            DivesToday = Mathf.Clamp(count, 0, MaxDivesPerDay);
            OnDivedToday?.Invoke();
        }

        /// Called by SaveBridge after a save is loaded.
        public void SetTotalDives(int count) => TotalDives = Mathf.Max(0, count);

        /// Called by GameManager when the player starts a dive. Returns false (and counts
        /// nothing) once today's dive limit is used up.
        public bool TryRecordDive()
        {
            if (!CanDive) return false;
            DivesToday++;
            TotalDives++;
            OnDivedToday?.Invoke();
            return true;
        }

        /// Called by the bed interactable — advances to the next day.
        public void AdvanceDay()
        {
            CurrentDay++;
            DivesToday = 0;
            GameLog.Info($"[DayManager] Day {CurrentDay} begins");

            onNewDay?.Invoke(CurrentDay);
            OnEnemyReset?.Invoke();
            OnDayAdvanced?.Invoke();
            TutorialEvents.RaiseAction(TutorialActionId.Sleep);
        }

        public void Collect(SaveData data)
        {
            data.currentDay    = CurrentDay;
            data.hasDivedToday = HasDivedToday;
            data.divesToday    = DivesToday;
            data.totalDives    = TotalDives;
        }

        public void Apply(SaveData data)
        {
            SetDay(data.currentDay);
            // Saves from before divesToday existed only carry the bool — count that as one dive.
            SetDivesToday(data.divesToday > 0 ? data.divesToday : (data.hasDivedToday ? 1 : 0));
            SetTotalDives(data.totalDives);
        }
    }
}
