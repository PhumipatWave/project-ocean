using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Tutorial checks for UI inside blocking panels, where TutorialGate is cleared.</summary>
    public static class TutorialStep
    {
        /// <summary>Some active quest's current step is a Tutorial step for this action
        /// (scans every active quest, like TutorialPromptUI, not just the tracked one).</summary>
        public static bool IsActive(TutorialActionId action)
        {
            if (QuestManager.Instance == null) return false;

            foreach (var quest in QuestManager.Instance.GetActiveQuests())
            {
                var step = quest.CurrentStep;
                if (quest.Status == QuestStatus.InProgress && step != null &&
                    step.Type == QuestType.Tutorial && step.TutorialAction == action)
                    return true;
            }
            return false;
        }

        /// <summary>A shop-panel tutorial step (sell / buy / upgrade) is active and wants an
        /// action other than this one — so this shop button must stay locked.</summary>
        public static bool BlocksShop(TutorialActionId action)
        {
            foreach (var shopAction in ShopActions)
                if (shopAction != action && IsActive(shopAction))
                    return true;
            return false;
        }

        private static readonly TutorialActionId[] ShopActions =
            { TutorialActionId.SellItem, TutorialActionId.BuyItem, TutorialActionId.UpgradeRope };
    }
}
