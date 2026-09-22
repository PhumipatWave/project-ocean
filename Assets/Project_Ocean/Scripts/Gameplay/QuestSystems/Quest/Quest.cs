using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public enum QuestType
    {
        Delivery,
        ExploreArea,
        Talk,
        Tutorial,
        // Added at the end — existing authored steps are serialized by enum index.
        Cutscene
    }

    /// <summary>Delivery only — what drives step progress. InventoryHeld: live-rechecked
    /// against inventory (QuestManager.HandleInventoryChanged), same as always. EnemyKills:
    /// one-shot kill events matching TargetEnemyIds (replaces the old, now-removed KillCount
    /// QuestType) — merged in here since it shared everything else with Delivery already.</summary>
    public enum DeliverySource
    {
        InventoryHeld,
        EnemyKills,
    }

    public enum QuestStatus
    {
        New,
        InProgress,
        Finish,
        ReadyToTurnIn,
        Reject
    }

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
        /// avatar, QuestDetailPanel's Accept/Reject, the compass target) — the first id in
        /// EffectiveNpcIds, or null. An Auto Display Dialogue step talks to nobody, so it has
        /// none — author its lines with a blank characterId. Other step types (Delivery, etc.)
        /// have no default speaker either.
        /// </summary>
        public string EffectiveNpcId => EffectiveNpcIds.Count > 0 ? EffectiveNpcIds[0] : null;

        /// <summary>ExploreArea steps reuse TargetEnemyId as the Area Id an ExploreAreaTrigger reports.</summary>
        public string EffectiveAreaId => Type == QuestType.ExploreArea ? TargetEnemyId : null;

        /// <summary>Cutscene steps reuse TargetEnemyId as the Cutscene Id to play — this is
        /// that value, named for what it actually is at this step type.</summary>
        public string EffectiveCutsceneId => Type == QuestType.Cutscene ? TargetEnemyId : null;

        /// <summary>Authored on QuestStepData — marks THIS checkpoint as guided, so TutorialPromptUI shows/gates it.</summary>
        public bool IsGuidedSequence;

        /// <summary>Talk steps only — see QuestStepData.autoDisplayDialogue (PlayerSelfDialogueUI shows it).</summary>
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

    [System.Serializable]
    public class Quest
    {
        [SerializeField]
        private int questId;
        public int QuestID => questId;

        [SerializeField]
        private QuestCategory category;
        public QuestCategory Category => category;

        [SerializeField]
        private QuestClassification classification;
        public QuestClassification Classification => classification;

        [SerializeField]
        private QuestStatus status;
        public QuestStatus Status
        {
            get => status;
            set => status = value;
        }

        // ── Checkpoints ──
        // A quest is a sequence of steps; only the current one accumulates progress.
        // Type/TargetEnemyId/TutorialAction/CompletionMode are deliberately NOT exposed
        // here — read them via CurrentStep so it's obvious at the call site that they're
        // the active checkpoint's, not some fixed quest-level fact. TargetCount/CurrentCount
        // stay as quest-level conveniences since they're purely for progress display.
        private List<QuestStep> steps = new();

        [SerializeField]
        private int currentStepIndex;
        public int CurrentStepIndex
        {
            get => currentStepIndex;
            set => currentStepIndex = Mathf.Clamp(value, 0, Mathf.Max(0, steps.Count - 1));
        }

        public int StepCount => steps.Count;

        public QuestStep CurrentStep =>
            steps.Count == 0 ? null : steps[currentStepIndex];

        public string StepDescription => CurrentStep?.StepDescription;

        public int TargetCount => CurrentStep.TargetCount;

        public int CurrentCount
        {
            get => CurrentStep.CurrentCount;
            set => CurrentStep.CurrentCount = value;
        }

        public string Progress => CurrentCount + " / " + TargetCount;

        public bool IsObjectiveComplete => CurrentStep != null && CurrentStep.IsComplete;

        // Sourced from the current checkpoint — no quest-level guided flag. Each step
        // can be guided independently (e.g. a Tutorial step guided, a later Delivery
        // step not).
        public bool IsGuidedSequence => CurrentStep?.IsGuidedSequence ?? false;

        /// <summary>What to do to NextQuestIds' quests when this one reaches Finish.</summary>
        public NextQuestMode NextQuestMode { get; private set; }
        public List<int> NextQuestIds { get; private set; }

        [SerializeField]
        private string questName;
        public string QuestName => questName;

        // Sourced from the current checkpoint — no quest-level NPC data. Only Talk steps
        // have one (see QuestStep.EffectiveNpcId); other step types resolve to null.
        public string NpcId => CurrentStep?.EffectiveNpcId;

        // Quest-level giver (authored on QuestData), falling back to the Talk-step NPC.
        // Display-only (name/avatar) — NpcId above stays the dialogue/navigation speaker.
        private string questGiverNpcId;
        public string DisplayNpcId => string.IsNullOrEmpty(questGiverNpcId) ? NpcId : questGiverNpcId;

        // Sourced from the current checkpoint — was quest-level (QuestData.questItemId),
        // now per-step so different checkpoints of the same quest can target different items.
        public string TargetQuestItemId => CurrentStep?.TargetQuestItemId;

        /// <summary>Delivery + CombinedTotal only — see QuestStep.TargetQuestItemIds.</summary>
        public List<string> TargetQuestItemIds => CurrentStep?.TargetQuestItemIds;

        /// <summary>Delivery + PerItem only — see QuestStep.TargetQuestItemRequirements.</summary>
        public List<QuestItemRequirement> TargetQuestItemRequirements => CurrentStep?.TargetQuestItemRequirements;

        public Quest(QuestData questData)
        {
            questId = questData.questId;
            category = questData.category;

            classification = questData.classification;
            status = questData.status;

            questName = questData.questName;
            questGiverNpcId = questData.questGiverNpcId;

            NextQuestMode = questData.nextQuestMode;
            NextQuestIds = questData.nextQuestIds ?? new List<int>();

            steps = BuildSteps(questData);
            currentStepIndex = 0;
        }

        /// <summary>Builds the runtime step list from QuestData's authored Steps.</summary>
        private static List<QuestStep> BuildSteps(QuestData questData)
        {
            var list = new List<QuestStep>();

            if (questData.steps == null)
                return list;

            foreach (var s in questData.steps)
            {
                // Legacy Delivery steps authored before these became lists only have the
                // singular id set — fall back to that as a one-entry list so old .asset data
                // keeps working without hand-editing every quest.
                var enemyIds = s.targetEnemyIds != null && s.targetEnemyIds.Count > 0
                    ? s.targetEnemyIds
                    : (string.IsNullOrEmpty(s.targetEnemyId) ? new List<string>() : new List<string> { s.targetEnemyId });
                var questItemIds = s.targetQuestItemIds != null && s.targetQuestItemIds.Count > 0
                    ? s.targetQuestItemIds
                    : (string.IsNullOrEmpty(s.targetQuestItemId) ? new List<string>() : new List<string> { s.targetQuestItemId });

                // Only consulted when DeliveryCountMode is PerItem (see ComputeDeliveryProgress)
                // — CombinedTotal uses questItemIds above directly instead. Falls back to that
                // same plain id list, one requirement per id at the step's old shared Target
                // Count, for a step authored before PerItem/requirements existed.
                var itemRequirements = s.targetQuestItemRequirements != null && s.targetQuestItemRequirements.Count > 0
                    ? s.targetQuestItemRequirements
                    : questItemIds.ConvertAll(id => new QuestItemRequirement { itemId = id, count = s.targetCount });

                // PerItem's "how many required" is the requirement count, not an authored
                // Target Count — deriving it here keeps the Inspector from showing a field
                // that would just be redundant/easy to leave mismatched. Talk's All mode is
                // the same idea: "how many NPCs" is just the list length. Never below 1: a
                // 0-target step is "complete" the moment it starts, so AddProgress (which bails
                // on an already-complete step) can never advance past it — the quest sticks on
                // it forever. Talk/Tutorial/Cutscene steps hide this field in the Inspector, so
                // a freshly added step is easily left at 0.
                bool isInventoryHeldPerItem = s.type == QuestType.Delivery &&
                    s.deliverySource == DeliverySource.InventoryHeld &&
                    s.deliveryCountMode == DeliveryCountMode.PerItem;
                bool isTalkAll = s.type == QuestType.Talk && !s.autoDisplayDialogue &&
                    s.talkCompletionMode == TalkCompletionMode.All;
                int targetCount = Mathf.Max(1,
                    isInventoryHeldPerItem ? itemRequirements.Count :
                    isTalkAll ? enemyIds.Count :
                    s.targetCount);

                var step = new QuestStep
                {
                    Type = s.type,
                    TargetEnemyId = s.targetEnemyId,
                    TargetEnemyIds = enemyIds,
                    TutorialAction = s.tutorialAction,
                    TargetQuestItemId = s.targetQuestItemId,
                    TargetQuestItemIds = questItemIds,
                    TargetQuestItemRequirements = itemRequirements,
                    DeliveryCountMode = s.deliveryCountMode,
                    DeliverySource = s.deliverySource,
                    TalkCompletionMode = s.talkCompletionMode,
                    ConsumeOnComplete = s.consumeOnComplete,
                    RequiredMoney = s.requiredMoney,
                    TargetCount = targetCount,
                    CompletionMode = s.completionMode,
                    StepDescription = s.stepDescription,
                    CurrentCount = 0,
                    IsGuidedSequence = s.isGuidedSequence,
                    AutoDisplayDialogue = s.type == QuestType.Talk && s.autoDisplayDialogue,
                    NotificationImages = s.notificationImages,
                    RewardItemId = s.rewardItemId,
                    RewardMoney = s.rewardMoney,
                    UnlocksFeatures = s.unlocksFeatures,
                    LocksFeatures = s.locksFeatures,
                };
                step.SetDialogue(s.dialogue);
                list.Add(step);
            }

            if (list.Count == 0)
                Debug.LogWarning($"[Quest] '{questData.name}' has no Steps authored — it will have no objective.");

            return list;
        }

        /// <summary>
        /// Adds progress to the current checkpoint only. Finishing a checkpoint that
        /// isn't the last one just advances to the next checkpoint (returns false —
        /// the quest overall isn't done). Finishing the last checkpoint flips the
        /// quest's status per that checkpoint's completionMode and returns true.
        /// </summary>
        public bool AddProgress(int amount = 1)
        {
            if (status != QuestStatus.InProgress)
                return false;

            var step = CurrentStep;
            if (step == null || step.IsComplete)
                return false;

            step.CurrentCount = Mathf.Min(step.CurrentCount + amount, step.TargetCount);

            if (!step.IsComplete)
                return false;

            if (currentStepIndex < steps.Count - 1)
            {
                currentStepIndex++;
                return false;
            }

            status = step.CompletionMode == QuestCompletionMode.AutoComplete
                ? QuestStatus.Finish
                : QuestStatus.ReadyToTurnIn;

            return true;
        }

        /// <summary>
        /// Steps that have definitely already completed — for one-time-style migration
        /// purposes only (FeatureUnlockService re-deriving unlocks for a save made before
        /// that feature existed). Progression is sequential and one-way, so every step
        /// before CurrentStepIndex is guaranteed done; the step AT CurrentStepIndex counts
        /// too once the quest has reached Finish/ReadyToTurnIn (that index IS the last step,
        /// and it's what put the quest into that status).
        /// </summary>
        public IEnumerable<QuestStep> GetCompletedSteps()
        {
            int completedCount = (status == QuestStatus.Finish || status == QuestStatus.ReadyToTurnIn)
                ? currentStepIndex + 1
                : currentStepIndex;

            for (int i = 0; i < completedCount && i < steps.Count; i++)
                yield return steps[i];
        }

        public string TrackerText
        {
            get
            {
                if (status == QuestStatus.ReadyToTurnIn)
                    return "READY TO TURN IN";

                return steps.Count > 1
                    ? $"Step {currentStepIndex + 1}/{steps.Count} — {Progress}"
                    : Progress;
            }
        }
    }
}