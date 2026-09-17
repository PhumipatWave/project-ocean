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

        public int CalculateSellTotal(InventorySlot[] inventorySlots)
        {
            if (inventorySlots == null) return 0;

            int total = 0;
            foreach (InventorySlot slot in inventorySlots)
            {
                if (slot == null) continue;

                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI == null || itemUI.GetItem() == null || itemUI.GetItem().ItemType != ItemType.Normal) continue;

                total += GetSellPrice(itemUI.GetItem()) * itemUI.count;
            }

            return total;
        }

        public bool SellSingle(InventoryManager inventory, PlayerMoney playerMoney, int slotIndex)
        {
            if (inventory == null || playerMoney == null) return false;

            Draggableltem realItem = inventory.GetItemInSlot(slotIndex);
            if (realItem == null || realItem.GetItem() == null) return false;

            playerMoney.AddMoney(GetSellPrice(realItem.GetItem()));
            RemoveOne(realItem);
            inventory.NotifyChanged();

            if (!HasAnySellableItem(inventory.InventorySlots))
                TutorialEvents.RaiseAction(TutorialActionId.SellItem);

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
                if (itemUI == null || itemUI.GetItem() == null || itemUI.GetItem().ItemType != ItemType.Normal) continue;

                total += GetSellPrice(itemUI.GetItem()) * itemUI.count;
                Object.Destroy(itemUI.gameObject);
                soldAny = true;
            }

            if (!soldAny) return false;

            playerMoney.AddMoney(total);
            inventory.NotifyChanged();

            // SellAll destroys every sellable item in one pass, so this is always empty
            // afterward — checked anyway rather than raising unconditionally, so the two
            // sell paths can't disagree about what "sold everything" means.
            if (!HasAnySellableItem(inventory.InventorySlots))
                TutorialEvents.RaiseAction(TutorialActionId.SellItem);

            return true;
        }

        /// <summary>True if any slot still holds a sellable (ItemType.Normal) item — used to
        /// gate TutorialActionId.SellItem so it only fires once the player has actually sold
        /// everything, not on the first single sale.</summary>
        private static bool HasAnySellableItem(InventorySlot[] slots)
        {
            if (slots == null) return false;

            foreach (InventorySlot slot in slots)
            {
                if (slot == null) continue;

                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI != null && itemUI.GetItem() != null && itemUI.GetItem().ItemType == ItemType.Normal)
                    return true;
            }

            return false;
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
