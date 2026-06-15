using UnityEngine;
using UnityEngine.Events;

namespace Game.Testing
{
    public class DayManager : MonoBehaviour
    {
        public static DayManager Instance { get; private set; }

        public int CurrentDay { get; private set; } = 1;

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
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Called by SaveBridge after a save is loaded.</summary>
        public void SetDay(int day)
        {
            CurrentDay = Mathf.Max(1, day);
        }

        /// <summary>Called by the bed interactable — advances to the next day.</summary>
        public void AdvanceDay()
        {
            CurrentDay++;
            Debug.Log($"[DayManager] Day {CurrentDay} begins");

            onNewDay?.Invoke(CurrentDay);

            // Notify any scene-local subscriber (e.g. EnemySpawnManager in Land scene).
            // If no one is subscribed (e.g. during Ocean scene), this is a silent no-op.
            OnEnemyReset?.Invoke();

            // Auto-save (and anything else that reacts to a completed day change)
            // happens via subscription — fired last so the world is fully updated.
            OnDayAdvanced?.Invoke();
        }
    }
}
