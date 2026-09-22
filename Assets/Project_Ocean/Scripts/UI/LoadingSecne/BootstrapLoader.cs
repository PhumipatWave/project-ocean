using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class BootstrapLoader : MonoBehaviour
    {
        private void Start()
        {
            if (LoadingManager.Instance != null)
                LoadingManager.Instance.LoadScene(SceneNames.MainMenu, LoadingType.Menu);
            else
                SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
