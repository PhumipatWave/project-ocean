using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
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
        [SerializeField] private TMPro.TMP_Text divePerDayText;

        [Header("Day Panel")]
        [SerializeField] private Button nextDayButton;

        [Header("Home Panel")]
        //[SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;

        // Panels hidden while a dialogue shows, with the CanvasGroup state to restore afterwards.
        private readonly Dictionary<CanvasGroup, (float alpha, bool interactable, bool blocksRaycasts)> hiddenPanels = new();

        private void OnEnable()
        {
            DialogueState.OnChanged += HandleDialogueChanged;
            DayManager.OnDivedToday  += RefreshNextDayButton;
            DayManager.OnDayAdvanced += RefreshNextDayButton;
            DayManager.OnDivedToday  += RefreshDiveUI;
            DayManager.OnDayAdvanced += RefreshDiveUI;
            FeatureUnlockService.OnFeatureUnlocked += HandleFeatureUnlocked;
            LoadingManager.OnLoadingFinished += HandleLoadingFinished;
        }

        private void OnDisable()
        {
            DialogueState.OnChanged -= HandleDialogueChanged;
            RestorePanels();
            DayManager.OnDivedToday  -= RefreshNextDayButton;
            DayManager.OnDayAdvanced -= RefreshNextDayButton;
            DayManager.OnDivedToday  -= RefreshDiveUI;
            DayManager.OnDayAdvanced -= RefreshDiveUI;
            FeatureUnlockService.OnFeatureUnlocked -= HandleFeatureUnlocked;
            LoadingManager.OnLoadingFinished -= HandleLoadingFinished;
        }

        // Dive/shop/upgrade/board/home panels would otherwise stack under a dialogue box. They
        // stay open (and keep their PlayerUIState lock) — just invisible and unclickable until
        // the dialogue closes. Dialogue boxes themselves live under this canvas too, so skip them.
        private void HandleDialogueChanged()
        {
            if (DialogueState.IsShowing) HideOtherPanels();
            else RestorePanels();
        }

        private void HideOtherPanels()
        {
            foreach (Transform child in transform)
            {
                if (DialogueState.ContainsShowingPanel(child)) continue;

                var group = child.GetComponent<CanvasGroup>();
                if (group == null) group = child.gameObject.AddComponent<CanvasGroup>();
                if (!hiddenPanels.ContainsKey(group))
                    hiddenPanels[group] = (group.alpha, group.interactable, group.blocksRaycasts);

                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }

        private void RestorePanels()
        {
            foreach (var (group, state) in hiddenPanels)
            {
                if (group == null) continue;
                group.alpha = state.alpha;
                group.interactable = state.interactable;
                group.blocksRaycasts = state.blocksRaycasts;
            }
            hiddenPanels.Clear();
        }

        // Sleep unlocking mid-session (with no dive/day-advance event to piggyback a
        // refresh on) would otherwise leave the button showing its stale pre-unlock state.
        private void HandleFeatureUnlocked(GameFeature feature)
        {
            if (feature == GameFeature.Sleep)
                RefreshNextDayButton();
        }

        private void Start()
        {
            if (startDiveButton != null)
                startDiveButton.onClick.AddListener(() =>
                {
                    // Release the lock that InteractUI.Open() placed when E was pressed
                    PlayerUIState.Close();

                    // Out of dives: tell the player to sleep instead of opening the dive confirm.
                    if (NotifyIfOutOfDives())
                        return;

                    ConfirmationDialog.Instance?.Show(
                        "Are you sure you want to dive?",
                        () => { InventoryManager.Instance?.CloseInventory(); GameManager.Instance?.StartDive(); }
                    );
                });

            RefreshDiveUI();

            if (nextDayButton != null)
            {
                nextDayButton.onClick.AddListener(TryAdvanceDay);
                RefreshNextDayButton();
            }

            /*if (saveButton != null)
                saveButton.onClick.AddListener(() =>
                    ConfirmationDialog.Instance?.Show(
                        "Save game?",
                        () => SaveBridge.Instance?.SaveGame()
                    ));*/

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
            if (!FeatureUnlockService.Instance.IsUnlocked(GameFeature.Sleep)) return;
            ConfirmationDialog.Instance?.Show(
                "Are you sure you want to end the day?",
                () =>
                {
                    // Close HomePanel BEFORE advancing the day, not after — AdvanceDay() raises
                    // TutorialEvents' Sleep action, which can itself open other UI (e.g. an
                    // auto-display tutorial line for the next step). Closing first means
                    // CloseCurrent() unambiguously targets HomePanel's own slot (the current top
                    // right after ConfirmationDialog.Confirm() released its own), instead of
                    // racing whatever AdvanceDay() might push on top afterward.
                    PlayerUIState.CloseCurrent();
                    DayManager.Instance?.AdvanceDay();
                }
            );
        }

        // Returning to land (or loading a save) at 0 dives shows the notice unprompted.
        private void HandleLoadingFinished() => NotifyIfOutOfDives();

        private bool NotifyIfOutOfDives()
        {
            if (DayManager.Instance == null || DayManager.Instance.CanDive)
                return false;
            NotificationManager.Notify("You reach limit to dive today go to sleep");
            return true;
        }

        private void RefreshDiveUI()
        {
            var day = DayManager.Instance;
            if (day == null) return;

            if (divePerDayText != null)
                divePerDayText.text = $"Remain {day.DivesRemaining}/{DayManager.MaxDivesPerDay} in Day";
        }

        private void RefreshNextDayButton()
        {
            if (nextDayButton == null) return;
            nextDayButton.interactable = DayManager.Instance != null && DayManager.Instance.HasDivedToday &&
                FeatureUnlockService.Instance.IsUnlocked(GameFeature.Sleep);
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
