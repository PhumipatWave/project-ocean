using Game.Core;

namespace Game.Gameplay
{
    public enum PlayerInterior { None, House, Shop, UpgradeShop }

    /// <summary>
    /// Tracks which building interior the player is currently inside — purely from the
    /// MoveIn/MoveOut tutorial events DoorQuestMarker already raises on every door use,
    /// independent of whether any quest currently cares about them. Lets QuestCompass always
    /// point at the exit while indoors regardless of quest state (see CurrentExitAction).
    /// </summary>
    public static class PlayerInteriorState
    {
        public static PlayerInterior Current { get; private set; } = PlayerInterior.None;

        static PlayerInteriorState()
        {
            TutorialEvents.OnActionPerformed += HandleAction;
        }

        private static void HandleAction(TutorialActionId action)
        {
            switch (action)
            {
                case TutorialActionId.MoveInHouse: Current = PlayerInterior.House; break;
                case TutorialActionId.MoveInShop: Current = PlayerInterior.Shop; break;
                case TutorialActionId.MoveInUpgradeShop: Current = PlayerInterior.UpgradeShop; break;
                case TutorialActionId.MoveOutHouse:
                case TutorialActionId.MoveOutShop:
                case TutorialActionId.MoveOutUpgradeShop:
                    Current = PlayerInterior.None;
                    break;
            }
        }

        /// <summary>The entrance action paired with an interior's exit action, or null if
        /// <paramref name="exitAction"/> isn't an exit (MoveOutShop -> MoveInShop, ...).</summary>
        public static TutorialActionId? EntryActionFor(TutorialActionId exitAction) => exitAction switch
        {
            TutorialActionId.MoveOutHouse => TutorialActionId.MoveInHouse,
            TutorialActionId.MoveOutShop => TutorialActionId.MoveInShop,
            TutorialActionId.MoveOutUpgradeShop => TutorialActionId.MoveInUpgradeShop,
            _ => null,
        };

        /// <summary>The exit action for whichever interior the player is currently in, or null while outside.</summary>
        public static TutorialActionId? CurrentExitAction => Current switch
        {
            PlayerInterior.House => TutorialActionId.MoveOutHouse,
            PlayerInterior.Shop => TutorialActionId.MoveOutShop,
            PlayerInterior.UpgradeShop => TutorialActionId.MoveOutUpgradeShop,
            _ => null,
        };
    }
}
