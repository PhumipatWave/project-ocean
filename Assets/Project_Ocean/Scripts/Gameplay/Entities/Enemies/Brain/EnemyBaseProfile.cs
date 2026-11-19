using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Settings shared by every enemy brain profile.</summary>
    public abstract class EnemyBaseProfile : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Fish species ID (kills, quests, spawn lookups).")]
        public string fishId;

        [Header("School")]
        [Tooltip("Min followers EnemySpawner spawns with each of these fish (same prefab). Needs a Boid component on the prefab. 0 = no school.")]
        [Min(0)] public int schoolMinFollowers;

        [Tooltip("Max followers per school. 0 = no school.")]
        [Min(0)] public int schoolMaxFollowers;
        
        [Header("Movement")]
        [Tooltip("Normal movement speed used while wandering or following.")]
        public float speed = 2f;

        [Header("Vision")]
        [Tooltip("Maximum distance at which this enemy can detect the player.")]
        public float viewDistance = 6f;

        [Tooltip("Vision cone full angle in degrees, centered on the current move direction.")]
        public float viewAngle = 90f;

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
    

        [Header("Pathfinding")]
        [Tooltip("Clearance radius used when sampling NavMesh positions.")]
        public float fishClearanceRadius = 0.5f;

        [Tooltip("Maximum attempts to find a valid wander point.")]
        public int maxWanderSampleTries = 20;

        [Header("Near-Wall Sampling")]
        [Tooltip("Radius used when searching for a position near the player around walls.")]
        public float nearPlayerSearchRadius = 5f;

        [Tooltip("Number of sample points cast around the player when searching near walls.")]
        public int nearPlayerSearchSamples = 24;

        [Tooltip("Step size between concentric sample rings near the player.")]
        public float nearPlayerSearchStep = 0.5f;

        [Header("Navigation")]
        [Tooltip("Destinations are pushed at least this far in from the NavMesh border. Keep it >= Fish Clearance Radius.")]
        [Min(0f)] public float edgeMargin = 0.5f;

        [Tooltip("A target farther than this from the nearest NavMesh point counts as unreachable.")]
        [Min(0f)] public float reachTolerance = 1f;

        [Tooltip("Seconds between path queries while chasing, searching, wandering or fleeing (instead of every frame).")]
        [Min(0.05f)] public float repathInterval = 0.25f;

        [Tooltip("Time window used to detect a fish that is not making progress.")]
        [Min(0.1f)] public float stuckWindow = 1.5f;

        [Tooltip("Minimum distance a fish must cover within the stuck window, otherwise it picks a new target.")]
        [Min(0f)] public float stuckMinProgress = 0.3f;

        [Tooltip("Draw reach checks in the Scene view (green = reachable, yellow = approximate, red = unreachable).")]
        public bool debugNavigation;

        [Header("Contact Damage")]
        [Tooltip("Damage dealt to the player on body contact. 0 = no contact damage.")]
        public float contactDamage = 0f;

        [Tooltip("Minimum seconds between contact damage hits.")]
        public float contactDamageCooldown = 1f;

        [Header("Corpse")]
        [Tooltip("Seconds after death before auto-returning to pool.")]
        public float autoReturnDelay = 60f;

        [Header("Agent")]
        [Tooltip("Tick facing from NavMesh agent velocity each frame (for NavMesh-driven enemies without FSM-managed facing).")]
        public bool tickFacingFromVelocity = false;

        public virtual bool RequiresNavMesh => true;
        public virtual IEnemyBrain CreateBrain() => new EnemyBrain(this);
    }
}
