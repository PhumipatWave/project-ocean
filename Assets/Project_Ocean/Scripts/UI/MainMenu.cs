using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class MainMenu : MonoBehaviour
    {
        public void StartGame()
        {
            SceneManager.LoadScene("Land_Map");
        }

        public void QuiteGame()
        {
            Application.Quit();
        }
    }
}
