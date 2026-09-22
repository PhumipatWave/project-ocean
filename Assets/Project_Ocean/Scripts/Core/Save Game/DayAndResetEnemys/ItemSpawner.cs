using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    [Serializable]
    public class ZoneItemEntry
    {
        [Tooltip("Index in GameObjectSpawnManager.allItems list (0 = first entry, 1 = second, etc.).")]
        public int itemIndex;

        [Tooltip("Relative spawn chance for this zone. Higher = more likely to be picked.")]
        [Min(0f)] public float weight = 1f;
    }

    /// Place one ItemSpawner in the scene per zone.
    /// Assign all possible spawn point Transforms, then set how many spawn per day.
    /// Each day it randomly picks spawnPerDay points from the list and spawns one item at each.
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Zone Item Entries")]
        [Tooltip("Which items can spawn and their relative weights. IDs must match GameObjectSpawnManager.")]
        [SerializeField] private List<ZoneItemEntry> spawnEntries;

        [Header("Spawn Points")]
        [Tooltip("All possible spawn positions in this zone.")]
        [SerializeField] private List<Transform> spawnPoints;

        [Tooltip("How many of the spawn points are used per day. Cannot exceed the number of spawn points.")]
        [SerializeField, Min(1)] private int spawnPerDay = 1;

        [Tooltip("Which GameFeature must be unlocked for this zone to spawn — set by a quest step's Unlocks Features. Defaults to InteractItem (every zone's original shared gate) so existing spawners are unaffected.")]
        [SerializeField] private GameFeature requiredFeature = GameFeature.InteractItem;

        private readonly List<GameObject> _spawnedGOs = new();

        private void OnEnable()
        {
            GameObjectSpawnManager.Instance?.RegisterItemSpawner(this);
            FeatureUnlockService.OnFeatureUnlocked += HandleFeatureUnlocked;
        }

        private void OnDisable()
        {
            GameObjectSpawnManager.Instance?.UnregisterItemSpawner(this);
            FeatureUnlockService.OnFeatureUnlocked -= HandleFeatureUnlocked;
        }

        private void Start()
        {
            GameObjectSpawnManager.Instance?.RegisterItemSpawner(this);
            SpawnItems();
        }

        // Spawning is otherwise only re-rolled once a day (ResetSpawner) — this lets a zone
        // that was locked at Start() populate immediately the moment its quest unlocks it,
        // instead of waiting for the next day change.
        private void HandleFeatureUnlocked(GameFeature feature)
        {
            if (feature == requiredFeature && _spawnedGOs.Count == 0)
                SpawnItems();
        }

        // ── Public API ──

        public void ResetSpawner()
        {
            foreach (var go in _spawnedGOs)
                if (go != null) ObjectPoolManager.Instance?.Return(go);

            _spawnedGOs.Clear();
            SpawnItems();
        }

        public void OnItemCollected(GameObject collectedGO)
        {
            _spawnedGOs.Remove(collectedGO);
        }

        // ── Internal ──

        private void SpawnItems()
        {
            if (!FeatureUnlockService.Instance.IsUnlocked(requiredFeature)) return;

            if (spawnEntries == null || spawnEntries.Count == 0)
            {
                Debug.LogWarning($"[ItemSpawner] '{name}' has no spawn entries assigned.");
                return;
            }

            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                Debug.LogWarning($"[ItemSpawner] '{name}' has no spawn points assigned.");
                return;
            }

            int count = Mathf.Min(spawnPerDay, spawnPoints.Count);
            var selectedPoints = PickRandomPoints(count);

            foreach (var point in selectedPoints)
            {
                if (point == null) continue;

                var entry = PickEntry();
                if (entry == null) continue;

                var item = GameObjectSpawnManager.Instance?.GetItemAt(entry.itemIndex);
                if (item == null) continue;

                if (item.ItemPrefab == null)
                {
                    Debug.LogWarning($"[ItemSpawner] Item '{item.ItemName}' has no ItemPrefab assigned.");
                    continue;
                }

                var go = ObjectPoolManager.Instance != null
                    ? ObjectPoolManager.Instance.Get(item.ItemPrefab, point.position, Quaternion.identity)
                    : Instantiate(item.ItemPrefab, point.position, Quaternion.identity);

                var drop = go.GetComponentInChildren<ItemDrop>();
                if (drop != null)
                    drop.Setup(item, this);
                else
                    Debug.LogWarning($"[ItemSpawner] Prefab '{item.ItemPrefab.name}' is missing an ItemDrop component.");

                _spawnedGOs.Add(go);
            }
        }

        /// Returns a shuffled subset of spawnPoints with the given count.
        private List<Transform> PickRandomPoints(int count)
        {
            var indices = new List<int>(spawnPoints.Count);
            for (int i = 0; i < spawnPoints.Count; i++)
                indices.Add(i);

            // Fisher-Yates shuffle
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            var result = new List<Transform>(count);
            for (int i = 0; i < count; i++)
                result.Add(spawnPoints[indices[i]]);

            return result;
        }

        private ZoneItemEntry PickEntry()
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
            if (spawnPoints == null) return;

            Gizmos.color = Color.cyan;
            foreach (var p in spawnPoints)
            {
                if (p == null) continue;
                Gizmos.DrawWireSphere(p.position, 0.3f);
                Gizmos.DrawLine(p.position + Vector3.up * 0.3f,    p.position - Vector3.up * 0.3f);
                Gizmos.DrawLine(p.position + Vector3.right * 0.3f, p.position - Vector3.right * 0.3f);
            }
        }
    }
}
