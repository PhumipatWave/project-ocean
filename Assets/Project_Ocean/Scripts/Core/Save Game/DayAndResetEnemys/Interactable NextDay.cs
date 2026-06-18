using UnityEngine;

namespace Game.Testing
{
    public class InteractableNextDay : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("UI")]
        [SerializeField] private GameObject promptUI;

        private bool _playerInRange;

        void Awake()
        {
            if (promptUI != null)
                promptUI.SetActive(false);
        }

        void Update()
        {
            if (_playerInRange && Input.GetKeyDown(interactKey))
                TrySleep();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = true;
                promptUI?.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = false;
                promptUI?.SetActive(false);
            }
        }

        private void TrySleep()
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
