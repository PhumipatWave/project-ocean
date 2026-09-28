using Game.Core;

namespace Game.Gameplay
{
    public interface IItemEffect
    {
        bool CanExecute(Player player);
        void ExecuteItem(Player player);

        // If true: item stays in inventory when used; cleared only when all placed instances are gone.
        bool KeepInInventory => false;

        // Hint text shown in UI when this item is the active building slot (e.g. "T - Teleport").
        // Empty means no hint panel shown.
        string ActionHint => string.Empty;
    }
}

