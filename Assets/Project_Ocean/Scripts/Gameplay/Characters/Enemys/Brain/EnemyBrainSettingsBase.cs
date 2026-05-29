using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Testing
{
    /// <summary>
    /// Abstract base ScriptableObject shared by all enemy brain settings.
    /// Contains every field that is common to ALL enemy types.
    /// </summary>
    public abstract class EnemyBrainSettingsBase : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Normal movement speed used while wandering or following.")]
        public float speed = 2f;

        [Header("Vision")]
        [Tooltip("Maximum distance at which this enemy can detect the player.")]
        public float viewDistance = 6f;

        [Tooltip("Vision cone full angle in degrees, centered on the current move direction.")]
        public float viewAngle = 90f;

        [Tooltip("Layers that block vision. If empty, wallLayer is used instead.")]
        [FormerlySerializedAs("obstacleLayer")]
        public LayerMask visionObstacleMask;

        [Tooltip("Print detection messages when the player enters radius, is seen, or is blocked.")]
        public bool debugVision;

        [Header("Wall Avoidance")]
        [Tooltip("Layers treated as walls or blocked space.")]
        public LayerMask wallLayer;

        [Tooltip("Distance used for wall detection raycasts in front of the enemy.")]
        public float wallDetectDistance = 1f;

        [Header("Wander")]
        [Tooltip("Radius around the enemy used to pick random wander destinations.")]
        public float wanderRadius = 3f;

        [Tooltip("Time in seconds before choosing a new wander point.")]
        public float wanderInterval = 2f;

        [Header("Hit Reaction")]
        [Tooltip("How long being hit keeps this enemy in an alerted or chase state.")]
        public float hitChaseDuration = 5f;

        [Tooltip("If enabled, being hit or colliding with the player forces an alert reaction.")]
        public bool chasePlayerWhenHit = true;

        [Header("Follow")]
        [Tooltip("How long the enemy follows the last known player position after losing sight.")]
        public float followAfterLost = 6f;
    }
}
