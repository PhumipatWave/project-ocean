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

            var money = GameManager.CurPlayer != null ? GameManager.CurPlayer.GetComponent<PlayerMoney>() : null;
            return money != null && money.CurrentMoney >= step.RequiredMoney;
        }

        /// <summary>WaitingQuest only — records the day this step became current, once. Safe to
        /// call repeatedly (AcceptQuest/CompleteProgress/LoadSaveState all call it) — a no-op once
        /// DayStepBecameCurrent is already set, since a step only becomes current once.</summary>
        public static void StampWaitingStart(QuestStep step)
        {
            if (step != null && step.Type == QuestType.WaitingQuest &&
                step.DayStepBecameCurrent < 0 && DayManager.Instance != null)
                step.DayStepBecameCurrent = DayManager.Instance.CurrentDay;
        }

        // Locks always win over unlocks (see FeatureUnlockService.IsUnlocked) — order here
        // doesn't matter, but unlocking first reads naturally as "grant, then restrict".
        public static void ApplyFeatureChanges(QuestStep step)
        {
            if (step.UnlocksFeatures != null)
                foreach (var feature in step.UnlocksFeatures)
                    FeatureUnlockService.Instance.Unlock(feature);

            if (step.LocksFeatures != null)
                foreach (var feature in step.LocksFeatures)
                    FeatureUnlockService.Instance.Lock(feature);
        }

        /// <summary>
        /// Removes the item(s) the given step consumes — called when that step completes (or at
        /// turn-in for a final ReadyToHandIn step). Delivery steps skip entirely when
        /// ConsumeOnComplete is off. InventoryHeld removes according to its count mode: PerItem
        /// takes each requirement's own count from TargetQuestItemRequirements; CombinedTotal has
        /// no per-item counts to go by (its ids are the plain TargetQuestItemIds list), so it
        /// takes greedily in list order until the shared Target Count is consumed. EnemyKills
        /// (and every non-Delivery type) keeps the single-item field (e.g. a kill bounty's
        /// hand-in loot item) — always consumed, no ConsumeOnComplete toggle for those.
        /// </summary>
        public static void RemoveQuestItems(QuestStep step)
        {
            if (Inventory.Current == null || step == null) return;

            if (step.Type == QuestType.Delivery)
            {
                if (!step.ConsumeOnComplete) return;

                if (step.RequiredMoney > 0)
                {
                    var money = GameManager.CurPlayer != null ? GameManager.CurPlayer.GetComponent<PlayerMoney>() : null;
                    money?.Use(step.RequiredMoney);
                }

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
