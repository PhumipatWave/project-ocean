using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Rotates this object (the Player prefab's "Navigation" arrow) to point at the tracked
    /// quest's current target — the NPC for a Talk step, the relevant door for a Tutorial
    /// step that requires walking through one (MoveInHouse/Shop/UpgradeShop, MoveOutHouse/
    /// Shop/UpgradeShop), or a TutorialActionMarker-tagged object for any other Tutorial
    /// action with one registered (e.g. PlayerShip for Dive). Hides itself whenever there's
    /// no resolvable position (every other quest/step type), once the player is within
    /// QuestNavigation.CloseRangeMeters of the target (at that range the target's own Marker —
    /// NPC/DoorQuestMarker/TutorialActionMarker — takes over instead, so only one arrow is ever
    /// showing), or while indoors (see PlayerInteriorState) — there's nothing on the player to
    /// navigate to inside a building except the exit, and DoorQuestMarker's own Marker already
    /// covers that regardless of quest state. The parent Player never rotates its own
    /// transform (facing is done by flipping sprites, not rotating), so this component's own
    /// rotation is already world space, no counter-rotation needed.
    /// </summary>
    public class QuestCompass : MonoBehaviour
    {
        [Tooltip("Degrees/second the arrow eases toward the target direction, instead of snapping.")]
        [SerializeField] private float rotationSpeed = 360f;

        [Tooltip("The visible arrow child, shown only while a target is resolved and out of range.")]
        [SerializeField] private GameObject visual;

        private void Update()
        {
            // Re-resolved every frame rather than cached off QuestManager.OnQuestUpdated —
            // a door target (MoveInHouse/Shop/UpgradeShop) self-registers on its own OnEnable
            // (see DoorQuestMarker), which can race the quest step actually advancing onto it
            // (e.g. right after an additive scene load). Caching off the event alone could
            // resolve null at that instant and never retry until the NEXT quest update: NPCs
            // are always present from scene load, so only door targets ever hit this.
            var target = QuestNavigation.ResolveCompassTarget();

            // Indoors, the exit door's own Marker (see DoorQuestMarker) is the only guidance —
            // no point spinning an arrow around inside a small room.
            if (target == null || PlayerInteriorState.Current != PlayerInterior.None)
            {
                if (visual != null) visual.SetActive(false);
                return;
            }

            // Distance uses the target's actual interact trigger (its Collider2D, if it has
            // one), not necessarily its transform pivot — PlayerShip's collider sits ~6 units
            // from its own transform, so comparing raw positions meant "in range" was
            // essentially unreachable. Rotation below aims at that same point, not the pivot.
            bool inRange = Vector3.Distance(transform.position, QuestNavigation.InteractPosition(target)) <= QuestNavigation.CloseRangeMeters;
            if (visual != null) visual.SetActive(!inRange);
            if (inRange)
                return;

            Vector3 direction = QuestNavigation.InteractPosition(target) - transform.position;
            // The Arrow child's rest position is straight up (+Y) from this pivot, not along
            // +X — so 0° rotation here already points it up. Atan2 measures from +X, so the
            // rotation this pivot actually needs is offset by -90° to match that rest pose.
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
