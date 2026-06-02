using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
{
    /// <summary>
    /// Calculates boid flocking forces (separation, alignment, cohesion) and feeds
    /// the result to the NavMeshAgent as a destination.  The NavAgent stays in full
    /// control of actual movement — Boid only influences *where* it wants to go.
    ///
    /// Boid steering is automatically paused while the brain FSM is in any
    /// non-Wander state (Follow / Flee / Attack), giving the brain full control.
    /// </summary>
    public class Boid : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        //  Inspector
        // ─────────────────────────────────────────────
        [Header("Neighbourhood")]
        public float neighborRadius    = 3f;
        public float separationDistance = 1.5f;

        [Header("Steering weights")]
        public float coeffSeparation = 1.2f;
        public float coeffAlignment  = 0.5f;
        public float coeffCohesion   = 0.3f;

        [Header("Destination projection")]
        [Tooltip("How far ahead of the fish the computed steering target is placed.")]
        public float steeringLookAhead = 1.5f;

        // ─────────────────────────────────────────────
        //  Runtime (read by neighbours for alignment)
        // ─────────────────────────────────────────────
        // ─────────────────────────────────────────────
        //  Flock identity
        // ─────────────────────────────────────────────
        /// <summary>Matches the fishId on EnemyBaseProfile. Written by EnemyNavAgentBase at Start.</summary>
        [HideInInspector] public string fishId;

        /// <summary>The leader that claimed this boid. Null means unclaimed.</summary>
        [HideInInspector] public FlockLeader assignedLeader;

        
[HideInInspector] public Vector2 velocity;

        private NavMeshAgent navAgent;
        private FlockManager myFlock;
        private bool         boidPaused;

        // ─────────────────────────────────────────────
        //  Unity loop — auto-register with scene FlockManager
        // ─────────────────────────────────────────────
        private void Start()
        {
            FlockManager.Instance?.Register(this);
        }   // true while FSM brain is in control

        // ─────────────────────────────────────────────
        //  Wiring — called by EnemyNavAgentBase.Start()
        // ─────────────────────────────────────────────
        /// <summary>Connect the boid to its agent and flock registry.</summary>
        /// <summary>Swap flock membership when a FlockLeader claims this boid.</summary>
        public void AssignToLeader(FlockLeader leader)
        {
            assignedLeader = leader;
            myFlock        = leader.GroupFlock;
        }

        

        public void Initialize(NavMeshAgent agent, FlockManager flock)
        {
            navAgent = agent;
            myFlock  = flock;
        }

        /// <summary>Pause boid steering (brain FSM takes over — Follow / Flee / Attack).</summary>
        public void PauseBoidSteering()  => boidPaused = true;

        /// <summary>Resume boid steering (FSM returned to Wander).</summary>
        public void ResumeBoidSteering() => boidPaused = false;

        public bool IsPaused => boidPaused;

        // ─────────────────────────────────────────────
        //  Unity loop
        // ─────────────────────────────────────────────
        private void Update()
        {
            // Keep velocity cache current so neighbours can read it even while paused.
            if (navAgent != null)
                velocity = navAgent.velocity;

            if (boidPaused)            return;
            if (navAgent == null)      return;
            if (!navAgent.isOnNavMesh) return;
            if (myFlock == null)       return;

            List<Boid> neighbors = myFlock.GetNeighbors(this);

            Vector2 steer =
                CalculateSeparation(neighbors) * coeffSeparation +
                CalculateAlignment(neighbors)  * coeffAlignment  +
                CalculateCohesion(neighbors)   * coeffCohesion;

            // Convert the steering vector into a NavAgent destination.
            Vector3 steerTarget = transform.position + (Vector3)(steer.normalized * steeringLookAhead);
            navAgent.SetDestination(steerTarget);
        }

        // ─────────────────────────────────────────────
        //  Boid forces
        // ─────────────────────────────────────────────
        private Vector2 CalculateSeparation(List<Boid> neighbors)
        {
            Vector2 move  = Vector2.zero;
            int     count = 0;

            foreach (var n in neighbors)
            {
                float dist = Vector2.Distance(transform.position, n.transform.position);
                if (dist < separationDistance && dist > 0.001f)
                {
                    move += (Vector2)(transform.position - n.transform.position).normalized / dist;
                    count++;
                }
            }
            return count > 0 ? move / count : Vector2.zero;
        }

        private Vector2 CalculateAlignment(List<Boid> neighbors)
        {
            if (neighbors.Count == 0) return Vector2.zero;

            Vector2 avg = Vector2.zero;
            foreach (var n in neighbors) avg += n.velocity;
            avg /= neighbors.Count;
            return avg - velocity;
        }

        private Vector2 CalculateCohesion(List<Boid> neighbors)
        {
            if (neighbors.Count == 0) return Vector2.zero;

            Vector2 avg = Vector2.zero;
            foreach (var n in neighbors) avg += (Vector2)n.transform.position;
            avg /= neighbors.Count;
            return avg - (Vector2)transform.position;
        }
    }
}
