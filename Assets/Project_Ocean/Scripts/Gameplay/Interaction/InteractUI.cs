using Game.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Gameplay
{
    /// <summary>
    /// Shows/hides an interact icon when the player enters the trigger zone.
    /// Press E to fire onInteract — wire this in the Inspector to
    /// ShopManager.Instance.OpenShopWindow(), UpgradeManager.OpenFromShop(), etc.
    ///
    /// Every InteractUI the player is currently standing inside subscribes to the same
    /// raw Interact press, so overlapping zones (e.g. two NPCs close together) would
    /// otherwise all fire onInteract on one press, opening every panel at once. holdingLock
    /// tracks whether THIS instance is the one that claimed PlayerUIState — only it may
    /// react to further presses (so the SAME zone can still advance/close its own panel)
    /// or release the lock; any other InteractUI sees the lock already held by someone
    /// else and ignores the press entirely.
    /// </summary>
    public class InteractUI : MonoBehaviour
    {
        [SerializeField] private GameObject interactIcon;

        [Tooltip("Fired when the player presses E while inside this zone.")]
        public UnityEvent onInteract;

        [Tooltip("Extra feature this specific interactable requires (e.g. Shop for a shop NPC), on top of the global Interact gate. Leave off if it only needs Interact unlocked.")]
        [SerializeField] private bool requireSpecificFeature;
        [SerializeField] private GameFeature specificFeature;

        /// <summary>Raised when the player leaves this zone (after the lock is released).</summary>
        public event System.Action PlayerExited;

        private bool _subscribed;
        private bool holdingLock;
        private bool _playerInRange;

        private bool IsFeatureLocked()
        {
            if (!FeatureUnlockService.Instance.IsUnlocked(GameFeature.Interact)) return true;
            if (requireSpecificFeature && !FeatureUnlockService.Instance.IsUnlocked(specificFeature)) return true;
            return false;
        }

        private void HandleInteract(bool pressed)
        {
            if (!pressed) return;
            if (TutorialGate.Blocks(TutorialActionId.Interact)) return;
            if (CutsceneGate.IsPlaying) return;
            if (LoadingManager.IsLoading) return;
            if (PlayerSelfDialogueUI.BlocksInteraction) return;
            if (IsFeatureLocked())
            {
                NotificationManager.Notify("Locked — progress the main quest to unlock this.");
                return;
            }
            if (PlayerUIState.IsLocked && !holdingLock) return; // another interactable already has the floor

            if (!PlayerUIState.IsLocked)
            {
                PlayerUIState.Open();
                holdingLock = true;
            }

            TutorialEvents.RaiseAction(TutorialActionId.Interact);
            onInteract?.Invoke();
        }

        private void Subscribe()
        {
            if (_subscribed) return;
            var reader = PlayerRegistry.Local?.input;
            if (reader == null) return;
            reader.OnInteracted += HandleInteract;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            var reader = PlayerRegistry.Local?.input;
            if (reader != null) reader.OnInteracted -= HandleInteract;
            _subscribed = false;
        }

        private void OnDisable() => Unsubscribe();

        // Cutscenes have no "started" event to react to (only OnCutsceneFinished), and feature
        // unlocks can land mid-scene too — both are just polled each frame rather than pushed,
        // cheap enough for a single SetActive check per prompt. A locked shop/upgrade NPC never
        // shows its icon at all, on top of HandleInteract already refusing to open its UI.
        private void Update()
        {
            if (interactIcon != null)
                interactIcon.SetActive(_playerInRange && !CutsceneGate.IsPlaying && !PlayerSelfDialogueUI.BlocksInteraction && !IsFeatureLocked());
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Subscribe();
                _playerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Unsubscribe();
                _playerInRange = false;
                ReleasePlayerLock();
                PlayerExited?.Invoke();
            }
        }

        // Only actually closes the lock if THIS instance is the one holding it — otherwise
        // walking out of a second, never-opened interactable's zone would wrongly close
        // whichever other interactable's panel is still legitimately open.
        public void ReleasePlayerLock()
        {
            if (!holdingLock) return;

            PlayerUIState.Close();
            holdingLock = false;
        }

        public void HideIcon()
        {
            if (interactIcon != null)
                interactIcon.SetActive(false);
        }
    }
}
