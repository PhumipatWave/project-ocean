using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    [Serializable]
    public class ZoneItemEntry : IWeightedEntry
    {
        [Tooltip("Index in SpawnCatalog.allItems list (0 = first entry, 1 = second, etc.).")]
        public int itemIndex;

        [Tooltip("Relative spawn chance for this zone. Higher = more likely to be picked.")]
        [Min(0f)] public float weight = 1f;

        public float Weight => weight;
    }

    /// <summary>One per zone: each day spawns an item at spawnPerDay random points from its list.</summary>
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Zone Item Entries")]
        [Tooltip("Which items can spawn and their relative weights. IDs must match SpawnCatalog.")]
        [SerializeField] private List<ZoneItemEntry> spawnEntries;

        [Header("Spawn Points")]
        [Tooltip("All possible spawn positions in this zone.")]
        [SerializeField] private List<Transform> spawnPoints;

        [Tooltip("How many of the spawn points are used per day. Cannot exceed the number of spawn points.")]
        [SerializeField, Min(1)] private int spawnPerDay = 1;

        [Tooltip("Which GameFeature must be unlocked for this zone to spawn — set by a quest step's Unlocks Features. Defaults to InteractItem (every zone's original shared gate) so existing spawners are unaffected.")]
        [SerializeField] private GameFeature requiredFeature = GameFeature.InteractItem;

        [Tooltip("Spawn Per Day becomes the total for the whole playthrough: collected items never respawn (saved via OneTimePickupTracker, keyed by this GameObject's name — renaming resets it).")]
        [SerializeField] private bool oneTimeOnly;

        private readonly List<GameObject> _spawnedGOs = new();

        private void OnEnable()
        {
            SpawnCatalog.Instance?.RegisterItemSpawner(this);
            FeatureUnlockService.OnFeatureUnlocked += HandleFeatureUnlocked;
        }

        private void OnDisable()
        {
            SpawnCatalog.Instance?.UnregisterItemSpawner(this);
            FeatureUnlockService.OnFeatureUnlocked -= HandleFeatureUnlocked;
        }

        private void Start()
        {
            SpawnCatalog.Instance?.RegisterItemSpawner(this);
            SpawnItems();
        }

        // A zone locked at Start fills as soon as its quest unlocks it, not on the next day.
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

            if (oneTimeOnly)
                OneTimePickupTracker.Instance?.RegisterPickup(name);
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
            if (oneTimeOnly && OneTimePickupTracker.Instance != null)
                count = Mathf.Max(0, count - OneTimePickupTracker.Instance.CountFor(name));
            var selectedPoints = PickRandomPoints(count);

            foreach (var point in selectedPoints)
            {
                if (point == null) continue;

                var entry = PickEntry();
                if (entry == null) continue;

                var item = SpawnCatalog.Instance?.GetItemAt(entry.itemIndex);
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

        private ZoneItemEntry PickEntry() => WeightedPicker.Pick(spawnEntries);

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
