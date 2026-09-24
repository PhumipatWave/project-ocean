using System;

namespace Game.Testing
{
    /// <summary>Fixed set of teachable player actions a Tutorial/Talk quest step can require.</summary>
    public enum TutorialActionId
    {
        Move,
        Interact,
        BuyItem,
        SellItem,
        Dive,
        OpenQuestBoard,
        Sleep,
        MoveInHouse,
        MoveInShop,
        MoveInUpgradeShop,
        MoveOutHouse,
        MoveOutShop,
        MoveOutUpgradeShop,
        OpenInventory,
        BackToLand,
        HitEnemy,
        // Added at the end — existing authored tutorialAction values are serialized by enum index.
        // Raised when the player clicks the quest detail panel's Accept button (QuestDetailPanel).
        AcceptQuest,
        // Raised when a Rope upgrade is actually bought (UpgradeService.TryBuyUpgrade succeeds).
        UpgradeRope,
        // Raised when the player actually uses an item from the inventory (Player.UseItem succeeds).
        UseItem,
        // Raised when the player swims past the rope's Max Rope Length (OxygenRope.CheckLimit) —
        // only fires while OxygenRope's Enable Breach Timeout is on, same as OnRopeBreachStart.
        BreakRopeLimit,
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

        /// <summary>Call when Blocks() returns true for a direct key press (Sleep/Dive/OpenInventory)
        /// so the player gets a reason instead of the key silently doing nothing. Pulses
        /// TutorialPromptUI's persistent banner rather than a Notify() toast — a toast is
        /// silently dropped while a persistent message owns the notification slot, which is
        /// always the case whenever Blocks() can return true.</summary>
        public static void NotifyBlocked() => NotificationManager.PulsePersistent();
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
