using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Uses the player's item of a given type: runs its IItemEffect, or spawns its prefab at the player.</summary>
    public static class PlayerItemUse
    {
        public static void Use(Player player, ItemType type)
        {
            var prefab = Inventory.Current.PeekItem(type);
            if (prefab != null)
            {
                var effect = prefab.ItemPrefab?.GetComponent<IItemEffect>();
                if (effect != null && !effect.CanExecute(player))
                {
                    GameLog.Info($"[Player] Item use blocked by CanExecute: {type}");
                    return;
                }

                // KeepInInventory: execute without consuming — the effect clears the slot itself later
                if (effect != null && effect.KeepInInventory)
                {
                    effect.ExecuteItem(player);
                    TutorialEvents.RaiseAction(TutorialActionId.UseItem);
                    return;
                }
            }

            Item itemData;
            bool success = Inventory.Current.ConsumeItem(type, out itemData);

            if (!success)
            {
                GameLog.Info("No item : " + type);
                return;
            }

            Execute(player, itemData);
            TutorialEvents.RaiseAction(TutorialActionId.UseItem);
        }

        private static void Execute(Player player, Item item)
        {
            if (item.ItemPrefab == null)
            {
                Debug.LogWarning($"[Player] Item '{item.ItemName}' has no prefab assigned.");
                return;
            }

            var effect = item.ItemPrefab.GetComponent<IItemEffect>();
            if (effect != null)
            {
                // IItemEffect handles all spawn/teleport logic itself
                effect.ExecuteItem(player);
            }
            else
            {
                // Fallback: plain building item with no special behavior (e.g. MiniHub)
                Object.Instantiate(item.ItemPrefab, player.transform.position, Quaternion.identity);
            }
        }
    }
}
