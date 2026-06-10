using TMPro;
using UnityEngine;

namespace Game.Testing
{
    public class InventoryManager : MonoBehaviour
    {
        // I to Show UI
        public GameObject InventoryMenu;
        private bool menuActivetad;

        [SerializeField] private AudioClip openInventorySound;

        [SerializeField] private InventorySlot[] inventorySlots;
        public InventorySlot[] InventorySlots => inventorySlots;

        [SerializeField] private GameObject InventoryItemPrefab;

        [Header("Item Database")]
        // ลากไฟล์ไอเทม (.asset) ทั้งหมดในเกมมาใส่รวมกันไว้ในอาร์เรย์นี้ที่หน้าต่าง Inspector
        [SerializeField] private Item[] allItems;

        int selectedSlot = -1;

        public static InventoryManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ChangeSelectedSlot(0);
            
            int startSlot = 5;
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (i >= startSlot && i != 14 && i != 15)
                {
                    inventorySlots[i].SetLocked(true);
                }
            } // Lock Slot +1

        } // Start ===

        private void Update()
        {
            if (Input.GetButtonDown("Inventory") && menuActivetad)
            {
                OpenInventory();
            } // I to Show UI
            else if (Input.GetButtonDown("Inventory") && !menuActivetad)
            {
                CloseInventory();
            } // I to Show UI
            
            if (Input.inputString != null)
            {
                int number;
                bool isNumber = int.TryParse(Input.inputString, out number);
                if (isNumber && number > 0 && number < 2)
                {
                    ChangeSelectedSlot(number - 1);
                }
            }
        }

        public void OpenInventory()
        {
            Debug.Log("I to Inventory");
            GameManager.CurPlayer.Audio.PlayOneShot(openInventorySound);
            Time.timeScale = 1;
            InventoryMenu.SetActive(false);
            menuActivetad = false;
        }

        public void CloseInventory()
        {
            Time.timeScale = 0;
            InventoryMenu.SetActive(true);
            menuActivetad = true;
        }

        void ChangeSelectedSlot(int newValue)
        {
            if (selectedSlot >= 0)
            {
                inventorySlots[selectedSlot].Deselect();
            }
            inventorySlots[newValue].Select();
            selectedSlot = newValue;
            
        } // ChangeSelectedSlot ===

        public bool AddItem(Item item)
        {
            if (item.ItemType == ItemType.Building)
            {
                Draggableltem buildingItemSlot01 = inventorySlots[14].GetComponentInChildren<Draggableltem>();
                if (buildingItemSlot01 == null)
                {
                    SpawnNewItem(item, inventorySlots[14]);
                    return true;
                }
                else if (buildingItemSlot01.item == item && buildingItemSlot01.count < item.MaxStackable)
                {
                    buildingItemSlot01.count++;
                    buildingItemSlot01.RefreshCount();
                    return true;
                }
                else if (buildingItemSlot01.item == item && buildingItemSlot01.count >= item.MaxStackable)
                {
                    Debug.Log("Full of this building item!");
                    return false;
                }

                Draggableltem buildingItemSlot02 = inventorySlots[15].GetComponentInChildren<Draggableltem>();
                if (buildingItemSlot02 == null)
                {
                    SpawnNewItem(item, inventorySlots[15]);
                    return true;
                }
                else if (buildingItemSlot02.item == item && buildingItemSlot02.count < item.MaxStackable)
                {
                    buildingItemSlot02.count++;
                    buildingItemSlot02.RefreshCount();
                    return true;
                }
                else if (buildingItemSlot02.item == item && buildingItemSlot02.count >= item.MaxStackable)
                {
                    Debug.Log("Full of this building item!");
                    return false;
                }

                Debug.Log("No space for building item!");
                return false;
            }
            else
            {
                for (int i = 0; i < inventorySlots.Length - 2; i++)
                {
                    InventorySlot slot = inventorySlots[i];
                    if (!slot.isUnlocked) continue;
                    Draggableltem itemInSlot = slot.GetComponentInChildren<Draggableltem>();

                    if (itemInSlot == null)
                    {
                        SpawnNewItem(item, slot);
                        return true;
                    }
                    else if (itemInSlot != null && itemInSlot.item == item)
                    {
                        if (itemInSlot.count < item.MaxStackable)
                        {
                            itemInSlot.count++;
                            itemInSlot.RefreshCount();
                            return true;
                        }
                    }
                }
            }

            return false;

        } // AddItem

        public void AddSlot(InventorySlot newSlot)
        {
            InventorySlot[] newSlots = new InventorySlot[inventorySlots.Length + 1];
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                newSlots[i] = inventorySlots[i];
            }
            newSlots[newSlots.Length - 1] = newSlot;
            inventorySlots = newSlots;
        } // AddSlot ===
        
        void SpawnNewItem(Item item, InventorySlot slot)
        {
            GameObject newItemGo;
            newItemGo = Instantiate(InventoryItemPrefab, slot.transform);
            Draggableltem inventoryItem = newItemGo.GetComponent<Draggableltem>();
            inventoryItem.InitialiseItem(item);
        } // SpawnNewItem ===

        public Item FindItem(ItemType checkType, int checkID)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                Draggableltem itemInSlot = inventorySlots[i].GetComponentInChildren<Draggableltem>();

                if (itemInSlot != null &&
                    itemInSlot.item.ItemType == checkType &&
                    itemInSlot.item.ItemID == checkID)
                {
                    return itemInSlot.item;
                }
            }
            return null;
        }

        /// <summary>Search allItems database by ID — used by the save system on load.</summary>
        public Item FindItemInDatabase(int id)
        {
            if (allItems == null) return null;
            foreach (var item in allItems)
                if (item != null && item.ItemID == id) return item;
            return null;
        }

        /// <summary>Spawn an item directly into a specific slot index — used by the save system on load.</summary>
        public void SpawnItemAtSlot(Item item, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length) return;
            SpawnNewItem(item, inventorySlots[slotIndex]);
        }

        public Item GetSelectedItem()
        {
            InventorySlot slot = inventorySlots[selectedSlot];
            Draggableltem itemInSlot = slot.GetComponentInChildren<Draggableltem>();
            if (itemInSlot != null)
            {
                return itemInSlot.item;
            }
            return null;
        } // GetSelectedItem ===

        public bool ConsumeItem(ItemType checkType, out Item foundItem)
        {
            foundItem = null;

            for (int i = 14; i <= 15; i++)
            {
                Draggableltem itemInSlot = inventorySlots[i].GetComponentInChildren<Draggableltem>();

                if (itemInSlot != null && itemInSlot.item.ItemType == checkType)
                {
                    foundItem = itemInSlot.item;

                    itemInSlot.count--;

                    if (itemInSlot.count <= 0)
                        Destroy(itemInSlot.gameObject);
                    else
                        itemInSlot.RefreshCount();

                    return true;
                }
            }
            return false;
        }

        public Item PeekItem(ItemType checkType)
        {
            for (int i = 14; i <= 15; i++)
            {
                Draggableltem itemInSlot = inventorySlots[i].GetComponentInChildren<Draggableltem>();
                if (itemInSlot != null && itemInSlot.item.ItemType == checkType)
                    return itemInSlot.item;
            }
            return null;
        }

        public void UnlockNextSlot()
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (!inventorySlots[i].isUnlocked)
                {
                    inventorySlots[i].SetLocked(false);
                    return;
                }
            }

            Debug.Log("UnLocked Slot Full!!!");
        } // UnlockNextSlot ===

        // Quest ==================================================
        public bool HasQuestItem(int itemId)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] == null) continue;

                Draggableltem itemInSlot = inventorySlots[i].GetComponentInChildren<Draggableltem>();

                if (itemInSlot != null && itemInSlot.item != null && itemInSlot.item.ItemID == itemId)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveQuestItem(int itemId)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] == null) continue;

                Draggableltem itemInSlot = inventorySlots[i].GetComponentInChildren<Draggableltem>();

                if (itemInSlot != null && itemInSlot.item != null && itemInSlot.item.ItemID == itemId)
                {
                    if (itemInSlot.count > 1)
                    {
                        itemInSlot.count--;
                        itemInSlot.RefreshCount();
                    }
                    else
                    {
                        Destroy(itemInSlot.gameObject);
                    }
                    return;
                }
            }
        }

        public void AddRewardItem(int itemId)
        {
            Debug.Log("QuestManager สั่งจ่ายไอเทมรางวัลรหัส: " + itemId);

            Item rewardItem = null;
            if (allItems != null)
            {
                foreach (Item item in allItems)
                {
                    if (item != null && item.ItemID == itemId)
                    {
                        rewardItem = item;
                        break;
                    }
                }
            }

            if (rewardItem != null)
            {
                bool isAdded = AddItem(rewardItem);

                if (isAdded)
                {
                    Debug.Log($"[Quest Reward] ผู้เล่นได้รับไอเทมรางวัล: {rewardItem.ItemName} เรียบร้อยแล้ว!");
                }
                else
                {
                    Debug.LogWarning("กระเป๋าเต็ม! ไม่สามารถรับไอเทมรางวัลจากเควสได้");
                }
            }
            else
            {
                Debug.LogError($"ไม่พบไอเทมที่มี ID: '{itemId}' ในคลัง All Items ของ InventoryManager! กรุณาตรวจสอบการลากไฟล์ลงช่องบอร์ด");
            }
        }

        public void RemoveSelectedItem()
        {
            if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length)
            {
                InventorySlot slot = inventorySlots[selectedSlot];
                Draggableltem itemInSlot = slot.GetComponentInChildren<Draggableltem>();

                if (itemInSlot != null)
                {
                    Debug.Log($"[Inventory] ลบไอเทม {itemInSlot.item.ItemName} ออกจากช่องที่ {selectedSlot}");
                    Destroy(itemInSlot.gameObject);
                }
                else
                {
                    Debug.LogWarning("ช่องที่เลือกไม่มีไอเทมให้ลบ!");
                }
            }
        }

        public void CloseInventoryViaButton()
        {
            OpenInventory();
        }

    } // InventoryManager ===
}
