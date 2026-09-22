using System;
using System.Collections;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Testing
{
    public class GameHUD : MonoBehaviour
    {
        [Header("Day Count")]
        [SerializeField] private TextMeshProUGUI dayCountText;
        [Tooltip("Optional second day label, e.g. the one in the pause menu.")]
        [SerializeField] private TextMeshProUGUI pauseDayCountText;

        [Header("Rope Length")]
        [SerializeField] private TextMeshProUGUI currentOxygenTubeLength;

        [SerializeField] private TextMeshProUGUI dangerLengthText;
        [SerializeField] private GameObject maxLengthPanel;

        [SerializeField] private GameObject menuButton;
        [SerializeField] private GameObject landButton;
        [SerializeField] private GameObject weaponUIPanel;

        [Header("Pause Menu")]
        [SerializeField] private Button pauseSaveButton;
        [SerializeField] private UISettingManager pauseMenu;
        [SerializeField] private InputReader inputReader;

        [Header("Black Fade")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 1f;

        [Header("Hideable HUD Content")]
        [Tooltip("Assign a CanvasGroup on a CHILD that wraps only the toggleable HUD panels (day count, rope panel, menu/land buttons, weapon UI). Must NOT be an ancestor of NotificationTextUI/ConfirmationDialog, or they'll be hidden along with the HUD.")]
        [SerializeField] private CanvasGroup hudContentGroup;

        public static GameHUD instance {  get; private set; }
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (hudContentGroup == null)
                Debug.LogWarning("GameHUD: hudContentGroup is not assigned — HUD will not hide when land UI panels open. Assign a CanvasGroup on a child that wraps the toggleable HUD content (excluding NotificationTextUI/ConfirmationDialog).", this);
        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            DayManager.OnDayAdvanced += RefreshDayText;
            PlayerUIState.OnLockChanged += HandleLockChanged;

            if (inputReader != null)
                inputReader.OnMenuToggled += HandleMenuToggle;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            DayManager.OnDayAdvanced -= RefreshDayText;
            PlayerUIState.OnLockChanged -= HandleLockChanged;

            if (inputReader != null)
                inputReader.OnMenuToggled -= HandleMenuToggle;
        }

        private void HandleMenuToggle()
        {
            if (PlayerUIState.IsLocked && !CutsceneGate.IsPlaying) return;
            // In-game Esc is the PauseMenu window (AS_PlayerHUD); ToggleMenu asks for the MainMenu
            // window, which only exists in MainMenuScene.
            if (UIManager.Instance != null)
                UIManager.Instance.TogglePauseMenu();
            else
                pauseMenu?.ToggleMenu();
        }

        private void HandleLockChanged(bool isLocked)
        {
            SetHudVisible(!isLocked);
        }

        private void SetHudVisible(bool visible)
        {
            if (hudContentGroup == null) return;
            hudContentGroup.alpha = visible ? 1f : 0f;
            hudContentGroup.interactable = visible;
            hudContentGroup.blocksRaycasts = visible;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool isOcean = scene.name == "Ocean_Map";

            //weaponUIPanel?.SetActive(isOcean);

            // A fresh scene load always starts with no land UI open — except the main menu,
            // which has no gameplay HUD to show at all (day count, rope panel, weapon UI,
            // menu/land buttons); leaving it visible there stacks it on top of the menu's own
            // UI and blocks its raycasts. NotificationTextUI/TutorialPromptUI/ConfirmationDialog
            // live outside hudContentGroup, so they're unaffected either way.
            // Still respects PlayerUIState.IsLocked — a scene load landing while something's
            // already locked (e.g. a Cutscene-type step that plays the instant the scene loads,
            // like Prologue) must not force the HUD back on over it; HandleLockChanged already
            // hid it and won't fire again since the lock itself didn't change.
            SetHudVisible(scene.name != SceneNames.MainMenu && !PlayerUIState.IsLocked);

            if (scene.name == "MainMenuScene")
            {
                landButton.SetActive(false);
            }
            else if (scene.name == "Land_Map")
            {
                landButton.SetActive(false);
                RefreshDayText();
            }
            else if (isOcean)
            {
                RefreshLandButton();
            }
        }

        /// <summary>Hides the "Back to Land" button until GameFeature.BackToLand is unlocked
        /// (a quest step's Unlocks Features list) — a permanent, one-time unlock like
        /// Shop/Inventory/Sleep, not a per-step gate: TutorialGate.RequiredAction only equals
        /// BackToLand during MQ_1001's own "click Back to Land" step, so gating on that alone
        /// would re-hide the button on every later dive (e.g. MQ_1002's), which never asks for
        /// BackToLand as its required action.</summary>
        private void RefreshLandButton()
        {
            if (landButton == null) return;
            landButton.SetActive(FeatureUnlockService.Instance.IsUnlocked(GameFeature.BackToLand));
        }

        private void Start()
        {
            maxLengthPanel.SetActive(false);
            RefreshDayText();
            WireConfirmButtons();
        }

        private void WireConfirmButtons()
        {
            var menuBtn = menuButton?.GetComponent<UnityEngine.UI.Button>();
            if (menuBtn != null)
            {
                menuBtn.onClick.RemoveAllListeners();
                menuBtn.onClick.AddListener(() =>
                    ConfirmationDialog.Instance?.Show(
                        "Return to main menu?",
                        () => GameManager.Instance?.ToMainMenu()
                    ));
            }

            var landBtn = landButton?.GetComponent<UnityEngine.UI.Button>();
            if (landBtn != null)
            {
                landBtn.onClick.RemoveAllListeners();
                landBtn.onClick.AddListener(() =>
                    ConfirmationDialog.Instance?.Show(
                        "Return to land?",
                        () => GameManager.Instance?.ReturnToLand()
                    ));
            }

            if (pauseSaveButton != null)
            {
                pauseSaveButton.onClick.RemoveAllListeners();
                pauseSaveButton.onClick.AddListener(() =>
                    ConfirmationDialog.Instance?.Show(
                        "Save game?",
                        () => SaveBridge.Instance?.SaveGame()
                    ));
            }
        }

        private void RefreshDayText()
        {
            int day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;
            if (dayCountText != null) dayCountText.text = $"Day {day}";
            if (pauseDayCountText != null) pauseDayCountText.text = $"Day {day}";
        }

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Ocean)
            {
                maxLengthPanel.SetActive(false);
                return;
            }

            maxLengthPanel.SetActive(true);
            RefreshLandButton();

            var rope = GameManager.CurPlayer.GetComponentInChildren<OxygenRope>();
            if (rope == null) return;

            // Shows remaining health (shrinks on hit) — the player can still always swim out to
            // rope.MaxRopeLength (the standard/upgraded distance), damage no longer restricts that.
            dangerLengthText.text = $"{Mathf.Abs(GameManager.CurPlayer.CurrentHealth).ToString("F2")} m.";

            if (currentOxygenTubeLength != null)
            {
                currentOxygenTubeLength.text = $"{(int)rope.CurrentLength} m.";
                currentOxygenTubeLength.color = rope.CurrentLength >= rope.HealthLimit ? Color.red : Color.blue;
            }
        }

        public void FadeIn()
        {
            StartCoroutine(FadeRoutine());
        }

        private IEnumerator FadeRoutine()
        {
            fadeImage.enabled = true;

            yield return new WaitForSeconds(3f);
            fadeImage.enabled = false;
        }
    }
}
