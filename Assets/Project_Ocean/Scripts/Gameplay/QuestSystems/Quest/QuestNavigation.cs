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
    }
}
