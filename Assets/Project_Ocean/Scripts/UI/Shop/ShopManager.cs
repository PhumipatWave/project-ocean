using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class ShopManager : MonoBehaviour
    {
        [Header("Play")]
        [SerializeField] private InventoryManager inventory;
        [SerializeField] private PlayerMoney playerMoney;

        [Header("UI Objects (Drag & Drop Here)")]
        [SerializeField] private GameObject sellAllButton;
        [SerializeField] private GameObject sellSelectButton;
        [SerializeField] private GameObject inventorySlot;      // แผงรวมสล็อต 16 ช่องของร้านค้า (ที่คุณเปลี่ยนชื่อใหม่)
        [SerializeField] private GameObject shopbuySlot;

        [Header("NEW: Shop 16 Slots Display (ภาพ 1)")]
        [Tooltip("ลากสล็อตทั้ง 16 ช่องในหน้า Shop มาใส่ที่นี่เรียงลำดับจาก 0-15")]
        [SerializeField] private InventorySlot[] shopInventorySlots;
        [Tooltip("ลาก Prefab ตัว InventoryItemPrefab มาใส่")]
        [SerializeField] private GameObject inventoryItemPrefab;

        [Header("Display")]
        [SerializeField] private TMP_Text totalPriceText;
        [SerializeField] private TMP_Text clickedItemPriceText;
        private bool isSellMode = true;

        [Header("Ammo UI Display")]
        [SerializeField] private TMP_Text[] ammoTexts;
        private int[] ammoCounts = new int[4];

        // [แก้ปัญหาที่ 2] ฟังก์ชันสำหรับให้ปุ่มเปิดร้านค้า หรือ NPC ร้านค้าเรียกใช้งาน
        public void OpenShopWindow()
        {
            gameObject.SetActive(true);

            // [แก้ปัญหาที่ 1] บังคับปิดกระเป๋าหลักของผู้เล่นทิ้งทันทีเพื่อไม่ให้ขึ้นมาทับซ้อน
            if (InventoryManager.Instance != null && InventoryManager.Instance.InventoryMenu != null)
            {
                InventoryManager.Instance.InventoryMenu.SetActive(false);
            }

            // สลับเข้าสู่โหมดขายของเพื่อดึงข้อมูลไอเทมล่าสุดมาอัปเดตลงช่อง 16 ช่องทันที
            SwitchToSellMode();
        }

        private void OnEnable()
        {
            RefreshShopInventory();
        }

        void Update()
        {
            UpdateUI();
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
        }

        // ฟังก์ชันสั่งสำเนาไอเทมจริงจากผู้เล่นมาแปะไว้ที่ช่องจำลองของหน้า Shop
        public void RefreshShopInventory()
        {
            // ล้างข้อมูลภาพเก่าในช่องจำลองของร้านค้าออกทั้งหมดก่อน
            for (int i = 0; i < shopInventorySlots.Length; i++)
            {
                foreach (Transform child in shopInventorySlots[i].transform)
                {
                    Destroy(child.gameObject);
                }
            }

            if (InventoryManager.Instance == null) return;
            InventorySlot[] realPlayerSlots = InventoryManager.Instance.InventorySlots;

            // วนลูปสร้างรูปไอเทมจำลอง
            for (int i = 0; i < shopInventorySlots.Length; i++)
            {
                if (i >= realPlayerSlots.Length) break;

                if (realPlayerSlots[i].isUnlocked)
                {
                    shopInventorySlots[i].SetLocked(false);
                    shopInventorySlots[i].Deselect(); // เคลียร์สีเหลืองเก่าค้างออกด้วย

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

            UpdateClickedItemPriceDisplay();
        }

        // ฟังก์ชันคำนวณและแสดงราคาของชิ้นที่คลิกเลือก
        public void UpdateClickedItemPriceDisplay()
        {
            if (clickedItemPriceText == null) return;

            if (isSellMode)
            {
                InventorySlot selectedInventorySlot = GetSelectedSlot();
                if (selectedInventorySlot != null)
                {
                    Draggableltem itemUI = selectedInventorySlot.GetComponentInChildren<Draggableltem>();
                    if (itemUI != null && itemUI.item != null)
                    {
                        int unitPrice = (int)(itemUI.item.ItemPrice - itemUI.item.ItemPrice * 0.1f);
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
                ShopBuySlot selectedShopSlot = GetSelectedShopSlot();
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
        }

        private ShopBuySlot GetSelectedShopSlot()
        {
            if (shopbuySlot == null) return null;
            ShopBuySlot[] allSlots = shopbuySlot.GetComponentsInChildren<ShopBuySlot>();
            foreach (var slot in allSlots)
            {
                if (slot.image.color == slot.selectedColor) return slot;
            }
            return null;
        }

        public void SwitchToBuyMode()
        {
            if (sellAllButton != null) sellAllButton.SetActive(false);
            if (sellSelectButton != null) sellSelectButton.SetActive(false);
            if (inventorySlot != null) inventorySlot.SetActive(false);
            if (shopbuySlot != null) shopbuySlot.SetActive(true);

            isSellMode = false;
            clickedItemPriceText.text = "Price: 0";
        }

        public void SwitchToSellMode()
        {
            if (sellAllButton != null) sellAllButton.SetActive(true);
            if (sellSelectButton != null) sellSelectButton.SetActive(true);
            if (inventorySlot != null) inventorySlot.SetActive(true);
            if (shopbuySlot != null) shopbuySlot.SetActive(false);

            isSellMode = true;
            RefreshShopInventory();
        }

        public int CalculateTotal()
        {
            int total = 0;
            var slots = InventoryManager.Instance != null ? InventoryManager.Instance.InventorySlots : inventory.InventorySlots;

            foreach (InventorySlot slot in slots)
            {
                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI != null && itemUI.item.ItemType == ItemType.Normal)
                {
                    total += (int)(itemUI.item.ItemPrice - itemUI.item.ItemPrice * 0.1f) * itemUI.count;
                }
            }
            return total;
        }

        // ฟังก์ชันทำงานร่วมกับปุ่ม "Sell Select" เพื่อลบและขายของชิ้นที่คลิกเลือก
        public void Sell()
        {
            InventorySlot selectedSlot = GetSelectedSlot();
            if (selectedSlot == null) return;

            Draggableltem itemUI = selectedSlot.GetComponentInChildren<Draggableltem>();
            if (itemUI == null) return;

            // ค้นหาตำแหน่งดัชนี (Index) ว่าตรงกับสล็อตกระเป๋าจริงช่องไหน
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

                    // สั่งรีเฟรชภาพของฝั่งร้านค้าใหม่ให้ลดจำนวนตามกระเป๋าจริงของผู้เล่น
                    Invoke("RefreshShopInventory", 0.02f);
                }
            }
            StartCoroutine(SellRoutine());
        }

        public IEnumerator SellRoutine()
        {
            yield return new WaitForSeconds(1f);
        }

        // ฟังก์ชันทำงานร่วมกับปุ่ม "Sell All" ขายไอเทมประเภท Normal ทั้งกระเป๋า
        public void SellAll()
        {
            PlayerMoney playerMoney = GameManager.CurPlayer.GetComponent<PlayerMoney>();
            if (playerMoney == null || InventoryManager.Instance == null) return;

            int total = 0;
            foreach (InventorySlot slot in InventoryManager.Instance.InventorySlots)
            {
                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI == null || itemUI.item.ItemType != ItemType.Normal) continue;

                total += (int)(itemUI.item.ItemPrice - itemUI.item.ItemPrice * 0.1f) * itemUI.count;
                Destroy(itemUI.gameObject);
            }
            playerMoney.AddMoney(total);

            Invoke("RefreshShopInventory", 0.05f);
            StartCoroutine(SellRoutine());
        }

        private InventorySlot GetSelectedSlot()
        {
            if (shopInventorySlots == null) return null;
            foreach (InventorySlot slot in shopInventorySlots)
            {
                if (slot != null && slot.image.color == slot.SelectedColor) return slot;
            }
            return null;
        }

        public void BuySelectedItem()
        {
            ShopBuySlot selectedSlot = GetSelectedShopSlot();
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
                        }
                        else if (itemInShop.item.ItemType == ItemType.Building || itemInShop.item.ItemType == ItemType.Normal)
                        {
                            if (InventoryManager.Instance.AddItem(itemInShop.item))
                            {
                                pm.Use(itemInShop.item.ItemPrice);
                                RefreshShopInventory(); // อัปเดตช่องแสดงไอเทมกระเป๋าฝั่งร้านค้าทันทีเมื่อซื้อสำเร็จ
                            }
                        }
                    }
                }
            }
            StartCoroutine(SellRoutine());
        }
    }
}