using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class SaveBridge : MonoBehaviour
    {
        public static SaveBridge Instance { get; private set; }

        /// Fired after a successful save so UI can show a "Game Saved" toast.
        public static event System.Action OnSaved;

        private SaveData _pendingData;

        // Order is intentional and stable. Built fresh each call because scene-level
        // managers (QuestManager, EnemyKillTracker) may not exist at save time.
        private List<ISaveParticipant> BuildParticipants()
        {
            var list = new List<ISaveParticipant> { UpgradeService.Instance };
            if (QuestManager.Instance != null)     list.Add(QuestManager.Instance);
            if (EnemyKillTracker.Instance != null) list.Add(EnemyKillTracker.Instance);
            return list;
        }

        void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            // Persistence is handled by the parent GameManager root's DontDestroyOnLoad.
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            DayManager.OnDayAdvanced += SaveGame; // auto-save on day change
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            DayManager.OnDayAdvanced -= SaveGame;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_pendingData != null)
            {
                var data = _pendingData;
                _pendingData = null;
                StartCoroutine(ApplyAfterFrame(data));
                return;
            }

            // Auto-save whenever the player returns to land from the ocean.
            if (scene.name == SceneNames.Land && GameManager.Instance != null)
                SaveGame();
        }

        private IEnumerator ApplyAfterFrame(SaveData data)
        {
            yield return null;

            try
            {
                // Position is applied by GameManager.OnSceneLoaded (via SceneLoadContext),
                // not here.
                ApplyPlayerStats(data);
                ApplyInventory(data);
                ApplyDay(data);

                // Upgrade + quest slices are applied by their owning systems,
                // in the fixed order defined by Participants.
                foreach (var p in BuildParticipants())
                    p.Apply(data);

                SceneLoadContext.Clear();
                Debug.Log("[SaveBridge] Post-load data applied!");
            }
            catch (System.Exception e)
            {
                SceneLoadContext.Clear();
                Debug.LogError($"[SaveBridge] Failed to apply save data: {e.Message}");
            }
        }

        // -------------------- Apply helpers -------------------- //

        private void ApplyPlayerStats(SaveData data)
        {
            var player = GameManager.CurPlayer;
            if (player == null)
            {
                Debug.LogWarning("[SaveBridge] ApplyPlayerStats: CurPlayer is null");
                return;
            }

            var tube = player.OxygenTube;
            if (tube != null)
                tube.MaxRopeLength = data.maxRopeLength;

            var playerMoney = player.GetComponent<PlayerMoney>();
            if (playerMoney != null)
            {
                playerMoney.Use(playerMoney.CurrentMoney);
                playerMoney.AddMoney(data.money);
            }
        }

        private void ApplyInventory(SaveData data)
        {
            var inv = InventoryManager.Instance;
            if (inv == null)
            {
                Debug.LogWarning("[SaveBridge] ApplyInventory: InventoryManager not found");
                return;
            }

            // Clear all existing items first
            foreach (var slot in inv.InventorySlots)
            {
                var existing = slot.GetComponentInChildren<Draggableltem>();
                if (existing != null) Destroy(existing.gameObject);
            }

            foreach (var saved in data.inventoryItems)
            {
                if (!int.TryParse(saved.itemID, out int id))
                {
                    Debug.LogWarning($"[SaveBridge] Invalid itemID: '{saved.itemID}'");
                    continue;
                }

                // Look up item from the allItems database — NOT from current inventory slots
                var item = inv.FindItemInDatabase(id);
                if (item == null)
                {
                    Debug.LogWarning($"[SaveBridge] Item ID {id} not found in allItems database");
                    continue;
                }

                if (saved.slotIndex >= inv.InventorySlots.Length)
                {
                    Debug.LogWarning($"[SaveBridge] slotIndex {saved.slotIndex} out of range");
                    continue;
                }

                // Spawn with the saved stack count in one step — the item is born
                // with the correct count and a single RefreshCount(), so the count
                // label shows immediately regardless of load order.
                inv.SpawnItemAtSlot(item, saved.slotIndex, Mathf.Max(1, saved.count));
            }
        }

        private void ApplyDay(SaveData data)
        {
            if (DayManager.Instance == null) { Debug.LogWarning("[SaveBridge] ApplyDay: DayManager not found"); return; }
            DayManager.Instance.SetDay(data.currentDay);
            DayManager.Instance.SetDivedToday(data.hasDivedToday);
        }

        // -------------------- Public API -------------------- //

        public void SaveGame()
        {
            var gm = GameManager.Instance;
            if (gm == null) { Debug.LogError("[SaveBridge] GameManager not found"); return; }

            var data = new SaveData
            {
                gameState        = gm.GameState,
                currentScene     = SceneManager.GetActiveScene().name,
                miniHubAvailable = gm.MiniHubAvaliable,
            };

            CollectPosition(data);
            CollectPlayerStats(data);
            CollectInventory(data);
            CollectDay(data);

            // Upgrade + quest slices are collected by their owning systems,
            // in the fixed order defined by Participants.
            foreach (var p in BuildParticipants())
                p.Collect(data);

            try
            {
                SaveSystem.Save(data);
                Debug.Log("[SaveBridge] Game saved!");
                OnSaved?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveBridge] SaveGame failed: {e.Message}");
            }
        }

        public void LoadGame()
        {
            if (!SaveSystem.HasSave())
            {
                if (SaveSystem.RestoreBackup())
                    Debug.Log("[SaveBridge] Restored from backup, retrying load...");
                else
                {
                    Debug.LogWarning("[SaveBridge] No save file found");
                    return;
                }
            }

            var data = SaveSystem.Load();
            if (data == null)
            {
                Debug.LogError("[SaveBridge] Failed to load save data");
                return;
            }

            var gm = GameManager.Instance;
            if (gm == null) { Debug.LogError("[SaveBridge] GameManager not found"); return; }

            gm.GameState        = data.gameState;
            gm.MiniHubAvaliable = data.miniHubAvailable;

            _pendingData = data;

            // Hand off the load position + flag through the shared context.
            // Position MUST be set before LoadScene() — BeginLoad does both atomically.
            SceneLoadContext.BeginLoad(new Vector3(data.posX, data.posY, data.posZ));

            SceneManager.LoadScene(data.currentScene);
            Debug.Log("[SaveBridge] Loading scene...");
        }

        // -------------------- Collect helpers -------------------- //

        private void CollectPosition(SaveData data)
        {
            var player = GameManager.CurPlayer;
            if (player == null) return;

            var pos   = player.transform.position;
            data.posX = pos.x;
            data.posY = pos.y;
            data.posZ = pos.z;
        }

        private void CollectPlayerStats(SaveData data)
        {
            var player = GameManager.CurPlayer;
            if (player == null) return;

            var tube = player.OxygenTube;
            if (tube != null)
                data.maxRopeLength = tube.MaxRopeLength;

            var playerMoney = player.GetComponent<PlayerMoney>();
            if (playerMoney != null)
                data.money = playerMoney.CurrentMoney;
        }

        private void CollectInventory(SaveData data)
        {
            data.inventoryItems = new List<ItemSaveData>();
            if (InventoryManager.Instance == null) return;

            var slots = InventoryManager.Instance.InventorySlots;
            for (int i = 0; i < slots.Length; i++)
            {
                var item = slots[i].GetComponentInChildren<Draggableltem>();
                if (item?.GetItem() == null) continue;

                data.inventoryItems.Add(new ItemSaveData
                {
                    slotIndex = i,
                    itemID    = item.GetItem().ItemID.ToString(),
                    count     = item.count,
                });
            }
        }

        private void CollectDay(SaveData data)
        {
            if (DayManager.Instance == null) return;
            data.currentDay    = DayManager.Instance.CurrentDay;
            data.hasDivedToday = DayManager.Instance.HasDivedToday;
        }
    }
}
