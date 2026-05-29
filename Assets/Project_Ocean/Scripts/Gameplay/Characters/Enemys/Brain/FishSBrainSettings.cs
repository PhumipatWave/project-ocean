using UnityEngine;

namespace Game.Testing
{
    [CreateAssetMenu(fileName = "FishSBrainSettings", menuName = "Enemy/Fish S Brain Settings")]
    public class FishSBrainSettings : EnemyBrainSettingsBase
    {
        [Header("Movement (Small Fish)")]
        [Tooltip("How far ahead the fish asks the NavMeshAgent to move each tick.")]
        public float navDestinationDistance = 3f;

        [Header("Wall Avoidance (Small Fish)")]
        [Tooltip("Circle radius used when checking whether the fish has enough clearance.")]
        public float fishClearanceRadius = 0.5f;

        [Tooltip("Distance checked in front of the fish for nearby walls via CircleCast.")]
        public float wallCheckDistance = 0.5f;

        [Tooltip("Maximum attempts used when sampling a safe wander point.")]
        public int maxWanderSampleTries = 20;

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

        [Header("Wall Avoid (Flock)")]
        [Tooltip("Strength of the steering force applied away from detected walls.")]
        public float wallAvoidWeight = 3f;

        [Header("Search")]
        [Tooltip("How long the fish searches the area after losing sight of the player.")]
        public float searchDuration = 4f;

        [Tooltip("Radius around the last seen player position used while searching.")]
        public float searchRadius = 2.5f;

        [Header("Prepare (Scatter)")]
        [Tooltip("Time spent pausing before scattering away from the player.")]
        public float prepareTime = 1f;

        [Header("Near-Wall Sampling")]
        [Tooltip("Radius used to find a reachable destination near the player.")]
        public float nearPlayerSearchRadius = 5f;

        [Tooltip("Number of sample directions checked around the player.")]
        public int nearPlayerSearchSamples = 24;

        [Tooltip("Distance between sample rings when searching around the player.")]
        public float nearPlayerSearchStep = 0.5f;

        public float detectDistance => viewDistance;
    }
}
