using UnityEngine;

namespace Game.Testing
{
    public class InventoryManager : MonoBehaviour
    {
        public static System.Action OnInventoryChanged;

        // Kept public so existing scene and button references do not break.
        public GameObject InventoryMenu;

        [Header("Inventory UI")]
        [SerializeField] private AudioClip openInventorySound;

        [Header("Inventory Slots")]
        [SerializeField] private InventorySlot[] inventorySlots;
        [SerializeField] private int firstUnlockedSlotCount = 5;
        [SerializeField] private int firstBuildingSlotIndex = 14;
        [SerializeField] private int lastBuildingSlotIndex = 15;
        public InventorySlot[] InventorySlots => inventorySlots;

        [Header("Item View")]
        [SerializeField] private GameObject InventoryItemPrefab;

        [Header("Item Database")]
        [SerializeField] private Item[] allItems;

        private int selectedSlot = -1;
        private InventoryUIController uiController;
        private InventoryItemDatabase itemDatabase;

        public static InventoryManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            uiController = GetComponent<InventoryUIController>();
            if (uiController == null)
                uiController = gameObject.AddComponent<InventoryUIController>();

            uiController.Initialize(InventoryMenu, openInventorySound);
            itemDatabase = new InventoryItemDatabase(allItems);
        }

        private void Start()
        {
            SelectSlot(0);
            LockStartingSlots();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Inventory"))
                uiController.ToggleInventory();

            if (Input.inputString != null)
            {
                bool isNumber = int.TryParse(Input.inputString, out int number);
                if (isNumber && number > 0 && number < 2)
                    SelectSlot(number - 1);
            }
        }

        public void OpenInventory()
        {
            uiController.OpenInventory();
        }

        public void CloseInventory()
        {
            uiController.CloseInventory();
        }

        public void CloseInventoryViaButton()
        {
            uiController.OpenInventory();
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
                NotifyInventoryChanged();

            return itemAdded;
        }

        public void AddSlot(InventorySlot newSlot)
        {
            InventorySlot[] newSlots = new InventorySlot[inventorySlots.Length + 1];
            for (int i = 0; i < inventorySlots.Length; i++)
                newSlots[i] = inventorySlots[i];

            newSlots[newSlots.Length - 1] = newSlot;
            inventorySlots = newSlots;
            NotifyInventoryChanged();
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

            return itemDatabase.FindById(id);
        }

        public void SpawnItemAtSlot(Item item, int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex) || item == null) return;

            SpawnNewItem(item, inventorySlots[slotIndex]);
            NotifyInventoryChanged();
        }

        public Item GetSelectedItem()
        {
            Draggableltem itemInSlot = GetItemInSlot(selectedSlot);
            return itemInSlot != null ? itemInSlot.GetItem() : null;
        }

        public bool ConsumeItem(ItemType checkType, out Item foundItem)
        {
            foundItem = null;

            for (int i = firstBuildingSlotIndex; i <= lastBuildingSlotIndex; i++)
            {
                Draggableltem itemInSlot = GetItemInSlot(i);
                if (itemInSlot == null || itemInSlot.GetItem().ItemType != checkType) continue;

                foundItem = itemInSlot.GetItem();
                RemoveOneItem(itemInSlot);
                NotifyInventoryChanged();
                return true;
            }

            return false;
        }

        public Item PeekItem(ItemType checkType)
        {
            for (int i = firstBuildingSlotIndex; i <= lastBuildingSlotIndex; i++)
            {
                Draggableltem itemInSlot = GetItemInSlot(i);
                if (itemInSlot != null && itemInSlot.GetItem().ItemType == checkType)
                    return itemInSlot.GetItem();
            }

            return null;
        }

        public void UnlockNextSlot()
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] == null || inventorySlots[i].isUnlocked) continue;

                inventorySlots[i].SetLocked(false);
                NotifyInventoryChanged();
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

        public void RemoveQuestItem(int itemId)
        {
            if (InventoryQuestService.RemoveQuestItem(inventorySlots, itemId))
                NotifyInventoryChanged();
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

            Debug.Log($"[Inventory] Remove item {itemInSlot.GetItem().ItemName} from slot {selectedSlot}");
            Destroy(itemInSlot.gameObject);
            NotifyInventoryChanged();
        }

        public Draggableltem GetItemInSlot(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex)) return null;
            return inventorySlots[slotIndex].GetComponentInChildren<Draggableltem>();
        }

        public void NotifyChanged()
        {
            NotifyInventoryChanged();
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
            for (int i = firstBuildingSlotIndex; i <= lastBuildingSlotIndex; i++)
            {
                if (TryAddToSlot(item, inventorySlots[i], false))
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

        private void SpawnNewItem(Item item, InventorySlot slot)
        {
            GameObject newItemGo = Instantiate(InventoryItemPrefab, slot.transform);
            Draggableltem inventoryItem = newItemGo.GetComponent<Draggableltem>();
            inventoryItem.InitialiseItem(item);
        }

        private void RemoveOneItem(Draggableltem itemInSlot)
        {
            itemInSlot.count--;
            if (itemInSlot.count <= 0)
                Destroy(itemInSlot.gameObject);
            else
                itemInSlot.RefreshCount();
        }

        private bool IsBuildingSlot(int slotIndex)
        {
            return slotIndex >= firstBuildingSlotIndex && slotIndex <= lastBuildingSlotIndex;
        }

        private bool IsValidSlotIndex(int slotIndex)
        {
            return inventorySlots != null && slotIndex >= 0 && slotIndex < inventorySlots.Length && inventorySlots[slotIndex] != null;
        }

        private void NotifyInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }
    }
}
