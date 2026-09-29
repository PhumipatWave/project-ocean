using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Game.Core;

namespace Game.Gameplay
{
    [Serializable]
    public class ZoneSpawnEntry
    {
        [Tooltip("Must match an ID in EnemySpawnManager.allEnemyEntries.")]
        public string enemyId;

        [Tooltip("Relative spawn chance for this zone. Higher = more likely to be picked.")]
        [Min(0f)] public float weight = 1f;
    }

    /// Place one EnemySpawner per zone in the Ocean scene.
    /// References enemy types by ID — prefabs live in EnemySpawnManager.
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Zone Spawn Entries")]
        [Tooltip("Which enemy types spawn here and their relative weights. IDs must match EnemySpawnManager.")]
        [SerializeField] private List<ZoneSpawnEntry> spawnEntries;

        [Tooltip("This zone only spawns once GameFeature.EnemySpawnLevel<N> is unlocked (1-3) — see FeatureUnlockService.")]
        [Range(1, 3)]
        [SerializeField] private int requiredLevel = 1;

        [Header("Spawn Settings")]
        [Tooltip("How many enemies to spawn in this zone.")]
        [SerializeField] private int enemyCount = 5;

        [Tooltip("Radius of the circle area enemies spawn randomly within.")]
        [SerializeField] private float spawnRadius = 5f;

        [Tooltip("How far from a candidate position to search for a valid NavMesh point.")]
        [SerializeField] private float navSampleDistance = 2f;

        [Tooltip("Max random attempts per slot before giving up on that slot.")]
        [SerializeField] private int maxSpawnAttempts = 10;

        [Header("Spawn Clearance")]
        [Tooltip("Overlap radius used to reject positions inside walls.")]
        [SerializeField] private float spawnClearanceRadius = 0.3f;

        [Tooltip("Layers counted as obstacles for the clearance check.")]
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Ground Snap")]
        [Tooltip("Ground/floor layer to raycast against. NavMesh height can drift from the real floor collider, so we snap to this after sampling. Same tilemap collider as 'Wall' (Environments/Grid/Main) — there is no separate ground layer.")]
        [SerializeField] private LayerMask groundLayer = 1 << 9;

        [Tooltip("How far above the sampled point to start the downward ground raycast.")]
        [SerializeField] private float groundCheckUpOffset = 1f;

        [Tooltip("How far below the start point to search for ground.")]
        [SerializeField] private float groundCheckDistance = 20f;

        [Tooltip("Gravity-driven enemies (e.g. crabs) only spawn where they'd start resting on the ground: the spawn point may be at most this far above it. Swimmers are unaffected.")]
        [SerializeField] private float maxGroundGap = 0.6f;

        [Header("Proximity Activation")]
        [Tooltip("Spawn enemies when the player enters this radius from the zone centre.")]
        [SerializeField] private float activateRadius = 30f;

        [Tooltip("How often (seconds) to check player distance.")]
        [SerializeField] private float proximityCheckInterval = 0.4f;

        // School followers spawn within this radius of their leader.
        private const float SchoolSpawnRadius = 2f;

        private readonly List<GameObject> _spawnedEnemies = new();
        private bool _isActive;

        private void OnEnable()  => GameObjectSpawnManager.Instance?.RegisterSpawner(this);
        private void OnDisable() => GameObjectSpawnManager.Instance?.UnregisterSpawner(this);

        private void Start()
        {
            GameObjectSpawnManager.Instance?.RegisterSpawner(this);
            StartCoroutine(ProximityCheckRoutine());
        }

        // ── Public API ──

        public void ResetZone()
        {
            foreach (var go in _spawnedEnemies)
                if (go != null) ObjectPoolManager.Instance?.Return(go);

            _spawnedEnemies.Clear();
            _isActive = false;
        }

        // ── Internal ──

        private IEnumerator ProximityCheckRoutine()
        {
            var wait = new WaitForSeconds(proximityCheckInterval);
            while (true)
            {
                CheckProximity();
                yield return wait;
            }
        }

        private static readonly GameFeature[] LevelFeatures =
        {
            GameFeature.EnemySpawnLevel1, GameFeature.EnemySpawnLevel2, GameFeature.EnemySpawnLevel3,
        };

        private void CheckProximity()
        {
            if (_isActive) return;
            if (!FeatureUnlockService.Instance.IsUnlocked(LevelFeatures[requiredLevel - 1])) return;

            var player = PlayerRegistry.Local;
            if (player == null) return;

            float sqrDist = ((Vector2)(player.transform.position - transform.position)).sqrMagnitude;
            if (sqrDist <= activateRadius * activateRadius)
            {
                _isActive = true;
                SpawnEnemies();
            }
        }

        private void SpawnEnemies()
        {
            _spawnedEnemies.Clear();

            if (spawnEntries == null || spawnEntries.Count == 0)
            {
                Debug.LogWarning($"[EnemySpawner] '{name}' has no spawn entries assigned.");
                return;
            }

            if (EnemyKillTracker.Instance == null)
                Debug.LogError($"[EnemySpawner] EnemyKillTracker.Instance is null — killed enemies will respawn.");

            int spawned = 0;
            for (int i = 0; i < enemyCount; i++)
            {
                string spawnId = $"{gameObject.name}_{i}";

                if (EnemyKillTracker.Instance != null && EnemyKillTracker.Instance.IsKilled(spawnId))
                    continue;

                var entry = PickEntry();
                if (entry == null) continue;

                var prefab = GameObjectSpawnManager.Instance?.GetEnemyPrefab(entry.enemyId);
                if (prefab == null) continue;

                if (!TryGetNavMeshPosition(transform.position, spawnRadius, NeedsGround(prefab), out Vector3 spawnPos))
                {
                    Debug.LogWarning($"[EnemySpawner] '{name}' slot {i}: no valid position after {maxSpawnAttempts} attempts — skipped.");
                    continue;
                }

                var go = ObjectPoolManager.Instance != null
                    ? ObjectPoolManager.Instance.Get(prefab, spawnPos, Quaternion.identity)
                    : Instantiate(prefab, spawnPos, Quaternion.identity);

                if (go.TryGetComponent<EnemyNavAgentBase>(out var agent))
                    agent.SetSpawnId(spawnId);

                _spawnedEnemies.Add(go);
                spawned++;

                spawned += SpawnSchool(go, prefab, spawnId);
            }

            GameLog.Info($"[EnemySpawner] '{name}' spawned {spawned} enemies ({enemyCount} slots).");
        }

