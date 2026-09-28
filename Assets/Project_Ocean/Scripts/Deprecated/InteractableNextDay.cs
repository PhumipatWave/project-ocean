using Game.Core;
using UnityEngine;
using UnityEngine.UI;
using Game.Gameplay;
using Game.UI;

namespace Game.Deprecated
{
    // Raw-key handling (E press → TrySleep) is delegated to an InteractUI on the same
    // GameObject, wired to call TrySleep() — same shared PlayerUIState "floor" lock every
    // other interactable (NPC/door/chest) uses, instead of a second, ungated subscription
    // that used to fire TrySleep() even while another panel was already open.
    [RequireComponent(typeof(InteractUI))]
    public class InteractableNextDay : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject promptUI;
        /// Assign the Button inside promptUI. Its interactable state reflects HasDivedToday.
        /// Wire its onClick to TrySleep() in the Inspector.
        [SerializeField] private Button sleepButton;

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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            promptUI?.SetActive(true);
            RefreshSleepButton();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
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

            // A different action is the guided tutorial's current requirement — sleeping now
            // would advance the day and waste the Sleep step's one-shot completion event.
            if (TutorialGate.Blocks(TutorialActionId.Sleep)) { TutorialGate.NotifyBlocked(); return; }

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
