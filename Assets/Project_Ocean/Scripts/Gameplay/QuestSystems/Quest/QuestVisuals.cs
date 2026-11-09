using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Shared sprite lookups for the quest UI panels.</summary>
    public static class QuestVisuals
    {
        /// <summary>Resolves a quest item id (as authored on TargetQuestItemId — a stringified
        /// inventory item id) to that item's icon, or null if unset/unparseable/not found.</summary>
        public static Sprite GetItemSprite(string questItemId)
        {
            if (string.IsNullOrEmpty(questItemId)) return null;
            if (!int.TryParse(questItemId, out int id)) return null;
            return Inventory.Current?.FindItemInDatabase(id)?.ItemVisual;
        }

        /// <summary>Icon of the item the step's {item} text names, so image and text always agree.</summary>
        public static Sprite GetStepItemSprite(QuestStep step) => GetItemSprite(StepItemId(step));

        /// <summary>The shared "show it if we have art, hide it otherwise" pattern every quest
        /// panel applies to its item/avatar Image.</summary>
        public static void Apply(Image image, Sprite sprite)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        private static string ItemNameOf(int itemId) =>
            Inventory.Current?.FindItemInDatabase(itemId)?.ItemName ?? itemId.ToString();

        private static readonly Regex ItemToken = new(@"\{item(?::(\d+))?\}", RegexOptions.Compiled);

        /// <summary>Replaces {item:ID} with that item's name, and {item} with the step's own target item's name.</summary>
        public static string FormatItemTokens(string text, QuestStep step)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains("{item"))
                return text;

            return ItemToken.Replace(text, match =>
            {
                string idText = match.Groups[1].Success ? match.Groups[1].Value : StepItemId(step);
                var inventory = Inventory.Current;
                string name = inventory != null && int.TryParse(idText, out int id)
                    ? inventory.FindItemInDatabase(id)?.ItemName
                    : null;
                if (name != null) return name;

                // No inventory yet during load is expected; text is re-formatted on every read.
                if (inventory != null)
                    Debug.LogWarning($"[QuestVisuals] Unknown item token '{match.Value}' in: {text}");
                return match.Value; // left visible so authoring typos get noticed
            });
        }

        // InventoryHeld authors its items in the lists; TargetQuestItemId can be a stale leftover there.
        private static string StepItemId(QuestStep step)
        {
            if (step == null) return null;
            if (step.Type == QuestType.Delivery && step.DeliverySource == DeliverySource.InventoryHeld)
            {
                if (step.DeliveryCountMode == DeliveryCountMode.PerItem)
                    return step.TargetQuestItemRequirements is { Count: > 0 } reqs ? reqs[0].itemId : null;
                return step.TargetQuestItemIds is { Count: > 0 } ids ? ids[0] : null;
            }
            return step.TargetQuestItemId;
        }

        private static int HeldCountOf(int itemId) =>
            Inventory.Current != null ? Inventory.Current.GetQuestItemCount(itemId) : 0;

        /// <summary>Delivery only: "{item} {held}/{required}", plus " · {N} Money" when money is required; null for other steps.</summary>
        public static string GetDeliveryStatusSuffix(Quest quest)
        {
            var step = quest?.CurrentStep;
            if (step == null || step.Type != QuestType.Delivery || step.DeliverySource == DeliverySource.EnemyKills)
                return null;

            string itemsPart;
            if (step.DeliveryCountMode == DeliveryCountMode.PerItem)
            {
                var reqs = quest.TargetQuestItemRequirements;
                var parts = new System.Collections.Generic.List<string>();
                if (reqs != null)
                    foreach (var req in reqs)
                    {
                        if (!int.TryParse(req.itemId, out int id)) continue;
                        parts.Add($"{ItemNameOf(id)} {Mathf.Min(HeldCountOf(id), req.count)}/{req.count}");
                    }
                itemsPart = string.Join(", ", parts);
            }
            else
            {
                // Multi-item: any mix counts, so naming one item would mislead — show the count only.
                var ids = quest.TargetQuestItemIds;
                string count = $"{quest.CurrentCount}/{quest.TargetCount}";
                itemsPart = ids != null && ids.Count == 1 && int.TryParse(ids[0], out int id)
                    ? $"{ItemNameOf(id)} {count}"
                    : count;
            }

            if (step.RequiredMoney <= 0)
                return itemsPart;

            var money = Game.Gameplay.PlayerRegistry.Local != null
                ? Game.Gameplay.PlayerRegistry.Local.GetComponent<PlayerMoney>()
                : null;
            int currentMoney = money != null ? money.CurrentMoney : 0;
            return $"{itemsPart} · {Mathf.Min(currentMoney, step.RequiredMoney)}/{step.RequiredMoney} Money";
        }
    }
}
