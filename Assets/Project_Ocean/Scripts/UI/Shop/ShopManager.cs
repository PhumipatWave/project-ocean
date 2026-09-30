using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
        [SerializeField, FormerlySerializedAs("inventorySlot")] private GameObject sellPanel;
        [SerializeField, FormerlySerializedAs("shopbuySlot")] private GameObject buyPanel;

        [Header("Tutorial")]
        [Tooltip("The only item buyable while a Tutorial BuyItem step is active (MQ_1002: Mini Hub).")]
        [SerializeField] private Item tutorialBuyItem;

        [Header("Shop Slots Sell Display")]
        [SerializeField] private InventorySlot shopSlotPrefab;
        [SerializeField] private Transform shopSlotContainer;
        [SerializeField] private GameObject inventoryItemPrefab;
        private InventorySlot[] shopInventorySlots = new InventorySlot[0];

        [Header("Display")]
        [SerializeField] private TMP_Text totalPriceText;
        [SerializeField] private TMP_Text clickedItemPriceText;
        private bool isSellMode = true;

        [Header("Item Preview UI")]
        [SerializeField] private Image previewIcon;
        [SerializeField] private TMP_Text previewName;
        [SerializeField] private TMP_Text previewPrice;
        [SerializeField] private TMP_Text previewDescription;

        private readonly int[] ammoCounts = new int[4];
        private readonly ShopTransactionService transactionService = new ShopTransactionService();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

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

        public void OpenSellWindow()
        {
            if (InventoryManager.Instance != null && InventoryManager.Instance.InventoryMenu != null)
                InventoryManager.Instance.InventoryMenu.SetActive(false);

            SwitchToSellMode();
        }

        public void OpenBuyWindow()
        {
            if (InventoryManager.Instance != null && InventoryManager.Instance.InventoryMenu != null)
                InventoryManager.Instance.InventoryMenu.SetActive(false);

            SwitchToBuyMode();
        }

        public void RefreshShopInventory()
        {
            ClearShopInventoryMirror();

            var inventory = InventoryManager.Instance;
            if (inventory == null) return;
            EnsureShopSlots(inventory.PlayerSlotCount);

            for (int i = 0; i < shopInventorySlots.Length && i < inventory.PlayerSlotCount; i++)
                MirrorInventorySlot(inventory.GetStack(i), inventory.IsSlotUnlocked(i), shopInventorySlots[i]);

            UpdateClickedItemPriceDisplay();
        }

        // Built on first refresh, not Awake: the count comes from the persistent InventoryManager.
        private void EnsureShopSlots(int count)
        {
            if (shopInventorySlots.Length == count) return;

            foreach (InventorySlot old in shopInventorySlots)
                if (old != null) Destroy(old.gameObject);

            shopInventorySlots = SlotBuilder.Build(shopSlotPrefab, shopSlotContainer, count, "InShopSlot_Items");
            foreach (InventorySlot slot in shopInventorySlots)
                slot.isShopSlot = true;
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

        public void SwitchToBuyMode()
        {
            if (sellAllButton != null)    sellAllButton.SetActive(false);
            if (sellSelectButton != null) sellSelectButton.SetActive(false);
            if (sellPanel != null)        sellPanel.SetActive(false);
            if (buyPanel != null)         buyPanel.SetActive(true);

            isSellMode = false;
            if (clickedItemPriceText != null) clickedItemPriceText.text = "Price: 0";
            SelectFirstBuySlot();
        }

        public void SwitchToSellMode()
        {
            if (sellAllButton != null)    sellAllButton.SetActive(true);
            if (sellSelectButton != null) sellSelectButton.SetActive(true);
            if (sellPanel != null)        sellPanel.SetActive(true);
            if (buyPanel != null)         buyPanel.SetActive(false);

            isSellMode = true;
            RefreshSellButton();
            RefreshShopInventory();
        }

        public int CalculateTotal()
        {
            if (InventoryManager.Instance == null) return 0;
            return transactionService.CalculateSellTotal(InventoryManager.Instance);
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

            transactionService.SellSingle(InventoryManager.Instance, playerMoney, slotIndex);
        }

        public void SellAll()
        {
            if (SellAllBlocked()) return;

            PlayerMoney playerMoney = GetPlayerMoney();
            if (playerMoney == null || InventoryManager.Instance == null) return;

            transactionService.SellAll(InventoryManager.Instance, playerMoney);
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
        }

        public void DeselectAllShopSlots()
        {
            foreach (InventorySlot slot in shopInventorySlots)
                if (slot != null) slot.Deselect();
        }

        public void DeselectAllBuySlots()
        {
            if (buyPanel == null) return;
            foreach (ShopBuySlot slot in buyPanel.GetComponentsInChildren<ShopBuySlot>(true))
                slot.Deselect();
        }

        public void SelectFirstBuySlot()
        {
            if (buyPanel == null) return;
            foreach (ShopBuySlot slot in buyPanel.GetComponentsInChildren<ShopBuySlot>(true))
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

        private void UpdateUI()
        {
            if (totalPriceText != null)
                totalPriceText.text = "Total Value: " + CalculateTotal();
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

        private void MirrorInventorySlot(ItemStack stack, bool unlocked, InventorySlot shopSlot)
        {
            if (shopSlot == null) return;

            if (!unlocked) { shopSlot.SetLocked(true); return; }

            shopSlot.SetLocked(false);
            shopSlot.Deselect();
            if (stack.IsEmpty) return;

            DraggableItem shopItem = Instantiate(inventoryItemPrefab, shopSlot.transform).GetComponent<DraggableItem>();
            shopItem.InitialiseItem(stack.Item, stack.Count);
            shopItem.enabled = false;
        }

        private Item GetSelectedSellItem()
        {
            InventorySlot slot = GetSelectedSlot();
            if (slot == null || InventoryManager.Instance == null) return null;
            return InventoryManager.Instance.GetStack(GetShopSlotIndex(slot)).Item;
        }

        private Item GetSelectedBuyItem()
        {
            ShopBuySlot slot = GetSelectedShopSlot();
            if (slot == null || slot.currentItem == null) return null;
            return slot.currentItem.GetItem();
        }

        private ShopBuySlot GetSelectedShopSlot()
        {
            if (buyPanel == null) return null;
            foreach (ShopBuySlot slot in buyPanel.GetComponentsInChildren<ShopBuySlot>(true))
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
            if (PlayerRegistry.Local == null) return null;
            return PlayerRegistry.Local.GetComponent<PlayerMoney>();
        }
    }
}
