using System;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Picks a flee destination that is reachable and has open water behind it, instead of fleeing straight
    /// into a dead end. Directions are measured relative to "directly away from the threat".
    /// </summary>
    public sealed class FleeDestinationPicker
    {
        private const float MinDistance = 2f;
        private const float DistanceStep = 2f;
        private const float TowardThreatDot = -0.001f;

        private readonly Func<Vector3, ReachResult> resolve;
        private readonly int directionCount;

        public FleeDestinationPicker(Func<Vector3, ReachResult> resolve, int directionCount = 8)
        {
            this.resolve = resolve;
            this.directionCount = Mathf.Max(1, directionCount);
        }

        public bool TryPick(Vector3 self, Vector3 threat, float maxDistance, out Vector3 destination)
        {
            destination = self;

            Vector2 away = (Vector2)(self - threat);
            away = away.sqrMagnitude < 0.0001f ? Vector2.right : away.normalized;

            float selfToThreat = Vector2.Distance(self, threat);
            float startDistance = Mathf.Max(maxDistance, MinDistance);

            bool found = false;
            float bestDistance = 0f;
            float bestAlignment = -2f;

            for (int i = 0; i < directionCount; i++)
            {
                Vector3 direction = Quaternion.Euler(0f, 0f, i * 360f / directionCount) * new Vector3(away.x, away.y, 0f);
                float alignment = Vector2.Dot(direction, away);

                // Never swim toward the threat.
                if (alignment < TowardThreatDot)
                    continue;

                for (float d = startDistance; d >= MinDistance; d -= DistanceStep)
                {
                    Vector3 candidate = self + direction * d;
                    candidate.z = self.z;

                    ReachResult result = resolve(candidate);
                    if (result.Status != ReachStatus.Reachable)
                        continue;

                    if (Vector2.Distance(result.Position, threat) < selfToThreat)
                        continue;

                    // First hit while counting down is this direction's farthest reachable distance.
                    bool farther = d > bestDistance + 0.001f;
                    bool tieMoreAligned = Mathf.Abs(d - bestDistance) <= 0.001f && alignment > bestAlignment;

                    if (!found || farther || tieMoreAligned)
                    {
                        found = true;
                        bestDistance = d;
                        bestAlignment = alignment;
                        destination = result.Position;
                    }
                    break;
                }
            }

            return found;
        }
    }
}
