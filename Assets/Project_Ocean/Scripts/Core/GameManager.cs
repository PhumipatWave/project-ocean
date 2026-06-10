using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public enum GameState
    {
        Land,
        Ocean
    }

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Player playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;
        public Transform PlayerSpawnPoint => playerSpawnPoint;

        public static Player CurPlayer;

        [SerializeField] private GameObject[] gameUI;
        public GameObject[] GameUI { get { return gameUI; } set { gameUI = value; } }

        private GameState gameState;
        public GameState GameState { get { return gameState; } set { gameState = value; } }

        [SerializeField] private GameObject blackScene;

        [SerializeField] private bool miniHubAvaliable;
        public bool MiniHubAvaliable { get { return miniHubAvaliable; } set { miniHubAvaliable = value; } }

        [SerializeField] private Transform miniHubLocation;
        public Transform MiniHubLocation { get { return miniHubLocation; } set { miniHubLocation = value; } }

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) 
            { 
                Destroy(this.gameObject); 
                return; 
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            gameState = GameState.Land;
            CurPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        }

        private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (SaveBridge.IsLoadingSave)
            {
                // ✅ Load จาก save — GameManager จัดการ position อย่างเดียว
                // SaveBridge.LoadedPosition ถูก set ก่อน LoadScene() เสมอ
                MovePlayerTo(SaveBridge.LoadedPosition);

                // ✅ ไม่ clear IsLoadingSave ที่นี่
                // SaveBridge coroutine จะ clear เองหลัง apply stats/inventory เสร็จ
                return;
            }

            // ปกติ — ใช้ PlayerSpawnPoint ของ scene
            if (scene.name == "Land_Map" || scene.name == "Ocean_Map")
            {
                var spawnObj = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
                if (spawnObj != null)
                    MovePlayerTo(spawnObj.transform.position);
                else
                    Debug.LogWarning($"[GameManager] ไม่พบ PlayerSpawnPoint ใน {scene.name}");
            }
        }

        /// <summary>ย้าย Player ไปตำแหน่งที่กำหนด พร้อม reset velocity</summary>
        private void MovePlayerTo(Vector3 position)
        {
            if (CurPlayer == null)
            {
                Debug.LogWarning("[GameManager] MovePlayerTo: CurPlayer is null");
                return;
            }

            var rb = CurPlayer.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity  = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType        = RigidbodyType2D.Kinematic;
                CurPlayer.transform.position = position;
                rb.bodyType        = RigidbodyType2D.Dynamic;
            }
            else
            {
                CurPlayer.transform.position = position;
            }
        }

        public void ToMainMenu()
        {
            SceneManager.LoadScene("MainMenuScene");
            gameState = GameState.Ocean;
        }

        public void StartDive()
        {
            SceneManager.LoadScene("Ocean_Map");
            gameState = GameState.Ocean;
        }

        public void ReturnToLand()
        {
            CurPlayer.OxygenTube.ResetRopeData();
            SceneManager.LoadScene("Land_Map");
            gameState = GameState.Land;
        }

        private IEnumerator FadeRoutine()
        {
            blackScene.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            blackScene.SetActive(false);
        }
    }
}