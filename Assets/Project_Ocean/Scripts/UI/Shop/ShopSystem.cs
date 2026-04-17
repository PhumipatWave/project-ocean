using UnityEngine;
using TMPro;

namespace Game.Testing
{
    public class ShopSystem : MonoBehaviour
    {
        [Header("Core")]
        public PlayerMoney playerMoney;

        [Header("UI")]
        public GameObject shopPanel;
        public TMP_Text moneyText;
        public TMP_Text itemNameText;
        public TMP_Text itemPriceText;
        
        public Transform sellSlot;

        private Item selectedItem;
        

        void Start()
        {
            shopPanel.SetActive(false);
        }

        void Update()
        {
            if (GameManager.Instance.GameState != GameState.Land)
                return;

            // เปิด/ปิดร้าน
            /*if (Input.GetKeyDown(KeyCode.B))
            {
                shopPanel.SetActive(!shopPanel.activeSelf);
            }*/

            // อัปเดตเงิน
            moneyText.text = "Money: " + playerMoney.Money;
        }

        public void OpenShop()
        {
            shopPanel.SetActive(true);
        }

        public void CloseShop()
        {
            shopPanel.SetActive(false);
        }

        // เลือก Item จากร้าน
        public void SelectItem(Item item)
        {
            selectedItem = item;

            itemNameText.text = item.name;
            itemPriceText.text = "Buy: " + item.buyPrice + " / Sell: " + item.sellPrice;
        }

        // 🟢 ซื้อ
        public void Buy()
        {
            if (selectedItem == null) return;

            if (playerMoney.Use(selectedItem.buyPrice))
            {
                bool added = InventoryManager.Instance.AddItem(selectedItem);

                if (!added)
                {
                    playerMoney.Add(selectedItem.buyPrice); // คืนเงิน
                    Debug.Log("Inventory เต็ม");
                }
            }
            else
            {
                Debug.Log("เงินไม่พอ");
            }
        } // Buy ===

        // 🔴 ขาย
        public void Sell()
        {
            InventorySlot slot = GetSelectedSlot();
            if (slot == null) return;

            Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
            if (itemUI == null) return;

            Item item = itemUI.item;

            playerMoney.Add(item.sellPrice);

            itemUI.count--;
            itemUI.RefreshCount();

            if (itemUI.count <= 0)
            {
                Destroy(itemUI.gameObject);
            }
        } // Sell ===

        // หา slot ที่เลือก
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
        } // InventorySlot ===
        
        public void SellFromSlot()
        {
            if (sellSlot.childCount == 0)
            {
                Debug.Log("ไม่มีของในช่องขาย");
                return;
            }

            Draggableltem itemUI = sellSlot.GetChild(0).GetComponent<Draggableltem>();
            Item item = itemUI.item;

            int totalPrice = item.sellPrice * itemUI.count;

            playerMoney.Add(totalPrice);

            Debug.Log($"ขาย {item.name} x{itemUI.count} ได้ {totalPrice}");

            Destroy(itemUI.gameObject); // ลบออกจากช่องขาย
        } // SellFromSlot ===
    }
}