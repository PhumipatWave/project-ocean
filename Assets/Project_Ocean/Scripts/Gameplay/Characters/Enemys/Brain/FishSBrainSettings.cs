using UnityEngine;

[CreateAssetMenu(fileName = "FishSBrainSettings", menuName = "Project Ocean/Enemy/Fish Small Brain Settings")]
public class FishSBrainSettings : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Normal movement speed used by the small fish while wandering or flocking.")]
    public float speed = 2.5f;

    [Tooltip("How far ahead the fish asks the NavMeshAgent to move each tick.")]
    public float navDestinationDistance = 3f;

    [Header("Flock")]
    [Tooltip("Radius used to find nearby small fish in the same group.")]
    public float groupRadius = 3f;

    [Tooltip("Minimum preferred distance from nearby fish before separation pushes away.")]
    public float separationDistance = 1f;

    [Tooltip("How often nearby flock members are refreshed, in seconds.")]
    public float neighborUpdateInterval = 0.2f;

    [Header("Vision")]
    [Tooltip("Maximum distance at which the fish can notice the player.")]
    public float viewDistance = 6f;

    [Tooltip("Vision cone angle in degrees, centered on the current move direction.")]
    public float viewAngle = 90f;

    [Header("Wander")]
    [Tooltip("Radius around the fish used to pick random wander points.")]
    public float wanderRadius = 3f;

    [Tooltip("Time in seconds before choosing a new wander target.")]
    public float wanderInterval = 3f;

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
    [Tooltip("Distance used for raycasts that detect walls in front of the fish.")]
    public float wallDetectDistance = 2f;

    [Tooltip("Strength of steering away from detected walls.")]
    public float wallAvoidWeight = 3f;

    [Tooltip("Layers treated as walls or obstacles by the small fish.")]
    public LayerMask wallLayer;

    [Header("Hit")]
    [Tooltip("How long the fish keeps fleeing after being hit or startled.")]
    public float hitChaseDuration = 5f;
}
