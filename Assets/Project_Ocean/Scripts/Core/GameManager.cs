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

        private GameState gameState;
        public GameState GameState { get { return gameState; } set { gameState = value; } }

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
            // Reads only SceneLoadContext — no reference to SaveBridge, so the
            // GameManager <-> SaveBridge circular dependency is broken.
            if (SceneLoadContext.IsLoading)
            {
                // Load from save — GameManager only handles position.
                // SceneLoadContext.Position is always set before LoadScene().
                MovePlayerTo(SceneLoadContext.Position);

                // Do NOT clear the flag here.
                // SaveBridge's coroutine calls SceneLoadContext.Clear() after
                // applying stats/inventory/etc.
                return;
            }

            // Normal scene transition — find the spawn point in the newly loaded scene.
            if (scene.name == SceneNames.Land || scene.name == SceneNames.Ocean)
            {
                var spawnObj = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
                if (spawnObj != null)
                    MovePlayerTo(spawnObj.transform.position);
                else
                    Debug.LogWarning($"[GameManager] No PlayerSpawnPoint tag found in {scene.name}");
            }
        }

        /// <summary>Teleports the player to a position and resets their velocity.</summary>
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
            SceneManager.LoadScene(SceneNames.MainMenu);
            gameState = GameState.Land;
        }

        public void StartDive()
        {
            SceneManager.LoadScene(SceneNames.Ocean);
            gameState = GameState.Ocean;
        }

        public void ReturnToLand()
        {
            CurPlayer.OxygenTube.ResetRopeData();
            SceneManager.LoadScene(SceneNames.Land);
            gameState = GameState.Land;
        }
    }
}
