using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class ShopSystem : MonoBehaviour
    {
        [Header("Play")]
        public InventoryManager inventory;
        public PlayerMoney playerMoney;

        [Header("UI Objects (Drag & Drop Here)")]
        public GameObject sellAllButton;      // ปุ่ม Sell All
        public GameObject sellSelectButton;   // ปุ่ม Sell Select
        public GameObject inventorySlot;      // พื้นหลังหน้าขายของ (Parent ของ Inventory Slots)
        public GameObject shopbuySlot;        // พื้นหลังหน้าซื้อของ

        [Header("Display")]
        public TMP_Text totalPriceText;       // แสดงราคารวมสำหรับขายทั้งหมดที่มีในกระเป๋า
        public TMP_Text clickedItemPriceText; // แสดงราคาของชิ้นที่เราจิ้มเลือกอยู่ (ทั้งซื้อและขาย)
        private bool isSellMode = true;

        [Header("Ammo UI Display")]
        public TMP_Text[] ammoTexts;
        private int[] ammoCounts = new int[4];

        [Header("Item UI Display (Sup Items)")]
        public TMP_Text[] itemTexts;
        private int[] itemCounts = new int[2];

        void Update()
        {
            UpdateUI();
            UpdateClickedItemPriceDisplay(); // ตรวจสอบการคลิกเลือกไอเทมทุกเฟรม
        }

        void UpdateUI()
        {
            if (totalPriceText != null)
            {
                totalPriceText.text = "Total Value: " + CalculateTotal();
            }

            for (int i = 0; i < ammoTexts.Length; i++)
            {
                if (ammoTexts[i] != null)
                {
                    ammoTexts[i].text = ammoCounts[i].ToString();
                }
            }

            RefreshSupportItemDisplay();

        } // UpdateUI ===

        /// <summary>
        /// WP_TextMH / WP_TextCP show live inventory counts (not shop-only counters).
        /// </summary>
        public void RefreshSupportItemDisplay()
        {
            if (itemTexts == null)
                return;

            if (itemTexts.Length > 0 && itemTexts[0] != null)
                itemTexts[0].text = WeaponUIController.GetInventoryCount("CheckPoint").ToString();

            if (itemTexts.Length > 1 && itemTexts[1] != null)
                itemTexts[1].text = WeaponUIController.GetInventoryCount("MiniHub").ToString();

            if (WeaponUIController.Instance != null)
                WeaponUIController.Instance.Refresh();
        }

        public void OnSupportItemConsumed(string itemId)
        {
            if (itemId == "CheckPoint" && itemCounts[0] > 0)
                itemCounts[0]--;
            else if (itemId == "MiniHub" && itemCounts[1] > 0)
                itemCounts[1]--;

            RefreshSupportItemDisplay();
        }

        // ระบบตรวจสอบไอเทมที่ถูกเลือกและแสดงราคาชิ้นนั้นๆ 
        void UpdateClickedItemPriceDisplay()
        {
            if (clickedItemPriceText == null) return;

            if (isSellMode)
            {
                // โหมดขาย: หา Slot ที่ถูกเลือกใน Inventory
                InventorySlot selectedInventorySlot = GetSelectedSlot();
                if (selectedInventorySlot != null)
                {
                    Draggableltem itemUI = selectedInventorySlot.GetComponentInChildren<Draggableltem>();
                    if (itemUI != null)
                    {
                        int unitPrice = itemUI.item.sellPrice;
                        int totalValue = unitPrice * itemUI.count;

                        // แสดงทั้งราคาต่อชิ้น และราคารวมใน Stack ของมัน
                        clickedItemPriceText.text = $"Price: {unitPrice}";
                    }
                    else
                    {
                        clickedItemPriceText.text = "Price: 0"; //Sell
                    }
                }
                else
                {
                    clickedItemPriceText.text = "Price: 0"; //Sell
                }
            }
            else
            {
                // โหมดซื้อ: หา Slot ที่ถูกเลือกใน Shop
                ShopSlot selectedShopSlot = GetSelectedShopSlot();
                if (selectedShopSlot != null)
                {
                    Draggableltem itemInShop = selectedShopSlot.GetComponentInChildren<Draggableltem>();
                    if (itemInShop != null)
                    {
                        // แสดงราคาสำหรับซื้อ (Cost)
                        clickedItemPriceText.text = $"Price: {itemInShop.item.buyPrice}";
                    }
                    else
                    {
                        clickedItemPriceText.text = "Price: 0"; //Buy
                    }
                }
                else
                {
                    clickedItemPriceText.text = "Price: 0"; //Buy
                }
            }

        } // UpdateClickedItemPriceDisplay ===

        // ฟังก์ชันช่วยหา ShopSlot ที่กำลังถูกเลือก 
        private ShopSlot GetSelectedShopSlot()
        {
            if (shopbuySlot == null) return null;
            ShopSlot[] allSlots = shopbuySlot.GetComponentsInChildren<ShopSlot>();
            foreach (var slot in allSlots)
            {
                if (slot.image.color == slot.selectedColor) return slot;
            }
            return null;

        } // GetSelectedShopSlot ===

        // --- ฟังก์ชันสำหรับปุ่ม Buy (สลับไปหน้า Shop) ---
        public void SwitchToBuyMode()
        {
            if (sellAllButton != null) sellAllButton.SetActive(false);
            if (sellSelectButton != null) sellSelectButton.SetActive(false);
            if (inventorySlot != null) inventorySlot.SetActive(false);

            if (shopbuySlot != null) shopbuySlot.SetActive(true);

            isSellMode = false;
            if (clickedItemPriceText != null) clickedItemPriceText.text = "Select an item to buy...";
            Debug.Log("Switched to Buy Mode");

        } // SwitchToBuyMode ===

        // ฟังก์ชันกลับไปหน้าขาย
        public void SwitchToSellMode()
        {
            if (sellAllButton != null) sellAllButton.SetActive(true);
            if (sellSelectButton != null) sellSelectButton.SetActive(true);
            if (inventorySlot != null) inventorySlot.SetActive(true);
            if (shopbuySlot != null) shopbuySlot.SetActive(false);

            isSellMode = true;
            if (clickedItemPriceText != null) clickedItemPriceText.text = "Price :";

        } // SwitchToSellMode ===

        public int CalculateTotal()
        {
            int total = 0;
            var slots = InventoryManager.Instance != null ? InventoryManager.Instance.inventorySlots : inventory.inventorySlots;

            foreach (InventorySlot slot in slots)
            {
                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI != null)
                {
                    total += itemUI.item.sellPrice * itemUI.count;
                }
            }
            return total;

        } // CalculateTotal ===

        public void Sell()
        {
            InventorySlot selectedSlot = GetSelectedSlot();
            if (selectedSlot == null) return;

            Draggableltem itemUI = selectedSlot.GetComponentInChildren<Draggableltem>();
            if (itemUI == null) return;

            PlayerMoney pm = GameManager.CurPlayer.GetComponent<PlayerMoney>();
            if (pm != null)
            {
                int price = itemUI.item.sellPrice;
                pm.AddMoney(price);
                itemUI.count--;
                itemUI.RefreshCount();
                if (itemUI.count <= 0) Destroy(itemUI.gameObject);
                UpdateUI();
            }

            StartCoroutine(SellRoutine());

        } // Sell ===

        public IEnumerator SellRoutine()
        {
            yield return new WaitForSeconds(2f);
        }

        public void SellAll()
        {
            PlayerMoney pm = GameManager.CurPlayer.GetComponent<PlayerMoney>();
            if (pm == null || InventoryManager.Instance == null) return;

            int total = 0;
            foreach (InventorySlot slot in InventoryManager.Instance.inventorySlots)
            {
                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI == null) continue;
                total += itemUI.item.sellPrice * itemUI.count;
                Destroy(itemUI.gameObject);
            }
            pm.AddMoney(total);

            StartCoroutine(SellRoutine());

        } // SellAll ===

        private InventorySlot GetSelectedSlot()
        {
            if (InventoryManager.Instance == null) return null;
            foreach (InventorySlot slot in InventoryManager.Instance.inventorySlots)
            {
                if (slot.image.color == slot.SelectedColor) return slot;
            }
            return null;
        } // GetSelectedSlot ===

        public void BuySelectedItem()
        {
            ShopSlot selectedSlot = GetSelectedShopSlot(); // ใช้ฟังก์ชันที่ยุบรวมใหม่ให้สั้นลง

            if (selectedSlot != null)
            {
                Draggableltem itemInShop = selectedSlot.GetComponentInChildren<Draggableltem>();
                if (itemInShop != null)
                {
                    PlayerMoney pm = GameManager.CurPlayer.GetComponent<PlayerMoney>();

                    if (pm.Money >= itemInShop.item.buyPrice)
                    {
                        if (itemInShop.item.itemType == ItemType.Ammo)
                        {
                            if (itemInShop.item.name.Contains("Ammo1")) ammoCounts[0]++;
                            else if (itemInShop.item.name.Contains("Ammo2")) ammoCounts[1]++;
                            else if (itemInShop.item.name.Contains("Ammo3")) ammoCounts[2]++;
                            else if (itemInShop.item.name.Contains("Ammo4")) ammoCounts[3]++;

                            pm.Use(itemInShop.item.buyPrice);
                            UpdateUI();
                            Debug.Log("Bought Ammo: " + itemInShop.item.name);
                        }
                        else
                        {
                            if (InventoryManager.Instance.AddItem(itemInShop.item))
                            {
                                pm.Use(itemInShop.item.buyPrice);

                                if (itemInShop.item.itemID == "CheckPoint")
                                {
                                    itemCounts[0]++;
                                }
                                else if (itemInShop.item.itemID == "MiniHub")
                                {
                                    itemCounts[1]++;
                                }

                                RefreshSupportItemDisplay();
                            }
                        }
                    }
                }
            }

            StartCoroutine(SellRoutine());
        } // BuySelectedItem ===
    } // ShopSystem ===
}