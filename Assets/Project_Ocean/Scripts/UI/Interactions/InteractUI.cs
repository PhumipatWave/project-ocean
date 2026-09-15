using Game.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Testing
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

        private bool _subscribed;
        private bool holdingLock;

        private void HandleInteract(bool pressed)
        {
            if (!pressed) return;
            if (TutorialGate.Blocks(TutorialActionId.Interact)) return;
            if (CutsceneGate.IsPlaying) return;
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
            var reader = GameManager.CurPlayer?.input;
            if (reader == null) return;
            reader.OnInteracted += HandleInteract;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            var reader = GameManager.CurPlayer?.input;
            if (reader != null) reader.OnInteracted -= HandleInteract;
            _subscribed = false;
        }

        private void OnDisable() => Unsubscribe();

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Subscribe();
                if (interactIcon != null) interactIcon.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Unsubscribe();
                if (interactIcon != null) interactIcon.SetActive(false);
                ReleasePlayerLock();
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
