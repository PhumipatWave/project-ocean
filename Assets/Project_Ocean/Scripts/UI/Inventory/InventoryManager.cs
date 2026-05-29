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
                if (i >= startSlot && i != 15 && i != 16)
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
                Draggableltem buildingItemSlot01 = inventorySlots[15].GetComponentInChildren<Draggableltem>();
                if (buildingItemSlot01 == null)
                {
                    SpawnNewItem(item, inventorySlots[15]);
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

                Draggableltem buildingItemSlot02 = inventorySlots[16].GetComponentInChildren<Draggableltem>();
                if (buildingItemSlot02 == null)
                {
                    SpawnNewItem(item, inventorySlots[16]);
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

            for (int i = 0; i < inventorySlots.Length; i++)
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
    } // InventoryManager ===
}