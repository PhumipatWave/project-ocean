using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
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
