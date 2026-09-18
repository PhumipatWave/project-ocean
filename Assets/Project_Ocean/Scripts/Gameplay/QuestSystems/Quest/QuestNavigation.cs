using UnityEngine;

namespace Game.Testing
{
    /// <summary>Shared tuning for the quest-objective navigation arrows (QuestCompass on the
    /// player, and the per-objective Marker on NPC/DoorInteract/TutorialActionMarker) so the
    /// "close enough to switch" distance can't drift between them.</summary>
    public static class QuestNavigation
    {
        public const float CloseRangeMeters = 5f;

        /// <summary>
        /// The point proximity should actually be measured against for this object — its
        /// Collider2D's bounds center if it has one, else its own transform. A GameObject's
        /// interact trigger is often offset well away from its own pivot (e.g. PlayerShip's
        /// BoxCollider2D sits ~6 units from its transform), so measuring raw transform.position
        /// can put the "close enough" threshold nowhere near where the player actually stands
        /// to interact.
        /// </summary>
        public static Vector3 InteractPosition(Transform t)
        {
            var collider = t.GetComponent<Collider2D>();
            return collider != null ? (Vector3)collider.bounds.center : t.position;
        }

        /// <summary>
        /// What the tracked quest's current step actually wants right now — the NPC for a
        /// Talk step, the relevant door for a Tutorial step that requires walking through one,
        /// or a TutorialActionMarker-tagged object for any other Tutorial action with one
        /// registered. Null if the current step has no resolvable position (every other
        /// quest/step type). Shared by QuestCompass (what to point the player's own arrow at)
        /// and DoorInteract (an exit door's "always show while indoors" fallback must yield
        /// to this when it names something more specific, e.g. a shop counter inside the same
        /// building — see DoorInteract.ShouldShowMarker).
        /// </summary>
        public static Transform ResolveQuestTarget()
        {
            var step = QuestManager.Instance?.GetTrackedQuest()?.CurrentStep;
            if (step == null)
                return null;

            if (step.Type == QuestType.Talk)
                return NPCManager.Instance?.GetNpc(step.EffectiveNpcId)?.transform;

            if (step.Type == QuestType.Tutorial)
            {
                if (IsMoveDoorAction(step.TutorialAction))
                    return DoorInteract.FindByTutorialAction(step.TutorialAction);
                return TutorialActionMarker.FindByTutorialAction(step.TutorialAction);
            }

            return null;
        }

        private static bool IsMoveDoorAction(TutorialActionId action) => action is
            TutorialActionId.MoveInHouse or TutorialActionId.MoveInShop or TutorialActionId.MoveInUpgradeShop or
            TutorialActionId.MoveOutHouse or TutorialActionId.MoveOutShop or TutorialActionId.MoveOutUpgradeShop;
    }
}
