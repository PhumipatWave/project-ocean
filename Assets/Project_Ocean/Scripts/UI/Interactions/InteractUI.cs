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
        private bool uiOpened = false;

        private void Update()
        {
            if (playerInside && Input.GetKeyDown(KeyCode.E))
            {
                onInteract?.Invoke();
                if (!uiOpened)
                {
                    PlayerUIState.Open();
                    uiOpened = true;
                }
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
                if (uiOpened)
                {
                    PlayerUIState.Close();
                    uiOpened = false;
                }
            }
        }

        public void HideIcon()
        {
            if (interactIcon != null)
                interactIcon.SetActive(false);
        }
    }
}
