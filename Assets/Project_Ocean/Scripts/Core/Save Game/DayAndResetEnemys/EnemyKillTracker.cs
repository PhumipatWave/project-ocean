using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// Tracks which ocean enemy spawn points were cleared this day.
    /// Persists across ocean→land transitions so killed enemies don't respawn mid-day.
    /// Resets automatically when the player advances to the next day.
    /// Place this on the same persistent GameObject as GameManager / SaveBridge.
    public class EnemyKillTracker : MonoBehaviour, ISaveParticipant
    {
        public static EnemyKillTracker Instance { get; private set; }

        private readonly HashSet<string> killedSpawnIds = new();

        void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            // Persistence is handled by the parent GameManager root's DontDestroyOnLoad.
        }

        private void OnEnable()  => DayManager.OnEnemyReset += ResetAll;
        private void OnDisable() => DayManager.OnEnemyReset -= ResetAll;

        public void RegisterKill(string spawnId)
        {
            if (!string.IsNullOrEmpty(spawnId))
                killedSpawnIds.Add(spawnId);
        }

        public bool IsKilled(string spawnId) => killedSpawnIds.Contains(spawnId);

        public void ResetAll() => killedSpawnIds.Clear();

        // ── ISaveParticipant ──

        public void Collect(SaveData data) =>
            data.killedEnemySpawnIds = new List<string>(killedSpawnIds);

        public void Apply(SaveData data)
        {
            killedSpawnIds.Clear();
            if (data.killedEnemySpawnIds != null)
                foreach (var id in data.killedEnemySpawnIds)
                    killedSpawnIds.Add(id);
        }
    }
}
