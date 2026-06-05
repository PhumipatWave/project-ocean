namespace Game.Testing
{
    public interface IItemEffect
    {
        /// <summary>
        /// Called BEFORE ConsumeItem. Return false to block item use without consuming it.
        /// </summary>
        bool CanExecute(Player player);

        /// <summary>
        /// Called after CanExecute passes and the item is consumed from inventory.
        /// </summary>
        void ExecuteItem(Player player);
    }
}

