using Game.Core;

namespace Game.Gameplay
{
    /// <summary>An item and how many of it one slot holds; Item is null for an empty slot.</summary>
    public readonly struct ItemStack
    {
        public static readonly ItemStack Empty = default;

        public readonly Item Item;
        public readonly int Count;

        public ItemStack(Item item, int count)
        {
            Item = item;
            Count = count;
        }

        public bool IsEmpty => Item == null || Count <= 0;
    }
}
