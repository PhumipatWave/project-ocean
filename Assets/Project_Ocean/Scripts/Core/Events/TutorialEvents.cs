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
}
