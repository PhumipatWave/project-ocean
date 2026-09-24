using System.Collections.Generic;
using Game.Core;
using TMPro;
using UnityEngine;

namespace Game.Testing
{
    public class InventoryManager : MonoBehaviour
    {
        public static System.Action OnInventoryChanged;
        public static System.Action OnInventoryClosed;

        // Kept public so existing scene and button references do not break.
        public GameObject InventoryMenu;

        [Header("Inventory Slots")]
        [SerializeField] private InventorySlot[] inventorySlots;
        [SerializeField] private int firstUnlockedSlotCount = 3;
        [SerializeField] private List<InventorySlot> buildingSlots;
        public InventorySlot[] InventorySlots => inventorySlots;

        [Header("Item View")]
        [SerializeField] private GameObject InventoryItemPrefab;

        [Header("Item Database")]
        [SerializeField] private Item[] allItems;
        public System.Collections.Generic.IReadOnlyList<Item> AllItems => allItems;

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

        public static InventoryManager Instance { get; private set; }
        public int ActiveBuildingSlotIndex => activeBuildingSlotIndex;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            // Persistence is handled by the parent GameManager root's DontDestroyOnLoad.

            uiController = GetComponent<InventoryUIController>();
            if (uiController == null)
                uiController = gameObject.AddComponent<InventoryUIController>();

