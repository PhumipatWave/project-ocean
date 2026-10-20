using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Side effects of a quest step: money check, feature changes, item consumption, waiting start day.</summary>
    public static class QuestStepEffects
    {
        /// <summary>Delivery only — true if the step has no RequiredMoney, or the player can
        /// currently afford it. See QuestStep.RequiredMoney.</summary>
        public static bool MoneyRequirementMet(QuestStep step)
        {
            if (step.RequiredMoney <= 0) return true;

            var money = PlayerMoney.Current;
            return money != null && money.CurrentMoney >= step.RequiredMoney;
        }

        /// <summary>Records the day a WaitingQuest step became current; later calls do nothing.</summary>
        public static void StampWaitingStart(QuestStep step)
        {
            if (step != null && step.Type == QuestType.WaitingQuest &&
                step.DayStepBecameCurrent < 0 && DayManager.Instance != null)
                step.DayStepBecameCurrent = DayManager.Instance.CurrentDay;
        }

        public static void ApplyFeatureChanges(QuestStep step)
        {
            if (step.UnlocksFeatures != null)
                foreach (var feature in step.UnlocksFeatures)
                    FeatureUnlockService.Instance.Unlock(feature);
        }

        /// <summary>Removes the items a step consumes when it completes.</summary>
        // Delivery obeys ConsumeOnComplete. PerItem takes each requirement's count; CombinedTotal takes in
        // list order until Target Count is met. Other step types always consume their single item.
        public static void RemoveQuestItems(QuestStep step)
        {
            if (Inventory.Current == null || step == null) return;

            if (step.Type == QuestType.Delivery)
            {
                if (!step.ConsumeOnComplete) return;

                if (step.RequiredMoney > 0)
                    PlayerMoney.Current?.Use(step.RequiredMoney);

                if (step.DeliverySource == DeliverySource.EnemyKills)
                {
                    if (string.IsNullOrEmpty(step.TargetQuestItemId)) return;
                    if (!int.TryParse(step.TargetQuestItemId, out int lootItemId)) return;
                    Inventory.Current.RemoveQuestItem(lootItemId, step.TargetCount);
                    return;
                }

                if (step.DeliveryCountMode == DeliveryCountMode.PerItem)
                {
                    var requirements = step.TargetQuestItemRequirements;
                    if (requirements == null) return;

                    foreach (var req in requirements)
                        if (!string.IsNullOrEmpty(req.itemId) && int.TryParse(req.itemId, out int reqItemId))
                            Inventory.Current.RemoveQuestItem(reqItemId, req.count);
                }
                else
                {
                    var itemIds = step.TargetQuestItemIds;
                    if (itemIds == null) return;

                    int remaining = step.TargetCount;
                    foreach (var idStr in itemIds)
                    {
                        if (remaining <= 0) break;
                        if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out int listItemId)) continue;

                        int take = Mathf.Min(Inventory.Current.GetQuestItemCount(listItemId), remaining);
                        if (take <= 0) continue;

                        Inventory.Current.RemoveQuestItem(listItemId, take);
                        remaining -= take;
                    }
                }
                return;
            }

            if (string.IsNullOrEmpty(step.TargetQuestItemId)) return;
            if (!int.TryParse(step.TargetQuestItemId, out int itemId)) return;
            Inventory.Current.RemoveQuestItem(itemId, step.TargetCount);
        }
    }
}
