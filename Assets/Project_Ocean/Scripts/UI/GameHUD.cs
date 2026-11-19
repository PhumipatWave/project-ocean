using System;
using System.Collections;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Game.Gameplay;

namespace Game.UI
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
        [Tooltip("Assign a CanvasGroup on a CHILD that wraps only the toggleable HUD panels (day count, rope panel, menu/land buttons, weapon UI). Must NOT be an ancestor of NotificationLayer/ConfirmationDialog, or they'll be hidden along with the HUD.")]
        [SerializeField] private CanvasGroup hudContentGroup;

        private OxygenRope rope;

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
                Debug.LogWarning("GameHUD: hudContentGroup is not assigned — HUD will not hide when land UI panels open. Assign a CanvasGroup on a child that wraps the toggleable HUD content (excluding NotificationLayer/ConfirmationDialog).", this);
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
            // Bootstrap/Intro/menu have no player; a lock released there must not bring the HUD back.
            visible &= SceneNames.IsGameplay(SceneManager.GetActiveScene().name);
            hudContentGroup.alpha = visible ? 1f : 0f;
            hudContentGroup.interactable = visible;
            hudContentGroup.blocksRaycasts = visible;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool isOcean = scene.name == "Ocean_Map";

            //weaponUIPanel?.SetActive(isOcean);

            // The main menu has no gameplay HUD, and a locked scene load (e.g. the Prologue cutscene) keeps it hidden;
            // HandleLockChanged won't fire again, since the lock itself didn't change.
            SetHudVisible(SceneNames.IsGameplay(scene.name) && !PlayerUIState.IsLocked);

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

        /// <summary>Shows "Back to Land" once GameFeature.BackToLand is unlocked. It lives in the pause menu, so the menu itself hides it.</summary>
        // Only this method may toggle landButton — a second per-frame writer flickers it.
        private void RefreshLandButton()
        {
            if (landButton == null) return;
            bool show = FeatureUnlockService.Instance.IsUnlocked(GameFeature.BackToLand);
            if (landButton.activeSelf != show)
                landButton.SetActive(show);
        }

        private void Start()
        {
            maxLengthPanel.SetActive(false);

            // First boot: this HUD scene loads additively AFTER MainMenuScene's sceneLoaded fired,
            // so OnSceneLoaded never hid the gameplay HUD behind the main menu. Do it here too.
            bool inMenu = SceneManager.GetActiveScene().name == SceneNames.MainMenu
                          || GameSession.State == GameState.Menu;
            SetHudVisible(!inMenu && !PlayerUIState.IsLocked);

            RefreshDayText();
            WireConfirmButtons();
        }

        // Guided "Use MiniHub" step: the player must place it before leaving the ocean.
        // Only these player buttons — rope break / death still return to land on their own.
        private static bool BlockedByUseItemTutorial()
        {
            if (!TutorialStep.IsActive(TutorialActionId.UseItem)) return false;
            TutorialGate.NotifyBlocked(); // notifies the tutorial system; shows the prompt
            return true;
        }

        private void WireConfirmButtons()
        {
            var menuBtn = menuButton?.GetComponent<UnityEngine.UI.Button>();
            if (menuBtn != null)
            {
                menuBtn.onClick.RemoveAllListeners();
                menuBtn.onClick.AddListener(() =>
                {
                    if (BlockedByUseItemTutorial()) return;
                    ConfirmationDialog.Instance?.Show(
                        "Return to main menu?",
                        () => GameManager.Instance?.ToMainMenu()
                    );
                });
            }

            var landBtn = landButton?.GetComponent<UnityEngine.UI.Button>();
            if (landBtn != null)
            {
                landBtn.onClick.RemoveAllListeners();
                landBtn.onClick.AddListener(() =>
                {
                    if (BlockedByUseItemTutorial()) return;
                    ConfirmationDialog.Instance?.Show(
                        "Return to land?",
                        () =>
                        {
                            // Button sits in the pause menu: close it, or its lock and timeScale 0 follow us to land.
                            UIManager.Instance?.CloseAll();
                            GameManager.Instance?.ReturnToLand();
                        }
                    );
                });
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
            if (GameSession.State != GameState.Ocean)
            {
                maxLengthPanel.SetActive(false);
                return;
            }

            maxLengthPanel.SetActive(true);
            RefreshLandButton();

            var player = PlayerRegistry.Local;
            if (player == null) return;

            if (rope == null) // cached: was a GetComponentInChildren every frame
                rope = player.GetComponentInChildren<OxygenRope>();
            if (rope == null) return;

            // Remaining health in whole metres, same format as the current length below.
            dangerLengthText.text = $"{(int)Mathf.Abs(player.CurrentHealth)} m.";

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
