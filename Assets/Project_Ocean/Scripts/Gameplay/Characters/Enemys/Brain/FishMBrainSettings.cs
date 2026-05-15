using UnityEngine;

[CreateAssetMenu(fileName = "FishMBrainSettings", menuName = "Project Ocean/Enemy/Fish Medium Brain Settings")]
public class FishMBrainSettings : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Normal movement speed used while wandering or chasing the player.")]
    public float speed = 2.5f;

    [Header("Attack")]
    [Tooltip("Distance from the player required before this fish can bite.")]
    public float attackRange = 1.5f;

    [Tooltip("Time in seconds before another bite can happen.")]
    public float attackCooldown = 2f;

    [Tooltip("Damage dealt to the player when the bite connects.")]
    public float attackDamage = 7.5f;

    [Header("Wander")]
    [Tooltip("Radius around the fish used to choose random wander destinations.")]
    public float wanderRadius = 3f;

    [Tooltip("Time in seconds before choosing a new wander destination.")]
    public float wanderChangeInterval = 2f;

    [Header("Wall Avoid")]
    [Tooltip("Distance used for forward wall detection raycasts.")]
    public float wallDetectDistance = 1f;

    [Tooltip("Layers treated as walls or blocking geometry.")]
    public LayerMask wallLayer;

    [Header("Vision")]
    [Tooltip("Maximum distance at which this fish can detect the player.")]
    public float viewDistance = 6f;

    [Tooltip("Vision cone angle in degrees.")]
    public float viewAngle = 90f;

    [Tooltip("Layers that block line of sight to the player.")]
    public LayerMask obstacleLayer;

    [Header("Hit Reaction")]
    [Tooltip("How long this fish keeps chasing after being hit or touching the player.")]
    public float hitChaseDuration = 5f;

    [Tooltip("If enabled, being hit or colliding with the player forces a chase reaction.")]
    public bool chasePlayerWhenHit = true;
}
