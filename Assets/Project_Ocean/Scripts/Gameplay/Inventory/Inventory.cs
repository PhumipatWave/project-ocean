using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Access point to the live inventory for gameplay code, without referencing the UI class.</summary>
    public static class Inventory
    {
        public static IInventory Current { get; private set; }

        /// <summary>Raised whenever inventory contents change (InventoryManager.OnInventoryChanged forwards here).</summary>
        public static event System.Action OnChanged;

        public static void RaiseChanged() => OnChanged?.Invoke();

        public static void Register(IInventory inventory) => Current = inventory;

        // Only the registered instance may clear it, so a destroyed duplicate can't wipe the live one.
        public static void Unregister(IInventory inventory)
        {
            if (Current == inventory) Current = null;
        }
    }
}
