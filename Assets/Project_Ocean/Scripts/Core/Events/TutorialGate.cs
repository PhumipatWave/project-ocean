namespace Game.Core
{
    /// <summary>While a guided step is active, allows only the action it requires; TutorialPromptUI sets and clears it.</summary>
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

        /// <summary>True if this action is blocked because a different one is required.</summary>
        // Interact is allowed (it opens the door/panel most required actions happen in) except during Move.
        // OpenInventory is read-only, so it is never blocked.
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
