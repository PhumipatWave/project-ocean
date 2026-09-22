using UnityEngine;

namespace Game.Testing
{
    public class LoadingButton : MonoBehaviour
    {
        public void StartGame()
        {
            if (LoadingManager.Instance == null)
            {
                Debug.LogError("LoadingManager is not available.");
                return;
            }

            LoadingManager.Instance.StartGame();
        }

        public void BackToMenu()
        {
            if (LoadingManager.Instance == null)
            {
                Debug.LogError("LoadingManager is not available.");
                return;
            }

            LoadingManager.Instance.BackToMenu();
        }

        public void GoToOcean()
        {
            if (LoadingManager.Instance == null)
            {
                Debug.LogError("LoadingManager is not available.");
                return;
            }

            LoadingManager.Instance.GoToOcean();
        }

        public void BackToLand()
        {
            if (LoadingManager.Instance == null)
            {
                Debug.LogError("LoadingManager is not available.");
                return;
            }

            LoadingManager.Instance.BackToLand();
        }
    }
}