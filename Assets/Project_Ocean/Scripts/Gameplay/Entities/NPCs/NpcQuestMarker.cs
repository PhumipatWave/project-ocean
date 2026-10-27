using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Shows an NPC's marker while it is the nearby target of the active quest's Talk step.</summary>
    public class NpcQuestMarker
    {
        private readonly NPC npc;
        private readonly GameObject marker;

        public NpcQuestMarker(NPC npc, GameObject marker)
        {
            this.npc = npc;
            this.marker = marker;
        }

        public void Refresh()
        {
            if (marker != null)
                marker.SetActive(ShouldShow());
        }

        // Same unlock gate as NpcQuestConversation.Start; distance matches QuestCompass's hand-off range.
        private bool ShouldShow()
        {
            var quest = npc.ResolveActiveQuest();
            var step = quest?.CurrentStep;
            if (step == null || step.Type != QuestType.Talk || !step.EffectiveNpcIds.Contains(npc.NpcId) ||
                !step.PassesOnTalk(npc.NpcId))
                return false;

            bool targeted = quest.Status switch
            {
                QuestStatus.New => QuestManager.Instance != null && QuestManager.Instance.IsUnlocked(quest),
                QuestStatus.InProgress or QuestStatus.ReadyToTurnIn => true,
                _ => false,
            };
            return targeted && QuestNavigation.IsPlayerNear(npc.transform);
        }
    }
}
