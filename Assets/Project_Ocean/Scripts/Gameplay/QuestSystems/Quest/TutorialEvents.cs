using System;

namespace Game.Testing
{
    /// <summary>Fixed set of teachable player actions a Tutorial/Talk quest step can require.</summary>
    public enum TutorialActionId
    {
        Move,
        Interact,
    }

    public static class TutorialEvents
    {
        /// <summary>
        /// Raised when the player performs a teachable action. Matched by QuestManager
        /// against a Tutorial/Talk quest step's TutorialAction.
        /// </summary>
        public static event Action<TutorialActionId> OnActionPerformed;

        public static void RaiseAction(TutorialActionId actionId)
        {
            OnActionPerformed?.Invoke(actionId);
        }
    }

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

        /// <summary>True if this specific action should be blocked — a different action is required.</summary>
        public static bool Blocks(TutorialActionId action) =>
            RequiredAction.HasValue && RequiredAction.Value != action;
    }
}
