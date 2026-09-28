using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        [Header("UI Objects (Drag & Drop Here)")]
        [SerializeField] private GameObject sellAllButton;
        [SerializeField] private GameObject sellSelectButton;
        [SerializeField] private GameObject inventorySlot;
        [SerializeField] private GameObject shopbuySlot;

        [Header("Tutorial")]
        [Tooltip("The only item buyable while a Tutorial BuyItem step is active (MQ_1002: Mini Hub).")]
        [SerializeField] private Item tutorialBuyItem;

        [Header("Shop Slots Sell Display")]
        [SerializeField] private InventorySlot[] shopInventorySlots;
        [SerializeField] private GameObject inventoryItemPrefab;

        [Header("Display")]
        [SerializeField] private TMP_Text totalPriceText;
        [SerializeField] private TMP_Text clickedItemPriceText;
        private bool isSellMode = true;

        [Header("Item Preview UI")]
        [SerializeField] private Image previewIcon;
        [SerializeField] private TMP_Text previewName;
        [SerializeField] private TMP_Text previewPrice;
        [SerializeField] private TMP_Text previewDescription;

        private TMP_Text[] ammoTexts;
        private readonly int[] ammoCounts = new int[4];
        private readonly ShopTransactionService transactionService = new ShopTransactionService();

        private void Awake()
        {
            Instance = this;
            ammoTexts = new TMP_Text[]
            {
                FindTMP("AMM_WP1"), FindTMP("AMM_WP2"),
                FindTMP("AMM_WP3"), FindTMP("AMM_WP4"),
            };
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private static TMP_Text FindTMP(string goName)
        {
            var go = GameObject.Find(goName);
            return go != null ? go.GetComponent<TMP_Text>() : null;
        }

        private void OnEnable()
        {
            InventoryManager.OnInventoryChanged += RefreshShopInventory;
            InventoryManager.OnInventoryClosed += DeselectAllShopSlots;
            InventorySlot.OnShopSlotClicked += HandleShopSlotClicked;
            QuestManager.OnQuestUpdated += RefreshSellButton;
            RefreshSellButton();
        }

        private void OnDisable()
        {
            InventoryManager.OnInventoryChanged -= RefreshShopInventory;
            InventoryManager.OnInventoryClosed -= DeselectAllShopSlots;
            InventorySlot.OnShopSlotClicked -= HandleShopSlotClicked;
            QuestManager.OnQuestUpdated -= RefreshSellButton;
        }

        // The tutorial's SellItem step teaches Sell All, so single Sell stays locked until it's done;
        // a buy/upgrade tutorial step locks both sell buttons.
        private static bool SellBlocked() =>
            TutorialStep.IsActive(TutorialActionId.SellItem) || TutorialStep.BlocksShop(TutorialActionId.SellItem);

        private static bool SellAllBlocked() => TutorialStep.BlocksShop(TutorialActionId.SellItem);

        private void RefreshSellButton()
        {
            SetInteractable(sellSelectButton, !SellBlocked());
            SetInteractable(sellAllButton, !SellAllBlocked());
        }

        private static void SetInteractable(GameObject go, bool interactable)
        {
            var button = go != null ? go.GetComponent<Button>() : null;
            if (button != null) button.interactable = interactable;
        }

        /// <summary>Tutorial reason this item can't be bought right now, or null if it can.</summary>
        private string TutorialBuyBlockReason(Item item)
        {
            if (TutorialStep.BlocksShop(TutorialActionId.BuyItem))
                return "Follow the tutorial step first.";
            if (TutorialStep.IsActive(TutorialActionId.BuyItem) && tutorialBuyItem != null && item != tutorialBuyItem)
                return $"Buy the {tutorialBuyItem.ItemName} first.";
            return null;
        }

        private void HandleShopSlotClicked(InventorySlot clickedSlot)
        {
            DeselectAllShopSlots();
            clickedSlot.Select();
            UpdateClickedItemPriceDisplay();
        }

        private void Update()
        {
            UpdateUI();
        }

        // ──────────────────────────────────────────
        // Panel navigation (replaces UIManager)
        // ──────────────────────────────────────────

        public void OpenFishShopWindow()
        {
            if (InventoryManager.Instance != null && InventoryManager.Instance.InventoryMenu != null)
                InventoryManager.Instance.InventoryMenu.SetActive(false);

            SwitchToSellMode();
        }

        public void OpenShopWindow()
        {
            if (InventoryManager.Instance != null && InventoryManager.Instance.InventoryMenu != null)
                InventoryManager.Instance.InventoryMenu.SetActive(false);

            SwitchToBuyMode();
        }

        // ──────────────────────────────────────────
        // Inventory display
        // ──────────────────────────────────────────

        public void RefreshShopInventory()
        {
            ClearShopInventoryMirror();

            if (InventoryManager.Instance == null) return;
            InventorySlot[] realPlayerSlots = InventoryManager.Instance.InventorySlots;

            for (int i = 0; i < shopInventorySlots.Length; i++)
            {
                if (i >= realPlayerSlots.Length) break;
                MirrorInventorySlot(realPlayerSlots[i], shopInventorySlots[i]);
            }

            UpdateClickedItemPriceDisplay();
        }

        public void UpdateClickedItemPriceDisplay()
        {
            if (clickedItemPriceText == null) return;

            Item selectedItem = isSellMode ? GetSelectedSellItem() : GetSelectedBuyItem();
            int price = 0;

            if (selectedItem != null)
                price = isSellMode ? transactionService.GetSellPrice(selectedItem) : selectedItem.ItemPrice;

            clickedItemPriceText.text = $"Price: {price}";
        }

        // ──────────────────────────────────────────
        // Sell / Buy mode toggle
        // ──────────────────────────────────────────

        public void SwitchToBuyMode()
        {
            if (sellAllButton != null)    sellAllButton.SetActive(false);
            if (sellSelectButton != null) sellSelectButton.SetActive(false);
            if (inventorySlot != null)    inventorySlot.SetActive(false);
            if (shopbuySlot != null)      shopbuySlot.SetActive(true);

            isSellMode = false;
            if (clickedItemPriceText != null) clickedItemPriceText.text = "Price: 0";
            SelectFirstBuySlot();
        }

        public void SwitchToSellMode()
        {
            if (sellAllButton != null)    sellAllButton.SetActive(true);
            if (sellSelectButton != null) sellSelectButton.SetActive(true);
            if (inventorySlot != null)    inventorySlot.SetActive(true);
            if (shopbuySlot != null)      shopbuySlot.SetActive(false);

            isSellMode = true;
            RefreshSellButton();
            RefreshShopInventory();
        }

        // ──────────────────────────────────────────
        // Transactions
        // ──────────────────────────────────────────

        public int CalculateTotal()
        {
            if (InventoryManager.Instance == null) return 0;
            return transactionService.CalculateSellTotal(InventoryManager.Instance.InventorySlots);
        }

        public void Sell()
        {
            if (SellBlocked()) return;

            InventorySlot selectedSlot = GetSelectedSlot();
            if (selectedSlot == null) return;

            int slotIndex = GetShopSlotIndex(selectedSlot);
            if (slotIndex == -1) return;

            PlayerMoney playerMoney = GetPlayerMoney();
            if (playerMoney == null || InventoryManager.Instance == null) return;

            if (transactionService.SellSingle(InventoryManager.Instance, playerMoney, slotIndex))
                Invoke(nameof(RefreshShopInventory), 0.02f);

            StartCoroutine(SellRoutine());
        }

        public IEnumerator SellRoutine()
        {
            yield return new WaitForSeconds(1f);
        }

        public void SellAll()
        {
            if (SellAllBlocked()) return;

            PlayerMoney playerMoney = GetPlayerMoney();
            if (playerMoney == null || InventoryManager.Instance == null) return;

            if (transactionService.SellAll(InventoryManager.Instance, playerMoney))
                Invoke(nameof(RefreshShopInventory), 0.05f);

            StartCoroutine(SellRoutine());
        }

        public void TryBuySelectedItem()
        {
            Item item = GetSelectedBuyItem();
            if (item == null) return;

            string blocked = TutorialBuyBlockReason(item);
            if (blocked != null) { NotificationManager.Notify(blocked); return; }

            ConfirmationDialog.Instance?.Show(
                $"Buy {item.ItemName} for {item.ItemPrice}?",
                BuySelectedItem
            );
        }

        public void BuySelectedItem()
        {
            Item selectedItem = GetSelectedBuyItem();
            if (selectedItem == null || TutorialBuyBlockReason(selectedItem) != null) return;

            PlayerMoney playerMoney = GetPlayerMoney();
            if (playerMoney == null || InventoryManager.Instance == null) return;

            if (transactionService.BuyItem(InventoryManager.Instance, playerMoney, selectedItem, ammoCounts))
            {
                UpdateUI();
                RefreshShopInventory();
            }

            StartCoroutine(SellRoutine());
        }

        // ──────────────────────────────────────────
        // Slot helpers
        // ──────────────────────────────────────────

        public void DeselectAllShopSlots()
        {
            foreach (InventorySlot slot in shopInventorySlots)
                if (slot != null) slot.Deselect();
        }

        public void DeselectAllBuySlots()
        {
            if (shopbuySlot == null) return;
            foreach (ShopBuySlot slot in shopbuySlot.GetComponentsInChildren<ShopBuySlot>(true))
                slot.Deselect();
        }

        public void SelectFirstBuySlot()
        {
            if (shopbuySlot == null) return;
            foreach (ShopBuySlot slot in shopbuySlot.GetComponentsInChildren<ShopBuySlot>(true))
            {
                if (slot.currentItem == null || slot.currentItem.GetItem() == null) continue;
                DeselectAllBuySlots();
                slot.Select();
                ShowItemDetail(slot.currentItem.GetItem());
                UpdateClickedItemPriceDisplay();
                return;
            }
        }

        public void ShowItemDetail(Item item)
        {
            if (item == null) return;
            if (previewIcon != null)        previewIcon.sprite        = item.ItemVisual;
            if (previewName != null)        previewName.text          = item.ItemName;
            if (previewPrice != null)       previewPrice.text         = item.ItemPrice.ToString();
            if (previewDescription != null) previewDescription.text   = item.ItemDescription;
        }

        // ──────────────────────────────────────────
        // Private helpers
        // ──────────────────────────────────────────

        private void UpdateUI()
        {
            if (totalPriceText != null)
                totalPriceText.text = "Total Value: " + CalculateTotal();

            for (int i = 0; i < ammoTexts.Length; i++)
                if (ammoTexts[i] != null)
                    ammoTexts[i].text = ammoCounts[i].ToString();
        }

        private void ClearShopInventoryMirror()
        {
            if (shopInventorySlots == null) return;
            foreach (InventorySlot slot in shopInventorySlots)
            {
                if (slot == null) continue;
                foreach (Transform child in slot.transform)
                    Destroy(child.gameObject);
            }
        }

        private void MirrorInventorySlot(InventorySlot realSlot, InventorySlot shopSlot)
        {
            if (realSlot == null || shopSlot == null) return;

            if (!realSlot.isUnlocked) { shopSlot.SetLocked(true); return; }

            shopSlot.SetLocked(false);
            shopSlot.Deselect();

            DraggableItem realItem = realSlot.GetComponentInChildren<DraggableItem>();
            if (realItem == null) return;

            GameObject newItemGo   = Instantiate(inventoryItemPrefab, shopSlot.transform);
            DraggableItem shopItem = newItemGo.GetComponent<DraggableItem>();
            shopItem.InitialiseItem(realItem.GetItem());
            shopItem.count   = realItem.count;
            shopItem.RefreshCount();
            shopItem.enabled = false;
        }

        private Item GetSelectedSellItem()
        {
            InventorySlot slot = GetSelectedSlot();
            if (slot == null) return null;
            DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
            return item?.GetItem();
        }

        private Item GetSelectedBuyItem()
        {
            ShopBuySlot slot = GetSelectedShopSlot();
            if (slot == null || slot.currentItem == null) return null;
            return slot.currentItem.GetItem();
        }

        private ShopBuySlot GetSelectedShopSlot()
        {
            if (shopbuySlot == null) return null;
            foreach (ShopBuySlot slot in shopbuySlot.GetComponentsInChildren<ShopBuySlot>(true))
                if (slot.isSelected) return slot;
            return null;
        }

        private InventorySlot GetSelectedSlot()
        {
            if (shopInventorySlots == null) return null;
            foreach (InventorySlot slot in shopInventorySlots)
                if (slot != null && slot.isSelected) return slot;
            return null;
        }

        private int GetShopSlotIndex(InventorySlot selectedSlot)
        {
            for (int i = 0; i < shopInventorySlots.Length; i++)
                if (shopInventorySlots[i] == selectedSlot) return i;
            return -1;
        }

        private PlayerMoney GetPlayerMoney()
        {
            if (GameManager.CurPlayer == null) return null;
            return GameManager.CurPlayer.GetComponent<PlayerMoney>();
        }
    }
}
