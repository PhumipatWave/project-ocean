using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    /// Generic object pool. Place on the same persistent GameObject as GameManager.
    /// Supports any prefab — enemies, items, VFX, etc.
    /// Pooled objects are parented here (DontDestroyOnLoad) so they survive scene changes.
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        // prefab instanceID → queue of inactive instances
        private readonly Dictionary<int, Queue<GameObject>> pools = new();

        // spawned instance ID → prefab instanceID (for Return lookups)
        private readonly Dictionary<int, int> instanceToPrefabId = new();

        // prefab instanceID → prefab reference (for creating new instances)
        private readonly Dictionary<int, GameObject> prefabById = new();

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            // Persistence is handled by the parent GameManager root's DontDestroyOnLoad.
        }

        // ── Public API ──────────────────────────────────────────────────────

        /// Pre-creates <count> instances of <prefab> and stores them in the pool.
        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null) return;

            int pid = prefab.GetInstanceID();
            EnsurePool(pid, prefab);

            for (int i = 0; i < count; i++)
            {
                var go = CreateInstance(prefab, pid);
                go.SetActive(false);
                pools[pid].Enqueue(go);
            }
        }

        /// Gets an instance from the pool (or creates one), places it at position/rotation, and activates it.
        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            int pid = prefab.GetInstanceID();
            EnsurePool(pid, prefab);

            GameObject go = pools[pid].Count > 0
                ? pools[pid].Dequeue()
                : CreateInstance(prefab, pid);

            go.transform.SetParent(null);
            // Move from the DDOL scene into the currently active scene so the
            // object is destroyed naturally when that scene unloads.
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);

            foreach (var p in go.GetComponentsInChildren<IPoolable>(true))
                p.OnGetFromPool();

            return go;
        }

        /// Returns an instance to the pool. Safe to call even if the object is already inactive.
        public void Return(GameObject instance)
        {
            if (instance == null) return;
            if (!instance.activeSelf) return;

            int iid = instance.GetInstanceID();
            if (!instanceToPrefabId.TryGetValue(iid, out int pid))
            {
                // Not a pooled object — fall back to Destroy
                Destroy(instance);
                return;
            }

            foreach (var p in instance.GetComponentsInChildren<IPoolable>(true))
                p.OnReturnToPool();

            instance.SetActive(false);
            instance.transform.SetParent(transform); // keep in DontDestroyOnLoad
            pools[pid].Enqueue(instance);
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private void EnsurePool(int pid, GameObject prefab)
        {
            if (!pools.ContainsKey(pid))
            {
                pools[pid]    = new Queue<GameObject>();
                prefabById[pid] = prefab;
            }
        }

        private GameObject CreateInstance(GameObject prefab, int pid)
        {
            var go = Instantiate(prefab, transform); // parent = this (DontDestroyOnLoad)
            go.SetActive(false);
            instanceToPrefabId[go.GetInstanceID()] = pid;
            return go;
        }
    }
}
