using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Shared tuning for the quest-objective navigation arrows (QuestCompass on the
    /// player, and the per-objective Marker on NPC/DoorQuestMarker/TutorialActionMarker) so the
    /// "close enough to switch" distance can't drift between them.</summary>
    public static class QuestNavigation
    {
        public const float CloseRangeMeters = 5f;

        // ponytail: proximity heuristic — a target within this distance of an interior's exit door
        // is inside that interior (interiors are stacked far from the town, so outdoor targets
        // are never this close). Explicit per-target interior field if layouts ever overlap.
        private const float InteriorRadiusMeters = 40f;

        /// <summary>
        /// The point proximity should actually be measured against for this object — its
        /// Collider2D's bounds center if it has one (on itself or a child), else its own
        /// transform. A GameObject's interact trigger is often offset well away from its own
        /// pivot (e.g. PlayerShip's BoxCollider2D sits ~6 units from its transform; an NPC's
        /// Visual/Talk children sit ~116 units from the NPC root), so measuring raw
        /// transform.position can put the "close enough" threshold, or the compass arrow,
        /// nowhere near where the player actually stands to interact.
        /// </summary>
        public static Vector3 InteractPosition(Transform t)
        {
            var collider = t.GetComponentInChildren<Collider2D>();
            return collider != null ? (Vector3)collider.bounds.center : t.position;
        }

        /// <summary>
        /// What the tracked quest's current step actually wants right now — the NPC for a
        /// Talk step, the relevant door for a Tutorial step that requires walking through one,
        /// or a TutorialActionMarker-tagged object for any other Tutorial action with one
        /// registered. Null if the current step has no resolvable position (every other
        /// quest/step type, including ExploreArea — deliberately unguided, see
        /// ExploreAreaTrigger's class doc, so the player has to actually search rather than
        /// follow a compass to it). Shared by QuestCompass (what to point the player's own
        /// arrow at) and DoorQuestMarker (an exit door's "always show while indoors" fallback must
        /// yield to this when it names something more specific, e.g. a shop counter inside the
        /// same building — see DoorQuestMarker.ShouldShowMarker).
        /// </summary>
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

        /// <summary>
        /// What the compass should actually aim at: ResolveQuestTarget(), except that while the
        /// player is outdoors a target inside a building (NPC, counter, bed...) is replaced by
        /// that building's entrance door. Interiors sit far from the town, so aiming at the raw
        /// target points the arrow across the map in the wrong direction. Null if the entrance
        /// can't be found — hiding the arrow beats pointing the wrong way.
        /// </summary>
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
