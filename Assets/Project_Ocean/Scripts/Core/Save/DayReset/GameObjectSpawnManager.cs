using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    // ── Shared data types ─────────────────────────────────────────────────────

    [Serializable]
    public class EnemySpawnEntry
    {
        [Tooltip("Unique ID used by EnemySpawners to reference this enemy type.")]
        public string id;
        public GameObject prefab;
    }

    [Serializable]
    public class ItemSpawnEntry
    {
        [Tooltip("Item ScriptableObject. ItemID and ItemPrefab are read from it automatically.")]
        public Item item;
    }

    // ── Manager ───────────────────────────────────────────────────────────────

    public class GameObjectSpawnManager : MonoBehaviour
    {
        public static GameObjectSpawnManager Instance { get; private set; }

        [Header("Enemy Prefabs")]
        [Tooltip("Every enemy type in the game. Assign a unique string ID to each entry.")]
        [SerializeField] private List<EnemySpawnEntry> allEnemies;

        [Header("Item Prefabs")]
        [Tooltip("Every item that can spawn in the world. Uses Item.ItemID as the lookup key.")]
        [SerializeField] private List<ItemSpawnEntry> allItems;

        [Header("Pool Settings")]
        [Tooltip("How many instances per prefab to pre-create in the pool on Start.")]
        [SerializeField] private int prewarmPerPrefab = 10;

        private Dictionary<string, GameObject> enemyById = new();
        private Dictionary<int, Item> itemById = new();
        private List<EnemySpawner> enemySpawners = new();
        private List<ItemSpawner> itemSpawners = new();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            BuildLookups();
        }

        private void OnEnable()
        {
            DayManager.OnEnemyReset  += OnEnemyDayReset;
            DayManager.OnDayAdvanced += OnItemDayReset;
        }

        private void OnDisable()
        {
            DayManager.OnEnemyReset  -= OnEnemyDayReset;
            DayManager.OnDayAdvanced -= OnItemDayReset;
        }

        private void Start() => PrewarmPool();

        // ── Public API ────────────────────────────────────────────────────────

        public GameObject GetEnemyPrefab(string id)
        {
            if (enemyById.TryGetValue(id, out var prefab)) return prefab;
            Debug.LogError($"[GameObjectSpawnManager] No enemy prefab for ID '{id}'. Check allEnemies.");
            return null;
        }

        public Item GetItem(int itemId)
        {
            if (itemById.TryGetValue(itemId, out var item)) return item;
            Debug.LogError($"[GameObjectSpawnManager] No item for ItemID {itemId}. Check allItems.");
            return null;
        }

        /// Returns item by its index in the allItems list (0 = first entry, 1 = second, etc.).
        public Item GetItemAt(int index)
        {
            if (allItems == null || index < 0 || index >= allItems.Count)
            {
                Debug.LogError($"[GameObjectSpawnManager] allItems index {index} is out of range (list has {allItems?.Count ?? 0} entries).");
                return null;
            }
            return allItems[index]?.item;
        }

        // Enemy spawner registration
        public void RegisterSpawner(EnemySpawner spawner)
        {
            if (!enemySpawners.Contains(spawner))
                enemySpawners.Add(spawner);
        }

        public void UnregisterSpawner(EnemySpawner spawner) => enemySpawners.Remove(spawner);

        // Item spawner registration
        public void RegisterItemSpawner(ItemSpawner spawner)
        {
            if (!itemSpawners.Contains(spawner))
                itemSpawners.Add(spawner);
        }

        public void UnregisterItemSpawner(ItemSpawner spawner) => itemSpawners.Remove(spawner);

        // ── Internal ──────────────────────────────────────────────────────────

        private void BuildLookups()
        {
            enemyById.Clear();
            if (allEnemies != null)
            {
                foreach (var entry in allEnemies)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.id) || entry.prefab == null) continue;
                    if (!enemyById.TryAdd(entry.id, entry.prefab))
                        Debug.LogWarning($"[GameObjectSpawnManager] Duplicate enemy ID '{entry.id}' — first entry used.");
                }
            }

            itemById.Clear();
            if (allItems != null)
            {
                foreach (var entry in allItems)
                {
                    if (entry?.item == null) continue;
                    if (!itemById.TryAdd(entry.item.ItemID, entry.item))
                        Debug.LogWarning($"[GameObjectSpawnManager] Duplicate ItemID {entry.item.ItemID} ('{entry.item.ItemName}') — first entry used.");
                }
            }
        }

        private void PrewarmPool()
        {
            if (ObjectPoolManager.Instance == null) return;

            if (allEnemies != null)
                foreach (var entry in allEnemies)
                    if (entry?.prefab != null)
                        ObjectPoolManager.Instance.Prewarm(entry.prefab, prewarmPerPrefab);

            if (allItems != null)
                foreach (var entry in allItems)
                    if (entry?.item?.ItemPrefab != null)
                        ObjectPoolManager.Instance.Prewarm(entry.item.ItemPrefab, prewarmPerPrefab);
        }

        private void OnEnemyDayReset()
        {
            foreach (var spawner in enemySpawners)
                spawner?.ResetZone();
        }

        private void OnItemDayReset()
        {
            foreach (var spawner in itemSpawners)
                spawner?.ResetSpawner();
        }
    }
}
