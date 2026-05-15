using UnityEngine;

[CreateAssetMenu(fileName = "FishLBrainSettings", menuName = "Project Ocean/Enemy/Fish Large Brain Settings")]
public class FishLBrainSettings : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Normal movement speed used while wandering, searching, or following.")]
    public float speed = 2f;

    [Tooltip("High movement speed used during committed attack movement.")]
    public float boostSpeed = 7f;

    [Header("Vision")]
    [Tooltip("Vision cone angle in degrees while checking if the player is visible.")]
    public float viewAngle = 90f;

    [Tooltip("Layers that block vision. If empty, the wall layer is used instead.")]
    public LayerMask visionObstacleMask;

    [Tooltip("Print Fish L detection messages when the player enters radius, is seen, or is blocked by line of sight.")]
    public bool debugVision;

    [Header("Wall Avoidance")]
    [Tooltip("Layers treated as walls or blocked space by the large fish.")]
    public LayerMask wallLayer;

    [Tooltip("Circle radius used when checking whether the fish has enough clearance.")]
    public float fishClearanceRadius = 0.8f;

    [Tooltip("Distance checked in front of the fish for nearby walls.")]
    public float wallCheckDistance = 0.8f;

    [Tooltip("Maximum attempts used when sampling a safe wander point.")]
    public int maxWanderSampleTries = 20;

    [Header("Wander")]
    [Tooltip("Radius around the fish used to pick normal wander destinations.")]
    public float wanderRadius = 3f;

    [Tooltip("Time in seconds before choosing a new wander point.")]
    public float wanderChangeInterval = 2f;

    [Header("Search")]
    [Tooltip("How long the fish searches after losing the player.")]
    public float searchDuration = 4f;

    [Tooltip("Radius around the last seen player position used while searching.")]
    public float searchRadius = 2.5f;

    [Header("Attack")]
    [Tooltip("Maximum distance at which the large fish can detect the player.")]
    public float detectDistance = 5f;

    [Tooltip("Distance from the player required for the attack to deal damage.")]
    public float attackRange = 1.5f;

    [Tooltip("Time spent aiming at the player before the attack begins.")]
    public float prepareTime = 2f;

    [Tooltip("Time in seconds before the fish can attack again after a hit.")]
    public float attackCooldown = 2f;

    [Tooltip("How long the fish follows the last known player position after losing sight.")]
    public float followAfterLost = 6f;

    [Tooltip("Damage dealt to the player when the attack connects.")]
    public float attackDamage = 10.25f;

    [Header("Hit Reaction")]
    [Tooltip("How long being hit keeps the fish interested in the attacker.")]
    public float hitChaseDuration = 5f;

    [Tooltip("If enabled, being hit forces the fish to chase or follow the attacker.")]
    public bool chasePlayerWhenHit = true;

    [Header("Near Wall Attack")]
    [Tooltip("Radius used to find a reachable attack destination near the player.")]
    public float nearPlayerSearchRadius = 5f;

    [Tooltip("Number of sample directions checked around the player for safe attack movement.")]
    public int nearPlayerSearchSamples = 24;

    [Tooltip("Distance between sample rings when searching around the player.")]
    public float nearPlayerSearchStep = 0.5f;
}
