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

        public static bool RemoveQuestItem(InventorySlot[] slots, int itemId)
        {
            Draggableltem itemInSlot = FindItemById(slots, itemId);
            if (itemInSlot == null) return false;

            itemInSlot.count--;
            if (itemInSlot.count <= 0)
                Object.Destroy(itemInSlot.gameObject);
            else
                itemInSlot.RefreshCount();

            return true;
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
