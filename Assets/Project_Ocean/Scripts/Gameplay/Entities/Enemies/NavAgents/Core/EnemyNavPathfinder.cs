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
        #endregion

        #region Initialization
        public EnemyNavPathfinder(NavMeshAgent agent, LayerMask wallLayer, float fishClearanceRadius, int maxSampleTries, float nearSearchRadius = 5f, int nearSearchSamples = 24, float nearSearchStep = 0.5f)
        {
            this.agent = agent;
            this.wallLayer = wallLayer;
            this.fishClearanceRadius = fishClearanceRadius;
            this.maxSampleTries = maxSampleTries;
            this.nearSearchRadius = nearSearchRadius;
            this.nearSearchSamples = nearSearchSamples;
            this.nearSearchStep = nearSearchStep;
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

        public bool SetSafeDestination(Vector3 target)
        {
            if (agent == null || !agent.isOnNavMesh)
                return false;

            if (!CanFishFitAt(target) || !IsPathComplete(target))
                return false;

            return agent.SetDestination(target);
        }

        public bool TryPickWanderPoint(Vector3 origin, float radius, out Vector3 point)
        {
            for (int i = 0; i < maxSampleTries; i++)
            {
                Vector2 randomDir = Random.insideUnitCircle * radius;
                Vector3 candidate = origin + new Vector3(randomDir.x, randomDir.y, 0f);

                if (!TryProjectToNavMesh(candidate, out Vector3 navPoint))
                    continue;

                if (!CanFishFitAt(navPoint) || !IsPathComplete(navPoint))
                    continue;

                point = navPoint;
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

        public bool SetSafeDestinationNear(Vector3 center)
        {
            if (agent == null || !agent.isOnNavMesh)
                return false;

            Vector2 fromCenterToFish = agent.transform.position - center;

            if (fromCenterToFish.sqrMagnitude < 0.01f)
                fromCenterToFish = Random.insideUnitCircle;

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
