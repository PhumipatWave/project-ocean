namespace Game.Testing
{
    public class InventoryItemDatabase
    {
        private readonly Item[] allItems;

        public InventoryItemDatabase(Item[] allItems)
        {
            this.allItems = allItems;
        }

        public Item FindById(int id)
        {
            if (allItems == null) return null;

            foreach (Item item in allItems)
            {
                if (item != null && item.ItemID == id)
                    return item;
            }

            return null;
        }
    }
}
