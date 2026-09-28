using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Core
{
    // วิธีใช้:
    // 1. ผูกสคริปต์นี้กับ GameObject เดียวกับที่มี Video Player (เช่น VideoManager)
    // 2. ลาก Video Player component ของ object นั้นมาใส่ในช่อง Video Player ด้านล่าง
    // 3. ใส่ชื่อ Scene ถัดไป (เช่น "MainMenuScene") ในช่อง Next Scene Name
    // 4. (ถ้ามีปุ่ม Skip) ลากปุ่มมาใส่ในช่อง Skip Button แล้วไม่ต้องผูก OnClick เองใน Inspector สคริปต์จะผูกให้อัตโนมัติ

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
            // ถ้าไม่ได้ลาก Video Player มาใส่ใน Inspector จะลองหาใน GameObject เดียวกันให้อัตโนมัติ
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

        // เรียกอัตโนมัติเมื่อคลิปเล่นจบ
        private void OnIntroFinished(VideoPlayer vp)
        {
            LoadNextScene();
        }

        // เรียกเมื่อผู้เล่นกดปุ่ม Skip
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
            // กันโหลดซ้ำ เผื่อ event ยิงซ้ำหรือกด Skip พร้อมคลิปจบพอดี
            if (hasLoadedNextScene) return;
            hasLoadedNextScene = true;

            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning("IntroPlayer: ยังไม่ได้ตั้งชื่อ Next Scene Name ใน Inspector");
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
