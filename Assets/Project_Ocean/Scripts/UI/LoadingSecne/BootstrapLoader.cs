using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class BootstrapLoader : MonoBehaviour
    {
        // First launch: straight into the Intro, no loading screen. IntroPlayer uses the
        // loading screen for Intro -> Main Menu.
        private void Start()
        {
            SceneManager.LoadScene(SceneNames.Intro);
        }
    }
}
