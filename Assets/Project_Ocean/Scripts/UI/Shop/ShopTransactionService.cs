using UnityEngine;

namespace Game.Testing
{
    public class ShopTransactionService
    {
        private const float SellPriceMultiplier = 0.9f;

        public int GetSellPrice(Item item)
        {
            if (item == null) return 0;
            return (int)(item.ItemPrice * SellPriceMultiplier);
        }

        // SellAll only clears Normal-type items; CanSell (Item's Shop Settings) blocks any sale.
        private static bool IsSellAllItem(Item item) =>
            item != null && item.CanSell && item.ItemType == ItemType.Normal;

        public int CalculateSellTotal(InventorySlot[] inventorySlots)
        {
            if (inventorySlots == null) return 0;

            int total = 0;
            foreach (InventorySlot slot in inventorySlots)
            {
                if (slot == null) continue;

                DraggableItem itemUI = slot.GetComponentInChildren<DraggableItem>();
                if (itemUI == null || !IsSellAllItem(itemUI.GetItem())) continue;

                total += GetSellPrice(itemUI.GetItem()) * itemUI.count;
            }

            return total;
        }

        public bool SellSingle(InventoryManager inventory, PlayerMoney playerMoney, int slotIndex)
        {
            if (inventory == null || playerMoney == null) return false;

            DraggableItem realItem = inventory.GetItemInSlot(slotIndex);
            if (realItem == null || realItem.GetItem() == null) return false;

            if (!realItem.GetItem().CanSell)
            {
                NotificationManager.Notify("This item can't be sold.");
                return false;
            }

            Item sold = realItem.GetItem();
            playerMoney.AddMoney(GetSellPrice(sold));
            RemoveOne(realItem);
            inventory.NotifyChanged();
            inventory.RaiseItemDelta(sold, -1);
            return true;
        }

        public bool SellAll(InventoryManager inventory, PlayerMoney playerMoney)
        {
            if (inventory == null || playerMoney == null) return false;

            int total = 0;
            // One notification per item type, not per slot.
            var soldCounts = new System.Collections.Generic.Dictionary<Item, int>();
            foreach (InventorySlot slot in inventory.InventorySlots)
            {
                if (slot == null) continue;

                DraggableItem itemUI = slot.GetComponentInChildren<DraggableItem>();
                if (itemUI == null || !IsSellAllItem(itemUI.GetItem())) continue;

                Item item = itemUI.GetItem();
                total += GetSellPrice(item) * itemUI.count;
                soldCounts[item] = (soldCounts.TryGetValue(item, out int n) ? n : 0) + itemUI.count;
                Object.Destroy(itemUI.gameObject);
            }

            if (soldCounts.Count == 0) return false;

            playerMoney.AddMoney(total);
            inventory.NotifyChanged();
            foreach (var pair in soldCounts)
                inventory.RaiseItemDelta(pair.Key, -pair.Value);

            // SellAll destroys every sellable (Normal-type) item in this one pass, by
            // definition of what it does — reaching here already means "sold everything",
            // no need to re-scan inventory afterward (Object.Destroy doesn't take effect
            // until end of frame, so an immediate re-scan would still see the "destroyed"
            // items and never fire).
            TutorialEvents.RaiseAction(TutorialActionId.SellItem);

            return true;
        }

public bool BuyItem(InventoryManager inventory, PlayerMoney playerMoney, Item item, int[] ammoCounts)
        {
            if (inventory == null || playerMoney == null || item == null) return false;

            if (playerMoney.Money < item.ItemPrice)
            {
                NotificationManager.Notify("Not enough money");
                return false;
            }

            if (item.ItemType == ItemType.Ammo)
            {
                AddAmmo(item, ammoCounts);
                playerMoney.Use(item.ItemPrice);
                TutorialEvents.RaiseAction(TutorialActionId.BuyItem);
                return true;
            }

            if (item.ItemType == ItemType.Building || item.ItemType == ItemType.Normal)
            {
                if (!inventory.AddItem(item)) return false;

                playerMoney.Use(item.ItemPrice);
                TutorialEvents.RaiseAction(TutorialActionId.BuyItem);
                return true;
            }

            return false;
        }

        private void RemoveOne(DraggableItem itemUI)
        {
            itemUI.count--;
            if (itemUI.count <= 0)
                Object.Destroy(itemUI.gameObject);
            else
                itemUI.RefreshCount();
        }

        private void AddAmmo(Item item, int[] ammoCounts)
        {
            if (ammoCounts == null || ammoCounts.Length < 4) return;

            if (item.name.Contains("Ammo1")) ammoCounts[0]++;
            else if (item.name.Contains("Ammo2")) ammoCounts[1]++;
            else if (item.name.Contains("Ammo3")) ammoCounts[2]++;
            else if (item.name.Contains("Ammo4")) ammoCounts[3]++;
        }
    }
}
