using UnityEngine;

namespace Game.Testing
{
    public static class InventoryQuestService
    {
        public static bool HasQuestItem(InventorySlot[] slots, int itemId)
        {
            Draggableltem itemInSlot = FindItemById(slots, itemId);
            return itemInSlot != null;
        }

        public static bool RemoveQuestItem(InventorySlot[] slots, int itemId, int count = 1)
        {
            int remaining = count;
            foreach (InventorySlot slot in slots)
            {
                if (slot == null || remaining <= 0) continue;
                Draggableltem item = slot.GetComponentInChildren<Draggableltem>();
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

        private static Draggableltem FindItemById(InventorySlot[] slots, int itemId)
        {
            if (slots == null) return null;

            foreach (InventorySlot slot in slots)
            {
                if (slot == null) continue;

                Draggableltem itemInSlot = slot.GetComponentInChildren<Draggableltem>();
                if (itemInSlot != null && itemInSlot.GetItem() != null && itemInSlot.GetItem().ItemID == itemId)
                    return itemInSlot;
            }

            return null;
        }
    }
}
