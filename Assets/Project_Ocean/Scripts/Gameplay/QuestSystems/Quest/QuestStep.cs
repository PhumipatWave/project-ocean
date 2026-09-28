using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Runtime copy of one QuestStepData checkpoint, with its own live counter. Each step
    /// is self-contained: its own in-progress/turn-in dialogue and its own reward, paid
    /// when THIS step completes.
    /// </summary>
    public class QuestStep
    {
        public QuestType Type;
        public string TargetEnemyId;

        /// <summary>Delivery + EnemyKills: kills of any of these ids count toward TargetCount.
        /// Talk (non-auto-display): every NPC id that counts as this step's target — see
        /// EffectiveNpcIds, which is what actually reads it for that case. Whether talking to
        /// ANY one or ALL of them completes the step is TalkCompletionMode below.</summary>
        public List<string> TargetEnemyIds;
        public TutorialActionId TutorialAction;
        public string TargetQuestItemId;

        /// <summary>Delivery + InventoryHeld + CombinedTotal only — which items count toward the
        /// shared TargetCount, no per-item count. PerItem uses TargetQuestItemRequirements instead.</summary>
        public List<string> TargetQuestItemIds;

        /// <summary>Delivery + InventoryHeld + PerItem only — which items are required and how many of each.</summary>
        public List<QuestItemRequirement> TargetQuestItemRequirements;

        /// <summary>Delivery + InventoryHeld only — see QuestData.DeliveryCountMode.</summary>
        public DeliveryCountMode DeliveryCountMode;

        /// <summary>Delivery only — what drives progress. See QuestData.DeliverySource.</summary>
        public DeliverySource DeliverySource;

        /// <summary>Delivery only — whether completing this step removes the delivered/killed-for
        /// item(s) from inventory (see QuestManager.RemoveQuestItems). Defaults true, matching the
        /// always-consume behavior every Delivery/former-KillCount step had before this existed.</summary>
        public bool ConsumeOnComplete;

        /// <summary>Delivery only — extra money required alongside the item/kill count. See QuestData.requiredMoney.</summary>
        public int RequiredMoney;

        /// <summary>Delivery only — true while item/kill progress alone would complete this step
        /// but RequiredMoney hasn't been paid yet (CurrentCount is deliberately held one short,
        /// since Quest.AddProgress refuses to act on an already-complete step). Cleared once
        /// QuestManager.HandleMoneyChanged finalizes it, or if item/kill progress drops back
        /// below completion. See QuestManager for the only reader/writer.</summary>
        public bool MoneyPending;

        /// <summary>WaitingQuest only — at least this many day-advances must occur after the
        /// step becomes current. See QuestData.requiredDaysToWait.</summary>
        public int RequiredDaysToWait;

        /// <summary>WaitingQuest only — at least this many dives must happen on the day the
        /// wait finishes. See QuestData.requiredDivesThatDay.</summary>
        public int RequiredDivesThatDay;

        /// <summary>WaitingQuest only — DayManager.CurrentDay when this step became current,
        /// -1 until QuestManager stamps it (AcceptQuest/CompleteProgress/LoadSaveState — see
        /// QuestManager.StampWaitingQuestStart, the only writer). Persisted via
        /// QuestSaveData.dayStepBecameCurrent so a save/quit mid-wait isn't lost.</summary>
        public int DayStepBecameCurrent = -1;

        public int TargetCount;
        public QuestCompletionMode CompletionMode;
        public string StepDescription;
        public int CurrentCount;

        /// <summary>Talk (non-auto-display) only — every NPC id that counts as this step's
        /// target (talking to ANY of them completes it, and each independently shows its own
        /// marker/greeting — see NPC.ShouldShowMarker/QuestManager.HandleNpcTalk). Reuses
        /// TargetEnemyIds (already the merged list-or-singular-fallback Quest.BuildSteps
        /// computes for EnemyKills) rather than a separate field. Empty (not null) for every
        /// other case, so callers can Contains()/FirstOrDefault() without a null check.</summary>
        public List<string> EffectiveNpcIds =>
            Type == QuestType.Talk && !AutoDisplayDialogue ? TargetEnemyIds : EmptyNpcIds;
        private static readonly List<string> EmptyNpcIds = new();

        /// <summary>Default/primary speaker for context-free single-NPC lookups (board/tracker
        /// avatar, QuestDetailPanel's Accept/Reject) — the first id in EffectiveNpcIds, or null.
        /// An Auto Display Dialogue step talks to nobody, so it has none — author its lines with
        /// a blank characterId. Other step types (Delivery, etc.) have no default speaker either.
        /// Not used for the compass — see NextTalkTargetNpcId for that.
        /// </summary>
        public string EffectiveNpcId => EffectiveNpcIds.Count > 0 ? EffectiveNpcIds[0] : null;

        /// <summary>Talk (non-auto) only — which NPC the compass/marker should point at right
        /// now. AnyOne: same as EffectiveNpcId, any listed NPC is a valid target. All: the first
        /// listed id NOT already in talkedToNpcIds, so the arrow advances past an NPC once
        /// they're done instead of pointing at them forever (see QuestNavigation.ResolveQuestTarget,
        /// the only reader). Falls back to EffectiveNpcId if every NPC is already talked to
        /// (the step should already be complete by then).</summary>
        public string NextTalkTargetNpcId
        {
            get
            {
                if (TalkCompletionMode == TalkCompletionMode.AnyOne)
                    return EffectiveNpcId;

                foreach (var id in EffectiveNpcIds)
                    if (!talkedToNpcIds.Contains(id))
                        return id;

                return EffectiveNpcId;
            }
        }

        /// <summary>ExploreArea steps reuse TargetEnemyId as the Area Id an ExploreAreaTrigger reports.</summary>
        public string EffectiveAreaId => Type == QuestType.ExploreArea ? TargetEnemyId : null;

        /// <summary>Cutscene steps reuse TargetEnemyId as the Cutscene Id to play — this is
        /// that value, named for what it actually is at this step type.</summary>
        public string EffectiveCutsceneId => Type == QuestType.Cutscene ? TargetEnemyId : null;

        /// <summary>Authored on QuestStepData — marks THIS checkpoint as guided, so TutorialPromptUI shows/gates it.</summary>
        public bool IsGuidedSequence;

        /// <summary>Talk or Cutscene steps only — see QuestStepData.autoDisplayDialogue
        /// (PlayerSelfDialogueUI shows it). On a Cutscene step this takes over from
        /// CutsceneManager's own auto-play-on-current — see its RefreshQuestStep guard.</summary>
        public bool AutoDisplayDialogue;

        /// <summary>Talk (non-auto-display) only — whether ANY or ALL of EffectiveNpcIds must
        /// be talked to. See QuestManager.HandleNpcTalk, the only reader.</summary>
        public TalkCompletionMode TalkCompletionMode;

        private readonly HashSet<string> talkedToNpcIds = new();

        /// <summary>All mode only — records that npcId has been talked to. Returns false (a
        /// no-op for the caller) if it was already recorded, so re-talking to the same NPC
        /// never double-counts progress.</summary>
        public bool MarkNpcTalkedTo(string npcId) => talkedToNpcIds.Add(npcId);

        /// <summary>Optional images for this checkpoint's guide notification.</summary>
        public List<Sprite> NotificationImages;

        // ponytail: the notification banner only shows one image — the first in the list.
        // If a step ever needs to cycle through several, this is the place to extend.
        public Sprite NotificationImage =>
            NotificationImages != null && NotificationImages.Count > 0 ? NotificationImages[0] : null;

        private Dictionary<(string characterId, DialogueType type), string[]> dialogue;
        private Dictionary<(string characterId, DialogueType type), string> cutsceneIds;
        private List<QuestDialogueLine> orderedDialogue = new();

        public string RewardItemId;
        public int RewardMoney;

        /// <summary>Feature(s) unlocked when this checkpoint completes — see FeatureUnlockService.</summary>
        public List<GameFeature> UnlocksFeatures;

        /// <summary>Feature(s) locked when this checkpoint completes — always wins over an
        /// unlock, however/whenever that was granted. See FeatureUnlockService.</summary>
        public List<GameFeature> LocksFeatures;

        public bool IsComplete => CurrentCount >= TargetCount;

        /// <summary>
        /// Delivery only — how much of the requirement is currently satisfied, on the same
        /// scale as TargetCount. PerItem reads TargetQuestItemRequirements (number of
        /// individual requirements fully met); CombinedTotal reads the plain TargetQuestItemIds
        /// instead — no per-item counts to speak of, just summed held count across the listed
        /// items, clamped to TargetCount. Takes a lookup instead of touching InventoryManager
        /// directly, so Quest/QuestStep stay decoupled from the inventory layer — QuestManager
        /// supplies the real one.
        /// </summary>
        public int ComputeDeliveryProgress(Func<string, int> heldCount)
        {
            if (DeliveryCountMode == DeliveryCountMode.PerItem)
            {
                if (TargetQuestItemRequirements == null || TargetQuestItemRequirements.Count == 0)
                    return 0;

                int satisfied = 0;
                foreach (var req in TargetQuestItemRequirements)
                    if (heldCount(req.itemId) >= req.count)
                        satisfied++;
                return satisfied;
            }

            if (TargetQuestItemIds == null || TargetQuestItemIds.Count == 0)
                return 0;

            int total = 0;
            foreach (var itemId in TargetQuestItemIds)
                total += heldCount(itemId);
            return Mathf.Min(total, TargetCount);
        }

        public void SetDialogue(List<QuestDialogueLine> lines)
        {
            dialogue = new Dictionary<(string, DialogueType), string[]>();
            cutsceneIds = new Dictionary<(string, DialogueType), string>();
            orderedDialogue = lines != null ? new List<QuestDialogueLine>(lines) : new List<QuestDialogueLine>();
            if (lines == null)
                return;

            foreach (var line in lines)
            {
                var key = (line.characterId ?? "", line.type);
                dialogue[key] = line.lines ?? System.Array.Empty<string>();
                if (!string.IsNullOrEmpty(line.cutsceneId))
                    cutsceneIds[key] = line.cutsceneId;
            }
        }

        /// <summary>Every authored entry, in authored order and one per entry (the dictionary
        /// above keeps only the last per character+type) — a multi-speaker exchange (Auto
        /// Display Dialogue) plays these front to back, whatever type each was authored as.</summary>
        public IReadOnlyList<QuestDialogueLine> DialogueEntries => orderedDialogue;

        /// <summary>All lines for one character + dialogue type. Empty array if none authored.</summary>
        public string[] GetLines(string characterId, DialogueType type) =>
            dialogue != null && dialogue.TryGetValue((characterId ?? "", type), out var lines)
                ? lines
                : System.Array.Empty<string>();

        public string GetLine(string characterId, DialogueType type, int index = 0)
        {
            var lines = GetLines(characterId, type);
            return index >= 0 && index < lines.Length ? lines[index] : string.Empty;
        }

        /// <summary>Cutscene id authored on this (characterId, type) dialogue entry, if any — see QuestDialogueLine.cutsceneId.</summary>
        public string GetCutsceneId(string characterId, DialogueType type) =>
            cutsceneIds != null && cutsceneIds.TryGetValue((characterId ?? "", type), out var id) ? id : null;

        // Convenience for consumers that don't track their own character id (e.g. QuestDetailPanel) —
        // resolves against EffectiveNpcId (Talk steps only).
        public string[] QuestDialogue => GetLines(EffectiveNpcId, DialogueType.Offer);
        public string QuestionInProgress => GetLine(EffectiveNpcId, DialogueType.InProgress);
        public string AnswerFinish => GetLine(EffectiveNpcId, DialogueType.Finish);
        public string AnswerNotFinish => GetLine(EffectiveNpcId, DialogueType.NotFinish);
        public string AnswerAccept => GetLineFromAnySpeaker(DialogueType.Accept);
        public string AnswerReject => GetLineFromAnySpeaker(DialogueType.Reject);

        /// <summary>Accept/Reject button labels have no speaker context — board quests have no
        /// Talk NPC, and their lines are authored under any characterId (e.g. "questboard").
        /// Prefer the Talk NPC's line, else the first authored entry of this type.</summary>
        private string GetLineFromAnySpeaker(DialogueType type)
        {
            var line = GetLine(EffectiveNpcId, type);
            if (!string.IsNullOrEmpty(line)) return line;

            foreach (var entry in orderedDialogue)
                if (entry.type == type && entry.lines != null && entry.lines.Length > 0)
                    return entry.lines[0];
            return string.Empty;
        }
    }
}
