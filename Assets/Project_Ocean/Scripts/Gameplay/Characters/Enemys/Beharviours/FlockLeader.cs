using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
{
    /// <FlockLeader>
    /// Attach to a leader fish prefab alongside EnemyNavAgentBase + Boid.
    /// </FlockLeader>
    [RequireComponent(typeof(Boid))]
    public class FlockLeader : MonoBehaviour
    {
        #region FieldProperties
        [Header("Identity")]
        [SerializeField] private EnemyAgent enemyAgent;

        [Header("Claim settings")]
        [Tooltip("Radius within which this leader will claim unclaimed same-species fish.")]
        [SerializeField] private float claimRadius = 10f;

        [Tooltip("How often (seconds) the leader scans for unclaimed fish.")]
        [SerializeField] private float scanInterval = 0.5f;

        private FlockManager groupFlock;
        public FlockManager GroupFlock { get { return groupFlock; } }

        public List<Boid> MyMembers { get; } = new List<Boid>();

        private float scanTimer;
        #endregion

        #region UnityCallbacks
        private void Start()
        {
            // Create a private FlockManager for this leader's group so that
            // Boid.Update() GetNeighbors() only sees same-group fish.
            groupFlock = gameObject.AddComponent<FlockManager>();

            var leaderBoid = GetComponent<Boid>();
            if (leaderBoid != null)
            {
                leaderBoid.assignedLeader = this;
                GroupFlock.Register(leaderBoid);
                MyMembers.Add(leaderBoid);
            }

            scanTimer = scanInterval;
        }

        private void Update()
        {
            scanTimer -= Time.deltaTime;

            if (scanTimer > 0f) 
                return;

            scanTimer = scanInterval;
            ClaimNearbyFish();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, claimRadius);
        }
        #endregion

        private void ClaimNearbyFish()
        {
            if (FlockManager.Instance == null) 
                return;

            string id = enemyAgent.BaseProfile != null ? enemyAgent.BaseProfile.fishId : string.Empty;

            if (string.IsNullOrEmpty(id)) 
                return;

            List<Boid> unclaimedFish = FlockManager.Instance.GetUnclaimedByFishId(
                id, transform.position, claimRadius);

            foreach (var fish in unclaimedFish)
            {
                fish.AssignToLeader(this);
                GroupFlock.Register(fish);
                MyMembers.Add(fish);
            }
        }

        /// <RemoveMember>
        /// Called by FlockManager.Deregister when a member fish dies.
        /// </RemoveMember>
        public void RemoveMember(Boid member)
        {
            MyMembers.Remove(member);
            GroupFlock.Deregister(member);
        }

        /// <AlertAll>
        /// Trigger OnBulletHit on every member in this leader's group.
        /// </AlertAll>
        public void AlertAll()
        {
            foreach (var member in MyMembers)
            {
                if (member == null) continue;
                var agent = member.GetComponent<EnemyNavAgentBase>();
                if (agent == null) continue;
                (agent.ActiveBrain as IBrainHitReaction)?.OnBulletHit();
            }
        }
    }
}
