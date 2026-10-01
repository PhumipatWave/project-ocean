using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Inventory contents and rules without Unity objects: player slots first, then building slots.</summary>
    public sealed class InventoryModel
    {
        /// <summary>Raised after any change to contents or locks.</summary>
        public event Action Changed;
        /// <summary>Signed count of a player-visible item change; Load and ClearItems never raise it.</summary>
        public event Action<Item, int> ItemDelta;

        private readonly ItemStack[] stacks;
        private readonly bool[] unlocked;

        public int PlayerSlotCount { get; }
        public int BuildingSlotCount { get; }
        public int SlotCount => stacks.Length;

        public InventoryModel(int playerSlots, int buildingSlots)
        {
            PlayerSlotCount = playerSlots;
            BuildingSlotCount = buildingSlots;
            stacks = new ItemStack[playerSlots + buildingSlots];
            unlocked = new bool[stacks.Length];
            for (int i = 0; i < unlocked.Length; i++) unlocked[i] = true;
        }

        public int BuildingSlot(int buildingIndex) => PlayerSlotCount + buildingIndex;
        public ItemStack Get(int slot) => IsValid(slot) ? stacks[slot] : ItemStack.Empty;
        public bool IsUnlocked(int slot) => IsValid(slot) && unlocked[slot];

        /// <summary>Puts one item in the first slot that fits (empty, or same item below max stack); false when none does.</summary>
        public bool Add(Item item)
        {
            if (item == null) return false;

            bool building = item.ItemType == ItemType.Building;
            int end = building ? SlotCount : PlayerSlotCount;
            for (int i = building ? PlayerSlotCount : 0; i < end; i++)
            {
                if (!unlocked[i]) continue;

                ItemStack s = stacks[i];
                if (!s.IsEmpty && (s.Item != item || s.Count >= item.MaxStackable)) continue;

                stacks[i] = new ItemStack(item, s.IsEmpty ? 1 : s.Count + 1);
                Raise(item, 1);
                return true;
            }
            return false;
        }

        public Item RemoveOne(int slot)
        {
            ItemStack s = Get(slot);
            if (s.IsEmpty) return null;

            stacks[slot] = s.Count > 1 ? new ItemStack(s.Item, s.Count - 1) : ItemStack.Empty;
            Raise(s.Item, -1);
            return s.Item;
        }

        public ItemStack RemoveStack(int slot)
        {
            ItemStack s = Get(slot);
            if (s.IsEmpty) return ItemStack.Empty;

            stacks[slot] = ItemStack.Empty;
            Raise(s.Item, -s.Count);
            return s;
        }

        /// <summary>Total count of an item id in the player slots; a stack may span several slots.</summary>
        public int CountOf(int itemId)
        {
            int total = 0;
            for (int i = 0; i < PlayerSlotCount; i++)
                if (!stacks[i].IsEmpty && stacks[i].Item.ItemID == itemId) total += stacks[i].Count;
            return total;
        }

        /// <summary>Removes up to count of an item id from the player slots; returns how many were removed.</summary>
        public int Remove(int itemId, int count)
        {
            int removed = 0;
            Item item = null;
            for (int i = 0; i < PlayerSlotCount && removed < count; i++)
            {
                ItemStack s = stacks[i];
                if (s.IsEmpty || s.Item.ItemID != itemId) continue;

                int take = Math.Min(s.Count, count - removed);
                stacks[i] = s.Count > take ? new ItemStack(s.Item, s.Count - take) : ItemStack.Empty;
                removed += take;
                item = s.Item;
            }

            if (removed > 0) Raise(item, -removed);
            return removed;
        }

        /// <summary>Empties every player slot whose item matches; returns the removed count per item.</summary>
        public Dictionary<Item, int> RemoveAllWhere(Predicate<Item> match)
        {
            var removed = new Dictionary<Item, int>();
            for (int i = 0; i < PlayerSlotCount; i++)
            {
                ItemStack s = stacks[i];
                if (s.IsEmpty || !match(s.Item)) continue;

                removed[s.Item] = (removed.TryGetValue(s.Item, out int n) ? n : 0) + s.Count;
                stacks[i] = ItemStack.Empty;
            }

            if (removed.Count == 0) return removed;
            Changed?.Invoke();
            foreach (var pair in removed)
                ItemDelta?.Invoke(pair.Key, -pair.Value);
            return removed;
        }

        /// <summary>Moves a stack into an empty, unlocked, different slot; false and no change otherwise.</summary>
        public bool Move(int from, int to)
        {
            if (from == to || Get(from).IsEmpty || !IsUnlocked(to) || !Get(to).IsEmpty) return false;

            stacks[to] = stacks[from];
            stacks[from] = ItemStack.Empty;
            Changed?.Invoke();
            return true;
        }

        /// <summary>Swaps two unlocked slots' stacks (either may be empty); false and no change when a stack can't go there.</summary>
        public bool Swap(int a, int b)
        {
            if (a == b || !IsUnlocked(a) || !IsUnlocked(b)) return false;
            if (Get(a).IsEmpty && Get(b).IsEmpty) return false;
            if (!Fits(stacks[a], b) || !Fits(stacks[b], a)) return false;

            (stacks[a], stacks[b]) = (stacks[b], stacks[a]);
            Changed?.Invoke();
            return true;
        }

        // Same rule as Add: building items live only in building slots, everything else only in player slots.
        private bool Fits(ItemStack stack, int slot) =>
            stack.IsEmpty || (stack.Item.ItemType == ItemType.Building) == (slot >= PlayerSlotCount);

        /// <summary>Unlocks the first locked player slot; false when all are unlocked.</summary>
        public bool UnlockNext()
        {
            for (int i = 0; i < PlayerSlotCount; i++)
            {
                if (unlocked[i]) continue;
                unlocked[i] = true;
                Changed?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>New-game lock state: only the first firstUnlocked player slots are open.</summary>
        public void ResetLocks(int firstUnlocked)
        {
            for (int i = 0; i < PlayerSlotCount; i++) unlocked[i] = i < firstUnlocked;
            Changed?.Invoke();
        }

        /// <summary>Replaces all contents (save load); arrays longer than the slot count are cut.</summary>
        public void Load(ItemStack[] player, ItemStack[] building)
        {
            Array.Clear(stacks, 0, stacks.Length);
            if (player != null) Array.Copy(player, 0, stacks, 0, Math.Min(player.Length, PlayerSlotCount));
            if (building != null) Array.Copy(building, 0, stacks, PlayerSlotCount, Math.Min(building.Length, BuildingSlotCount));
            Changed?.Invoke();
        }

        public void ClearItems()
        {
            Array.Clear(stacks, 0, stacks.Length);
            Changed?.Invoke();
        }

        public ItemStack[] PlayerStacks() => Slice(0, PlayerSlotCount);
        public ItemStack[] BuildingStacks() => Slice(PlayerSlotCount, BuildingSlotCount);

        private ItemStack[] Slice(int start, int length)
        {
            var copy = new ItemStack[length];
            Array.Copy(stacks, start, copy, 0, length);
            return copy;
        }

        private bool IsValid(int slot) => slot >= 0 && slot < stacks.Length;

        private void Raise(Item item, int delta)
        {
            Changed?.Invoke();
            if (item != null && delta != 0) ItemDelta?.Invoke(item, delta);
        }
    }
}
