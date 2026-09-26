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

        private HubEntry hub;
        private GameObject hubPrefab;

        public bool HasHub => hub != null;

        public Vector2 HubPosition
        {
            get
            {
                if (hub == null)
                {
                    Debug.LogWarning("[MiniHub] No hub exists.");
                    return Vector2.zero;
                }

                return hub.pos;
            }
        }

        public MiniHub(GameObject hubPrefab)
        {
            this.hubPrefab = hubPrefab;
        }

        public void RegisterHub(Vector2 pos)
        {
            if (hub != null)
            {
                Debug.Log("[MiniHub] Already has.");
                return;
            }

            GameObject obj = hubPrefab != null
                ? Object.Instantiate(hubPrefab, pos, Quaternion.identity)
                : null;

            if (hubPrefab == null) Debug.Log("[MiniHub] No prefab assigned!");

            hub = new HubEntry { pos = pos, obj = obj };
            Debug.Log($"[MiniHub] Created at {pos}");
        }

        public void Clear()
        {
            if (hub?.obj != null) Object.Destroy(hub.obj);
            hub = null;
            Debug.Log("[MiniHub] Cleared.");
        }
    }
}
