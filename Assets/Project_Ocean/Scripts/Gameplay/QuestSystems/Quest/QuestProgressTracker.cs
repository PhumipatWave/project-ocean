using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Listens to gameplay events and advances the current step of every matching InProgress quest.</summary>
    public class QuestProgressTracker
    {
        private readonly List<Quest> quests;
        private readonly Action<Quest, int> completeProgress;
        private readonly Action raiseUpdated;

        public QuestProgressTracker(List<Quest> quests, Action<Quest, int> completeProgress, Action raiseUpdated)
        {
            this.quests = quests;
            this.completeProgress = completeProgress;
            this.raiseUpdated = raiseUpdated;
        }

        public void Subscribe()
        {
            EnemyEvents.OnEnemyKilled += HandleEnemyKilled;
            TutorialEvents.OnActionPerformed += HandleTutorialAction;
            NpcEvents.OnNpcTalkedTo += HandleNpcTalk;
            CutsceneEvents.OnCutsceneFinished += HandleCutsceneFinished;
            ExploreEvents.OnAreaReached += HandleAreaReached;
            Inventory.OnChanged += CheckInventory;
            PlayerMoney.OnMoneyChanged += HandleMoneyChanged;
            DayManager.OnDayAdvanced += CheckDayOrDive;
            DayManager.OnDivedToday += CheckDayOrDive;
        }

        public void Unsubscribe()
        {
            EnemyEvents.OnEnemyKilled -= HandleEnemyKilled;
            TutorialEvents.OnActionPerformed -= HandleTutorialAction;
            NpcEvents.OnNpcTalkedTo -= HandleNpcTalk;
            CutsceneEvents.OnCutsceneFinished -= HandleCutsceneFinished;
            ExploreEvents.OnAreaReached -= HandleAreaReached;
            Inventory.OnChanged -= CheckInventory;
            PlayerMoney.OnMoneyChanged -= HandleMoneyChanged;
            DayManager.OnDayAdvanced -= CheckDayOrDive;
            DayManager.OnDivedToday -= CheckDayOrDive;
        }

        // Not via TryProgress: a completing kill must stay one short while RequiredMoney is unpaid.
        private void HandleEnemyKilled(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return;

            foreach (var q in quests)
            {
                if (q.Status != QuestStatus.InProgress) continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != QuestType.Delivery ||
                    step.DeliverySource != DeliverySource.EnemyKills ||
                    step.TargetEnemyIds == null || !step.TargetEnemyIds.Contains(enemyId))
                    continue;

                if (step.CurrentCount + 1 >= step.TargetCount && !QuestStepEffects.MoneyRequirementMet(step))
                {
                    step.MoneyPending = true;
                    if (step.CurrentCount != step.TargetCount - 1)
                    {
                        step.CurrentCount = step.TargetCount - 1;
                        raiseUpdated();
                    }
                    continue;
                }

                completeProgress(q, 1);
            }
        }

        /// <summary>Completes Delivery steps held back only by RequiredMoney, and always redraws (the tracker shows the balance).</summary>
        private void HandleMoneyChanged(int _)
        {
            raiseUpdated();

            foreach (var q in quests)
            {
                if (q.Status != QuestStatus.InProgress) continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != QuestType.Delivery || !step.MoneyPending) continue;
                if (!QuestStepEffects.MoneyRequirementMet(step)) continue;

                step.MoneyPending = false;
                completeProgress(q, step.TargetCount - step.CurrentCount);
            }
        }

        /// <summary>Re-checks WaitingQuest steps (days passed, dives today) on every day change or dive.</summary>
        public void CheckDayOrDive()
        {
            if (DayManager.Instance == null) return;

            foreach (var q in quests)
            {
                if (q.Status != QuestStatus.InProgress) continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != QuestType.WaitingQuest) continue;

                QuestStepEffects.StampWaitingStart(step);

                bool dayReady = DayManager.Instance.CurrentDay >= step.DayStepBecameCurrent + step.RequiredDaysToWait;
                bool diveReady = DayManager.Instance.DivesToday >= step.RequiredDivesThatDay;
                if (dayReady && diveReady)
                    completeProgress(q, 1);
            }
        }

        private void HandleTutorialAction(TutorialActionId actionId) =>
            TryProgress(QuestType.Tutorial, step => step.TutorialAction == actionId);

        // Not via TryProgress: All mode must not count the same NPC twice.
        private void HandleNpcTalk(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return;

            foreach (var q in quests)
            {
                if (q.Status != QuestStatus.InProgress) continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != QuestType.Talk || !step.EffectiveNpcIds.Contains(npcId))
                    continue;

                // Already talked to this one — no further progress, no re-triggered rewards.
                if (step.TalkCompletionMode == TalkCompletionMode.All && !step.MarkNpcTalkedTo(npcId))
                    continue;

                completeProgress(q, 1);
            }
        }

        private void HandleAreaReached(string areaId)
        {
            if (!string.IsNullOrEmpty(areaId))
                TryProgress(QuestType.ExploreArea, step => step.EffectiveAreaId == areaId);
        }

        private void HandleCutsceneFinished(string cutsceneId)
        {
            if (!string.IsNullOrEmpty(cutsceneId))
                TryProgress(QuestType.Cutscene, step => step.EffectiveCutsceneId == cutsceneId);
        }

        /// <summary>Re-checks held items against Delivery steps on every inventory change.</summary>
        // Below target it only updates the count; reaching it goes through CompleteProgress like any other step.
        public void CheckInventory()
        {
            if (Inventory.Current == null)
                return;

            // Partial PerItem progress doesn't change CurrentCount, but the tracker shows live counts, so redraw anyway.
            bool perItemDisplayStale = false;

            foreach (var q in quests)
            {
                if (q.Status != QuestStatus.InProgress)
                    continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != QuestType.Delivery ||
                    step.DeliverySource != DeliverySource.InventoryHeld || step.IsComplete)
                    continue;

                if (step.DeliveryCountMode == DeliveryCountMode.PerItem)
                    perItemDisplayStale = true;

                int progress = step.ComputeDeliveryProgress(HeldCountOf);

                // Items ready but money unpaid: hold one short (AddProgress ignores a complete step) until HandleMoneyChanged.
                if (progress >= step.TargetCount && !QuestStepEffects.MoneyRequirementMet(step))
                {
                    step.MoneyPending = true;
                    int capped = step.TargetCount - 1;
                    if (step.CurrentCount != capped)
                    {
                        step.CurrentCount = capped;
                        raiseUpdated();
                    }
                    continue;
                }
                step.MoneyPending = false;

                if (progress == step.CurrentCount)
                    continue;

                // Player dropped/sold some — keep the displayed count live.
                if (progress < step.CurrentCount)
                {
                    step.CurrentCount = progress;
                    raiseUpdated();
                    continue;
                }

                // CompleteProgress is the only place that advances, pays and hands off; finishing the step here too paid twice.
                if (progress >= step.TargetCount)
                {
                    completeProgress(q, step.TargetCount - step.CurrentCount);
                }
                else
                {
                    step.CurrentCount = progress;
                    raiseUpdated();
                }
            }

            if (perItemDisplayStale)
                raiseUpdated();
        }

        private static int HeldCountOf(string itemIdStr) =>
            int.TryParse(itemIdStr, out int id) ? Inventory.Current.GetQuestItemCount(id) : 0;

        /// <summary>Progresses every InProgress quest whose current step has this type and a matching target.</summary>
        private void TryProgress(QuestType type, Func<QuestStep, bool> targetMatches)
        {
            foreach (var q in quests)
            {
                if (q.Status != QuestStatus.InProgress)
                    continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != type)
                    continue;

                if (!targetMatches(step))
                    continue;

                completeProgress(q, 1);
            }
        }
    }
}
