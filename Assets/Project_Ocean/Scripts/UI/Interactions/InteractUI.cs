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

        private bool playerInside = false;

        private void Update()
        {
            if (playerInside && Input.GetKeyDown(KeyCode.E))
            {
                if (!PlayerUIState.IsLocked) PlayerUIState.Open();
                onInteract?.Invoke();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                playerInside = true;
                if (interactIcon != null) interactIcon.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                playerInside = false;
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
