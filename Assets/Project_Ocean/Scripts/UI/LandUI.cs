using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// Binds all Land-scene buttons to global singleton managers at runtime,
    /// avoiding cross-scene Inspector references that break on scene reload.
    /// Also exposes Open/Close for wiring any land UI close button to PlayerUIState.
    public class LandUI : MonoBehaviour
    {
        /// <summary>Wire to any button that opens a blocking land UI.</summary>
        public void OpenUI()  => PlayerUIState.Open();

        /// <summary>Wire to every close/back button that fully dismisses a land UI.</summary>
        public void CloseUI() => PlayerUIState.Close();

        [Header("Dive Panel")]
        [SerializeField] private Button startDiveButton;

        [Header("Day Panel")]
        [SerializeField] private Button nextDayButton;

        [Header("Home Panel")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;

        private void OnEnable()
        {
            DayManager.OnDivedToday  += RefreshNextDayButton;
            DayManager.OnDayAdvanced += RefreshNextDayButton;
        }

        private void OnDisable()
        {
            DayManager.OnDivedToday  -= RefreshNextDayButton;
            DayManager.OnDayAdvanced -= RefreshNextDayButton;
        }

        private void Start()
        {
            if (startDiveButton != null)
                startDiveButton.onClick.AddListener(() =>
                {
                    // Release the lock that InteractUI.Open() placed when E was pressed
                    PlayerUIState.Close();
                    ConfirmationDialog.Instance?.Show(
                        "Are you sure you want to dive?",
                        () => { InventoryManager.Instance?.CloseInventory(); GameManager.Instance?.StartDive(); }
                    );
                });

            if (nextDayButton != null)
            {
                nextDayButton.onClick.AddListener(TryAdvanceDay);
                RefreshNextDayButton();
            }

            if (saveButton != null)
                saveButton.onClick.AddListener(() =>
                    ConfirmationDialog.Instance?.Show(
                        "Save game?",
                        () => SaveBridge.Instance?.SaveGame()
                    ));

            if (loadButton != null)
                loadButton.onClick.AddListener(() =>
                    ConfirmationDialog.Instance?.Show(
                        "Load game? Unsaved progress will be lost.",
                        () => SaveBridge.Instance?.LoadGame()
                    ));

            WireShopButtons();
        }

        private void TryAdvanceDay()
        {
            if (DayManager.Instance == null || !DayManager.Instance.HasDivedToday) return;
            ConfirmationDialog.Instance?.Show(
                "Are you sure you want to end the day?",
                () => DayManager.Instance?.AdvanceDay()
            );
        }

        private void RefreshNextDayButton()
        {
            if (nextDayButton == null) return;
            nextDayButton.interactable = DayManager.Instance != null && DayManager.Instance.HasDivedToday;
        }

        private void WireShopButtons()
        {
            WireButton("ShopPanel/ShopSell/AllButton/bin",
                () => InventoryManager.Instance?.RemoveSelectedItem());

            WireButton("ShopPanel/ShopSell/CloseButton",
                () => InventoryManager.Instance?.CloseInventory());
        }

        private void WireButton(string path, UnityEngine.Events.UnityAction action)
        {
            var btn = transform.Find(path)?.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(action);
        }
    }
}
