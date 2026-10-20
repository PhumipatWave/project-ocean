using UnityEngine;
using UnityEngine.AI;

namespace Game.Gameplay
{
    public enum ReachStatus { Reachable, Approximate, Unreachable }

    /// <summary>Outcome of resolving a world point against the NavMesh. Position is always the best valid mesh point available.</summary>
    public readonly struct ReachResult
    {
        public ReachStatus Status { get; }
        public Vector3 Position { get; }

        /// <summary>2D distance from Position to the original target.</summary>
        public float Gap { get; }

        public ReachResult(ReachStatus status, Vector3 position, float gap)
        {
            Status = status;
            Position = position;
            Gap = gap;
        }

        public static ReachResult Unreachable(Vector3 target) =>
            new ReachResult(ReachStatus.Unreachable, target, 0f);

        public static ReachResult Classify(NavMeshPathStatus pathStatus, Vector3 position, Vector3 target, float reachTolerance)
        {
            float gap = Vector2.Distance(position, target);
            ReachStatus status;

            switch (pathStatus)
            {
                case NavMeshPathStatus.PathComplete:
                    status = gap <= reachTolerance ? ReachStatus.Reachable : ReachStatus.Unreachable;
                    break;
                case NavMeshPathStatus.PathPartial:
                    status = ReachStatus.Approximate;
                    break;
                default:
                    status = ReachStatus.Unreachable;
                    break;
            }

            return new ReachResult(status, position, gap);
        }
    }
}
