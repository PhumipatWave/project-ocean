using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Testing
{
    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Instance { get; private set; }

        [Header("Loading UI")]
        [SerializeField] private GameObject loadingCanvas;
        [SerializeField] private Slider progressBar;

        [Header("Scene Names")]
        [SerializeField] private string mainMenuScene = "MainMenuScene";
        [SerializeField] private string landScene = "Land_Map";
        [SerializeField] private string oceanScene = "Ocean_Map";

        [Header("Settings")]
        [SerializeField] private float minimumLoadingTime = 0.5f;

        private bool isLoading;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            HideLoading();
        }

        // =========================================================
        // GENERAL
        // =========================================================

        public void LoadScene(string sceneName)
        {
            if (isLoading)
                return;

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("LoadingManager: Scene name is empty.");
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        // =========================================================
        // MAIN MENU
        // =========================================================

        public void StartGame()
        {
            LoadScene(landScene);
        }

        public void BackToMenu()
        {
            LoadScene(mainMenuScene);
        }

        // =========================================================
        // LAND / OCEAN
        // =========================================================

        public void GoToOcean()
        {
            LoadScene(oceanScene);
        }

        public void BackToLand()
        {
            LoadScene(landScene);
        }

        // =========================================================
        // LOADING
        // =========================================================

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            isLoading = true;

            ShowLoading();

            if (progressBar != null)
                progressBar.value = 0f;

            float startTime = Time.unscaledTime;

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(sceneName);

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                float progress =
                    Mathf.Clamp01(operation.progress / 0.9f);

                if (progressBar != null)
                    progressBar.value = progress;

                yield return null;
            }

            if (progressBar != null)
                progressBar.value = 1f;

            float elapsed = Time.unscaledTime - startTime;

            if (elapsed < minimumLoadingTime)
            {
                yield return new WaitForSecondsRealtime(
                    minimumLoadingTime - elapsed
                );
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                yield return null;
            }

            isLoading = false;

            HideLoading();
        }

        // =========================================================
        // UI
        // =========================================================

        private void ShowLoading()
        {
            if (loadingCanvas != null)
                loadingCanvas.SetActive(true);
        }

        private void HideLoading()
        {
            if (loadingCanvas != null)
                loadingCanvas.SetActive(false);
        }
    }
}
