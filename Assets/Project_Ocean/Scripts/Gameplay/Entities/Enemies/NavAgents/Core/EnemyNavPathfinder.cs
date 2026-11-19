using System;
using UnityEngine;
using UnityEngine.AI;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>NavMesh path checks, wall clearance and wander point sampling for enemies.</summary>
    public class EnemyNavPathfinder
    {
        #region FieldProperties
        private NavMeshAgent agent;
        private NavMeshPath path;

        private LayerMask wallLayer;

        private float fishClearanceRadius;
        private int maxSampleTries;
        private float nearSearchRadius;
        private int nearSearchSamples;
        private float nearSearchStep;

        private float edgeMargin;
        private float reachTolerance;
        private bool debugNavigation;
        #endregion

        #region Initialization
        public EnemyNavPathfinder(NavMeshAgent agent, LayerMask wallLayer, float fishClearanceRadius, int maxSampleTries, float nearSearchRadius = 5f, int nearSearchSamples = 24, float nearSearchStep = 0.5f, float edgeMargin = 0f, float reachTolerance = 1f, bool debugNavigation = false)
        {
            this.agent = agent;
            this.wallLayer = wallLayer;
            this.fishClearanceRadius = fishClearanceRadius;
            this.maxSampleTries = maxSampleTries;
            this.nearSearchRadius = nearSearchRadius;
            this.nearSearchSamples = nearSearchSamples;
            this.nearSearchStep = nearSearchStep;
            this.edgeMargin = edgeMargin;
            this.reachTolerance = reachTolerance;
            this.debugNavigation = debugNavigation;
            path = new NavMeshPath();
        }
        #endregion

        public bool CanFishFitAt(Vector3 point)
        {
            Collider2D hit = Physics2D.OverlapCircle(point, fishClearanceRadius, wallLayer);
            return hit == null;
        }

        public bool IsPathComplete(Vector3 target)
        {
            if (agent == null || !agent.isOnNavMesh)
                return false;

            path ??= new NavMeshPath();

            bool hasPath = agent.CalculatePath(target, path);
            return hasPath && path.status == NavMeshPathStatus.PathComplete;
        }

        #region Resolve
        /// <summary>
        /// Projects a world point onto the NavMesh, keeps it away from the border and checks the path once.
        /// Position is always the best valid mesh point available (the path end for Approximate).
        /// </summary>
        public ReachResult Resolve(Vector3 target)
        {
            ReachResult result = ResolveInternal(target);
            DrawDebug(result);
            return result;
        }

        /// <summary>Sets the agent destination to a resolved point. Unreachable results are refused unless allowUnreachable is set (swim to the nearest mesh point).</summary>
        public bool MoveTo(in ReachResult result, bool allowUnreachable = false)
        {
            if (agent == null || !agent.isOnNavMesh)
                return false;

            if (result.Status == ReachStatus.Unreachable && !allowUnreachable)
                return false;

            return agent.SetDestination(result.Position);
        }

        private ReachResult ResolveInternal(Vector3 target)
        {
            if (agent == null || !agent.isOnNavMesh)
                return ReachResult.Unreachable(target);

            if (!TryProjectWide(target, out Vector3 projected))
                return ReachResult.Unreachable(target);

            Vector3 inset = InsetFromEdge(projected);
            ReachResult result = CheckPath(inset, projected, target);

            // Narrow passage: the inset point may be unreachable although the plain projection is fine.
            bool wasInset = (inset - projected).sqrMagnitude > 0.0001f;
            if (wasInset && result.Status != ReachStatus.Reachable)
            {
                ReachResult plain = CheckPath(projected, projected, target);
                if (plain.Status < result.Status)
                    result = plain;
            }

            return result;
        }

        // One path query. The gap is measured from the plain projection to the original target,
        // so the inset shift does not make a nearby player look unreachable.
        private ReachResult CheckPath(Vector3 point, Vector3 projected, Vector3 target)
        {
            path ??= new NavMeshPath();

            if (!agent.CalculatePath(point, path))
                return ReachResult.Classify(NavMeshPathStatus.PathInvalid, point, target, reachTolerance);

            ReachResult classified = ReachResult.Classify(path.status, projected, target, reachTolerance);

            Vector3 position = point;
            if (path.status == NavMeshPathStatus.PathPartial && path.corners.Length > 0)
                position = path.corners[path.corners.Length - 1];

            return new ReachResult(classified.Status, position, classified.Gap);
        }

        // Pushes a point inward when it sits closer to the NavMesh border than edgeMargin.
        private Vector3 InsetFromEdge(Vector3 point)
        {
            if (edgeMargin <= 0f)
                return point;

            if (!NavMesh.FindClosestEdge(point, out NavMeshHit edge, NavMesh.AllAreas) || edge.distance >= edgeMargin)
                return point;

            Vector3 pushed = point + edge.normal * (edgeMargin - edge.distance);
            if (!NavMesh.SamplePosition(pushed, out NavMeshHit hit, edgeMargin, NavMesh.AllAreas))
                return point;

            // Accept only if it really moved away from the border (guards against a flipped normal).
            if (NavMesh.FindClosestEdge(hit.position, out NavMeshHit after, NavMesh.AllAreas) && after.distance <= edge.distance)
                return point;

            return hit.position;
        }

        // Normal projection first; if the point is far from the mesh (player standing on a ledge), find the nearest mesh point.
        private bool TryProjectWide(Vector3 candidate, out Vector3 navPoint)
        {
            if (TryProjectToNavMesh(candidate, out navPoint))
                return true;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, Mathf.Max(nearSearchRadius, 1f), NavMesh.AllAreas))
            {
                navPoint = hit.position;
                return true;
            }

            navPoint = candidate;
            return false;
        }

        private void DrawDebug(in ReachResult result)
        {
            if (!debugNavigation || agent == null)
                return;

            Color color = result.Status == ReachStatus.Reachable ? Color.green
                : result.Status == ReachStatus.Approximate ? Color.yellow
                : Color.red;

            Debug.DrawLine(agent.transform.position, result.Position, color, 0.3f);
        }
        #endregion

        public bool SetSafeDestination(Vector3 target)
        {
            if (agent == null || !agent.isOnNavMesh)
                return false;

            ReachResult result = Resolve(target);
            if (result.Status != ReachStatus.Reachable || !CanFishFitAt(result.Position))
                return false;

            return agent.SetDestination(result.Position);
        }

        /// <summary>Random reachable point around origin. On failure returns false and point is just origin: callers must not use it.</summary>
        public bool TryPickWanderPoint(Vector3 origin, float radius, out Vector3 point)
        {
            for (int i = 0; i < maxSampleTries; i++)
            {
                Vector2 randomDir = UnityEngine.Random.insideUnitCircle * radius;
                Vector3 candidate = origin + new Vector3(randomDir.x, randomDir.y, 0f);

                ReachResult result = Resolve(candidate);
                if (result.Status != ReachStatus.Reachable || !CanFishFitAt(result.Position))
                    continue;

                point = result.Position;
                return true;
            }

            point = origin;
            return false;
        }

        private bool TryProjectToNavMesh(Vector3 candidate, out Vector3 navPoint)
        {
            float sampleDistance = Mathf.Max(nearSearchStep, fishClearanceRadius * 2f, 0.5f);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
            {
                navPoint = hit.position;
                return true;
            }

            navPoint = candidate;
            return false;
        }

        [Obsolete("Deprecated by Resolve + MoveTo. Remove once nothing calls it.")]
        public bool SetSafeDestinationNear(Vector3 center)
        {
            if (agent == null || !agent.isOnNavMesh)
                return false;

            Vector2 fromCenterToFish = agent.transform.position - center;

            if (fromCenterToFish.sqrMagnitude < 0.01f)
                fromCenterToFish = UnityEngine.Random.insideUnitCircle;

            fromCenterToFish.Normalize();

            for (float radius = nearSearchStep; radius <= nearSearchRadius; radius += nearSearchStep)
            {
                Vector3 candidate = center + (Vector3)(fromCenterToFish * radius);

                if (CanFishFitAt(candidate) && IsPathComplete(candidate))
                {
                    agent.SetDestination(candidate);
                    return true;
                }
            }

            for (int i = 0; i < nearSearchSamples; i++)
            {
                float angle = (360f / nearSearchSamples) * i;
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                for (float radius = nearSearchStep; radius <= nearSearchRadius; radius += nearSearchStep)
                {
                    Vector3 candidate = center + (Vector3)(dir * radius);

                    if (!CanFishFitAt(candidate) || !IsPathComplete(candidate))
                        continue;

                    agent.SetDestination(candidate);
                    return true;
                }
            }

            return false;
        }
    }
}
