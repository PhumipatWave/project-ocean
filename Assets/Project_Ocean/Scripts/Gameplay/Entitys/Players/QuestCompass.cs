using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Rotates this object (the Player prefab's "Navigation" arrow) to point at the tracked
    /// quest's current target — the NPC for a Talk step, the relevant door for a Tutorial
    /// step that requires walking through one (MoveInHouse/Shop/UpgradeShop, MoveOutHouse/
    /// Shop/UpgradeShop), or a TutorialActionMarker-tagged object for any other Tutorial
    /// action with one registered (e.g. PlayerShip for Dive). Hides itself whenever there's
    /// no resolvable position (every other quest/step type), once the player is within
    /// QuestNavigation.CloseRangeMeters of the target (at that range the target's own Marker —
    /// NPC/DoorInteract/TutorialActionMarker — takes over instead, so only one arrow is ever
    /// showing), or while indoors (see PlayerInteriorState) — there's nothing on the player to
    /// navigate to inside a building except the exit, and DoorInteract's own Marker already
    /// covers that regardless of quest state. The parent Player never rotates its own
    /// transform (facing is done by flipping sprites, not rotating), so this component's own
    /// rotation is already world space, no counter-rotation needed.
    /// </summary>
    public class QuestCompass : MonoBehaviour
    {
        [Tooltip("Degrees/second the arrow eases toward the target direction, instead of snapping.")]
        [SerializeField] private float rotationSpeed = 360f;

        [Tooltip("The visible arrow child, shown only while a target is resolved and out of " +
            "range. Toggling this (not this GameObject) keeps QuestCompass itself enabled so " +
            "it stays subscribed to QuestManager.OnQuestUpdated and can re-show the arrow later.")]
        [SerializeField] private GameObject visual;

        private Transform target;

        private void OnEnable()
        {
            QuestManager.OnQuestUpdated += RefreshTarget;
            RefreshTarget();
        }

        private void OnDisable() => QuestManager.OnQuestUpdated -= RefreshTarget;

        private void RefreshTarget() => target = ResolveTarget();

        private static Transform ResolveTarget()
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

        private void Update()
        {
            // Indoors, the exit door's own Marker (see DoorInteract) is the only guidance —
            // no point spinning an arrow around inside a small room.
            if (target == null || PlayerInteriorState.Current != PlayerInterior.None)
            {
                if (visual != null) visual.SetActive(false);
                return;
            }

            // Distance uses the target's actual interact trigger (its Collider2D, if it has
            // one), not necessarily its transform pivot — PlayerShip's collider sits ~6 units
            // from its own transform, so comparing raw positions meant "in range" was
            // essentially unreachable. Rotation below still aims at the visual pivot itself.
            bool inRange = Vector3.Distance(transform.position, QuestNavigation.InteractPosition(target)) <= QuestNavigation.CloseRangeMeters;
            if (visual != null) visual.SetActive(!inRange);
            if (inRange)
                return;

            Vector3 direction = target.position - transform.position;
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
