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

            // A fresh scene load always starts with no land UI open.
            SetHudVisible(true);

            if (scene.name == "MainMenuScene")
            {
                menuButton.SetActive(false);
                landButton.SetActive(false);
            }
            else if (scene.name == "Land_Map")
            {
                menuButton.SetActive(true);
                landButton.SetActive(false);
                RefreshDayText();
            }
            else if (isOcean)
            {
                menuButton.SetActive(false);
                landButton.SetActive(true);
            }
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
            if (dayCountText == null) return;
            int day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;
            dayCountText.text = $"Day {day}";
        }

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Ocean)
            {
                maxLengthPanel.SetActive(false);
                return;
            }

            maxLengthPanel.SetActive(true);

            var rope = GameManager.CurPlayer.GetComponentInChildren<OxygenRope>();
            if (rope == null) return;

            dangerLengthText.text = $"{Mathf.Abs(rope.MaxRopeLength).ToString("F2")} m.";

            if (currentOxygenTubeLength != null)
            {
                currentOxygenTubeLength.text = $"{(int)rope.CurrentLength} m.";
                currentOxygenTubeLength.color = rope.CurrentLength >= rope.MaxRopeLength ? Color.red : Color.white;
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
