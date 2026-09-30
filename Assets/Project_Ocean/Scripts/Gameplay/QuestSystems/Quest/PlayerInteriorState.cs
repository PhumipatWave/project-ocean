using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public enum PlayerInterior { None, House, Shop, UpgradeShop }

    /// <summary>Which building interior the player is in, worked out from the nearest interior exit door.</summary>
    public static class PlayerInteriorState
    {
        private static int cachedFrame = -1;
        private static PlayerInterior cached;

        // Derived from position, not door events, so it is right after a save load or a return to the menu.
        public static PlayerInterior Current
        {
            get
            {
                // Every door marker reads this each frame; one door scan per frame is enough.
                if (cachedFrame != Time.frameCount)
                {
                    cached = FromPlayerPosition();
                    cachedFrame = Time.frameCount;
                }
                return cached;
            }
        }

        private static PlayerInterior FromPlayerPosition()
        {
            var player = PlayerRegistry.Local;
            if (player == null)
                return PlayerInterior.None;

            return DoorQuestMarker.FindNearestExitAction(player.transform.position, QuestNavigation.InteriorRadiusMeters) switch
            {
                TutorialActionId.MoveOutHouse => PlayerInterior.House,
                TutorialActionId.MoveOutShop => PlayerInterior.Shop,
                TutorialActionId.MoveOutUpgradeShop => PlayerInterior.UpgradeShop,
                _ => PlayerInterior.None,
            };
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
