using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
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

        public int SellValue(ItemStack stack) =>
            IsSellAllItem(stack.Item) ? GetSellPrice(stack.Item) * stack.Count : 0;

        public int CalculateSellTotal(InventoryManager inventory)
        {
            if (inventory == null) return 0;

            int total = 0;
            for (int i = 0; i < inventory.PlayerSlotCount; i++)
                total += SellValue(inventory.GetStack(i));
            return total;
        }

        public bool SellSingle(InventoryManager inventory, PlayerMoney playerMoney, int slotIndex)
        {
            // Shop mirrors player slots only; a higher index would reach the building slot.
            if (inventory == null || playerMoney == null || (uint)slotIndex >= (uint)inventory.PlayerSlotCount) return false;

            Item item = inventory.GetStack(slotIndex).Item;
            if (item == null) return false;

            if (!item.CanSell)
            {
                NotificationManager.Notify("This item can't be sold.");
                return false;
            }

            playerMoney.AddMoney(GetSellPrice(item));
            inventory.RemoveOneAt(slotIndex);
            return true;
        }

        public bool SellAll(InventoryManager inventory, PlayerMoney playerMoney)
        {
            if (inventory == null || playerMoney == null) return false;

            var sold = inventory.RemoveAllWhere(IsSellAllItem);
            if (sold.Count == 0) return false;

            int total = 0;
            foreach (var pair in sold)
                total += GetSellPrice(pair.Key) * pair.Value;
            playerMoney.AddMoney(total);

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

            if (item.ItemType == ItemType.Consumable || item.ItemType == ItemType.Normal)
            {
                if (!inventory.AddItem(item)) return false;

                playerMoney.Use(item.ItemPrice);
                TutorialEvents.RaiseAction(TutorialActionId.BuyItem);
                return true;
            }

            return false;
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
