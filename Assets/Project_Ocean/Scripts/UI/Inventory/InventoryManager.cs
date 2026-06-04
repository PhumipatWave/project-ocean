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

        public InventorySlot[] inventorySlots;
        public GameObject InventoryItemPrefab;

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
                    inventorySlots[i].SetLocked(true); // บรรทัดนี้จะสั่งปิดตาช่องที่ 6 เป็นต้นไปอัตโนมัติเมื่อเริ่มเกม
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
                    ChangeSelectedSlot(number -1);
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
        
        void SpawnNewItem(Item item,InventorySlot slot)
        {
            GameObject newItemGo;
            newItemGo = Instantiate(InventoryItemPrefab,slot.transform);
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

            for (int i = 0; i < inventorySlots.Length; i++)
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

                    //NotifySupportItemConsumed(foundItem);
                    return true;
                }
            }
            return false;
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
            // วนลูปตรวจเช็กสล็อตทั้งหมดในกระเป๋าของคุณ
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] == null) continue;

                // ดึงสคริปต์ Draggableltem (ตัวพิมพ์แอลเล็ก) ที่อยู่ภายในช่องสล็อตออกมาเช็ก
                Draggableltem itemInSlot = inventorySlots[i].GetComponentInChildren<Draggableltem>();

                // ตรวจเช็กว่าถ้ามีไอเทมอยู่ในช่อง และข้อมูล itemID ตรงกับที่เควสต้องการ
                if (itemInSlot != null && itemInSlot.item != null && itemInSlot.item.ItemID == itemId)
                {
                    return true; // เจอไอเทมส่งเควสในกระเป๋าแล้ว!
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
                    // ลอจิกการหักของ: ถ้าในกองไอเทมมีจำนวนมากกว่า 1 ชิ้น
                    if (itemInSlot.count > 1)
                    {
                        itemInSlot.count--; // ลดจำนวนลง 1
                        itemInSlot.RefreshCount(); // เรียกใช้ฟังก์ชันอัปเดตตัวเลข UI บนสล็อตจริงของคุณ
                    }
                    else
                    {
                        // ถ้าเหลือเป็นชิ้นสุดท้าย ให้ทำลาย GameObject ของไอเทมนั้นทิ้งทันที
                        Destroy(itemInSlot.gameObject);
                    }
                    return; // เมื่อหักของชิ้นแรกที่เจอเรียบร้อยแล้ว ให้จบลูปทันที
                }
            }
        }

        public void AddRewardItem(int itemId)
        {
            Debug.Log("QuestManager สั่งจ่ายไอเทมรางวัลรหัส: " + itemId);

            // 1. วนลูปค้นหาไฟล์ข้อมูลไอเทมในคลัง (allItems) ที่มี itemID ตรงกับที่เควสส่งมา
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

            // 2. ถ้าเจอไอเทมในคลัง ให้ทำการแอดเข้ากระเป๋าโดยใช้ฟังก์ชัน AddItem ตัวเดิมของคุณ
            if (rewardItem != null)
            {
                // ฟังก์ชัน AddItem จะหาช่องว่างและเสก Prefab (SpawnNewItem) ให้เองตามลอจิกเดิมของคุณ
                bool isAdded = AddItem(rewardItem);

                if (isAdded)
                {
                    Debug.Log($"[Quest Reward] ผู้เล่นได้รับไอเทมรางวัล: {rewardItem.ItemName} เรียบร้อยแล้ว!");
                }
                else
                {
                    Debug.LogWarning("กระเป๋าเต็ม! ไม่สามารถรับไอเทมรางวัลจากเควสได้");
                    // 💡 ทริคเสริม: ถ้าอยากทำไอเทมตกพื้น สามารถเขียนโค้ด Instantiate(rewardItem.itemPrefab) ตรงนี้ได้เลยครับ
                }
            }
            else
            {
                Debug.LogError($"ไม่พบไอเทมที่มี ID: '{itemId}' ในคลัง All Items ของ InventoryManager! กรุณาตรวจสอบการลากไฟล์ลงช่องบอร์ด");
            }
        }
        // สั่งลบไอเทมออกจากช่องที่กำลังเลือก (Selected Slot)
        public void RemoveSelectedItem()
        {
            // ตรวจสอบก่อนว่าเลือกช่องที่ถูกต้องและไม่เอ้าท์ออฟบาวด์
            if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length)
            {
                InventorySlot slot = inventorySlots[selectedSlot];
                // ค้นหาไอเทมที่อยู่ในช่องนั้น
                Draggableltem itemInSlot = slot.GetComponentInChildren<Draggableltem>();

                if (itemInSlot != null)
                {
                    Debug.Log($"[Inventory] ลบไอเทม {itemInSlot.item.ItemName} ออกจากช่องที่ {selectedSlot}");

                    // ลบวัตถุไอเทมออกจาก Scene ทันที
                    Destroy(itemInSlot.gameObject);
                }
                else
                {
                    Debug.LogWarning("ช่องที่เลือกไม่มีไอเทมให้ลบ!");
                }
            }
        }

        // ฟังก์ชันสำหรับผูกกับปุ่มกากบาท (Close Button) บน UI
        public void CloseInventoryViaButton()
        {
            // เรียกใช้ฟังก์ชัน OpenInventory เดิมของคุณ (ที่ตั้งชื่อสวนทางไว้ว่าเปิด แต่ข้างในสั่งปิด Active สลับกัน)
            // หรือปรับให้ตรงตามลอจิกเดิม: สั่งให้ UI ปิดตัวลงและกลับมาเล่นเกมต่อ
            OpenInventory();
        }

    } // InventoryManager ===
}