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

        // Not routed through TryProgress — a kill that would complete the step still has to
        // hold back one short of completion when RequiredMoney isn't paid yet (see
        // MoneyRequirementMet/HandleMoneyChanged), which a plain type+match predicate can't express.
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

        /// <summary>Finalizes any Delivery step that was held back solely on RequiredMoney
        /// (see QuestStep.MoneyPending) now that the balance can cover it. Also pokes
        /// OnQuestUpdated unconditionally — QuestVisuals.GetDeliveryStatusSuffix shows the
        /// player's live balance against RequiredMoney, so the tracker needs to redraw on
        /// every money change even when no step actually completes.</summary>
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

        /// <summary>
        /// Not routed through TryProgress — WaitingQuest is a live state condition (day count +
        /// today's dive count), not a one-shot event match, same reasoning as Delivery's
        /// CheckInventory. Re-checks every InProgress quest on every day-advance or dive.
        /// </summary>
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

        // Not routed through the generic TryProgress — All mode needs per-NPC dedup (talking
        // to the same one twice must not double-count) that a plain type+match predicate can't
        // express, so this owns its own loop over the quests instead.
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

        /// <summary>
        /// Delivery has no single discrete "did the thing" event like every other step type —
        /// it's satisfied by whatever the player is currently holding, which can go up or down
        /// as they pick up/drop/sell items. So instead of TryProgress's one-shot event match,
        /// this re-checks live inventory against the current step's requirements on every
        /// change. Below the requirement, it just updates CurrentCount for display (no reward
        /// side effects — see QuestManager.CompleteProgress's doc). At/above it, routes the completing delta
        /// through the normal CompleteProgress pipeline so Delivery pays out/advances exactly
        /// like every other type.
        /// </summary>
        public void CheckInventory()
        {
            if (Inventory.Current == null)
                return;

            // PerItem progress only counts fully-met requirements, so picking up 3 of 5 leaves
            // CurrentCount unchanged and nothing below fires — but the tracker text shows live
            // held counts (QuestVisuals), so it must still redraw.
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

                // Items are ready but RequiredMoney hasn't been paid yet — hold one short of
                // completion (AddProgress refuses to act on an already-complete step) and flag
                // it; HandleMoneyChanged finalizes once the player can afford it.
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

                // Complete through CompleteProgress — the one place that advances the step, pays its
                // reward, applies its feature changes and hands off to the next quest. An older inline
                // copy of that logic used to run first: it finished the step itself, so CompleteProgress
                // below then paid the reward a SECOND time (a hand-in quest paid again at turn-in), and
                // AutoComplete quests never handed off to their next quest.
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

        /// <summary>
        /// Routes an event to every InProgress quest whose current checkpoint is of the
        /// given type and whose target matches. Shared by HandleTutorialAction/HandleAreaReached/
        /// HandleCutsceneFinished — they differ only in which step type and which field they match on.
        /// </summary>
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
