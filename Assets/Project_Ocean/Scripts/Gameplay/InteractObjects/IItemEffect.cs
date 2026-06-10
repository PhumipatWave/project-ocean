namespace Game.Testing
{
    public interface IItemEffect
    {
        bool CanExecute(Player player);
        void ExecuteItem(Player player);
    }
}

