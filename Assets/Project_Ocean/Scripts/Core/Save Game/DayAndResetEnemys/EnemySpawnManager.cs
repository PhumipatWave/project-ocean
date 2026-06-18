using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
{
    [Serializable]
    public class EnemySpawnEntry
    {
        public GameObject prefab;

        [Tooltip("Relative spawn chance. Higher = more likely to be picked.")]
        [Min(0f)] public float weight = 1f;
    }

    /// Place one EnemySpawnManager per zone in the Ocean scene.
    /// Add child GameObjects as spawn point positions.
    /// Each manager spawns up to enemyCount enemies, choosing prefabs by weighted random.
    /// Enemies of the same type within this zone share the same flock group.
    public class EnemySpawnManager : MonoBehaviour
    {
        [Header("Spawn Entries")]
        [Tooltip("Enemy prefabs and their relative spawn weights.")]
        [SerializeField] private List<EnemySpawnEntry> enemyEntries;

        [Tooltip("How many instances per prefab to pre-create in the pool on Start.")]
        [SerializeField] private int prewarmPerPrefab = 10;

        [Header("Spawn Settings")]
        [Tooltip("How many enemies to spawn in this zone.")]
        [SerializeField] private int enemyCount = 5;

        [Tooltip("Radius of the circle area enemies spawn randomly within.")]
        [SerializeField] private float spawnRadius = 5f;

        [Tooltip("How far from a candidate position to search for a valid NavMesh point.")]
        [SerializeField] private float navSampleDistance = 2f;

        [Tooltip("Max random attempts per slot before giving up on that slot.")]
        [SerializeField] private int maxSpawnAttempts = 10;

        private readonly List<GameObject> spawnedEnemies = new();

        private void OnEnable()  => DayManager.OnEnemyReset += OnDayReset;
        private void OnDisable() => DayManager.OnEnemyReset -= OnDayReset;
        private void Start()
        {
            PrewarmPool();
            SpawnEnemies();
        }

        // ── Internal ──

        private void PrewarmPool()
        {
            if (ObjectPoolManager.Instance == null || enemyEntries == null) return;

            foreach (var entry in enemyEntries)
                if (entry?.prefab != null)
                    ObjectPoolManager.Instance.Prewarm(entry.prefab, prewarmPerPrefab);
        }

        private void OnDayReset()
        {
            foreach (var go in spawnedEnemies)
                if (go != null) ObjectPoolManager.Instance?.Return(go);

            spawnedEnemies.Clear();
            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            spawnedEnemies.Clear();

            if (enemyEntries == null || enemyEntries.Count == 0)
            {
                Debug.LogWarning($"[EnemySpawnManager] '{name}' has no enemy entries assigned.");
                return;
            }

            if (EnemyKillTracker.Instance == null)
                Debug.LogError($"[EnemySpawnManager] EnemyKillTracker.Instance is null — killed enemies will respawn. Add EnemyKillTracker component to your persistent Bootstrap GameObject.");

            int spawned = 0;
            for (int i = 0; i < enemyCount; i++)
            {
                string spawnId = $"{gameObject.name}_{i}";

                if (EnemyKillTracker.Instance != null && EnemyKillTracker.Instance.IsKilled(spawnId))
                {
                    Debug.Log($"[EnemySpawnManager] '{name}' slot {i} skipped (killed this day).");
                    continue;
                }

                var entry = PickEntry();
                if (entry?.prefab == null) continue;

                if (!TryGetNavMeshPosition(out Vector3 spawnPos))
                {
                    Debug.LogWarning($"[EnemySpawnManager] '{name}' slot {i}: no valid NavMesh position found after {maxSpawnAttempts} attempts — skipped.");
                    continue;
                }

                var go = ObjectPoolManager.Instance != null
                    ? ObjectPoolManager.Instance.Get(entry.prefab, spawnPos, Quaternion.identity)
                    : Instantiate(entry.prefab, spawnPos, Quaternion.identity);

                var agent = go.GetComponent<EnemyNavAgentBase>();
                if (agent != null)
                {
                    agent.SetSpawnId(spawnId);

                    // Zone-scope the flock ID so same-type fish only group within this zone
                    string flockId = $"{gameObject.name}_{agent.FishId}";
                    agent.SetFlockId(flockId);
                }

                spawnedEnemies.Add(go);
                spawned++;
            }

            Debug.Log($"[EnemySpawnManager] '{name}' spawned {spawned}/{enemyCount} enemies.");
        }

        private bool TryGetNavMeshPosition(out Vector3 result)
        {
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                Vector2 offset      = UnityEngine.Random.insideUnitCircle * spawnRadius;
                Vector3 candidate   = transform.position + new Vector3(offset.x, offset.y, 0f);

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navSampleDistance, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }

            result = transform.position;
            return false;
        }

        private EnemySpawnEntry PickEntry()
        {
            float totalWeight = 0f;
            foreach (var e in enemyEntries)
                if (e?.prefab != null) totalWeight += e.weight;

            if (totalWeight <= 0f) return null;

            float roll       = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var e in enemyEntries)
            {
                if (e?.prefab == null) continue;
                cumulative += e.weight;
                if (roll <= cumulative) return e;
            }

            return enemyEntries[^1];
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
