using System.Collections.Generic;

namespace Game.Testing
{
    /// <summary>The inventory operations gameplay needs; implemented by the UI InventoryManager.</summary>
    public interface IInventory
    {
        IReadOnlyList<Item> AllItems { get; }
        int ActiveBuildingSlotIndex { get; }
        bool AddItem(Item item);
        Item FindItemInDatabase(int id);
        bool ConsumeItem(ItemType checkType, out Item foundItem);
        Item PeekItem(ItemType checkType);
        int GetQuestItemCount(int itemId);
        void RemoveQuestItem(int itemId, int count = 1);
        void ClearBuildingSlot(int index);
        void RefreshBuildingHints();
        void ResetToDefault();
    }
}
