using NUnit.Framework.Interfaces;
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
        public GameObject inventorySlot;  // ช่องสี่เหลี่ยมสีฟ้า (Parent ของ Inventory Slots)
        public GameObject shopbuySlot;              // พื้นหลังหน้าซื้อของ

        [Header("Display")]
        public TMP_Text totalPriceText;
        private bool isSellMode = true;

        [Header("Ammo UI Display")]
        public TMP_Text[] ammoTexts; // ลาก Text ตัวเลขกระสุน 4 อันมาใส่เรียงตามลำดับ (แดง, เหลือง, ฟ้า, เขียว)
        private int[] ammoCounts = new int[4]; // ตัวแปรเก็บจำนวนกระสุนจริงในโปรแกรม



        void Update()
        {
            UpdateUI();
        }

        void UpdateUI()
        {
            if (totalPriceText != null)
            {
                totalPriceText.text = "Sell Total: " + CalculateTotal();
            }

            for (int i = 0; i < ammoTexts.Length; i++)
            {
                if (ammoTexts[i] != null)
                {
                    ammoTexts[i].text = ammoCounts[i].ToString();
                }
            }
        }

        // --- เพิ่มฟังก์ชันสำหรับปุ่ม Buy ---
        public void SwitchToBuyMode()
        {
            // ปิด UI ของระบบขาย
            if (sellAllButton != null) sellAllButton.SetActive(false);
            if (sellSelectButton != null) sellSelectButton.SetActive(false);
            if (inventorySlot != null) inventorySlot.SetActive(false);

            // เปิด UI ของระบบซื้อ
            if (shopbuySlot != null) shopbuySlot.SetActive(true);

            isSellMode = false;
            Debug.Log("Switched to Buy Mode");
        }

        // ฟังก์ชันกลับไปหน้าขาย (เผื่อคุณอยากทำปุ่ม Back)
        public void SwitchToSellMode()
        {
            if (sellAllButton != null) sellAllButton.SetActive(true);
            if (sellSelectButton != null) sellSelectButton.SetActive(true);
            if (inventorySlot != null) inventorySlot.SetActive(true);
            if (shopbuySlot != null) shopbuySlot.SetActive(false);

            isSellMode = true;
        }

        public int CalculateTotal()
        {
            int total = 0;
            // ใช้ InventoryManager.Instance ตามที่คุณเขียนไว้ใน SellAll เพื่อความชัวร์
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
        }

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
        }

        private InventorySlot GetSelectedSlot()
        {
            if (InventoryManager.Instance == null) return null;
            foreach (InventorySlot slot in InventoryManager.Instance.inventorySlots)
            {
                if (slot.image.color == slot.SelectedColor) return slot;
            }
            return null;
        }

        public void BuySelectedItem()
        {
            ShopSlot selectedSlot = null;
            ShopSlot[] allSlots = shopbuySlot.GetComponentsInChildren<ShopSlot>();

            foreach (var slot in allSlots)
            {
                if (slot.image.color == slot.selectedColor)
                {
                    selectedSlot = slot;
                    break;
                }
            }

            if (selectedSlot != null)
            {
                Draggableltem itemInShop = selectedSlot.GetComponentInChildren<Draggableltem>();
                if (itemInShop != null)
                {
                    PlayerMoney pm = GameManager.CurPlayer.GetComponent<PlayerMoney>();

                    if (pm.Money >= itemInShop.item.buyPrice)
                    {
                        // --- ส่วนที่เพิ่มเข้ามาเพื่ออัปเดตกระสุน ---
                        if (itemInShop.item.type == ItemType.Ammo) // ตรวจสอบว่าเป็นชนิด Ammo
                        {
                            // สมมติว่าเราเช็คจากชื่อหรือการตั้งค่าใน Item Data
                            // ตัวอย่าง: ถ้าชื่อมีคำว่า Red ให้บวกช่องที่ 0
                            if (itemInShop.item.name.Contains("Ammo1")) ammoCounts[0]++;
                            else if (itemInShop.item.name.Contains("Ammo2")) ammoCounts[1]++;
                            else if (itemInShop.item.name.Contains("Ammo3")) ammoCounts[2]++;
                            else if (itemInShop.item.name.Contains("Ammo4")) ammoCounts[3]++;

                            pm.Use(itemInShop.item.buyPrice);
                            UpdateUI(); // สั่งให้ UI เปลี่ยนตัวเลขทันที
                            Debug.Log("Bought Ammo: " + itemInShop.item.name);
                        }
                        else
                        {
                            // ถ้าไม่ใช่กระสุน (เช่น ไอเทมสีส้ม) ให้ส่งเข้ากระเป๋าปกติ
                            if (InventoryManager.Instance.AddItem(itemInShop.item))
                            {
                                pm.Use(itemInShop.item.buyPrice);
                            }
                        }
                    }
                }

            } // selectedSlot != null ===

        } // BuySelectedItem ===

    } // ShopSystem ===


}