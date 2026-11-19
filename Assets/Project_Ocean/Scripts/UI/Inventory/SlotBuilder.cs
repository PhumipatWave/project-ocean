using UnityEngine;

namespace Game.UI
{
    /// <summary>Fills a layout container with slot instances; slot i is the container's i-th new child.</summary>
    public static class SlotBuilder
    {
        public static InventorySlot[] Build(InventorySlot prefab, Transform container, int count, string namePrefix)
        {
            if (prefab == null || container == null)
            {
                Debug.LogError($"[SlotBuilder] {namePrefix}: slot prefab or container not set");
                return new InventorySlot[0];
            }

            var slots = new InventorySlot[count];
            for (int i = 0; i < count; i++)
            {
                slots[i] = Object.Instantiate(prefab, container);
                slots[i].name = $"{namePrefix} ({i})";
            }
            return slots;
        }
    }
}
