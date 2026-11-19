using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>Lets Esc close this Land panel through its own Close button; the interact zone owns the lock.</summary>
    public class EscClosablePanel : MonoBehaviour
    {
        private void OnEnable()
        {
            PlayerUIState.SetCloser(() =>
            {
                gameObject.SetActive(false);
                PlayerUIState.Close();
            });
        }
    }
}
