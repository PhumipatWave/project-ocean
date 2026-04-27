using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class MiniHub : MonoBehaviour
    {
        private class HubEntry
        {
            public Vector2 pos;
            public GameObject obj;
        }

        private List<HubEntry> hubs = new();
        private GameObject hubPrefab;

        public int Count => hubs.Count;

        public List<Vector2> HubPositions
        {
            get
            {
                List<Vector2> list = new();
                foreach (var h in hubs) list.Add(h.pos);
                return list;
            }
        }

        public MiniHub(GameObject hubPrefab)
        {
            this.hubPrefab = hubPrefab;
        }

        public void RegisterHub(Vector2 pos)
        {
            // Prevent duplicates
            foreach (var h in hubs)
            {
                if (Vector2.Distance(h.pos, pos) < 0.1f)
                {
                    Debug.Log("[MiniHub] Hub already exists.");
                    return;
                }
            }

            GameObject obj = null;

            if (hubPrefab != null)
            {
                obj = Object.Instantiate(hubPrefab, pos, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("[MiniHub] No prefab assigned!");
            }

            hubs.Add(new HubEntry
            {
                pos = pos,
                obj = obj
            });

            Debug.Log($"[MiniHub] Created Hub #{hubs.Count} at {pos}");
        }

        public bool HasHubs() => hubs.Count > 0;

        public Vector2 GetHub(int index)
        {
            if (index < 0 || index >= hubs.Count)
            {
                Debug.LogWarning("[MiniHub] Invalid index");
                return Vector2.zero;
            }

            return hubs[index].pos;
        }

        public void DebugList(bool hasCheckpoint)
        {
            Debug.Log("=== TELEPORT OPTIONS ===");

            if (hubs.Count > 0)
            {
                Debug.Log("[Mini Hubs]");
                for (int i = 0; i < hubs.Count; i++)
                {
                    Debug.Log($"{i}: {hubs[i].pos}");
                }
            }
            else
            {
                Debug.Log("[Mini Hubs] None");
            }

            if (hasCheckpoint)
                Debug.Log("[Checkpoint] Last available");
            else
                Debug.Log("[Checkpoint] None");
        }

        public void ClearAll()
        {
            foreach (var h in hubs)
            {
                if (h.obj != null)
                    Object.Destroy(h.obj);
            }

            hubs.Clear();
            Debug.Log("[MiniHub] Cleared all hubs.");
        }
    }
}