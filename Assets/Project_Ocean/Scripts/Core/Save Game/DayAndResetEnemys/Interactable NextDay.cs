using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class InteractableNextDay : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("UI")]
        [SerializeField] private GameObject promptUI;
        /// Assign the Button inside promptUI. Its interactable state reflects HasDivedToday.
        /// Wire its onClick to TrySleep() in the Inspector.
        [SerializeField] private Button sleepButton;

        private bool _playerInRange;

        private void Awake()
        {
            if (promptUI != null)
                promptUI.SetActive(false);
        }

        private void OnEnable()
        {
            DayManager.OnDivedToday  += RefreshSleepButton;
            DayManager.OnDayAdvanced += RefreshSleepButton;
        }

        private void OnDisable()
        {
            DayManager.OnDivedToday  -= RefreshSleepButton;
            DayManager.OnDayAdvanced -= RefreshSleepButton;
        }

        private void Update()
        {
            if (_playerInRange && Input.GetKeyDown(interactKey))
                TrySleep();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            promptUI?.SetActive(true);
            RefreshSleepButton();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            promptUI?.SetActive(false);
        }

        private void RefreshSleepButton()
        {
            if (sleepButton == null) return;
            sleepButton.interactable = DayManager.Instance != null && DayManager.Instance.HasDivedToday;
        }

        /// Called by E key and by the sleep button's onClick.
        public void TrySleep()
        {
            if (DayManager.Instance == null)
            {
                Debug.LogError("[InteractableNextDay] DayManager not found");
                return;
            }

            if (!DayManager.Instance.HasDivedToday)
            {
                Debug.Log("[InteractableNextDay] Cannot sleep — player has not dived today.");
                return;
            }

            promptUI?.SetActive(false);
            DayManager.Instance.AdvanceDay();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}
