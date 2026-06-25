using System;
using System.Collections;
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

        [Header("Black Fade")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 1f;

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
        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            DayManager.OnDayAdvanced += RefreshDayText;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            DayManager.OnDayAdvanced -= RefreshDayText;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool isOcean = scene.name == "Ocean_Map";

            //weaponUIPanel?.SetActive(isOcean);

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
            }
            else
            {
                maxLengthPanel.SetActive(true);
                dangerLengthText.text = $"{Mathf.Abs(GameManager.CurPlayer.GetComponentInChildren<OxygenRope>().MaxRopeLength).ToString("F2")} m.";
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
