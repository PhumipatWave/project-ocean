using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// School link for small fish. EnemySpawner spawns a leader and links 3-5 followers to it.
    /// The leader wanders normally; each follower's Wander state swims to GetFollowTarget()
    /// (leader + own offset + spacing from schoolmates). Any member fleeing makes the school flee.
    /// </summary>
    public class Boid : MonoBehaviour, IPoolable
    {
        [Header("Formation")]
        [Tooltip("Closest a follower's spot around the leader can be.")]
        [SerializeField] private float followRadiusMin = 1f;

        [Tooltip("Farthest a follower's spot around the leader can be.")]
        [SerializeField] private float followRadiusMax = 2.5f;

        [Tooltip("Followers closer than this to a schoolmate aim a little away from it.")]
        public float separationDistance = 1.5f;

        // Guards AlertSchool against re-entry: each alerted member's Flee state calls AlertSchool again.
        private static bool isAlertingSchool;

        private readonly List<Boid> followers = new();
        private Vector2 followOffset;

        public Boid Leader { get; private set; }
        public bool HasLeader => Leader != null;

        public void AddFollower(Boid follower)
        {
            if (follower == null || follower == this || followers.Contains(follower))
                return;

            follower.Leader = this;
            follower.followOffset = Random.insideUnitCircle.normalized * Random.Range(followRadiusMin, followRadiusMax);
            followers.Add(follower);
        }

        /// <summary>Leader position + this follower's spot, nudged away from schoolmates that are too close.</summary>
        public Vector3 GetFollowTarget()
        {
            Vector2 self = transform.position;
            Vector2 target = (Vector2)Leader.transform.position + followOffset;

            foreach (var mate in Leader.followers)
                target += SeparationFrom(mate, self);

            target += SeparationFrom(Leader, self);

            return new Vector3(target.x, target.y, transform.position.z);
        }

        /// <summary>Makes every other school member flee.</summary>
        public void AlertSchool()
        {
            if (isAlertingSchool)
                return;

            Boid leader = Leader != null ? Leader : this;
            isAlertingSchool = true;
            try
            {
                Alert(leader);
                foreach (var member in leader.followers.ToArray())
                    Alert(member);
            }
            finally
            {
                isAlertingSchool = false;
            }
        }

        private void Alert(Boid member)
        {
            if (member == null || member == this || !member.isActiveAndEnabled)
                return;

            if (member.TryGetComponent<EnemyNavAgentBase>(out var agent))
                (agent.ActiveBrain as IBrainHitReaction)?.OnHit();
        }

        private Vector2 SeparationFrom(Boid mate, Vector2 self)
        {
            if (mate == null || mate == this)
                return Vector2.zero;

            Vector2 away = self - (Vector2)mate.transform.position;
            float distance = away.magnitude;

            if (distance >= separationDistance || distance < 0.001f)
                return Vector2.zero;

            return away / distance * (separationDistance - distance);
        }

        // Caught, killed (EntityDeathHandler disables this) or returned to the pool: leave the school.
        private void OnDisable()
        {
            if (Leader != null)
            {
                Leader.followers.Remove(this);
                Leader = null;
                return;
            }

            PromoteNewLeader();
        }

        // The first remaining follower takes over; the rest follow it with their existing spots.
        private void PromoteNewLeader()
        {
            followers.RemoveAll(f => f == null || !f.isActiveAndEnabled);
            if (followers.Count == 0)
                return;

            Boid newLeader = followers[0];
            newLeader.Leader = null;

            for (int i = 1; i < followers.Count; i++)
            {
                followers[i].Leader = newLeader;
                newLeader.followers.Add(followers[i]);
            }

            followers.Clear();
        }

        // ── IPoolable ──

        // Death disables this component; a reused fish must start enabled and unlinked.
        public void OnGetFromPool()
        {
            Leader = null;
            followers.Clear();
            enabled = true;
        }

        public void OnReturnToPool() { }
    }
}
