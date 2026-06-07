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

        void Awake()
        {
            if (Instance != null) 
            { 
                Destroy(this); 
                return; 
            }

            Instance = this;
        }

        public void SaveGame()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            var data = new SaveData
            {
                gameState = gm.GameState,
                currentScene = SceneManager.GetActiveScene().name,
                miniHubAvailable = gm.MiniHubAvaliable,
            };

            if (GameManager.CurPlayer != null)
            {
                var pos = GameManager.CurPlayer.transform.position;
                data.posX = pos.x;
                data.posY = pos.y;
                data.posZ = pos.z;

                // Oxygen Tube
                var tube = GameManager.CurPlayer.OxygenTube;
                if (tube != null)
                {
                    data.maxRopeLength = tube.MaxRopeLength;
                    data.reduceMaxRopeLength = tube.ReduceMaxRopeLenght;
                }

                // เงิน
                var playerMoney = GameManager.CurPlayer.GetComponent<PlayerMoney>();
                if (playerMoney != null)
                    data.money = playerMoney.CurrentMoney;
            }

            // Inventory
            data.inventoryItems = new List<ItemSaveData>();
            if (InventoryManager.Instance != null)
            {
                var slots = InventoryManager.Instance.InventorySlots;
                for (int i = 0; i < slots.Length; i++)
                {
                    var item = slots[i].GetComponentInChildren<Draggableltem>();
                    if (item != null && item.GetItem() != null)
                    {
                        data.inventoryItems.Add(new ItemSaveData
                        {
                            slotIndex = i,
                            itemID = item.GetItem().ItemID.ToString(),
                            count = item.count,
                        });
                    }
                }
            }

            // Upgrade
            var invUpgrade = FindFirstObjectByType<InventoryUpgrade>();
            if (invUpgrade != null)
                data.inventoryUpgradeLevel = invUpgrade.currentLevel;

            var dmgUpgrade = FindFirstObjectByType<DamageUpgrade>();
            if (dmgUpgrade != null)
            {
                data.damageUpgradeLevel = dmgUpgrade.level;
                data.currentDamage = dmgUpgrade.currentDamage;
            }

            // ✅ Rope Upgrade
            var ropeUpgrade = FindFirstObjectByType<RopeUpgrade>();
            if (ropeUpgrade != null)
                data.ropeUpgradeLevel = ropeUpgrade.level;

            SaveSystem.Save(data);
            Debug.Log("[SaveBridge] Game saved!");
        }

        public void LoadGame()
        {
            if (!SaveSystem.HasSave())
            {
                Debug.LogWarning("[SaveBridge] No save file found");
                return;
            }

            var data = SaveSystem.Load();
            var gm = GameManager.Instance;

            gm.GameState = data.gameState;
            gm.MiniHubAvaliable = data.miniHubAvailable;

            if (GameManager.CurPlayer != null)
            {
                // Oxygen Tube
                var tube = GameManager.CurPlayer.OxygenTube;
                if (tube != null)
                {
                    tube.MaxRopeLength = data.maxRopeLength;
                    tube.ReduceMaxRopeLenght = data.reduceMaxRopeLength;
                }

                // เงิน
                var playerMoney = GameManager.CurPlayer.GetComponent<PlayerMoney>();
                if (playerMoney != null)
                {
                    playerMoney.Use(playerMoney.CurrentMoney);
                    playerMoney.AddMoney(data.money);
                }
            }

            // Inventory
            if (InventoryManager.Instance != null)
            {
                var slots = InventoryManager.Instance.InventorySlots;
                foreach (var slot in slots)
                {
                    var existing = slot.GetComponentInChildren<Draggableltem>();
                    if (existing != null) Destroy(existing.gameObject);
                }

                foreach (var saved in data.inventoryItems)
                {
                    if (saved.slotIndex >= slots.Length) continue;

                    var item = InventoryManager.Instance.FindItem(ItemType.Normal, int.Parse(saved.itemID))
                            ?? InventoryManager.Instance.FindItem(ItemType.Consumable, int.Parse(saved.itemID))
                            ?? InventoryManager.Instance.FindItem(ItemType.Building, int.Parse(saved.itemID))
                            ?? InventoryManager.Instance.FindItem(ItemType.Quest, int.Parse(saved.itemID));

                    if (item == null)
                    {
                        Debug.LogWarning($"[SaveBridge] ไม่พบ item ID: {saved.itemID}");
                        continue;
                    }

                    for (int n = 0; n < saved.count; n++)
                        InventoryManager.Instance.AddItem(item);
                }
            }

            // Upgrade
            var invUpgrade = FindFirstObjectByType<InventoryUpgrade>();
            if (invUpgrade != null)
                invUpgrade.currentLevel = data.inventoryUpgradeLevel;

            var dmgUpgrade = FindFirstObjectByType<DamageUpgrade>();
            if (dmgUpgrade != null)
            {
                dmgUpgrade.level         = data.damageUpgradeLevel;
                dmgUpgrade.currentDamage = data.currentDamage;
            }

            // ✅ Rope Upgrade
            var ropeUpgrade = FindFirstObjectByType<RopeUpgrade>();
            if (ropeUpgrade != null)
                ropeUpgrade.level = data.ropeUpgradeLevel;

            // เซ็ต flag + ตำแหน่งก่อน LoadScene
            IsLoadingSave  = true;
            LoadedPosition = new Vector3(data.posX, data.posY, data.posZ);
            SceneManager.LoadScene(data.currentScene);
            Debug.Log("[SaveBridge] Game loaded!");
        }
    }
}