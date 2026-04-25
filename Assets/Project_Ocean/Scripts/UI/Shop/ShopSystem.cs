using UnityEngine;
using TMPro;
using UnityEngine.UI;



namespace Game.Testing
{
    public class ShopSystem : MonoBehaviour
    {
        [Header("Play")]
        public InventoryManager inventory;
        public PlayerMoney playerMoney;

        [Header("UI")]
        public TMP_Text totalPriceText;

        //public Button[] sellOneButtons;
        private bool isSellMode = false;

        //public TMP_Text statusText; // สำหรับบอกว่าขายสำเร็จหรือไม่ (ถ้ามี)

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
        }

        // คำนวณราคารวมของไอเทมทั้งหมดใน Inventory
        public int CalculateTotal()
        {
            int total = 0;
            foreach (InventorySlot slot in inventory.inventorySlots)
            {
                // เปลี่ยนจาก ItemUI เป็น Draggableltem ตามไฟล์ที่คุณมี
                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI != null)
                {
                    total += itemUI.item.sellPrice * itemUI.count;
                }
            }
            return total;
        }

        // ฟังก์ชันขายไอเทมจากช่องที่เลือก (Selected Slot)
        public void Sell()
        {
            InventorySlot selectedSlot = GetSelectedSlot();

            if (selectedSlot == null)
            {
                Debug.Log("Please click to select the item you wish to sell first.!");
                return;
            }

            Draggableltem itemUI = selectedSlot.GetComponentInChildren<Draggableltem>();
            if (itemUI == null)
            {
                Debug.Log("the Slot has no items!!");
                return;
            }

            PlayerMoney pm = GameManager.CurPlayer.GetComponent<PlayerMoney>();
            if (pm != null)
            {
                int price = itemUI.item.sellPrice;
                pm.AddMoney(price); // เพิ่มเงิน

                itemUI.count--;
                itemUI.RefreshCount();

                if (itemUI.count <= 0)
                {
                    Destroy(itemUI.gameObject);
                }

                // สั่งอัปเดต UI ของร้านค้าด้วย (ถ้ามี totalPriceText)
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
            Debug.Log("Sold All for: " + total);
        }

        private InventorySlot GetSelectedSlot()
        {
            foreach (InventorySlot slot in InventoryManager.Instance.inventorySlots)
            {
                // ถ้าช่องไหนมีสีตรงกับ SelectedColor แสดงว่าเป็นช่องที่ผู้เล่นคลิกเลือกไว้
                if (slot.image.color == slot.SelectedColor)
                {
                    return slot;
                }
            }
            return null; // ถ้าไม่มีช่องไหนถูกเลือกเลย
        }




    } // ShopSystem ===
    
} // Game.Testing ===