        // Leader fish whose profile has schoolMaxFollowers > 0: spawn that many of the same prefab
        // around it and link them. Followers don't use up spawner slots.
        private int SpawnSchool(GameObject leader, GameObject prefab, string leaderSpawnId)
        {
            if (!leader.TryGetComponent<EnemyAgent>(out var leaderAgent))
                return 0;

            var profile = leaderAgent.BaseProfile;
            if (profile == null || profile.schoolMaxFollowers <= 0)
                return 0;

            if (!leader.TryGetComponent<Boid>(out var leaderBoid))
            {
                Debug.LogWarning($"[EnemySpawner] '{prefab.name}' has school settings but no Boid component — spawned without a school.", leader);
                return 0;
            }

            int count = UnityEngine.Random.Range(profile.schoolMinFollowers, profile.schoolMaxFollowers + 1);
            int spawned = 0;

            for (int k = 0; k < count; k++)
            {
                string followerId = $"{leaderSpawnId}_f{k}";

                if (EnemyKillTracker.Instance != null && EnemyKillTracker.Instance.IsKilled(followerId))
                    continue;

                // Swimmers skip the ground snap so followers appear beside the leader, not on the floor below it.
                bool needsGround = NeedsGround(prefab);
                if (!TryGetNavMeshPosition(leader.transform.position, SchoolSpawnRadius, needsGround, out Vector3 pos, snapToGround: needsGround))
                    continue;

                var go = ObjectPoolManager.Instance != null
                    ? ObjectPoolManager.Instance.Get(prefab, pos, Quaternion.identity)
                    : Instantiate(prefab, pos, Quaternion.identity);

                if (go.TryGetComponent<EnemyNavAgentBase>(out var followerAgent))
                    followerAgent.SetSpawnId(followerId);

                if (go.TryGetComponent<Boid>(out var followerBoid))
                    leaderBoid.AddFollower(followerBoid);

                _spawnedEnemies.Add(go);
                spawned++;
            }

            return spawned;
        }

        // A dynamic body with gravity walks/falls rather than swims (crab) — spawning it in open
        // water leaves it hanging in the air or dropping from it.
        private static bool NeedsGround(GameObject prefab)
        {
            var rb = prefab.GetComponent<Rigidbody2D>();
            return rb != null && rb.bodyType == RigidbodyType2D.Dynamic && rb.gravityScale > 0f;
        }

        private bool TryGetNavMeshPosition(Vector3 center, float radius, bool requireGround, out Vector3 result, bool snapToGround = true)
        {
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                Vector2 offset    = UnityEngine.Random.insideUnitCircle * radius;
                Vector3 candidate = center + new Vector3(offset.x, offset.y, 0f);

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, navSampleDistance, NavMesh.AllAreas))
                    continue;
                candidate = navHit.position;

                if (obstacleLayer.value != 0 && Physics2D.OverlapCircle(candidate, spawnClearanceRadius, obstacleLayer) != null)
                    continue;

                if (snapToGround && groundLayer.value != 0)
                {
                    Vector2 rayOrigin = (Vector2)candidate + Vector2.up * groundCheckUpOffset;
                    var groundHit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckUpOffset + groundCheckDistance, groundLayer);

                    // Ground collider height can drift slightly from the baked NavMesh height.
                    // Only accept the ground-corrected point if it's still on the NavMesh —
                    // otherwise the agent spawns off-mesh and NEVER ticks (isOnNavMesh stays false).
                    if (groundHit.collider != null)
                    {
                        Vector3 groundCandidate = new(candidate.x, groundHit.point.y, candidate.z);
                        if (NavMesh.SamplePosition(groundCandidate, out NavMeshHit groundNavHit, navSampleDistance, NavMesh.AllAreas))
                            candidate = groundNavHit.position;
                    }

                    // Checked at the FINAL point (the NavMesh snaps above can move it sideways off
                    // the ledge the first ray found): ground must be right underneath, not
                    // somewhere far below in open water.
                    if (requireGround &&
                        Physics2D.Raycast((Vector2)candidate + Vector2.up * 0.05f, Vector2.down, maxGroundGap + 0.05f, groundLayer).collider == null)
                        continue;
                }

                result = candidate;
                return true;
            }

            result = center;
            return false;
        }

        private ZoneSpawnEntry PickEntry()
        {
            float totalWeight = 0f;
            foreach (var e in spawnEntries)
                if (e != null) totalWeight += e.weight;

            if (totalWeight <= 0f) return null;

            float roll       = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var e in spawnEntries)
            {
                if (e == null) continue;
                cumulative += e.weight;
                if (roll <= cumulative) return e;
            }

            return spawnEntries[^1];
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
            Gizmos.DrawWireSphere(transform.position, activateRadius);
        }
    }
}
