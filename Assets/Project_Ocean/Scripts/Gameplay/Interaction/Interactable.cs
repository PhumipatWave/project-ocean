using Game.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Gameplay
{
    /// <summary>Something the player can press Interact on while inside its trigger zone.</summary>
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("collectButton")] private GameObject interactIcon;

        [Tooltip("Extra feature this specific interactable requires (e.g. Shop for a shop NPC), on top of the global Interact gate. Leave off if it only needs Interact unlocked.")]
        [SerializeField] private bool requireSpecificFeature;
        [SerializeField] private GameFeature specificFeature;

        /// <summary>Raised when the player leaves this zone (after the lock is released).</summary>
        public event System.Action PlayerExited;

        private bool holdingLock;
        private PlayerInteractor interactor;

        /// <summary>Pickups return false: no UI lock, no tutorial gate or event, no locked toast.</summary>
        protected virtual bool TakesPlayerLock => true;

        protected virtual bool IsFeatureUnlocked =>
            FeatureUnlockService.Instance.IsUnlocked(GameFeature.Interact) &&
            (!requireSpecificFeature || FeatureUnlockService.Instance.IsUnlocked(specificFeature));

        /// <summary>False hides the icon and ignores presses without a toast (e.g. a closed shell).</summary>
        public virtual bool CanInteract => true;

        internal bool IsReady => CanInteract && IsFeatureUnlocked;
        internal bool HoldsLock => holdingLock && PlayerUIState.IsLocked;

        protected abstract void Interact(Player player);

        internal void Press(Player player)
        {
            if (TakesPlayerLock)
            {
                if (TutorialGate.Blocks(TutorialActionId.Interact)) return;
                if (!IsFeatureUnlocked)
                {
                    NotificationManager.Notify("Locked — progress the main quest to unlock this.");
                    return;
                }
                if (PlayerUIState.IsLocked && !holdingLock) return;

                if (!PlayerUIState.IsLocked)
                {
                    PlayerUIState.Open();
                    holdingLock = true;
                }
                TutorialEvents.RaiseAction(TutorialActionId.Interact);
            }
            else if (PlayerUIState.IsLocked || !IsReady)
            {
                return;
            }

            Interact(player);
        }

        // Only the instance that took the lock may close it, or leaving a second zone would close another zone's panel.
        public void ReleasePlayerLock()
        {
            if (!holdingLock) return;

            PlayerUIState.Close();
            holdingLock = false;
        }

        internal void SetIconVisible(bool visible)
        {
            if (interactIcon != null && interactIcon.activeSelf != visible)
                interactIcon.SetActive(visible);
        }

        public void HideIcon() => SetIconVisible(false);

        protected virtual void OnEnable() => HideIcon();

        protected virtual void OnDisable() => Leave();

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            interactor = other.GetComponentInParent<PlayerInteractor>();
            interactor?.Add(this);
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            Leave();
            ReleasePlayerLock();
            PlayerExited?.Invoke();
        }

        private void Leave()
        {
            interactor?.Remove(this);
            interactor = null;
            HideIcon();
        }
    }
}
