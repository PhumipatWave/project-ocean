using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;

namespace Game.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        /// <summary>Raised just before returning to the main menu, so persistent UI can close.</summary>
        public static event System.Action OnReturningToMenu;

        [SerializeField] private Player playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;
        public Transform PlayerSpawnPoint => playerSpawnPoint;

        /// <summary>
        /// The local player. Backed by PlayerRegistry — use PlayerRegistry.Local directly
        /// in new code. This property exists for backwards compatibility with existing callers.
        /// </summary>
        public static Player CurPlayer => PlayerRegistry.Local;

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

            // Lock game fps to 60
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            gameState = SceneManager.GetActiveScene().name == SceneNames.MainMenu
                ? GameState.Menu
                : GameState.Land;
        }

        /// <summary>Instantiates the player prefab once, the first time a gameplay scene loads
        /// (normally Land, via "Start Game" — Ocean too, in case a save is loaded straight into it).</summary>
        private void SpawnPlayer()
        {
            var spawnPos = playerSpawnPoint != null
                ? playerSpawnPoint.position
                : (GameObject.FindWithTag(Tags.PlayerSpawnPoint)?.transform.position ?? Vector3.zero);
            PlayerRegistry.Register(Instantiate(playerPrefab, spawnPos, Quaternion.identity));
        }

        private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == SceneNames.MainMenu)
            {
                gameState = GameState.Menu;
                return;
            }

            // StartDive/ReturnToLand also set this explicitly after their own LoadScene call,
            // but "Start Game" (MainMenu -> Land) never went through either — set it here too
            // so gameState is always correct regardless of which path loaded the scene.
            if (scene.name == SceneNames.Land)
                gameState = GameState.Land;
            else if (scene.name == SceneNames.Ocean)
                gameState = GameState.Ocean;

            // First time a gameplay scene loads (normal "Start Game" always lands on Land;
            // Ocean too, for a save loaded straight into a dive) — the player doesn't exist yet.
            if ((scene.name == SceneNames.Land || scene.name == SceneNames.Ocean) && CurPlayer == null)
                SpawnPlayer();

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
                var spawnObj = GameObject.FindGameObjectWithTag(Tags.PlayerSpawnPoint);
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
            if (LoadingManager.IsLoading) return;

            // UIManager (pause menu, etc.) is DontDestroyOnLoad and closes on this event — without it,
            // the menu while it's open leaves its panel active, its PlayerUIState lock held,
            // and (since ApplyBlockingState set it) Time.timeScale stuck at 0 forever, silently
            // freezing everything that loads next (including a fresh New Game).
            OnReturningToMenu?.Invoke();
            LoadWithLoadingScreen(SceneNames.MainMenu, LoadingType.Menu);
            gameState = GameState.Menu;
        }

        public void StartDive()
        {
            // A load is already running — LoadingManager would ignore this request, but the dive
            // would still be counted below (a double trigger burned 2 of the 3 daily dives).
            if (LoadingManager.IsLoading) return;

            if (!FeatureUnlockService.Instance.IsUnlocked(GameFeature.Ocean))
            {
                NotificationManager.Notify("Locked — progress the main quest to unlock the ocean.");
                return;
            }

            // A different action is the guided tutorial's current requirement — diving now
            // would waste the Dive step's one-shot completion event (and burn a daily dive).
            if (TutorialGate.Blocks(TutorialActionId.Dive)) { TutorialGate.NotifyBlocked(); return; }

            if (DayManager.Instance != null && !DayManager.Instance.TryRecordDive())
            {
                NotificationManager.Notify($"You can only dive {DayManager.MaxDivesPerDay} times per day.");
                return;
            }

            TutorialEvents.RaiseAction(TutorialActionId.Dive);
            LoadWithLoadingScreen(SceneNames.Ocean, LoadingType.Ocean);
            gameState = GameState.Ocean;
        }

        public void ReturnToLand()
        {
            if (LoadingManager.IsLoading) return; // already transitioning: don't re-raise BackToLand or flip gameState

            CurPlayer.OxygenTube.ResetRope();
            TutorialEvents.RaiseAction(TutorialActionId.BackToLand);
            LoadWithLoadingScreen(SceneNames.Land, LoadingType.Land);
            gameState = GameState.Land;
        }

        private static void LoadWithLoadingScreen(string sceneName, LoadingType loadingType)
        {
            if (LoadingManager.Instance != null)
                LoadingManager.Instance.LoadScene(sceneName, loadingType);
            else
                SceneManager.LoadScene(sceneName);
        }
    }
}
