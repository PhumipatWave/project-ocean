using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UI;


namespace Game.Testing
{
    public class InventoryManager : MonoBehaviour
    {
        // I to Show UI
        public GameObject InventoryMenu;
        private bool menuActivetad;
        
        
        public int maxStackedItems = 4;
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
        }

        private void Start()
        {
            ChangeSelectedSlot(0);
            
            int startSlot = 5;
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (i >= startSlot)
                {
                    inventorySlots[i].SetLocked(true);
                }
            } // Lock Slot +1
            
        } // Start ===

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Ocean)
                return;

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

            /*if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeSelectedSlot(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeSelectedSlot(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ChangeSelectedSlot(2);
            }*/
            
        }

        public void OpenInventory()
        {
            Debug.Log("I to Inventory");
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
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                InventorySlot slot = inventorySlots[i];
                Draggableltem itemInSlot = slot.GetComponentInChildren<Draggableltem>();
                if (itemInSlot != null && itemInSlot.item == item && itemInSlot.count < maxStackedItems)
                {
                    itemInSlot.count++;
                    itemInSlot.RefreshCount();
                    return true;
                }
            }
            
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                InventorySlot slot = inventorySlots[i];
                Draggableltem itemInSlot = slot.GetComponentInChildren<Draggableltem>();
                if (itemInSlot == null)
                {
                    SpawnNewItem(item,slot);
                    return true;
                }
            }
            return false;
            
        } // AddItem ===

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
