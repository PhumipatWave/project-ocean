using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Gameplay
{
    /// <summary>Picks the day's board dailies: one weighted pick per category among the eligible daily quests.</summary>
    public static class DailyQuestRoller
    {
        /// <summary>A category with no eligible daily gets no pick; roll01 is injected so tests can fix it.</summary>
        public static List<Quest> Roll(IEnumerable<Quest> quests, Func<Quest, bool> eligible, Func<float> roll01)
        {
            var picks = new List<Quest>();
            foreach (var category in quests.Where(q => q.IsDaily && eligible(q)).GroupBy(q => q.Category).OrderBy(g => g.Key))
            {
                var pick = WeightedPicker.Pick(category.ToList(), roll01());
                if (pick != null)
                    picks.Add(pick);
            }
            return picks;
        }
    }
}
