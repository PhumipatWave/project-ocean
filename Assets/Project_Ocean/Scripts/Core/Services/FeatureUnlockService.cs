using System;
using System.Collections.Generic;

namespace Game.Testing
{
    /// <summary>
    /// Single source of truth for which gate-able systems (see GameFeature) are unlocked.
    /// Plain C# (no MonoBehaviour, no scene presence) — reachable from any scene, same shape
    /// as UpgradeService. Everything starts locked; QuestManager.CompleteProgress unlocks a
    /// step's GameFeature list the moment that step completes.
    /// </summary>
    public class FeatureUnlockService : ISaveParticipant
    {
        private static FeatureUnlockService instance;
        public static FeatureUnlockService Instance => instance ??= new FeatureUnlockService();

        /// <summary>Raised the first time a feature is unlocked (not on redundant re-unlocks).</summary>
        public static event Action<GameFeature> OnFeatureUnlocked;

        private readonly HashSet<GameFeature> unlocked = new();

        public bool IsUnlocked(GameFeature feature) => unlocked.Contains(feature);

        public void Unlock(GameFeature feature)
        {
            if (unlocked.Add(feature))
                OnFeatureUnlocked?.Invoke(feature);
        }

        /// <summary>
        /// Re-derives unlocks from every quest's already-completed steps. Purely additive
        /// (Unlock is idempotent), so always safe to run — this is what lets a save made
        /// before this feature existed retroactively unlock what it already should have,
        /// instead of soft-locking an in-progress player.
        /// </summary>
        private void UnlockFromQuestProgress()
        {
            if (QuestManager.Instance == null)
                return;

            foreach (var q in QuestManager.Instance.GetAllQuests())
                foreach (var step in q.GetCompletedSteps())
                    if (step.UnlocksFeatures != null)
                        foreach (var feature in step.UnlocksFeatures)
                            Unlock(feature);
        }

        // ── ISaveParticipant ──

        public void Collect(SaveData data)
        {
            data.unlockedFeatures = new List<string>();
            foreach (var f in unlocked)
                data.unlockedFeatures.Add(f.ToString());
        }

        public void Apply(SaveData data)
        {
            unlocked.Clear();

            if (data.unlockedFeatures != null)
                foreach (var s in data.unlockedFeatures)
                    if (Enum.TryParse<GameFeature>(s, out var f))
                        unlocked.Add(f);

            // Must run after QuestManager.Apply (see SaveBridge.BuildParticipants ordering).
            UnlockFromQuestProgress();
        }
    }
}
