using UnityEngine;

namespace Game.Testing
{
    [CreateAssetMenu(fileName = "FishSBrainSettings", menuName = "Enemy/Fish S Brain Settings")]
    public class FishSBrainSettings : EnemyBrainSettingsBase
    {
        [Header("Movement (Small Fish)")]
        [Tooltip("How far ahead the fish asks the NavMeshAgent to move each tick.")]
        public float navDestinationDistance = 3f;

        [Header("Flock")]
        [Tooltip("Radius used to find nearby small fish in the same group.")]
        public float groupRadius = 3f;

        [Tooltip("Minimum preferred distance from nearby fish before separation pushes away.")]
        public float separationDistance = 1f;

        [Tooltip("How often nearby flock members are refreshed, in seconds.")]
        public float neighborUpdateInterval = 0.2f;

        [Header("Flocking Weights")]
        [Tooltip("Strength of steering toward the flock center.")]
        public float cohesionWeight = 1f;

        [Tooltip("Strength of steering away from nearby fish.")]
        public float separationWeight = 1.5f;

        [Tooltip("Strength of matching nearby fish movement direction.")]
        public float alignmentWeight = 0.5f;

        [Header("Panic Flee")]
        [Tooltip("Speed multiplier applied while fleeing from the player or bullets.")]
        public float panicSpeedMultiplier = 2.5f;

        [Tooltip("How long panic fleeing lasts after being triggered.")]
        public float panicDuration = 3f;

        [Tooltip("Random angle range added to panic direction so fish do not flee identically.")]
        public float panicRandomAngle = 60f;

        [Header("Wall Avoid")]
        [Tooltip("Strength of the steering force applied away from detected walls.")]
        public float wallAvoidWeight = 3f;
    }
}