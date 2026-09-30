using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>Converts slot contents to and from save rows; slotIndex is the index in the given list.</summary>
    public static class InventorySaveMapper
    {
        public static List<ItemSaveData> ToSave(IReadOnlyList<ItemStack> slots)
        {
            var rows = new List<ItemSaveData>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty) continue;
                rows.Add(new ItemSaveData
                {
                    slotIndex = i,
                    itemID    = slots[i].Item.ItemID.ToString(),
                    count     = slots[i].Count,
                });
            }
            return rows;
        }

        /// <summary>Rebuilds slot contents from save rows; bad, unknown or out-of-range rows are logged and skipped.</summary>
        public static ItemStack[] FromSave(List<ItemSaveData> rows, int slotCount, System.Func<int, Item> findItem, string label)
        {
            var slots = new ItemStack[slotCount];
            // Null on saves written before building slots were saved.
            if (rows == null) return slots;

            foreach (var saved in rows)
            {
                if (!int.TryParse(saved.itemID, out int id))
                {
                    Debug.LogWarning($"[InventorySaveMapper] {label} itemID '{saved.itemID}' is not a number");
                    continue;
                }

                Item item = findItem(id);
                if (item == null)
                {
                    // Error, not warning: the item is lost from the player's inventory.
                    Debug.LogError($"[InventorySaveMapper] {label} item ID {id} not found in the item database — dropped on load. " +
                                   "Add its asset to InventoryManager's Item Database list.");
                    continue;
                }

                if (saved.slotIndex < 0 || saved.slotIndex >= slotCount)
                {
                    Debug.LogWarning($"[InventorySaveMapper] {label} slotIndex {saved.slotIndex} out of range");
                    continue;
                }

                slots[saved.slotIndex] = new ItemStack(item, Mathf.Max(1, saved.count));
            }
            return slots;
        }
    }
}
