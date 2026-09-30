using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// Enemy spawn slots cleared today, so killed enemies don't respawn until the next day.
    /// Lives on the persistent GameManager root.
    public class EnemyKillTracker : Singleton<EnemyKillTracker>, ISaveParticipant
    {

        private readonly HashSet<string> killedSpawnIds = new();

        // Shares its GameObject; persistence comes from the GameManager root.
        protected override bool DestroyGameObjectOnDuplicate => false;

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
