using System.Collections.Generic;

namespace Game.Core
{
    /// Counts pickups from oneTimeOnly spawners for the whole playthrough (reset only by New Game),
    /// so each stays capped at its spawnPerDay. Lives on the persistent GameManager root.
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
