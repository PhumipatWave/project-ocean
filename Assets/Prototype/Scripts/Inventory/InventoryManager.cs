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

        private void Start()
        {
            ChangeSelectedSlot(0);
        }

        private void Update()
        {
            if (Input.GetButtonDown("Inventory") && menuActivetad)
            {
                Debug.Log("I to Inventory");
                Time.timeScale = 1;
                InventoryMenu.SetActive(false);
                menuActivetad = false;
            } // I to Show UI
            else if (Input.GetButtonDown("Inventory") && !menuActivetad)
            {
                Time.timeScale = 0;
                InventoryMenu.SetActive(true);
                menuActivetad = true;
            } // I to Show UI
            
            if (Input.inputString != null)
            {
                int number;
                bool isNumber = int.TryParse(Input.inputString, out number);
                if (isNumber && number > 0 && number < 8)
                {
                    ChangeSelectedSlot(number -1);
                }
            }
        }
        
        void ChangeSelectedSlot(int newValue)
        {
            if (selectedSlot >= 0)
            {
                inventorySlots[selectedSlot].Deselect();
                inventorySlots[newValue].Select();
                selectedSlot = newValue;
            }
            
        }
        
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
        }
        
        void SpawnNewItem(Item item,InventorySlot slot)
        {
            GameObject newItemGo;
            newItemGo = Instantiate(InventoryItemPrefab,slot.transform);
            Draggableltem inventoryItem = newItemGo.GetComponent<Draggableltem>();
            inventoryItem.InitialiseItem(item);
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
        }

        
    }
    
    
}
