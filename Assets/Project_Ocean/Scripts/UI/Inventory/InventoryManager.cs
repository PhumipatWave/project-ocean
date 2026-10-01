using System.Collections.Generic;
using Game.Core;
using TMPro;
using UnityEngine;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>Inventory entry point and view: owns the InventoryModel and draws it into the slot UI.</summary>
    public class InventoryManager : Singleton<InventoryManager>, ISaveParticipant, IInventory
    {
        // Forwards to Inventory.OnChanged so gameplay listens without referencing this UI class.
        public static event System.Action OnInventoryChanged
        {
            add => Inventory.OnChanged += value;
            remove => Inventory.OnChanged -= value;
        }
        public static event System.Action OnInventoryClosed;

        /// <summary>Signed count of a player-visible item change; save load and reset never raise it.</summary>
        public static event System.Action<Item, int> OnItemDelta;

        // Kept public so existing scene and button references do not break.
        public GameObject InventoryMenu;

        [Header("Inventory Slots")]
        [SerializeField] private InventorySlot slotPrefab;
        [SerializeField] private Transform slotContainer;
        // Saves store slot indices: going below an old save's highest index drops those items on load.
        [SerializeField, Min(1)] private int playerSlotCount = 14;
        [SerializeField] private int firstUnlockedSlotCount = 3;
        [SerializeField] private List<InventorySlot> buildingSlots;

        private InventorySlot[] inventorySlots;

        [Header("Item View")]
        [SerializeField] private GameObject InventoryItemPrefab;

        [Header("Item Database")]
        [SerializeField] private Item[] allItems;
        public IReadOnlyList<Item> AllItems => allItems;

        [Header("Input")]
        [SerializeField] private InputReader inputReader;

        [Header("Building Slot Hint UI")]
        [SerializeField] private List<GameObject> buildingSlotUsePanels;     // F key — shown on active slot
        [SerializeField] private List<GameObject> buildingSlotKeyPanels;     // special action key — shown on active slot if item has ActionHint
        [SerializeField] private List<TextMeshProUGUI> buildingSlotKeyTexts; // text label for special key panel

        private int selectedSlot = -1;
        private int activeBuildingSlotIndex = 0;
        private InventoryUIController uiController;
        private InventoryItemDatabase itemDatabase;
        private InventoryModel model;
        private InventorySlot[] slotViews; // same indices as the model: player slots, then building slots

        public int ActiveBuildingSlotIndex => activeBuildingSlotIndex;
        public int PlayerSlotCount => model.PlayerSlotCount;

        // Shares its GameObject; persistence comes from the GameManager root.
        protected override bool DestroyGameObjectOnDuplicate => false;

        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicate) return;

            uiController = GetComponent<InventoryUIController>();
            if (uiController == null)
                uiController = gameObject.AddComponent<InventoryUIController>();

            uiController.Initialize(InventoryMenu);
            itemDatabase = new InventoryItemDatabase(allItems);
            CreateModel();
            Inventory.Register(this);
        }

        private void CreateModel()
        {
            inventorySlots = SlotBuilder.Build(slotPrefab, slotContainer, playerSlotCount, "Slot_Items");
            int buildingCount = buildingSlots != null ? buildingSlots.Count : 0;
            model = new InventoryModel(inventorySlots.Length, buildingCount);
            slotViews = new InventorySlot[model.SlotCount];
            inventorySlots.CopyTo(slotViews, 0);
            for (int i = 0; i < buildingCount; i++)
                slotViews[model.BuildingSlot(i)] = buildingSlots[i];

            model.Changed += HandleModelChanged;
            model.ItemDelta += (item, delta) => OnItemDelta?.Invoke(item, delta);
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
                Inventory.Unregister(this);
            base.OnDestroy();
        }

        private void OnEnable()
        {
            UpgradeService.OnLevelGranted += HandleUpgradeGranted;
            UpgradeService.OnLevelsLoaded += HandleUpgradesLoaded;

            if (inputReader != null)
                inputReader.OnInventoryToggled += HandleInventoryToggle;
        }

        private void OnDisable()
        {
            UpgradeService.OnLevelGranted -= HandleUpgradeGranted;
            UpgradeService.OnLevelsLoaded -= HandleUpgradesLoaded;

            if (inputReader != null)
                inputReader.OnInventoryToggled -= HandleInventoryToggle;
        }

        private void Start()
        {
            SelectSlot(0);
            model.ResetLocks(firstUnlockedSlotCount);
        }

        private void Update()
        {
            if (Input.inputString != null)
            {
                bool isNumber = int.TryParse(Input.inputString, out int number);
                if (isNumber && number >= 1 && number <= 9)
                    SelectSlot(number - 1);
            }
        }

        // ── Open / close / select ─────────────────────────────

        private void HandleInventoryToggle(bool pressed)
        {
            if (!pressed) return;
            // Always allow closing, even though the inventory itself holds the UI lock.
            if (uiController.IsOpen) { HideAndDeselect(); return; }
            if (PlayerUIState.IsLocked) return;
            // TutorialGate never blocks OpenInventory: checking the bag can't derail a guided step.
            if (!FeatureUnlockService.Instance.IsUnlocked(GameFeature.Inventory))
            {
                NotificationManager.Notify("Locked — progress the main quest to unlock the inventory.");
                return;
            }
            OpenInventory();
        }

        public void OpenInventory()
        {
            uiController.ShowInventory();
            PlayerUIState.SetCloser(CloseInventory);
            TutorialEvents.RaiseAction(TutorialActionId.OpenInventory);
        }

        public void CloseInventory() => HideAndDeselect();

        public void CloseInventoryViaButton() => HideAndDeselect();

        private void HideAndDeselect()
        {
            uiController.HideInventory();
            if (IsValidSlotIndex(selectedSlot))
                inventorySlots[selectedSlot].Deselect();
            selectedSlot = -1;
            OnInventoryClosed?.Invoke();
        }

        public void SelectSlot(int newValue)
        {
            if (!IsValidSlotIndex(newValue)) return;

            if (IsValidSlotIndex(selectedSlot))
                inventorySlots[selectedSlot].Deselect();

            inventorySlots[newValue].Select();
            selectedSlot = newValue;
        }

        // ── Contents (IInventory + shop API) ──────────────────

        public bool AddItem(Item item)
        {
            if (item == null) return false;
            if (model.Add(item)) return true;

            NotificationManager.Notify("Inventory is full");
            return false;
        }

        public Item FindItemInDatabase(int id)
        {
            Item found = itemDatabase.FindById(id);
            if (found != null) return found;

            // allItems here and in SpawnCatalog are separate Inspector lists; fall back
            // so a desynced item isn't dropped on load, and log loudly so the list gets fixed.
            if (SpawnCatalog.Instance != null)
            {
                Item fallback = SpawnCatalog.Instance.GetItem(id);
                if (fallback != null)
                {
                    Debug.LogError($"[InventoryManager] Item ID {id} ('{fallback.ItemName}') is missing from " +
                                   $"InventoryManager.allItems but exists in SpawnCatalog — add it to " +
                                   $"the InventoryManager 'Item Database' list in the Inspector to fix this properly.");
                    return fallback;
                }
            }

            return null;
        }

        /// <summary>What a slot holds; player slots are 0..PlayerSlotCount-1 and building slots follow.</summary>
        public ItemStack GetStack(int slot) => model.Get(slot);

        public bool IsSlotUnlocked(int slot) => model.IsUnlocked(slot);

        /// <summary>Slot index of a slot transform, or -1 when it isn't an inventory slot (e.g. a shop slot).</summary>
        public int IndexOfSlot(Transform slotTransform)
        {
            for (int i = 0; i < slotViews.Length; i++)
                if (slotViews[i] != null && slotViews[i].transform == slotTransform) return i;
            return -1;
        }

        /// <summary>Removes one item from a slot and returns it, or null when the slot is empty.</summary>
        public Item RemoveOneAt(int slot) => model.RemoveOne(slot);

        /// <summary>Empties every player slot whose item matches; returns the removed count per item.</summary>
        public Dictionary<Item, int> RemoveAllWhere(System.Predicate<Item> match) => model.RemoveAllWhere(match);

        public bool ConsumeItem(ItemType checkType, out Item foundItem)
        {
            foundItem = PeekItem(checkType);
            if (foundItem == null) return false;
            model.RemoveOne(ActiveBuildingSlot());
            return true;
        }

        public Item PeekItem(ItemType checkType)
        {
            if (model.BuildingSlotCount == 0) return null;
            ItemStack stack = model.Get(ActiveBuildingSlot());
            return !stack.IsEmpty && stack.Item.ItemType == checkType ? stack.Item : null;
        }

        public int GetQuestItemCount(int itemId) => model.CountOf(itemId);

        public void RemoveQuestItem(int itemId, int count = 1) => model.Remove(itemId, count);

        public void ClearBuildingSlot(int index)
        {
            if (index < 0 || index >= model.BuildingSlotCount) return;
            model.RemoveOne(model.BuildingSlot(index));
            RefreshHintUI();
        }

        /// <summary>Destroys every held item and relocks upgrade slots, so a new game starts clean.</summary>
        public void ResetToDefault()
        {
            model.ClearItems();
            model.ResetLocks(firstUnlockedSlotCount);
        }

        // ── Slot unlocks ──────────────────────────────────────

        public void UnlockNextSlot()
        {
            if (!model.UnlockNext())
                GameLog.Info("UnLocked Slot Full!!!");
        }

        /// <summary>Idempotent restore for save loading: new-game locks, then unlocks extraSlots more.</summary>
        public void SetUnlockedExtraSlots(int extraSlots)
        {
            model.ResetLocks(firstUnlockedSlotCount);
            for (int i = 0; i < extraSlots; i++)
                UnlockNextSlot();
        }

        private void HandleUpgradeGranted(UpgradeData data)
        {
            if (data.type == UpgradeType.Inventory)
                UnlockNextSlot();
        }

        private void HandleUpgradesLoaded() =>
            SetUnlockedExtraSlots(UpgradeService.Instance.GetLevel(UpgradeType.Inventory));

        // ── Drag, drop, discard ───────────────────────────────

        /// <summary>Called by DraggableItem when a drag ends; moves the stack if it landed on a free slot.</summary>
        public void HandleItemDropped(Transform fromSlot, Transform toSlot)
        {
            // A refused move leaves the dragged view in the wrong place; re-render puts it back.
            if (!model.Move(IndexOfSlot(fromSlot), IndexOfSlot(toSlot)))
                Render();
        }

        /// <summary>Swaps the stacks of two slot views (gamepad pick-and-place); false when the model refuses.</summary>
        public bool SwapSlots(Transform a, Transform b) => model.Swap(IndexOfSlot(a), IndexOfSlot(b));

        public void RemoveSelectedItem()
        {
            // A gamepad pick (A on a slot, then A on the bin) wins over the hotbar selection.
            int slot = InventorySlot.Picked != null ? IndexOfSlot(InventorySlot.Picked.transform) : selectedSlot;
            InventorySlot.ClearPick();
            if (slot < 0 || slot >= slotViews.Length) return;

            Item item = model.Get(slot).Item;
            if (item == null)
            {
                Debug.LogWarning("Selected slot has no item to remove.");
                return;
            }

            ConfirmationDialog.Instance?.Show($"Discard {item.ItemName}?", () => DiscardStack(slot, item));
        }

        public void DiscardDraggedItem(DraggableItem itemToDiscard)
        {
            if (itemToDiscard == null || itemToDiscard.GetItem() == null) return;

            // The bin doesn't re-parent, so parentAfterDrag is still the slot the drag started in.
            int slot = IndexOfSlot(itemToDiscard.parentAfterDrag);
            if (slot < 0) return;

            Item item = itemToDiscard.GetItem();
            ConfirmationDialog.Instance?.Show($"Discard {item.ItemName}?", () => DiscardStack(slot, item));
        }

        // The dialog waits for the player; only discard if the slot still holds what they confirmed.
        private void DiscardStack(int slot, Item item)
        {
            if (model.Get(slot).Item != item) return;
            GameLog.Info($"[Inventory] Discard {item.ItemName} from slot {slot}");
            model.RemoveStack(slot);
        }

        // ── View ──────────────────────────────────────────────

        private void HandleModelChanged()
        {
            Render();
            Inventory.RaiseChanged();
        }

        private void Render()
        {
            for (int i = 0; i < slotViews.Length; i++)
            {
                InventorySlot view = slotViews[i];
                if (view == null) continue;

                // Building slots are always unlocked in the model; leave their scene setting alone.
                if (i < model.PlayerSlotCount && view.isUnlocked != model.IsUnlocked(i))
                    view.SetLocked(!model.IsUnlocked(i));

                RenderSlot(view, model.Get(i));
            }
        }

        private void RenderSlot(InventorySlot view, ItemStack stack)
        {
            DraggableItem shown = null;
            foreach (DraggableItem child in view.GetComponentsInChildren<DraggableItem>(true))
            {
                if (shown == null && !stack.IsEmpty) { shown = child; continue; }
                // Detach first: Destroy is deferred and a same-frame render must not find it again.
                child.transform.SetParent(null, false);
                Destroy(child.gameObject);
            }

            if (stack.IsEmpty) return;
            if (shown == null)
                shown = Instantiate(InventoryItemPrefab, view.transform).GetComponent<DraggableItem>();
            shown.InitialiseItem(stack.Item, stack.Count);
        }

        public void RefreshBuildingHints() => RefreshHintUI();

        private void RefreshHintUI()
        {
            if (buildingSlots == null) return;

            for (int i = 0; i < buildingSlots.Count; i++)
            {
                bool isActive = i == activeBuildingSlotIndex;

                if (buildingSlotUsePanels != null && i < buildingSlotUsePanels.Count && buildingSlotUsePanels[i] != null)
                    buildingSlotUsePanels[i].SetActive(isActive);

                if (buildingSlotKeyPanels == null || i >= buildingSlotKeyPanels.Count || buildingSlotKeyPanels[i] == null)
                    continue;

                if (!isActive)
                {
                    buildingSlotKeyPanels[i].SetActive(false);
                    continue;
                }

                Item item = model.Get(model.BuildingSlot(i)).Item;
                var effect = item != null && item.ItemPrefab != null ? item.ItemPrefab.GetComponent<IItemEffect>() : null;
                string hint = effect?.ActionHint ?? string.Empty;

#if ENABLE_CHECKPOINT_ITEM
                // Show "T" in the active slot while any checkpoint is placed in the world
                if (string.IsNullOrEmpty(hint) && PlayerRegistry.Local?.CheckpointCount > 0)
                    hint = "T";
#endif

                bool show = !string.IsNullOrEmpty(hint);
                buildingSlotKeyPanels[i].SetActive(show);
                if (show && buildingSlotKeyTexts != null && i < buildingSlotKeyTexts.Count && buildingSlotKeyTexts[i] != null)
                    buildingSlotKeyTexts[i].text = hint;
            }
        }

        private int ActiveBuildingSlot() => model.BuildingSlot(activeBuildingSlotIndex);

        private bool IsValidSlotIndex(int slotIndex) =>
            slotIndex >= 0 && slotIndex < inventorySlots.Length && inventorySlots[slotIndex] != null;

        // ── Save ──────────────────────────────────────────────

        public void Collect(SaveData data)
        {
            data.inventoryItems = InventorySaveMapper.ToSave(model.PlayerStacks());
            data.buildingItems  = InventorySaveMapper.ToSave(model.BuildingStacks());
        }

        public void Apply(SaveData data)
        {
            model.Load(
                InventorySaveMapper.FromSave(data.inventoryItems, model.PlayerSlotCount, FindItemInDatabase, "Inventory"),
                InventorySaveMapper.FromSave(data.buildingItems, model.BuildingSlotCount, FindItemInDatabase, "Building"));
            RefreshBuildingHints();
        }
    }
}
