using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Copies quest runtime state to and from QuestSaveData.</summary>
    public static class QuestSaveMapper
    {
        /// <summary>Collects the mutable runtime state (id, status, kill progress) of each quest.</summary>
        public static List<QuestSaveData> ToSaveData(IEnumerable<Quest> quests)
        {
            var list = new List<QuestSaveData>();
            foreach (var q in quests)
                list.Add(new QuestSaveData
                {
                    questId         = q.QuestID,
                    status          = (int)q.Status,
                    currentCount    = q.CurrentCount,
                    currentStepIndex = q.CurrentStepIndex,
                    dayStepBecameCurrent = q.CurrentStep?.DayStepBecameCurrent ?? -1,
                });
            return list;
        }

        /// <summary>Restores quest status + kill progress from saved data; unknown ids are skipped.</summary>
        public static void Apply(List<QuestSaveData> saved, Func<int, Quest> findQuest)
        {
            foreach (var s in saved)
            {
                var quest = findQuest(s.questId);

                if (quest != null)
                {
                    quest.Status           = (QuestStatus)s.status;
                    quest.CurrentStepIndex = s.currentStepIndex; // set before CurrentCount — CurrentCount targets the step at this index
                    quest.CurrentCount     = s.currentCount;

                    if (quest.CurrentStep != null && s.dayStepBecameCurrent >= 0)
                        quest.CurrentStep.DayStepBecameCurrent = s.dayStepBecameCurrent;
                }
            }
        }
    }
}
