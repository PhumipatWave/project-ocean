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
        public Transform PlayerSpawnPoint { get { return playerSpawnPoint; } }

        public static Player CurPlayer;

        [SerializeField] private GameObject[] gameUI;
        public GameObject[] GameUI { get { return gameUI; } set { gameUI = value; } }

        private GameState gameState;
        public GameState GameState { get { return gameState; } set { gameState = value; } }

        [SerializeField] private GameObject blackScene;

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            CurPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        }

        private void Start()
        {
            gameState = GameState.Land;
        }

        public void StartDive()
        {
            StartCoroutine(FadeRoutine());

            SceneManager.LoadScene("Graybox_Oceans");
            gameState = GameState.Ocean;
        }

        public void ReturnToLand()
        {
            StartCoroutine(FadeRoutine());

            SceneManager.LoadScene("Graybox_Land");
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