            uiController.Initialize(InventoryMenu);
            itemDatabase = new InventoryItemDatabase(allItems);
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.OnInventoryToggled += HandleInventoryToggle;
                // Switch item disabled: only 1 building slot now, nothing to cycle.
                //inputReader.OnAmmoSelected += HandleCycleBuildingSlot;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.OnInventoryToggled -= HandleInventoryToggle;
                //inputReader.OnAmmoSelected -= HandleCycleBuildingSlot;
            }
        }

        private void HandleInventoryToggle(bool pressed)
        {
            if (!pressed) return;
            // Always allow closing, even though the inventory itself holds the UI lock.
            if (uiController.IsOpen) { HideAndDeselect(); return; }
            if (PlayerUIState.IsLocked) return;
            // TutorialGate never blocks OpenInventory (see TutorialGate.Blocks) — checking your
            // bag is read-only and can't derail a guided step, so it's always allowed here.
            if (!FeatureUnlockService.Instance.IsUnlocked(GameFeature.Inventory))
            {
                NotificationManager.Notify("Locked — progress the main quest to unlock the inventory.");
                return;
            }
            OpenInventory();
        }

        private void Start()
        {
            SelectSlot(0);
            LockStartingSlots();
            //SelectBuildingSlot(0);
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

        // Switch item disabled: only 1 building slot now. Re-enable with the OnAmmoSelected
        // subscription in OnEnable/OnDisable if more building slots come back.
        /*private void HandleCycleBuildingSlot(bool pressed)
        {
            if (!pressed) return;
            if (buildingSlots != null && buildingSlots.Count > 1)
                CycleBuildingSlot();
        }*/

        public void OpenInventory()
        {
            uiController.ShowInventory();
            PlayerUIState.SetCloser(CloseInventory);
            TutorialEvents.RaiseAction(TutorialActionId.OpenInventory);
        }

        public void CloseInventory()
        {
            HideAndDeselect();
        }

        public void CloseInventoryViaButton()
        {
            HideAndDeselect();
        }

        private void HideAndDeselect()
        {
            uiController.HideInventory();
            if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length && inventorySlots[selectedSlot] != null)
                inventorySlots[selectedSlot].Deselect();
            selectedSlot = -1;
            OnInventoryClosed?.Invoke();
        }

        public void SelectSlot(int newValue)
        {
            if (!IsValidSlotIndex(newValue)) return;

            if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length)
                inventorySlots[selectedSlot].Deselect();

            inventorySlots[newValue].Select();
            selectedSlot = newValue;
        }

        public bool AddItem(Item item)
        {
            if (item == null) return false;

            bool itemAdded = item.ItemType == ItemType.Building
                ? TryAddToBuildingSlots(item) : TryAddToPlayerSlots(item);

            if (itemAdded)
                NotifyChanged();
            else
                NotificationManager.Notify("Inventory is full");

            return itemAdded;
        }

        public void AddSlot(InventorySlot newSlot)
        {
            System.Array.Resize(ref inventorySlots, inventorySlots.Length + 1);
            inventorySlots[inventorySlots.Length - 1] = newSlot;
            NotifyChanged();
        }

        public Item FindItem(ItemType checkType, int checkID)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                Draggableltem itemInSlot = GetItemInSlot(i);
                if (itemInSlot != null &&
                    itemInSlot.GetItem().ItemType == checkType &&
                    itemInSlot.GetItem().ItemID == checkID)
                {
                    return itemInSlot.GetItem();
                }
            }

            return null;
        }

        public Item FindItemInDatabase(int id)
        {
            if (itemDatabase == null)
                itemDatabase = new InventoryItemDatabase(allItems);

            Item found = itemDatabase.FindById(id);
            if (found != null) return found;

            // Fallback: InventoryManager.allItems and GameObjectSpawnManager.allItems
            // are two separate Inspector lists that must be kept in sync manually.
            // If an item is missing from THIS list (but exists in the spawn manager's),
            // it would otherwise be silently dropped on load. Use the spawn manager's
            // database as a safety net and warn loudly so the desync gets fixed at the source.
            if (GameObjectSpawnManager.Instance != null)
            {
                Item fallback = GameObjectSpawnManager.Instance.GetItem(id);
                if (fallback != null)
                {
                    Debug.LogError($"[InventoryManager] Item ID {id} ('{fallback.ItemName}') is missing from " +
                                    $"InventoryManager.allItems but exists in GameObjectSpawnManager — add it to " +
                                    $"the InventoryManager 'Item Database' list in the Inspector to fix this properly.");
                    return fallback;
                }
            }

            return null;
        }

        public void SpawnItemAtSlot(Item item, int slotIndex, int count = 1)
        {
            if (!IsValidSlotIndex(slotIndex) || item == null) return;

            SpawnNewItem(item, inventorySlots[slotIndex], count);
            NotifyChanged();
        }

        public Item GetSelectedItem()
        {
            Draggableltem itemInSlot = GetItemInSlot(selectedSlot);
            return itemInSlot != null ? itemInSlot.GetItem() : null;
        }

        public bool ConsumeItem(ItemType checkType, out Item foundItem)
        {
            foundItem = null;
            if (buildingSlots == null || buildingSlots.Count == 0) return false;

            InventorySlot slot = buildingSlots[activeBuildingSlotIndex];
            if (slot == null) return false;

            Draggableltem itemInSlot = slot.GetComponentInChildren<Draggableltem>();
            if (itemInSlot == null || itemInSlot.GetItem().ItemType != checkType) return false;

            foundItem = itemInSlot.GetItem();
            RemoveOneItem(itemInSlot);
            NotifyChanged();
            return true;
        }

        public Item PeekItem(ItemType checkType)
        {
            if (buildingSlots == null || buildingSlots.Count == 0) return null;

            InventorySlot slot = buildingSlots[activeBuildingSlotIndex];
            if (slot == null) return null;

            Draggableltem itemInSlot = slot.GetComponentInChildren<Draggableltem>();
            if (itemInSlot != null && itemInSlot.GetItem().ItemType == checkType)
                return itemInSlot.GetItem();

            return null;
        }

        public void UnlockNextSlot()
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] == null || inventorySlots[i].isUnlocked) continue;

                inventorySlots[i].SetLocked(false);
                NotifyChanged();
                return;
            }

            Debug.Log("UnLocked Slot Full!!!");
        }

        /// <summary>
        /// Idempotent restore used by save loading: locks slots back to the
        /// new-game state, then unlocks <paramref name="extraSlots"/> additional slots.
        /// Safe to call mid-session (won't over-unlock on repeated loads).
        /// </summary>
        public void SetUnlockedExtraSlots(int extraSlots)
        {
            LockStartingSlots();
            for (int i = 0; i < extraSlots; i++)
                UnlockNextSlot();
        }


        public bool HasQuestItem(int itemId)
        {
            return InventoryQuestService.HasQuestItem(inventorySlots, itemId);
        }

        public int GetQuestItemCount(int itemId)
        {
            return InventoryQuestService.GetQuestItemCount(inventorySlots, itemId);
        }

        public void RemoveQuestItem(int itemId, int count = 1)
        {
            if (InventoryQuestService.RemoveQuestItem(inventorySlots, itemId, count))
                NotifyChanged();
        }

        public void AddRewardItem(int itemId)
        {
            Debug.Log("QuestManager reward item id: " + itemId);

            Item rewardItem = FindItemInDatabase(itemId);
            if (rewardItem == null)
            {
                Debug.LogError($"Item ID '{itemId}' was not found in InventoryManager allItems database.");
                return;
            }

            bool isAdded = AddItem(rewardItem);
            if (isAdded)
                Debug.Log($"[Quest Reward] Player received reward item: {rewardItem.ItemName}");
            else
                Debug.LogWarning("Inventory is full. Could not add quest reward item.");
        }

        public void RemoveSelectedItem()
        {
            if (!IsValidSlotIndex(selectedSlot)) return;

            Draggableltem itemInSlot = GetItemInSlot(selectedSlot);
            if (itemInSlot == null)
            {
                Debug.LogWarning("Selected slot has no item to remove.");
                return;
            }

            string itemName = itemInSlot.GetItem().ItemName;
            int slotToRemove = selectedSlot;
            ConfirmationDialog.Instance?.Show(
                $"Discard {itemName}?",
                () =>
                {
                    Draggableltem toRemove = GetItemInSlot(slotToRemove);
                    if (toRemove == null) return;
                    Debug.Log($"[Inventory] Remove item {toRemove.GetItem().ItemName} from slot {slotToRemove}");
                    Destroy(toRemove.gameObject);
                    NotifyChanged();
                }
            );
        }

        public void DiscardDraggedItem(Draggableltem itemToDiscard)
        {
            if (itemToDiscard == null)
                return;

            Item item = itemToDiscard.GetItem();

            if (item == null)
                return;

            string itemName = item.ItemName;

            ConfirmationDialog.Instance?.Show(
                $"Discard {itemName}?",
                () =>
                {
                    if (itemToDiscard == null)
                        return;

                    Debug.Log(
                        $"[Inventory] Discard dragged item: {itemName}"
                    );

                    Destroy(itemToDiscard.gameObject);
                    NotifyChanged();
                }
            );
        }

        public Draggableltem GetItemInSlot(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex)) return null;
            return inventorySlots[slotIndex].GetComponentInChildren<Draggableltem>();
        }

        /// <summary>Destroys every held item and relocks any slot unlocked via upgrades —
        /// called by SaveBridge.ResetForNewGame() so a fresh game doesn't inherit a previous
        /// playthrough's items/unlocked slots.</summary>
        public void ResetToDefault()
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                var item = GetItemInSlot(i);
                if (item != null)
                    Destroy(item.gameObject);
            }

            LockStartingSlots();
        }

        private void LockStartingSlots()
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] == null) continue;

                if (i >= firstUnlockedSlotCount && !IsBuildingSlot(i))
                    inventorySlots[i].SetLocked(true);
            }
        }

        private bool TryAddToBuildingSlots(Item item)
        {
            if (buildingSlots == null) return false;
            foreach (InventorySlot slot in buildingSlots)
            {
                if (TryAddToSlot(item, slot, false))
                    return true;
            }

            Debug.Log("No space for building item!");
            return false;
        }

        private bool TryAddToPlayerSlots(Item item)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (IsBuildingSlot(i)) continue;

                InventorySlot slot = inventorySlots[i];
                if (slot == null || !slot.isUnlocked) continue;

                if (TryAddToSlot(item, slot, true))
                    return true;
            }

            return false;
        }

        private bool TryAddToSlot(Item item, InventorySlot slot, bool allowDifferentItem)
        {
            if (slot == null) return false;

            Draggableltem itemInSlot = slot.GetComponentInChildren<Draggableltem>();
            if (itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }

            if (itemInSlot.GetItem() != item) return false;

            if (itemInSlot.count >= item.MaxStackable)
            {
                if (!allowDifferentItem)
                    Debug.Log("Full of this building item!");

                return false;
            }

            itemInSlot.count++;
            itemInSlot.RefreshCount();
            return true;
        }

        private void SpawnNewItem(Item item, InventorySlot slot, int count = 1)
        {
            GameObject newItemGo = Instantiate(InventoryItemPrefab, slot.transform);
            Draggableltem inventoryItem = newItemGo.GetComponent<Draggableltem>();
            inventoryItem.InitialiseItem(item, count);
        }

        private void RemoveOneItem(Draggableltem itemInSlot)
        {
            itemInSlot.count--;
            if (itemInSlot.count <= 0)
                Destroy(itemInSlot.gameObject);
            else
                itemInSlot.RefreshCount();
        }

        /*private void SelectBuildingSlot(int index)
        {
            if (buildingSlots == null || buildingSlots.Count == 0) return;
            if (buildingSlots[activeBuildingSlotIndex] != null)
                buildingSlots[activeBuildingSlotIndex].Deselect();
            activeBuildingSlotIndex = index;
            if (buildingSlots[activeBuildingSlotIndex] != null)
                buildingSlots[activeBuildingSlotIndex].Select();
            RefreshHintUI();
        }

        private void CycleBuildingSlot()
        {
            SelectBuildingSlot((activeBuildingSlotIndex + 1) % buildingSlots.Count);
        }*/

        public void ClearBuildingSlot(int index)
        {
            if (buildingSlots == null || index < 0 || index >= buildingSlots.Count) return;
            var draggable = buildingSlots[index]?.GetComponentInChildren<Draggableltem>();
            if (draggable != null) RemoveOneItem(draggable);
            NotifyChanged();
            RefreshHintUI();
        }

        private void RefreshHintUI()
        {
            if (buildingSlots == null) return;

            for (int i = 0; i < buildingSlots.Count; i++)
            {
                bool isActive = i == activeBuildingSlotIndex;

                // F key panel — visible only on the active slot
                if (buildingSlotUsePanels != null && i < buildingSlotUsePanels.Count && buildingSlotUsePanels[i] != null)
                    buildingSlotUsePanels[i].SetActive(isActive);

                // Special action key panel — visible only on active slot AND only if item has ActionHint
                if (buildingSlotKeyPanels != null && i < buildingSlotKeyPanels.Count && buildingSlotKeyPanels[i] != null)
                {
                    if (!isActive)
                    {
                        buildingSlotKeyPanels[i].SetActive(false);
                    }
                    else
                    {
                        var draggable = buildingSlots[i]?.GetComponentInChildren<Draggableltem>();
                        var effect = draggable?.GetItem()?.ItemPrefab?.GetComponent<IItemEffect>();
                        string hint = effect?.ActionHint ?? string.Empty;

                        // Show "T" in the active slot while any checkpoint is placed in the world
                        if (string.IsNullOrEmpty(hint) && GameManager.CurPlayer?.CheckpointCount > 0)
                            hint = "T";

                        bool show = !string.IsNullOrEmpty(hint);
                        buildingSlotKeyPanels[i].SetActive(show);
                        if (show && buildingSlotKeyTexts != null && i < buildingSlotKeyTexts.Count && buildingSlotKeyTexts[i] != null)
                            buildingSlotKeyTexts[i].text = hint;
                    }
                }
            }
        }

        private bool IsBuildingSlot(int slotIndex)
        {
            return buildingSlots != null && buildingSlots.Contains(inventorySlots[slotIndex]);
        }

        private bool IsValidSlotIndex(int slotIndex)
        {
            return inventorySlots != null && slotIndex >= 0 && slotIndex < inventorySlots.Length && inventorySlots[slotIndex] != null;
        }

        public void NotifyChanged()
        {
            OnInventoryChanged?.Invoke();
        }

        public void RefreshBuildingHints() => RefreshHintUI();
    }
}
