using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Runtime copy of one QuestStepData step, with its own live counter, dialogue and reward.</summary>
    public class QuestStep
    {
        public QuestType Type;
        public string TargetEnemyId;

        /// <summary>EnemyKills: ids whose kills count. Talk: the target NPC ids (read through EffectiveNpcIds).</summary>
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

        /// <summary>Delivery only: remove the delivered items when the step completes (default true).</summary>
        public bool ConsumeOnComplete;

        /// <summary>Delivery only — extra money required alongside the item/kill count. See QuestData.requiredMoney.</summary>
        public int RequiredMoney;

        /// <summary>Delivery only: items are done but RequiredMoney isn't paid; CurrentCount is held one short until it is.</summary>
        public bool MoneyPending;

        /// <summary>WaitingQuest only — at least this many day-advances must occur after the
        /// step becomes current. See QuestData.requiredDaysToWait.</summary>
        public int RequiredDaysToWait;

        /// <summary>WaitingQuest only — at least this many dives must happen on the day the
        /// wait finishes. See QuestData.requiredDivesThatDay.</summary>
        public int RequiredDivesThatDay;

        /// <summary>WaitingQuest only: the day this step became current, -1 until stamped; saved so a quit mid-wait isn't lost.</summary>
        public int DayStepBecameCurrent = -1;

        public int TargetCount;
        public QuestCompletionMode CompletionMode;
        public string StepDescription;
        public int CurrentCount;

        /// <summary>Talk only: every NPC id that counts as the target; empty (never null) for other steps.</summary>
        public List<string> EffectiveNpcIds =>
            Type == QuestType.Talk && !AutoDisplayDialogue ? TargetEnemyIds : EmptyNpcIds;
        private static readonly List<string> EmptyNpcIds = new();

        /// <summary>First target NPC id, used where one speaker is needed (board avatar, Accept/Reject); null if none.</summary>
        public string EffectiveNpcId => EffectiveNpcIds.Count > 0 ? EffectiveNpcIds[0] : null;

        /// <summary>Talk only: the NPC the compass should point at now; in All mode, the first not yet talked to.</summary>
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

        /// <summary>Talk/Cutscene only: lines play through PlayerSelfDialogueUI instead of an NPC or auto-played cutscene.</summary>
        public bool AutoDisplayDialogue;

        /// <summary>Talk (non-auto-display) only — whether ANY or ALL of EffectiveNpcIds must
        /// be talked to. See QuestManager.HandleNpcTalk, the only reader.</summary>
        public TalkCompletionMode TalkCompletionMode;

        private readonly HashSet<string> talkedToNpcIds = new();

        /// <summary>All mode: records an NPC as talked to; false if already recorded, so repeats don't double-count.</summary>
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

        public bool IsComplete => CurrentCount >= TargetCount;

        /// <summary>Delivery only: progress on TargetCount's scale; takes a count lookup so quests don't depend on the inventory.</summary>
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

        /// <summary>Every authored line in order (the lookup keeps only the last per speaker and type); used for multi-speaker dialogue.</summary>
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

        // Board quests have no Talk NPC, so prefer the Talk NPC's line, else the first line of this type.
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
