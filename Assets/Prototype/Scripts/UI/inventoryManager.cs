using UnityEngine;

namespace Game.Testing
{
    public class inventoryManager : MonoBehaviour
    {
        public GameObject inventoryMenu;
        private bool munuActivated;
        public ItemSlot[] itemSlot;

        void Start()
        {
            
        } // Start ========

        void Update()
        {
            if (Input.GetButtonDown("Inventory") && munuActivated)
            {
                Time.timeScale = 1;
                inventoryMenu.SetActive(false);
                munuActivated = false;
                
            } // Deactivates the Menu
            
            else if (Input.GetButtonDown("Inventory") && !munuActivated)
            {
                Time.timeScale = 0;
                inventoryMenu.SetActive(true);
                munuActivated = true;
                
            } // Activates the Menu
        
        } // Update ========

        public void AddItam(string itemName, int quantity, Sprite itemSprite)
        {
            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (itemSlot[i].isFull == false)
                {
                    itemSlot[i].AddItem(itemName, quantity, itemSprite);
                    return;
                }
            }
            
        } // AddItems
        
        
        
    }
}
