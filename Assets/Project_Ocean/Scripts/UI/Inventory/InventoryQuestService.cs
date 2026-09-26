using UnityEngine;

namespace Game.Testing
{
    public static class InventoryQuestService
    {
        public static bool HasQuestItem(InventorySlot[] slots, int itemId)
        {
            DraggableItem itemInSlot = FindItemById(slots, itemId);
            return itemInSlot != null;
        }

        /// <summary>Total held count of an item id across every slot — items can stack across
        /// more than one slot, so this isn't just the first match's count.</summary>
        public static int GetQuestItemCount(InventorySlot[] slots, int itemId)
        {
            if (slots == null) return 0;

            int total = 0;
            foreach (InventorySlot slot in slots)
            {
                if (slot == null) continue;
                DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
                if (item != null && item.GetItem() != null && item.GetItem().ItemID == itemId)
                    total += item.count;
            }
            return total;
        }

        public static bool RemoveQuestItem(InventorySlot[] slots, int itemId, int count = 1)
        {
            int remaining = count;
            foreach (InventorySlot slot in slots)
            {
                if (slot == null || remaining <= 0) continue;
                DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
                if (item == null || item.GetItem() == null || item.GetItem().ItemID != itemId) continue;

                int take = Mathf.Min(item.count, remaining);
                item.count -= take;
                remaining -= take;

                if (item.count <= 0)
                    Object.Destroy(item.gameObject);
                else
                    item.RefreshCount();
            }
            return remaining == 0;
        }

        private static DraggableItem FindItemById(InventorySlot[] slots, int itemId)
        {
            if (slots == null) return null;

            foreach (InventorySlot slot in slots)
            {
                if (slot == null) continue;

                DraggableItem itemInSlot = slot.GetComponentInChildren<DraggableItem>();
                if (itemInSlot != null && itemInSlot.GetItem() != null && itemInSlot.GetItem().ItemID == itemId)
                    return itemInSlot;
            }

            return null;
        }
    }
}
