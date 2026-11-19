using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    
    public class EnemyKillTracker : Singleton<EnemyKillTracker>, ISaveParticipant
    {
        public const string PermanentPrefix = "Boss_";

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

        public void ResetAll() =>
            killedSpawnIds.RemoveWhere(id => !id.StartsWith(PermanentPrefix));

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
