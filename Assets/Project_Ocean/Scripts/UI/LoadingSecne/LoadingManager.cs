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

        [SerializeField]
        private LoadingVisualController loadingVisualController;

        private bool isLoading;

        /// <summary>True while the loading screen is up. Tutorial/dialogue UI and interaction wait on this.</summary>
        public static bool IsLoading => Instance != null && Instance.isLoading;

        /// <summary>Fired after the loading screen hides.</summary>
        public static event System.Action OnLoadingFinished;

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
            // BootstrapLoader.Start may already have begun the first load.
            if (!isLoading)
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
            LoadScene(landScene, LoadingType.Menu);
        }

        public void BackToMenu()
        {
            LoadScene(mainMenuScene, LoadingType.Menu);
        }

        // =========================================================
        // LAND / OCEAN
        // =========================================================

        public void GoToOcean()
        {
            LoadScene(oceanScene, LoadingType.Ocean);
        }

        public void BackToLand()
        {
            LoadScene(landScene, LoadingType.Land);
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

            OnLoadingFinished?.Invoke();
        }

        public void LoadScene(string sceneName, LoadingType loadingType)
        {
            if (isLoading)
                return;

            // Show first: Animator.Play is ignored on an inactive icon.
            ShowLoading();

            if (loadingVisualController != null)
            {
                loadingVisualController.ApplyProfile(loadingType);
            }

            LoadScene(sceneName);
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
