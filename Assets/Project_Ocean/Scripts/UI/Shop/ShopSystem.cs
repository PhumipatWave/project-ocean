using UnityEngine;
using TMPro;
using UnityEngine.UI;



namespace Game.Testing
{
    public class ShopSystem : MonoBehaviour
    {
        public InventoryManager inventory;
        public PlayerMoney playerMoney;

        [Header("UI")]
        public TMP_Text moneyText;
        public TMP_Text totalPriceText;

        public Button[] sellOneButtons;
        private bool isSellMode = false;

        
        

        void Update()
        {
            UpdateUI();
            
        } // Update ===

        void UpdateUI()
        {
            moneyText.text = "Money: " + playerMoney.CurrentMoney;
            totalPriceText.text = "Sell Total: " + CalculateTotal();
            
        } // UpdateUI ===

        public int CalculateTotal()
        {
            int total = 0;

            foreach (InventorySlot slot in inventory.inventorySlots)
            {
                ItemUI itemUI = slot.GetComponentInChildren<ItemUI>();

                if (itemUI != null)
                {
                    Item item = itemUI.itemData;
                    total += item.sellPrice;
                }
            }

            return total;
        } // CalculateTotal ===

        void Start()
        {
            for (int i = 0; i < sellOneButtons.Length; i++)
            {
                int index = i; // กัน bug index เพี้ยน
                sellOneButtons[i].onClick.AddListener(() => SellItem(index));
            }
        }

        void SellItem(int index)
        {
            Debug.Log("ขายช่องที่: " + index);
        }

        public void Sell()
        {
            InventorySlot slot = GetSelectedSlot();
            if (slot == null)
            {
                Debug.Log("GetSelectedSlot!!");
                return;
            }

            Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
            if (itemUI == null)
            {
                Debug.Log("No Item!!");
                return;
            }

            Item item = itemUI.item;

            GameManager.CurPlayer.GetComponent<PlayerMoney>().Add(item.sellPrice);

            Debug.Log($"Sell {item.name} + {item.sellPrice}");
            

            itemUI.count--;
            itemUI.RefreshCount();

            if (itemUI.count <= 0)
            {
                Destroy(itemUI.gameObject);
            }
            
        } // Sell ===

        public void SellAll()
        {
            if (!isSellMode) return;
            
            int total = 0;

            foreach (InventorySlot slot in InventoryManager.Instance.inventorySlots)
            {
                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI == null) continue;

                total += itemUI.item.sellPrice * itemUI.count;

                Destroy(itemUI.gameObject);
            }

            GameManager.CurPlayer.GetComponent<PlayerMoney>().Add(total);

            Debug.Log("Sell All: " + total);
            
        } // SellAll ===

        public void ToggleSellMode()
        {
            isSellMode = !isSellMode;

            foreach (Button btn in sellOneButtons)
            {
                btn.gameObject.SetActive(isSellMode);
            }

            Debug.Log("Sell Mode: " + isSellMode);
        } // ToggleSellMode สลับ Mode

        private InventorySlot GetSelectedSlot()
        {
            foreach (InventorySlot slot in InventoryManager.Instance.inventorySlots)
            {
                if (slot.image.color == slot.SelectedColor)
                {
                    return slot;
                }
            }
            return null;
            
        } // GetSelectedSlot ===
        
    } // ShopSystem ===
    
} // Game.Testing ===