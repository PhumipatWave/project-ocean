using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Runs an NPC's quest dialogue: offer lines, progress line, and turn-in.</summary>
    public class NpcQuestConversation
    {
        private readonly NPC npc;
        private Quest quest;
        private int lineIndex;

        public NpcQuestConversation(NPC npc) => this.npc = npc;

        public bool IsActive { get; private set; }

        public void Interact()
        {
            if (!IsActive)
                Start();
            else
                Advance();
        }

        public void End() => IsActive = false;

        private void Start()
        {
            quest = npc.ResolveActiveQuest();
            if (quest == null)
                return;

            // A New quest with an unfinished prerequisite isn't offered yet.
            if (quest.Status == QuestStatus.New &&
                QuestManager.Instance != null && !QuestManager.Instance.IsUnlocked(quest))
                return;

            IsActive = true;
            lineIndex = 0;

            npc.ShowPanel();
            ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            var view = npc.View;
            if (view == null)
                return;

            switch (quest.Status)
            {
                case QuestStatus.New:
                    view.SetText(quest.CurrentStep.GetLine(npc.NpcId, DialogueType.Offer, lineIndex));
                    break;
                case QuestStatus.InProgress:
                    view.SetText(quest.CurrentStep.GetLine(npc.NpcId, DialogueType.InProgress));
                    break;
                // The player is about to turn in, so show the Finish line, not a blank.
                case QuestStatus.ReadyToTurnIn:
                case QuestStatus.Finish:
                    view.SetText(quest.CurrentStep.GetLine(npc.NpcId, DialogueType.Finish));
                    break;
                case QuestStatus.Reject:
                    view.SetText("?");
                    break;
            }
        }

        // Talked-to fires only after the player has read the line, never on open.
        private void Advance()
        {
            if (quest.Status == QuestStatus.New)
            {
                lineIndex++;
                var offerLines = quest.CurrentStep.GetLines(npc.NpcId, DialogueType.Offer);
                if (lineIndex < offerLines.Length)
                {
                    npc.View?.SetText(offerLines[lineIndex]);
                    return;
                }

                Finish(DialogueType.Offer);
                npc.QuestOfferView?.ShowPanel(quest, null);
            }
            else if (quest.Status == QuestStatus.ReadyToTurnIn)
            {
                Finish(DialogueType.Finish);

                if (!QuestManager.Instance.TurnInQuest(quest.QuestID))
                    Debug.LogWarning($"[NPCInteract] TurnInQuest failed for id {quest.QuestID}");
            }
            else
            {
                Finish(quest.Status == QuestStatus.Finish ? DialogueType.Finish : DialogueType.InProgress);
            }
        }

        // Cutscene starts after the dialogue closes, once the exchange is actually over.
        private void Finish(DialogueType cutsceneLine)
        {
            npc.RaiseTalkedTo();
            npc.CloseDialogue();

            var id = quest.CurrentStep.GetCutsceneId(npc.NpcId, cutsceneLine);
            if (!string.IsNullOrEmpty(id))
                CutsceneManager.Instance?.Play(id);
        }
    }
}
