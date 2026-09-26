using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
{
    /// <summary>
    /// Shared references passed from the nav host into every enemy brain.
    /// </summary>
    public class EnemyBrainContext
    {
        public Enemy Owner;
        public Transform Transform;
        public NavMeshAgent Agent;
        public EnemyFacingController Facing;
        public FishAlertUI AlertUI;
        public MonoBehaviour Host;
        public Boid School;

        public Player Player => Owner != null ? Owner.Player : null;
        public Transform PlayerTransform => Player != null ? Player.transform : null;

        public bool IsDead => Owner != null && Owner.IsDead;
    }
}
