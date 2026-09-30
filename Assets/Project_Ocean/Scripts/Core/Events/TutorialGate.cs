namespace Game.Core
{
    /// <summary>
    /// While a Tutorial/Talk checkpoint is active, restricts the player to only the
    /// action that checkpoint requires (ESC/pause is deliberately never gated by this —
    /// see GameHUD/PlayerUIState for that). TutorialPromptUI owns RequiredAction and
    /// CurrentPrompt: it sets them while its prompt is showing and clears them when it hides.
    /// </summary>
    public static class TutorialGate
    {
        public static TutorialActionId? RequiredAction { get; private set; }

        /// <summary>Prompt text currently in effect, or null — re-shown when the player presses a blocked key.</summary>
        public static string CurrentPrompt { get; private set; }

        public static void Set(TutorialActionId requiredAction, string prompt)
        {
            RequiredAction = requiredAction;
            CurrentPrompt = prompt;
        }

        public static void Clear()
        {
            RequiredAction = null;
            CurrentPrompt = null;
        }

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
            NotificationManager.Notify(CurrentPrompt, NotificationChannel.Center);
    }
}
