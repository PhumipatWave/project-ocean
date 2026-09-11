using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
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

        [Header("Proximity Activation")]
        [Tooltip("Spawn enemies when the player enters this radius from the zone centre.")]
        [SerializeField] private float activateRadius = 30f;

        [Tooltip("How often (seconds) to check player distance.")]
        [SerializeField] private float proximityCheckInterval = 0.4f;

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

        private void CheckProximity()
        {
            if (_isActive) return;

            var player = GameManager.CurPlayer;
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

                if (!TryGetNavMeshPosition(out Vector3 spawnPos))
                {
                    Debug.LogWarning($"[EnemySpawner] '{name}' slot {i}: no valid position after {maxSpawnAttempts} attempts — skipped.");
                    continue;
                }

                var go = ObjectPoolManager.Instance != null
                    ? ObjectPoolManager.Instance.Get(prefab, spawnPos, Quaternion.identity)
                    : Instantiate(prefab, spawnPos, Quaternion.identity);

                var agent = go.GetComponent<EnemyNavAgentBase>();
                if (agent != null)
                {
                    agent.SetSpawnId(spawnId);
                    agent.SetFlockId($"{gameObject.name}_{agent.FishId}");
                }

                _spawnedEnemies.Add(go);
                spawned++;
            }

            Debug.Log($"[EnemySpawner] '{name}' spawned {spawned}/{enemyCount} enemies.");
        }

        private bool TryGetNavMeshPosition(out Vector3 result)
        {
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                Vector2 offset    = UnityEngine.Random.insideUnitCircle * spawnRadius;
                Vector3 candidate = transform.position + new Vector3(offset.x, offset.y, 0f);

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, navSampleDistance, NavMesh.AllAreas))
                    continue;
                candidate = navHit.position;

                if (obstacleLayer.value != 0 && Physics2D.OverlapCircle(candidate, spawnClearanceRadius, obstacleLayer) != null)
                    continue;

                if (groundLayer.value != 0)
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
                }

                result = candidate;
                return true;
            }

            result = transform.position;
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
