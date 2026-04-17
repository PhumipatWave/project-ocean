using UnityEngine;
using TMPro;

namespace Game.Testing
{
    public class InventoryUpgrade : MonoBehaviour
    {
        public InventoryManager inventory;
        public PlayerMoney playerMoney;

        [Header("UI")]
        public TMP_Text priceText;
        

        [Header("Upgrade Setting")]
        public int currentLevel = 1;
        public int basePrice = 20;

        [Header("Slot")]
        public GameObject slotPrefab;
        public Transform slotParent;

        void Update()
        {
            priceText.text = "$" + GetPrice();
            
        } // Update ===

        public void UpgradeSlot()
        {
            int price = GetPrice();

            if (!playerMoney.Use(price))
            {
                Debug.Log("No Money to Upgrade!!");
                return;
            }

            inventory.UnlockNextSlot();
            currentLevel++;

            Debug.Log("Unlocked Slot !!!");
            
        } // UpgradeSlot ===

        int GetPrice()
        {
            return basePrice * currentLevel;
            
        } // GetPrice ===
        
        
    }
}
