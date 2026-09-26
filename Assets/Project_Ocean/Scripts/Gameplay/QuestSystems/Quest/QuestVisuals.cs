using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// <summary>
    /// Shared sprite lookups/application for the quest UI panels (QuestSlot, QuestDetailPanel,
    /// QuestTrackerUI, ReviceQuestSlot) — was copy-pasted identically into each of them.
    /// </summary>
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

        private static int HeldCountOf(int itemId) =>
            Inventory.Current != null ? Inventory.Current.GetQuestItemCount(itemId) : 0;

        /// <summary>Delivery only — "{item name} {held}/{required}" (PerItem: one per requirement,
        /// comma-joined; CombinedTotal: one entry against the shared Target Count, name omitted when
        /// several items count), plus
        /// " · {N} Money" appended when the step has RequiredMoney. Null for every other step
        /// type — QuestTrackerUI/ReviceQuestSlot fall back to their normal description text.</summary>
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

            var money = Game.Testing.GameManager.CurPlayer != null
                ? Game.Testing.GameManager.CurPlayer.GetComponent<PlayerMoney>()
                : null;
            int currentMoney = money != null ? money.CurrentMoney : 0;
            return $"{itemsPart} · {Mathf.Min(currentMoney, step.RequiredMoney)}/{step.RequiredMoney} Money";
        }
    }
}
