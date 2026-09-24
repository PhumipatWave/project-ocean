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

                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI == null || !IsSellAllItem(itemUI.GetItem())) continue;

                total += GetSellPrice(itemUI.GetItem()) * itemUI.count;
            }

            return total;
        }

        public bool SellSingle(InventoryManager inventory, PlayerMoney playerMoney, int slotIndex)
        {
            if (inventory == null || playerMoney == null) return false;

            Draggableltem realItem = inventory.GetItemInSlot(slotIndex);
            if (realItem == null || realItem.GetItem() == null) return false;

            if (!realItem.GetItem().CanSell)
            {
                NotificationManager.Notify("This item can't be sold.");
                return false;
            }

            playerMoney.AddMoney(GetSellPrice(realItem.GetItem()));
            RemoveOne(realItem);
            inventory.NotifyChanged();
            return true;
        }

        public bool SellAll(InventoryManager inventory, PlayerMoney playerMoney)
        {
            if (inventory == null || playerMoney == null) return false;

            int total = 0;
            bool soldAny = false;
            foreach (InventorySlot slot in inventory.InventorySlots)
            {
                if (slot == null) continue;

                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI == null || !IsSellAllItem(itemUI.GetItem())) continue;

                total += GetSellPrice(itemUI.GetItem()) * itemUI.count;
                Object.Destroy(itemUI.gameObject);
                soldAny = true;
            }

            if (!soldAny) return false;

            playerMoney.AddMoney(total);
            inventory.NotifyChanged();

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

        private void RemoveOne(Draggableltem itemUI)
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
