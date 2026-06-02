using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <FlockManager>
    /// Scene-level singleton registry of every Boid.
    /// </FlockManager>
    public class FlockManager : MonoBehaviour
    {
        [SerializeField] private readonly List<Boid> members = new List<Boid>();

        public static FlockManager Instance { get; private set; }
        #region UnityCallbacks
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        #endregion

        public void Register(Boid boid)
        {
            if (boid != null && !members.Contains(boid))
                members.Add(boid);
        }

        public void Deregister(Boid boid)
        {
            if (boid == null) 
                return;

            members.Remove(boid);
            boid.assignedLeader?.RemoveMember(boid);
        }

        public List<Boid> GetUnclaimedByFishId(string fishId, Vector3 origin, float radius)
        {
            var result = new List<Boid>();
            foreach (var b in members)
            {
                if (b == null || b.fishId != fishId || b.assignedLeader != null || Vector2.Distance(origin, b.transform.position) > radius)                
                    continue;

                result.Add(b);
            }
            return result;
        }

        public List<Boid> GetNeighbors(Boid neighborFish)
        {
            var result = new List<Boid>(members.Count);
            foreach (var member in members)
            {
                if (member == null || member == neighborFish) continue;
                if (Vector2.Distance(neighborFish.transform.position, member.transform.position)
                    < neighborFish.neighborRadius)
                {
                    result.Add(member);
                }
            }
            return result;
        }

        /// <AlertAll>
        /// Trigger OnBulletHit on every member's brain.
        /// </AlertAll>
        public void AlertAll()
        {
            foreach (var m in members)
            {
                if (m == null) continue;
                var agent = m.GetComponent<EnemyNavAgentBase>();
                if (agent == null) continue;
                (agent.ActiveBrain as IBrainHitReaction)?.OnBulletHit();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (members == null) return;
            Gizmos.color = Color.cyan;
            foreach (var m in members)
            {
                if (m == null) continue;
                Gizmos.DrawWireSphere(m.transform.position, m.neighborRadius);
            }
        }
    }
}
