using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Core
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
        [Tooltip("The progress bar fills over this many seconds (never ahead of the real load).")]
        [SerializeField] private float minimumLoadingTime = 0.5f;

        [Tooltip("Extra realtime seconds the screen stays up after the scene activates, so Start() and Animators (player land/ocean sprite) settle out of view.")]
        [SerializeField] private float postLoadHold = 0.3f;

        [Tooltip("Max realtime seconds to wait for Block()ers (e.g. SaveBridge applying save data) before hiding anyway.")]
        [SerializeField] private float readyTimeout = 5f;

        // Systems still preparing the new scene. The screen stays up until each calls Unblock.
        private static readonly HashSet<object> readyBlockers = new();

        /// <summary>Hold the loading screen up until Unblock(owner) — call before starting the load
        /// (or in Awake of a scene object). Always pair with Unblock, ideally in a finally.</summary>
        public static void Block(object owner) => readyBlockers.Add(owner);

        public static void Unblock(object owner) => readyBlockers.Remove(owner);

        // How fast the bar may move toward its target, in bar-lengths per second. Only smooths the
        // steps of the real load progress; the time-based fill is already linear.
        private const float FillSmoothing = 2f;

        // Typed as MonoBehaviour so Core doesn't reference the UI class; same name keeps the scene reference.
        [SerializeField]
        private MonoBehaviour loadingVisualController;

        // A dragged GameObject can land on the wrong component (e.g. its Image); keep the one implementing the interface.
        private void OnValidate()
        {
            if (loadingVisualController != null && !(loadingVisualController is ILoadingVisual))
                loadingVisualController = loadingVisualController.GetComponent<ILoadingVisual>() as MonoBehaviour;
        }

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
                readyBlockers.Clear(); // no load will run to release them
                HideLoading(); // LoadScene(name, type) shows the canvas first — don't leave it up
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

            // Null when the scene isn't in Build Settings. Without this the coroutine threw here,
            // leaving isLoading stuck true — every later load was silently ignored (soft-lock).
            if (operation == null)
            {
                Debug.LogError($"LoadingManager: cannot load scene '{sceneName}' (missing from Build Settings?).");
                isLoading = false;
                readyBlockers.Clear(); // no scene to prepare — don't leave the next load waiting on them
                HideLoading();
                yield break;
            }

            operation.allowSceneActivation = false;

            // The bar fills across the loading wait time and reaches 100% exactly when the scene is
            // ready AND the wait time has passed. Target = whichever is BEHIND: time keeps a fast load
            // from jumping to full (the old bug: bar full instantly, then sat there for the wait),
            // real progress keeps a slow load from showing more than is actually done.
            float shown = 0f;
            while (shown < 1f)
            {
                float byTime = minimumLoadingTime > 0f
                    ? Mathf.Clamp01((Time.unscaledTime - startTime) / minimumLoadingTime)
                    : 1f;
                float byLoad = Mathf.Clamp01(operation.progress / 0.9f); // 0.9 = "ready to activate"

                shown = Mathf.MoveTowards(shown, Mathf.Min(byTime, byLoad), Time.unscaledDeltaTime * FillSmoothing);

                if (progressBar != null)
                    progressBar.value = shown;

                yield return null;
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                yield return null;
            }

            // Old scene's textures/audio/meshes no longer referenced — free them behind the screen.
            yield return Resources.UnloadUnusedAssets();

            // Wait for systems still preparing the scene (e.g. SaveBridge applying a save).
            float waitStart = Time.unscaledTime;
            while (readyBlockers.Count > 0 && Time.unscaledTime - waitStart < readyTimeout)
                yield return null;

            if (readyBlockers.Count > 0)
            {
                Debug.LogWarning($"[LoadingManager] Ready timeout — still blocked by: {string.Join(", ", readyBlockers)}");
                readyBlockers.Clear();
            }

            // Scene is active but its Start() and Animator updates haven't run yet — hiding now
            // shows the player's land/ocean sprite swap. Let them run behind the screen first.
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(postLoadHold);

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

            if (loadingVisualController is ILoadingVisual visual)
            {
                visual.ApplyProfile(loadingType);
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
