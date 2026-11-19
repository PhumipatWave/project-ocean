using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
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
