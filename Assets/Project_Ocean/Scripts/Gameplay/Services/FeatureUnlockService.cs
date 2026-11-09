using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Which GameFeatures are unlocked; quest steps unlock and lock them as they complete.</summary>
    // A lock always beats an unlock: IsUnlocked checks both sets, so the order they were granted doesn't matter.
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

        /// <summary>Unlocks a feature and clears an earlier Lock(), so a later step can reopen it.</summary>
        public void Unlock(GameFeature feature)
        {
            locked.Remove(feature);
            if (unlocked.Add(feature))
                OnFeatureUnlocked?.Invoke(feature);
        }

        public void Lock(GameFeature feature)
        {
            if (locked.Add(feature))
                OnFeatureLocked?.Invoke(feature);
        }

        /// <summary>Clears every unlock/lock — called by SaveBridge.ResetForNewGame() so a
        /// fresh game doesn't inherit a previous playthrough's unlocked features.</summary>
        public void ResetToDefault()
        {
            unlocked.Clear();
            locked.Clear();
        }

        /// <summary>Replays every completed step's unlocks; safe to repeat, and fixes saves made before a gate existed.</summary>
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
