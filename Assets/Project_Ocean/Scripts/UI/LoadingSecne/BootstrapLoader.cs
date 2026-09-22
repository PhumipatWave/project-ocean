using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class BootstrapLoader : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}