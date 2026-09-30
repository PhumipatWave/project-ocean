using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>Picks which of several in-range targets an Interact press goes to.</summary>
    public static class InteractionTargeting
    {
        public static T PickClosest<T>(Vector2 from, IEnumerable<T> candidates, Func<T, Vector2> position, Func<T, bool> isReady) where T : class
        {
            T best = null;
            float bestSqr = float.MaxValue;
            foreach (T c in candidates)
            {
                if (!isReady(c)) continue;

                float sqr = (position(c) - from).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = c;
                }
            }
            return best;
        }
    }
}
