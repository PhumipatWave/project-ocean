using Game.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Testing
{
    /// <summary>
    /// Shows/hides an interact icon when the player enters the trigger zone.
    /// Press E to fire onInteract — wire this in the Inspector to
    /// ShopManager.Instance.OpenShopWindow(), UpgradeManager.OpenFromShop(), etc.
    /// </summary>
    public class InteractUI : MonoBehaviour
    {
        [SerializeField] private GameObject interactIcon;

        [Tooltip("Fired when the player presses E while inside this zone.")]
        public UnityEvent onInteract;

        private bool _subscribed;

        private void HandleInteract(bool pressed)
        {
            if (!pressed) return;
            if (!PlayerUIState.IsLocked) PlayerUIState.Open();
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
                PlayerUIState.Close();
            }
        }

        public void ReleasePlayerLock() => PlayerUIState.Close();

        public void HideIcon()
        {
            if (interactIcon != null)
                interactIcon.SetActive(false);
        }
    }
}
