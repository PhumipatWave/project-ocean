using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Shared quest-target lookups and distances for QuestCompass and the objective markers.</summary>
    public static class QuestNavigation
    {
        public const float CloseRangeMeters = 5f;

        // ponytail: anything within this distance of an interior exit door is inside it (interiors sit far from town);
        // add an explicit interior field per target if layouts ever overlap.
        internal const float InteriorRadiusMeters = 40f;

        /// <summary>Where to measure distance to: the object's collider center, since triggers sit far from the pivot.</summary>
        public static Vector3 InteractPosition(Transform t)
        {
            var collider = t.GetComponentInChildren<Collider2D>();
            return collider != null ? (Vector3)collider.bounds.center : t.position;
        }

        /// <summary>The tracked quest's current Tutorial action, or null if its step isn't a Tutorial step.</summary>
        public static TutorialActionId? CurrentTutorialAction()
        {
            var step = QuestManager.Instance?.GetTrackedQuest()?.CurrentStep;
            return step != null && step.Type == QuestType.Tutorial ? step.TutorialAction : (TutorialActionId?)null;
        }

        /// <summary>True when the local player is within CloseRangeMeters of this object's interact point.</summary>
        public static bool IsPlayerNear(Transform t)
        {
            var player = PlayerRegistry.Local;
            return player != null && Vector3.Distance(player.transform.position, InteractPosition(t)) <= CloseRangeMeters;
        }

        /// <summary>The tracked step's target: the Talk NPC, the door, or the object for its Tutorial action; null otherwise.</summary>
        // ExploreArea stays unguided on purpose: finding the area is the point of the step.
        public static Transform ResolveQuestTarget()
        {
            var step = QuestManager.Instance?.GetTrackedQuest()?.CurrentStep;
            if (step == null)
                return null;

            if (step.Type == QuestType.Talk)
                return NPCManager.Instance?.GetNpc(step.NextTalkTargetNpcId)?.transform;

            if (step.Type == QuestType.Tutorial)
            {
                if (IsMoveDoorAction(step.TutorialAction))
                    return DoorQuestMarker.FindByTutorialAction(step.TutorialAction);
                return TutorialActionMarker.FindByTutorialAction(step.TutorialAction);
            }

            return null;
        }

        /// <summary>The compass target: outdoors, a target inside a building becomes that building's entrance door.</summary>
        public static Transform ResolveCompassTarget()
        {
            var target = ResolveQuestTarget();
            if (target == null || PlayerInteriorState.Current != PlayerInterior.None)
                return target;

            var exit = DoorQuestMarker.FindNearestExitAction(InteractPosition(target), InteriorRadiusMeters);
            if (exit == null)
                return target;

            var entry = PlayerInteriorState.EntryActionFor(exit.Value);
            return entry != null ? DoorQuestMarker.FindByTutorialAction(entry.Value) : null;
        }

        private static bool IsMoveDoorAction(TutorialActionId action) => action is
            TutorialActionId.MoveInHouse or TutorialActionId.MoveInShop or TutorialActionId.MoveInUpgradeShop or
            TutorialActionId.MoveOutHouse or TutorialActionId.MoveOutShop or TutorialActionId.MoveOutUpgradeShop;
    }
}
