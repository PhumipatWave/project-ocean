using System.Collections.Generic;

namespace Game.Gameplay
{
    /// <summary>An entry with a relative chance of being picked.</summary>
    public interface IWeightedEntry
    {
        float Weight { get; }
    }

    /// <summary>Picks one entry at random, weighted by <see cref="IWeightedEntry.Weight"/>.</summary>
    public static class WeightedPicker
    {
        /// <summary>A random weighted entry, or null if the list is empty or every weight is zero.</summary>
        public static T Pick<T>(IReadOnlyList<T> entries) where T : class, IWeightedEntry =>
            Pick(entries, UnityEngine.Random.value);

        /// <summary>The entry that <paramref name="roll01"/> (0..1) lands on; split out so tests can fix the roll.</summary>
        public static T Pick<T>(IReadOnlyList<T> entries, float roll01) where T : class, IWeightedEntry
        {
            if (entries == null) return null;

            float totalWeight = 0f;
            foreach (var e in entries)
                if (e != null) totalWeight += e.Weight;

            if (totalWeight <= 0f) return null;

            float roll = roll01 * totalWeight;
            float cumulative = 0f;
            T last = null;

            foreach (var e in entries)
            {
                if (e == null) continue;
                cumulative += e.Weight;
                last = e;
                if (roll <= cumulative) return e;
            }

            // Float rounding can leave roll a hair above the total.
            return last;
        }
    }
}
