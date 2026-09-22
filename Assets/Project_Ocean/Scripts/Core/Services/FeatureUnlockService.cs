using System;
using System.Collections.Generic;

namespace Game.Testing
{
    /// <summary>
    /// Single source of truth for which gate-able systems (see GameFeature) are unlocked.
    /// Plain C# (no MonoBehaviour, no scene presence) — reachable from any scene, same shape
    /// as UpgradeService. Everything starts locked; QuestManager.CompleteProgress unlocks a
    /// step's GameFeature list the moment that step completes — and locks its Locks Features
    /// list the same moment. A lock always wins over an unlock, however either was granted
    /// (a different step's Unlocks Features, Unlock() called directly, any order/timing) —
    /// IsUnlocked checks both sets rather than removing from `unlocked`, so that holds
    /// structurally instead of depending on Apply()'s re-derivation order.
    /// </summary>
    public class FeatureUnlockService : ISaveParticipant
    {
        private static FeatureUnlockService instance;
        public static FeatureUnlockService Instance => instance ??= new FeatureUnlockService();

        /// <summary>Raised the first time a feature is unlocked (not on redundant re-unlocks).</summary>
        public static event Action<GameFeature> OnFeatureUnlocked;

        /// <summary>Raised the first time a feature is locked (not on redundant re-locks).</summary>
        public static event Action<GameFeature> OnFeatureLocked;

        private readonly HashSet<GameFeature> unlocked = new();
        private readonly HashSet<GameFeature> locked = new();

        public bool IsUnlocked(GameFeature feature) => unlocked.Contains(feature) && !locked.Contains(feature);

        public void Unlock(GameFeature feature)
        {
            if (unlocked.Add(feature))
                OnFeatureUnlocked?.Invoke(feature);
        }

        /// <summary>One-way, like Unlock — there's no matching "clear a lock" (nothing needs it
        /// yet; add one if a future step needs to re-open something it previously locked).</summary>
        public void Lock(GameFeature feature)
        {
            if (locked.Add(feature))
                OnFeatureLocked?.Invoke(feature);
        }

        /// <summary>
        /// Re-derives unlocks and locks from every quest's already-completed steps. Purely
        /// additive (both are idempotent), so always safe to run — this is what lets a save
        /// made before this feature existed retroactively unlock/lock what it already should
        /// have, instead of soft-locking an in-progress player.
        /// </summary>
        private void ApplyFromQuestProgress()
        {
            if (QuestManager.Instance == null)
                return;

            foreach (var q in QuestManager.Instance.GetAllQuests())
            {
                foreach (var step in q.GetCompletedSteps())
                {
                    if (step.UnlocksFeatures != null)
                        foreach (var feature in step.UnlocksFeatures)
                            Unlock(feature);

                    if (step.LocksFeatures != null)
                        foreach (var feature in step.LocksFeatures)
                            Lock(feature);
                }
            }
        }

        // ── ISaveParticipant ──

        public void Collect(SaveData data)
        {
            data.unlockedFeatures = new List<string>();
            foreach (var f in unlocked)
                data.unlockedFeatures.Add(f.ToString());

            data.lockedFeatures = new List<string>();
            foreach (var f in locked)
                data.lockedFeatures.Add(f.ToString());
        }

        public void Apply(SaveData data)
        {
            unlocked.Clear();
            locked.Clear();

            if (data.unlockedFeatures != null)
                foreach (var s in data.unlockedFeatures)
                    if (Enum.TryParse<GameFeature>(s, out var f))
                        unlocked.Add(f);

            if (data.lockedFeatures != null)
                foreach (var s in data.lockedFeatures)
                    if (Enum.TryParse<GameFeature>(s, out var f))
                        locked.Add(f);

            // Must run after QuestManager.Apply (see SaveBridge.BuildParticipants ordering).
            ApplyFromQuestProgress();
        }
    }
}
