using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Points the player's arrow at the tracked quest's target (see QuestNavigation.ResolveCompassTarget).</summary>
    // Hidden with no target, within CloseRangeMeters (the target's own marker takes over) and indoors.
    public class QuestCompass : MonoBehaviour
    {
        [Tooltip("Degrees/second the arrow eases toward the target direction, instead of snapping.")]
        [SerializeField] private float rotationSpeed = 360f;

        [Tooltip("The visible arrow child, shown only while a target is resolved and out of range.")]
        [SerializeField] private GameObject visual;

        private void Update()
        {
            // Resolved every frame: a door registers in its own OnEnable, which can land after the quest update.
            var target = QuestNavigation.ResolveCompassTarget();

            // Indoors, the exit door's own Marker (see DoorQuestMarker) is the only guidance —
            // no point spinning an arrow around inside a small room.
            if (target == null || PlayerInteriorState.Current != PlayerInterior.None)
            {
                if (visual != null) visual.SetActive(false);
                return;
            }

            // Measure to the interact trigger, not the pivot (PlayerShip's collider is ~6 units from its transform).
            bool inRange = Vector3.Distance(transform.position, QuestNavigation.InteractPosition(target)) <= QuestNavigation.CloseRangeMeters;
            if (visual != null) visual.SetActive(!inRange);
            if (inRange)
                return;

            Vector3 direction = QuestNavigation.InteractPosition(target) - transform.position;
            // The arrow sprite points up (+Y) at rest, while Atan2 measures from +X.
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
