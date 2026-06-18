using UnityEngine;
using UnityEngine.Events;

namespace Game.Testing
{
    public class DayManager : MonoBehaviour
    {
        public static DayManager Instance { get; private set; }

        public int CurrentDay { get; private set; } = 1;

        /// True once the player has dived into the ocean at least once today.
        /// Reset to false each time a new day begins.
        public bool HasDivedToday { get; private set; }

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

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            // Persistence is handled by the parent GameManager root's DontDestroyOnLoad.
        }

        /// Called by SaveBridge after a save is loaded.
        public void SetDay(int day) => CurrentDay = Mathf.Max(1, day);

        /// Called by SaveBridge after a save is loaded.
        public void SetDivedToday(bool value) => HasDivedToday = value;

        /// Called by GameManager when the player starts a dive.
        public void MarkDivedToday() => HasDivedToday = true;

        /// Called by the bed interactable — advances to the next day.
        public void AdvanceDay()
        {
            CurrentDay++;
            HasDivedToday = false;
            Debug.Log($"[DayManager] Day {CurrentDay} begins");

            onNewDay?.Invoke(CurrentDay);
            OnEnemyReset?.Invoke();
            OnDayAdvanced?.Invoke();
        }
    }
}
