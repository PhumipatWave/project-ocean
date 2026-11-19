using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Core
{
    /// <summary>Plays the intro video, then loads Next Scene Name; wires the optional Skip button itself.</summary>
    public class IntroPlayer : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private VideoPlayer videoPlayer;

        [Header("Scene To Load After Intro")]
        [SerializeField] private string nextSceneName = "MainMenuScene";

        [Header("Optional Skip Button")]
        [SerializeField] private Button skipButton;

        private bool hasLoadedNextScene = false;

        private void Awake()
        {
            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
            }
        }

        private void OnEnable()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached += OnIntroFinished;
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(SkipIntro);
            }
        }

        private void OnDisable()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnIntroFinished;
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(SkipIntro);
            }
        }

        private void Update()
        {
            // Space skips the intro (no InputReader exists this early — it lives with the player/HUD).
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                SkipIntro();
        }

        private void OnIntroFinished(VideoPlayer vp)
        {
            LoadNextScene();
        }

        public void SkipIntro()
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            LoadNextScene();
        }

        private void LoadNextScene()
        {
            // The clip can end on the same frame Skip is pressed; load only once.
            if (hasLoadedNextScene) return;
            hasLoadedNextScene = true;

            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning("IntroPlayer: Next Scene Name is not set in the Inspector.");
                return;
            }

            // Through the loading screen when started from Bootstrap; plain load when IntroScene is played directly.
            if (Game.Core.LoadingManager.Instance != null)
                Game.Core.LoadingManager.Instance.LoadScene(nextSceneName, Game.Core.LoadingType.Menu);
            else
                SceneManager.LoadScene(nextSceneName);
        }
    }
}
