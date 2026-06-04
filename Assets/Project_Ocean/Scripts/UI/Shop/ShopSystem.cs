using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class ShopSystem : MonoBehaviour
    {
        [Header("Play")]
        [SerializeField] private InventoryManager inventory;
        [SerializeField] private PlayerMoney playerMoney;

        [Header("UI Objects (Drag & Drop Here)")]
        [SerializeField] private GameObject sellAllButton;      // ปุ่ม Sell All
        [SerializeField] private GameObject sellSelectButton;   // ปุ่ม Sell Select
        [SerializeField] private GameObject inventorySlot;      // พื้นหลังหน้าขายของ (Parent ของ Inventory Slots)
        [SerializeField] private GameObject shopbuySlot;        // พื้นหลังหน้าซื้อของ

        [Header("Inventory In Shop Slots")]
        [SerializeField] private InventorySlot[] shopInventorySlots;
        [SerializeField] private GameObject inventoryItemPrefab;

        [Header("Display")]
        [SerializeField] private TMP_Text totalPriceText;       // แสดงราคารวมสำหรับขายทั้งหมดที่มีในกระเป๋า
        [SerializeField] private TMP_Text clickedItemPriceText; // แสดงราคาของชิ้นที่เราจิ้มเลือกอยู่ (ทั้งซื้อและขาย)
        private bool isSellMode = true;

        [Header("Ammo UI Display")]
        [SerializeField] private TMP_Text[] ammoTexts;
        private int[] ammoCounts = new int[4];

        private void OnEnable()
        {
            RefreshShopInventory();
        }
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

        } // UpdateUI ===

        public void RefreshShopInventory()
        {
            for (int i = 0; i < shopInventorySlots.Length; i++)
            {
                foreach (Transform child in shopInventorySlots[i].transform)
                {
                    Destroy(child.gameObject);
                }
            }

            if (InventoryManager.Instance == null) return;
            InventorySlot[] realPlayerSlots = InventoryManager.Instance.InventorySlots;

            for (int i = 0; i < shopInventorySlots.Length; i++)
            {
                if (i >= realPlayerSlots.Length) break;

                if (realPlayerSlots[i].isUnlocked)
                {
                    shopInventorySlots[i].SetLocked(false);

                    Draggableltem realItem = realPlayerSlots[i].GetComponentInChildren<Draggableltem>();
                    if (realItem != null)
                    {
                        GameObject newItemGo = Instantiate(inventoryItemPrefab, shopInventorySlots[i].transform);
                        Draggableltem shopItem = newItemGo.GetComponent<Draggableltem>();
                        shopItem.InitialiseItem(realItem.item);
                        shopItem.count = realItem.count;
                        shopItem.RefreshCount();
                    }
                }
                else
                {
                    shopInventorySlots[i].SetLocked(true);
                }
            }
        }

        // ระบบตรวจสอบไอเทมที่ถูกเลือกและแสดงราคาชิ้นนั้นๆ 
        void UpdateClickedItemPriceDisplay()
        {
            if (clickedItemPriceText == null) return;

            if (isSellMode)
            {
                // เปลี่ยนไปหาช่องที่ถูกเลือกจากฝั่งสล็อตของร้านค้าแทนกระเป๋าจริง
                InventorySlot selectedInventorySlot = GetSelectedSlot();
                if (selectedInventorySlot != null)
                {
                    Draggableltem itemUI = selectedInventorySlot.GetComponentInChildren<Draggableltem>();
                    if (itemUI != null)
                    {
                        int unitPrice = (int)(itemUI.item.ItemPrice - itemUI.item.ItemPrice * 0.1f); // Decrease price to 10% for selling
                        clickedItemPriceText.text = $"Price: {unitPrice}";
                    }
                    else
                    {
                        clickedItemPriceText.text = "Price: 0";
                    }
                }
                else
                {
                    clickedItemPriceText.text = "Price: 0";
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
                        clickedItemPriceText.text = $"Price: {itemInShop.item.ItemPrice}";
                    }
                    else
                    {
                        clickedItemPriceText.text = "Price: 0";
                    }
                }
                else
                {
                    clickedItemPriceText.text = "Price: 0";
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

            RefreshShopInventory(); // รีเฟรชไอเทมทุกครั้งที่สลับกลับมาหน้าขาย

        } // SwitchToSellMode ===

        public int CalculateTotal()
        {
            int total = 0;
            var slots = InventoryManager.Instance != null ? InventoryManager.Instance.InventorySlots : inventory.InventorySlots;

            foreach (InventorySlot slot in slots)
            {
                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI != null)
                {
                    total += (int)(itemUI.item.ItemPrice - itemUI.item.ItemPrice * 0.1f) * itemUI.count;
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

            // ตรวจสอบว่าช่องที่เลือกในร้านค้า คือช่องลำดับที่เท่าไหร่
            int slotIndex = -1;
            for (int i = 0; i < shopInventorySlots.Length; i++)
            {
                if (shopInventorySlots[i] == selectedSlot)
                {
                    slotIndex = i;
                    break;
                }
            }

            if (slotIndex == -1) return;

            PlayerMoney pm = GameManager.CurPlayer.GetComponent<PlayerMoney>();
            if (pm != null && InventoryManager.Instance != null)
            {
                // ดึงสล็อตจริงของผู้เล่นตาม Index มาหักจำนวนออก
                InventorySlot realSlot = InventoryManager.Instance.InventorySlots[slotIndex];
                Draggableltem realItem = realSlot.GetComponentInChildren<Draggableltem>();

                if (realItem != null)
                {
                    int unitPrice = (int)(realItem.item.ItemPrice - realItem.item.ItemPrice * 0.1f);
                    pm.AddMoney(unitPrice);

                    realItem.count--;
                    if (realItem.count <= 0)
                    {
                        Destroy(realItem.gameObject);
                    }
                    else
                    {
                        realItem.RefreshCount();
                    }

                    // สั่งให้สล็อตหน้าร้านค้ารีเฟรชตามข้อมูลล่าสุดทันที
                    Invoke("RefreshShopInventory", 0.02f);
                }
            }

            StartCoroutine(SellRoutine());

        } // Sell ===

        public IEnumerator SellRoutine()
        {
            yield return new WaitForSeconds(1f);
        }

        public void SellAll()
        {
            PlayerMoney playerMoney = GameManager.CurPlayer.GetComponent<PlayerMoney>();

            if (playerMoney == null || InventoryManager.Instance == null)
                return;

            int total = 0;
            foreach (InventorySlot slot in InventoryManager.Instance.InventorySlots)
            {
                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();

                if (itemUI == null || itemUI.item.ItemType != ItemType.Normal)
                    continue;

                total += (int)(itemUI.item.ItemPrice - itemUI.item.ItemPrice * 0.1f) * itemUI.count;
                Destroy(itemUI.gameObject);
            }
            playerMoney.AddMoney(total);

            // รีเฟรชหน้าร้านค้าหลังขายเสร็จทั้งหมด
            Invoke("RefreshShopInventory", 0.05f);
            StartCoroutine(SellRoutine());

        } // SellAll ===

        private InventorySlot GetSelectedSlot()
        {
            if (shopInventorySlots == null) return null;
            foreach (InventorySlot slot in shopInventorySlots)
            {
                if (slot != null && slot.image.color == slot.SelectedColor) return slot;
            }
            return null;
        } // GetSelectedSlot ===

        public void BuySelectedItem()
        {
            ShopSlot selectedSlot = GetSelectedShopSlot();

            if (selectedSlot != null)
            {
                Draggableltem itemInShop = selectedSlot.GetComponentInChildren<Draggableltem>();
                if (itemInShop != null)
                {
                    PlayerMoney pm = GameManager.CurPlayer.GetComponent<PlayerMoney>();

                    if (pm.Money >= itemInShop.item.ItemPrice)
                    {
                        if (itemInShop.item.ItemType == ItemType.Ammo)
                        {
                            if (itemInShop.item.name.Contains("Ammo1")) ammoCounts[0]++;
                            else if (itemInShop.item.name.Contains("Ammo2")) ammoCounts[1]++;
                            else if (itemInShop.item.name.Contains("Ammo3")) ammoCounts[2]++;
                            else if (itemInShop.item.name.Contains("Ammo4")) ammoCounts[3]++;

                            pm.Use(itemInShop.item.ItemPrice);
                            UpdateUI();
                            Debug.Log("Bought Ammo: " + itemInShop.item.name);
                        }
                        else
                        {
                            if (itemInShop.item.ItemType == ItemType.Building || itemInShop.item.ItemType == ItemType.Normal)
                            {
                                if (InventoryManager.Instance.AddItem(itemInShop.item))
                                {
                                    pm.Use(itemInShop.item.ItemPrice);
                                    // อัปเดตช่องแสดงไอเทมฝั่งขายเมื่อซื้อของสำเร็จ เผื่อเปิดดูคลังไอเทมล่าสุด
                                    RefreshShopInventory();
                                }
                            }
                        }
                    }
                }
            }

            StartCoroutine(SellRoutine());
        } // BuySelectedItem ===

    } // ShopSystem ===
}