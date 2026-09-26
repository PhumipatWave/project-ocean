namespace Game.Testing
{
    /// <summary>
    /// While a Tutorial/Talk checkpoint is active, restricts the player to only the
    /// action that checkpoint requires (ESC/pause is deliberately never gated by this —
    /// see GameHUD/PlayerUIState for that). TutorialPromptUI owns RequiredAction: it
    /// sets it while its prompt is showing and clears it when the prompt hides.
    /// </summary>
    public static class TutorialGate
    {
        public static TutorialActionId? RequiredAction;

        /// <summary>Any restriction active right now (regardless of which action).</summary>
        public static bool IsRestricting => RequiredAction.HasValue;

        /// <summary>
        /// True if this specific action should be blocked — a different action is required.
        /// Interact is never blocked on its own: it's the E-press gesture that opens the
        /// panel/door every other required action (Dive, BuyItem, SellItem, OpenQuestBoard,
        /// Sleep — pressing E on the house door opens the Home Panel the Next Day button
        /// lives in — MoveInHouse/Shop/UpgradeShop) actually fires from, so blocking it would
        /// make the required action unreachable. Only Move needs no Interact press first, so
        /// it's the only required action that still blocks a stray Interact press.
        /// OpenInventory is also never blocked — checking your bag is read-only and can't
        /// derail tutorial progress, unlike Sleep/Dive, so there's no reason to lock it to
        /// whichever single step currently owns the guided sequence.
        /// </summary>
        public static bool Blocks(TutorialActionId action)
        {
            if (!RequiredAction.HasValue || RequiredAction.Value == action) return false;
            if (action == TutorialActionId.Interact)
                return RequiredAction.Value == TutorialActionId.Move;
            if (action == TutorialActionId.OpenInventory)
                return false;
            // Ocean-only steps: the dive is how the player reaches them — blocking it soft-locks
            // the step once the player is back on land.
            if (action == TutorialActionId.Dive && IsOceanAction(RequiredAction.Value))
                return false;
            return true;
        }

        private static bool IsOceanAction(TutorialActionId a) =>
            a == TutorialActionId.BreakRopeLimit || a == TutorialActionId.HitEnemy ||
            a == TutorialActionId.BackToLand     || a == TutorialActionId.UseItem;

        /// <summary>Call when Blocks() rejects a key press, so the player sees the current prompt again.</summary>
        public static void NotifyBlocked() =>
            NotificationManager.Notify(TutorialPromptUI.CurrentPrompt, NotificationChannel.Center);
    }

    /// <summary>
    /// Reads the quest state directly, for UI inside blocking panels (shop, upgrade) — there
    /// TutorialGate.RequiredAction is always null, because TutorialPromptUI hides and clears it
    /// while any panel holds PlayerUIState's lock.
    /// </summary>
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
