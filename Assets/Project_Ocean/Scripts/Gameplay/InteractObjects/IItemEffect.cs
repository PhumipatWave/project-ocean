namespace Game.Testing
{
    public interface IItemEffect
    {
        /// <summary>
        /// Called by Player when the item is consumed from inventory (F key).
        /// </summary>
        void ExecuteItem(Player player);
    }
}
