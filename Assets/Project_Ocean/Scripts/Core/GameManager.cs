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
        private GameState gameState;
        public GameState GameState { get { return gameState; } set { gameState = value; } }

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
        }

        private void Start()
        {
            gameState = GameState.Land;
        }

        public void StartDive()
        {
            SceneManager.LoadScene("Graybox_Oceans");
            gameState = GameState.Ocean;
        }

        public void ReturnToLand()
        {
            SceneManager.LoadScene("Graybox_Land");
            gameState = GameState.Land;
        }
    }
}
