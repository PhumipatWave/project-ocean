
using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class LoadingButton : MonoBehaviour
    {
        public void StartGame()
        {
            // Start the game on land.
            SetUnderwaterState(false);

            if (LoadingManager.Instance == null)
            {
                Debug.LogError("LoadingManager is not available.");
                return;
            }

            LoadingManager.Instance.StartGame();
        }

        public void BackToMenu()
        {
            SetUnderwaterState(false);

            if (LoadingManager.Instance == null)
            {
                Debug.LogError("LoadingManager is not available.");
                return;
            }

            LoadingManager.Instance.BackToMenu();
        }

        public void GoToOcean()
        {
            // The player is entering the underwater area.
            SetUnderwaterState(true);

            if (LoadingManager.Instance == null)
            {
                Debug.LogError("LoadingManager is not available.");
                return;
            }

            LoadingManager.Instance.GoToOcean();
        }

        public void BackToLand()
        {
            // The player is returning to land.
            SetUnderwaterState(false);

            if (LoadingManager.Instance == null)
            {
                Debug.LogError("LoadingManager is not available.");
                return;
            }

            LoadingManager.Instance.BackToLand();
        }

        private void SetUnderwaterState(bool underwater)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetUnderwater(underwater);
            }
        }
    }
}
