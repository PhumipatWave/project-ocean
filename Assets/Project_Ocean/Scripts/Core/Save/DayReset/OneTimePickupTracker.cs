using System.Collections.Generic;

namespace Game.Core
{
    /// Counts pickups from ItemSpawners marked oneTimeOnly, keyed by spawner name.
    /// Unlike EnemyKillTracker it never resets on day change — only on New Game — so a
    /// one-time spawner's total across the whole playthrough stays capped at its spawnPerDay.
    /// Place this on the same persistent GameObject as GameManager / SaveBridge.
    public class OneTimePickupTracker : Singleton<OneTimePickupTracker>, ISaveParticipant
    {

        // One entry per pickup; the count of a spawner's name = how many it has given out.
        private readonly List<string> collected = new();

        // Shares its GameObject; persistence comes from the GameManager root.
        protected override bool DestroyGameObjectOnDuplicate => false;

        public void RegisterPickup(string spawnerId) => collected.Add(spawnerId);

        public int CountFor(string spawnerId) => collected.FindAll(id => id == spawnerId).Count;

        public void ResetAll() => collected.Clear();

        // ── ISaveParticipant ──

        public void Collect(SaveData data) => data.collectedPickupIds = new List<string>(collected);

        public void Apply(SaveData data)
        {
            collected.Clear();
            if (data.collectedPickupIds != null)
                collected.AddRange(data.collectedPickupIds);
        }
    }
}
