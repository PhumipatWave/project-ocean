using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class SaveBridge : MonoBehaviour
    {
        public static SaveBridge Instance { get; private set; }

        // Flag สำหรับโหลดจาก save
        public static bool IsLoadingSave  { get; private set; }
        public static Vector3 LoadedPosition { get; private set; }

        public static void ClearLoadFlag() => IsLoadingSave = false;

        private SaveData _pendingData;

        void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_pendingData == null) return;

            var data = _pendingData;
            _pendingData = null;

            // ✅ รอ 1 frame ให้ scene objects พร้อมก่อน
            StartCoroutine(ApplyAfterFrame(data));
        }

        private IEnumerator ApplyAfterFrame(SaveData data)
        {
            yield return null;

            try
            {
                // ❌ ไม่มี ApplyPosition ที่นี่ — GameManager.OnSceneLoaded จัดการแล้ว
                ApplyPlayerStats(data);
                ApplyInventory(data);
                ApplyUpgrades(data);
                ApplyDay(data);

                IsLoadingSave = false;
                Debug.Log("[SaveBridge] Post-load data applied!");
            }
            catch (System.Exception e)
            {
                IsLoadingSave = false;
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
            {
                tube.MaxRopeLength       = data.maxRopeLength;
            }

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
                // Parse ID
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

                // Spawn directly into the saved slot (preserves slot position)
                inv.SpawnItemAtSlot(item, saved.slotIndex);

                // Restore the correct stack count
                var spawned = inv.InventorySlots[saved.slotIndex].GetComponentInChildren<Draggableltem>();
                if (spawned != null && saved.count > 1)
                {
                    spawned.count = saved.count;
                    spawned.RefreshCount();
                }
            }
        }

        private void ApplyUpgrades(SaveData data)
        {
            var invUpgrade  = FindFirstObjectByType<InventoryUpgrade>();
            var dmgUpgrade  = FindFirstObjectByType<DamageUpgrade>();
            var ropeUpgrade = FindFirstObjectByType<RopeUpgrade>();

            if (invUpgrade  != null) invUpgrade.currentLevel = data.inventoryUpgradeLevel;

            if (dmgUpgrade  != null)
            {
                dmgUpgrade.level         = data.damageUpgradeLevel;
                dmgUpgrade.currentDamage = data.currentDamage;
            }

            if (ropeUpgrade != null) ropeUpgrade.level = data.ropeUpgradeLevel;
        }

        private void ApplyDay(SaveData data)
        {
            if (DayManager.Instance != null)
                DayManager.Instance.SetDay(data.currentDay);
            else
                Debug.LogWarning("[SaveBridge] ApplyDay: DayManager not found");
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
            CollectUpgrades(data);
            CollectDay(data);

            try
            {
                SaveSystem.Save(data);
                Debug.Log("[SaveBridge] Game saved!");
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

            _pendingData   = data;
            LoadedPosition = new Vector3(data.posX, data.posY, data.posZ); // ✅ set ก่อน LoadScene
            IsLoadingSave  = true;

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
            {
                data.maxRopeLength       = tube.MaxRopeLength;
            }

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

        private void CollectUpgrades(SaveData data)
        {
            var invUpgrade  = FindFirstObjectByType<InventoryUpgrade>();
            var dmgUpgrade  = FindFirstObjectByType<DamageUpgrade>();
            var ropeUpgrade = FindFirstObjectByType<RopeUpgrade>();

            if (invUpgrade  != null) data.inventoryUpgradeLevel = invUpgrade.currentLevel;

            if (dmgUpgrade  != null)
            {
                data.damageUpgradeLevel = dmgUpgrade.level;
                data.currentDamage = dmgUpgrade.currentDamage;
            }

            if (ropeUpgrade != null) data.ropeUpgradeLevel = ropeUpgrade.level;
        }

        private void CollectDay(SaveData data)
        {
            data.currentDay = DayManager.Instance != null
                ? DayManager.Instance.CurrentDay
                : 1;
        }
    }
}